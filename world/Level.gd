extends Node3D
## Procedurally-built arena: a floor, perimeter walls, and a set of raised
## platforms used as cover (Shooter), jumpable terrain (Platformer) or
## decoration (Collector). All geometry is built in code so the scene stays
## minimal and version-friendly.

const ARENA := 28.0           # half-extent of the floor
const WALL_H := 3.0
const WALL_T := 1.0

## Marker positions the spawner reads to decide where to place things.
@onready var platform_tops: Array[Vector3] = []
@onready var ground_points: Array[Vector3] = []
@onready var player_start: Vector3 = Vector3(0, 1.5, 0)


func _ready() -> void:
	_build_floor()
	_build_walls()
	_build_platforms()
	_build_ground_points()


func _build_floor() -> void:
	var body := StaticBody3D.new()
	body.name = "Floor"
	add_child(body)
	var floor_mesh := _box_mesh(ARENA * 2.0, 0.4, ARENA * 2.0, Color(0.18, 0.32, 0.20))
	floor_mesh.position = Vector3(0, -0.2, 0)
	body.add_child(floor_mesh)
	var col := CollisionShape3D.new()
	var shape := BoxShape3D.new()
	shape.size = Vector3(ARENA * 2.0, 0.4, ARENA * 2.0)
	col.shape = shape
	col.position = Vector3(0, -0.2, 0)
	body.add_child(col)


func _build_walls() -> void:
	var walls := Node3D.new()
	walls.name = "Walls"
	add_child(walls)
	var col_w := Color(0.35, 0.32, 0.28)
	for i in 4:
		var rot := i * PI / 2.0
		var b := StaticBody3D.new()
		walls.add_child(b)
		var length := ARENA * 2.0
		b.add_child(_box_mesh(WALL_T, WALL_H, length, col_w))
		b.position = Vector3(sin(rot) * ARENA, WALL_H * 0.5, cos(rot) * ARENA)
		b.rotation.y = rot
		var col := CollisionShape3D.new()
		var shape := BoxShape3D.new()
		shape.size = Vector3(WALL_T, WALL_H, length)
		col.shape = shape
		b.add_child(col)


func _build_platforms() -> void:
	var defs := [
		{"pos": Vector3(0, 2.0, -6), "size": Vector3(5, 0.5, 5)},
		{"pos": Vector3(-8, 3.5, -2), "size": Vector3(4, 0.5, 4)},
		{"pos": Vector3(7, 5.0, 0), "size": Vector3(3.5, 0.5, 3.5)},
		{"pos": Vector3(0, 4.5, 7), "size": Vector3(6, 0.5, 3)},
		{"pos": Vector3(-6, 6.5, 6), "size": Vector3(3, 0.5, 3)},
		{"pos": Vector3(9, 2.5, -7), "size": Vector3(3, 0.5, 3)},
	]
	var col_p := Color(0.42, 0.36, 0.30)
	for d in defs:
		var b := StaticBody3D.new()
		add_child(b)
		b.position = d.pos
		b.add_child(_box_mesh(d.size.x, d.size.y, d.size.z, col_p))
		var col := CollisionShape3D.new()
		var shape := BoxShape3D.new()
		shape.size = d.size
		col.shape = shape
		b.add_child(col)
		# Record the top surface for coin placement (Platformer mode).
		platform_tops.append(d.pos + Vector3(0, d.size.y * 0.5 + 0.6, 0))
		# Also use platform positions as scattered ground-level cover points.
		ground_points.append(d.pos)


func _build_ground_points() -> void:
	# A grid of candidate spawn points on the floor, away from centre start.
	var rng := RandomNumberGenerator.new()
	rng.seed = 1337
	for x in range(-5, 6, 2):
		for z in range(-5, 6, 2):
			var p := Vector3(x + rng.randf_range(-0.6, 0.6), 0.6, z + rng.randf_range(-0.6, 0.6))
			if p.length() < 3.0:
				continue
			ground_points.append(p)


func _box_mesh(sx: float, sy: float, sz: float, color: Color) -> MeshInstance3D:
	var mi := MeshInstance3D.new()
	var mesh := BoxMesh.new()
	mesh.size = Vector3(sx, sy, sz)
	mi.mesh = mesh
	var mat := StandardMaterial3D.new()
	mat.albedo_color = color
	mat.roughness = 0.9
	mi.material_override = mat
	return mi
