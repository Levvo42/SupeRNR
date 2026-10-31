"""Text checks of the layout, no Godot needed. python tools/drift_track/preview.py"""
import math, os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from layout import *

pts, arcs, height, ditch, kind = build_grid()
print(f"{len(pts)} m of track, heights {min(height):.2f} to {max(height):.2f} m")
for i, a in enumerate(arcs):
    print(f"corner {i:2}: start ({a['start'][0]:7.1f},{a['start'][1]:7.1f}) end ({a['end'][0]:7.1f},{a['end'][1]:7.1f})"
          f" turn {math.degrees(a['turn']):7.1f} r {a['r']:5.1f} centre ({a['centre'][0]:7.1f},{a['centre'][1]:7.1f})")
print("steep steps:", len(steep_steps(height)), steep_steps(height)[:8])
# Other parts of the road passing close to each other, apart from the planned crossing.
near = {}
for i, p in enumerate(pts):
    for j in range(i + 60, len(pts), 3):
        q = pts[j]
        if len(pts) - j + i < 60:
            continue
        d = math.hypot(p[0] - q[0], p[1] - q[1]) - half_width(p[4], p[5]) - half_width(q[4], q[5])
        key = (p[4], q[4])
        if d < 6 and d < near.get(key, (1e9,))[0]:
            near[key] = (round(d, 1), round(p[0]), round(p[1]))
print("road edges closer than 6 m:", near)
for run in rails(pts) + passage_rails(pts, arcs):
    print(f"rail {len(run):3} pts from ({run[0][0]:.1f},{run[0][1]:.1f}) to ({run[-1][0]:.1f},{run[-1][1]:.1f})")
