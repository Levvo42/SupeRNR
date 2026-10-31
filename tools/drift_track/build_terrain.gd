extends SceneTree
# Turns heights.bin + weights.bin (from build.py) into the terrain mesh and collision shape.

func _init() -> void:
	var dir := "res://.godot/drift_track"
	var meta: Dictionary = JSON.parse_string(FileAccess.get_file_as_string(dir + "/meta.json"))
	var nx: int = meta.nx
	var nz: int = meta.nz
	var heights := FileAccess.get_file_as_bytes(dir + "/heights.bin").to_float32_array()
	var weights := FileAccess.get_file_as_bytes(dir + "/weights.bin")
	assert(heights.size() == nx * nz)
	var half_x := (nx - 1) / 2.0
	var half_z := (nz - 1) / 2.0

	var verts := PackedVector3Array()
	var normals := PackedVector3Array()
	var colors := PackedColorArray()
	verts.resize(nx * nz)
	normals.resize(nx * nz)
	colors.resize(nx * nz)
	for gz in nz:
		for gx in nx:
			var i := gz * nx + gx
			verts[i] = Vector3(gx - half_x, heights[i], gz - half_z)
			var hl := heights[i - 1] if gx > 0 else heights[i]
			var hr := heights[i + 1] if gx < nx - 1 else heights[i]
			var hb := heights[i - nx] if gz > 0 else heights[i]
			var hf := heights[i + nx] if gz < nz - 1 else heights[i]
			normals[i] = Vector3(hl - hr, 2.0, hb - hf).normalized()
			colors[i] = Color(weights[i * 3] / 255.0, weights[i * 3 + 1] / 255.0, weights[i * 3 + 2] / 255.0)
	var indices := PackedInt32Array()
	indices.resize((nx - 1) * (nz - 1) * 6)
	var n := 0
	for gz in nz - 1:
		for gx in nx - 1:
			var a := gz * nx + gx
			for v in [a, a + 1, a + nx, a + 1, a + nx + 1, a + nx]:
				indices[n] = v
				n += 1
	var arrays := []
	arrays.resize(Mesh.ARRAY_MAX)
	arrays[Mesh.ARRAY_VERTEX] = verts
	arrays[Mesh.ARRAY_NORMAL] = normals
	arrays[Mesh.ARRAY_COLOR] = colors
	arrays[Mesh.ARRAY_INDEX] = indices
	var mesh := ArrayMesh.new()
	mesh.add_surface_from_arrays(Mesh.PRIMITIVE_TRIANGLES, arrays)
	print("mesh save: ", ResourceSaver.save(mesh, "res://game/spike/drift_track_terrain.res"))

	var shape := HeightMapShape3D.new()
	shape.map_width = nx
	shape.map_depth = nz
	shape.map_data = heights
	print("shape save: ", ResourceSaver.save(shape, "res://game/spike/drift_track_terrain_shape.res"))
	quit()
