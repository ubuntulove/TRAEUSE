# chunk.gd - A 16^3 voxel chunk: stores voxel ids and builds a face-culled mesh.
extends Node3D

const CHUNK_SIZE := 16
const CHUNK_SIZE_F := 16.0

# Each face: neighbor direction, 4 corner offsets (CCW from outside),
# color key for lookup, baked shade multiplier for fake directional lighting.
const FACES := [
	{dir = Vector3i(1, 0, 0), corners = [Vector3(1, 0, 0), Vector3(1, 1, 0), Vector3(1, 1, 1), Vector3(1, 0, 1)], key = "side", shade = 0.72},
	{dir = Vector3i(-1, 0, 0), corners = [Vector3(0, 0, 1), Vector3(0, 1, 1), Vector3(0, 1, 0), Vector3(0, 0, 0)], key = "side", shade = 0.72},
	{dir = Vector3i(0, 1, 0), corners = [Vector3(0, 1, 1), Vector3(1, 1, 1), Vector3(1, 1, 0), Vector3(0, 1, 0)], key = "top", shade = 1.00},
	{dir = Vector3i(0, -1, 0), corners = [Vector3(0, 0, 0), Vector3(1, 0, 0), Vector3(1, 0, 1), Vector3(0, 0, 1)], key = "bottom", shade = 0.50},
	{dir = Vector3i(0, 0, 1), corners = [Vector3(0, 0, 1), Vector3(0, 1, 1), Vector3(1, 1, 1), Vector3(1, 0, 1)], key = "side", shade = 0.85},
	{dir = Vector3i(0, 0, -1), corners = [Vector3(1, 0, 0), Vector3(1, 1, 0), Vector3(0, 1, 0), Vector3(0, 0, 0)], key = "side", shade = 0.85},
]

var chunk_pos: Vector3i = Vector3i.ZERO
var world: Node = null
var data: PackedByteArray = PackedByteArray()

var opaque_mesh: MeshInstance3D
var water_mesh: MeshInstance3D
var collider_body: StaticBody3D

var generated: bool = false


func _ready() -> void:
	opaque_mesh = MeshInstance3D.new()
	opaque_mesh.name = "OpaqueMesh"
	add_child(opaque_mesh)
	water_mesh = MeshInstance3D.new()
	water_mesh.name = "WaterMesh"
	add_child(water_mesh)
	collider_body = StaticBody3D.new()
	collider_body.name = "Collider"
	add_child(collider_body)
	data.resize(CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE)
	data.fill(0)
	position = Vector3(chunk_pos.x, chunk_pos.y, chunk_pos.z) * CHUNK_SIZE_F


func idx(x: int, y: int, z: int) -> int:
	return x + CHUNK_SIZE * (z + CHUNK_SIZE * y)


# Read a block at local coords. Out-of-range reads fall through to the world.
func get_block_local(x: int, y: int, z: int) -> int:
	if x < 0 or x >= CHUNK_SIZE or y < 0 or y >= CHUNK_SIZE or z < 0 or z >= CHUNK_SIZE:
		if world != null:
			var wp := Vector3i(chunk_pos.x * CHUNK_SIZE + x, chunk_pos.y * CHUNK_SIZE + y, chunk_pos.z * CHUNK_SIZE + z)
			return world.get_block(wp)
		return Blocks.ID.AIR
	return data[idx(x, y, z)]


func set_block_local(x: int, y: int, z: int, id: int) -> void:
	if x < 0 or x >= CHUNK_SIZE or y < 0 or y >= CHUNK_SIZE or z < 0 or z >= CHUNK_SIZE:
		return
	data[idx(x, y, z)] = id


func clear_mesh() -> void:
	opaque_mesh.mesh = null
	water_mesh.mesh = null
	for c in collider_body.get_children():
		c.queue_free()


func build_mesh() -> void:
	clear_mesh()
	if not generated:
		return

	# Opaque and water buffers built separately so water can use its own material.
	var o_verts := []
	var o_norm := []
	var o_color := []
	var o_idx := []
	var w_verts := []
	var w_norm := []
	var w_color := []
	var w_idx := []

	for y in CHUNK_SIZE:
		for z in CHUNK_SIZE:
			for x in CHUNK_SIZE:
				var id: int = data[idx(x, y, z)]
				if id == Blocks.ID.AIR:
					continue
				var is_water: bool = Blocks.is_liquid(id)
				for face in FACES:
					var d: Vector3i = face.dir
					var nid: int = get_block_local(x + d.x, y + d.y, z + d.z)
					if not _should_draw_face(id, nid):
						continue
					var verts: Array = w_verts if is_water else o_verts
					var norms: Array = w_norm if is_water else o_norm
					var cols: Array = w_color if is_water else o_color
					var idxs: Array = w_idx if is_water else o_idx
					var base: int = verts.size()
					var normal := Vector3(d.x, d.y, d.z)
					var shade: float = face.shade
					var col: Color = Blocks.get_color(id, face.key)
					col = Color(col.r * shade, col.g * shade, col.b * shade, col.a)
					for c in face.corners:
						var corner: Vector3 = c
						verts.append(Vector3(x, y, z) + corner)
						norms.append(normal)
						cols.append(col)
					idxs.append(base)
					idxs.append(base + 1)
					idxs.append(base + 2)
					idxs.append(base)
					idxs.append(base + 2)
					idxs.append(base + 3)

	if o_idx.size() > 0:
		_commit_mesh(opaque_mesh, o_verts, o_norm, o_color, o_idx, Blocks.opaque_material, true)
	if w_idx.size() > 0:
		_commit_mesh(water_mesh, w_verts, w_norm, w_color, w_idx, Blocks.water_material, false)


func _should_draw_face(id: int, nid: int) -> bool:
	if nid == Blocks.ID.AIR:
		return true
	if Blocks.is_liquid(id) and Blocks.is_liquid(nid):
		return false
	if Blocks.is_transparent(nid) and nid != id:
		return true
	return false


func _commit_mesh(mi: MeshInstance3D, verts: Array, norms: Array, cols: Array, idxs: Array, mat: Material, make_collision: bool) -> void:
	var arrays: Array = []
	arrays.resize(Mesh.ARRAY_MAX)
	arrays[Mesh.ARRAY_VERTEX] = PackedVector3Array(verts)
	arrays[Mesh.ARRAY_NORMAL] = PackedVector3Array(norms)
	arrays[Mesh.ARRAY_COLOR] = PackedColorArray(cols)
	arrays[Mesh.ARRAY_INDEX] = PackedInt32Array(idxs)
	var mesh := ArrayMesh.new()
	mesh.add_surface_from_arrays(Mesh.PRIMITIVE_TRIANGLES, arrays)
	mesh.surface_set_material(0, mat)
	mi.mesh = mesh
	if make_collision:
		var shape := mesh.create_trimesh_shape()
		var col := CollisionShape3D.new()
		col.shape = shape
		collider_body.add_child(col)
