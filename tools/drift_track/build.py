"""Rebuilds the drift track from layout.py. Close the Godot editor first.

    python tools/drift_track/build.py [project folder]
    godot --headless --path <project folder> --script res://tools/drift_track/build_terrain.gd

The first writes the terrain data and updates the rails and the moved tabletop in
drift_track.tscn, leaving the rest of the scene alone. The second bakes the terrain files.
"""
import json, os, re, struct, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from layout import *

ROOT = sys.argv[1] if len(sys.argv) > 1 else os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
DATA = os.path.join(ROOT, ".godot", "drift_track")
SCENE = os.path.join(ROOT, "game", "spike", "drift_track.tscn")
FIRST_NEW_ID = 1700000100
RAIL_SINK = 0.05    # metres the rail foot sits below the ground, so no gap shows on a slope

pts, arcs, height, ditch, kind = build_grid()
cliffs = steep_steps(height)
assert not cliffs, f"terrain has cliffs: {cliffs[:5]}"

# Vertex colours for the terrain shader: r = road, g = ditch depth, b = parking asphalt.
weights = bytearray()
for d, k in zip(ditch, kind):
    weights += bytes((255 if k == ROAD else 0, int(255 * min(1.0, -d / DITCH_DEPTH)) if k == DITCH else 0,
                      255 if k == LOTC else 0))
os.makedirs(DATA, exist_ok=True)
with open(os.path.join(DATA, "heights.bin"), "wb") as f:
    f.write(struct.pack(f"<{len(height)}f", *height))
with open(os.path.join(DATA, "weights.bin"), "wb") as f:
    f.write(bytes(weights))
with open(os.path.join(DATA, "meta.json"), "w") as f:
    json.dump(dict(nx=NX, nz=NZ), f)

with open(SCENE, encoding="utf-8", newline="") as f:
    text = f.read()
runs = rails(pts) + passage_rails(pts, arcs)


def curve_text(n):
    points = ", ".join(f"0, 0, 0, 0, 0, 0, {x:.2f}, {height_at(height, x, z) - RAIL_SINK:.2f}, {z:.2f}" for x, z in runs[n])
    tilts = ", ".join(["0"] * len(runs[n]))
    return (f'[sub_resource type="Curve3D" id="Curve_rail{n}"]\n_data = {{\n"points": PackedVector3Array({points}),\n'
            f'"tilts": PackedFloat32Array({tilts})\n}}\npoint_count = {len(runs[n])}\n\n')


# Curves: each one is rewritten where it stands; extra ones follow the last.
curve_block = re.compile(r'\[sub_resource type="Curve3D" id="Curve_rail(\d+)"\]\n.*?point_count = \d+\n\n', re.S)
last_old = max(int(m.group(1)) for m in curve_block.finditer(text))
extra = "".join(curve_text(n) for n in range(last_old + 1, len(runs)))
text = curve_block.sub(lambda m: (curve_text(int(m.group(1))) if int(m.group(1)) < len(runs) else "")
                       + (extra if int(m.group(1)) == last_old else ""), text)

# Rail nodes keep their unique ids; new ones count up from FIRST_NEW_ID.
section = re.search(r'(\[node name="Rails" type="Node3D" parent="\." unique_id=\d+\]\n\n)(.*?)(\[node name="StartLine")', text, re.S)
ids = re.findall(r'unique_id=(\d+)', section.group(2))
new_ids = iter(range(FIRST_NEW_ID, FIRST_NEW_ID + 2 * len(runs)))
nodes = []
for n in range(len(runs)):
    path_id = ids[2 * n] if 2 * n < len(ids) else next(new_ids)
    rail_id = ids[2 * n + 1] if 2 * n + 1 < len(ids) else next(new_ids)
    nodes.append(f'[node name="RailPath{n + 1}" type="Path3D" parent="Rails" unique_id={path_id}]\n'
                 f'curve = SubResource("Curve_rail{n}")\n\n'
                 f'[node name="Rail{n + 1}" type="CSGPolygon3D" parent="Rails" unique_id={rail_id}]\n'
                 'use_collision = true\npolygon = PackedVector2Array(-0.2, 0, -0.2, 0.7, 0.2, 0.7, 0.2, 0)\n'
                 f'mode = 2\npath_node = NodePath("../RailPath{n + 1}")\npath_interval_type = 0\npath_interval = 1.0\n'
                 'path_simplify_angle = 0.0\npath_rotation = 2\npath_rotation_accurate = false\npath_local = true\n'
                 'path_continuous_u = true\npath_u_distance = 1.0\npath_joined = false\n'
                 'material = SubResource("Mat_rail")\n\n')
text = text[:section.start(2)] + "".join(nodes) + text[section.end(2):]

# The long straight now starts further north, so its tabletop does too.
text, moved = re.subn(r'(\[node name="LongStraight"[^\n]*\ntransform = Transform3D\(0, 0, 1, 0, 1, 0, -1, 0, 0, 154, 0, )-?[\d.]+\)',
                      r'\g<1>-74)', text)
assert moved == 1, "LongStraight tabletop not found"

with open(SCENE, "w", encoding="utf-8", newline="") as f:
    f.write(text)
print(f"{len(pts)} m of track, {len(runs)} rail runs, heights {min(height):.2f} to {max(height):.2f} m")
