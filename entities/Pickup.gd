extends Area3D
## Collectible gem. Rotates and floats; collected when the player touches it.
## Used by Platformer and Collector modes.

var _mesh: MeshInstance3D
var _origin_y: float = 0.0


func _ready() -> void:
	add_to_group("pickup")
	monitoring = true
	body_entered.connect(_on_body_entered)

	_mesh = MeshInstance3D.new()
	var m := BoxMesh.new()
	m.size = Vector3(0.45, 0.45, 0.45)
	_mesh.mesh = m
	var mat := StandardMaterial3D.new()
	mat.albedo_color = Color(0.2, 0.95, 0.55)
	mat.emission_enabled = true
	mat.emission = Color(0.1, 0.7, 0.4)
	mat.emission_energy_multiplier = 0.8
	_mesh.material_override = mat
	add_child(_mesh)

	var col := CollisionShape3D.new()
	var s := SphereShape3D.new()
	s.radius = 0.5
	col.shape = s
	add_child(col)

	_origin_y = global_position.y


func _process(delta: float) -> void:
	rotate_y(delta * 2.5)
	if _mesh:
		_mesh.rotate_x(delta * 1.6)
		_mesh.position.y = sin(Time.get_ticks_msec() * 0.004) * 0.15


func _on_body_entered(body: Node) -> void:
	if body.is_in_group("player"):
		GameManager.add_score(1)
		GameManager.status_message.emit("Gem collected! +1")
		queue_free()
