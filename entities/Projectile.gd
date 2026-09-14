class_name Projectile
extends Area3D
## Projectile fired by the player in Shooter mode. Travels straight; damages the
## first enemy it overlaps, then disappears. Also despawns after a lifetime.

const SPEED := 30.0
const LIFETIME := 2.2

var _dir := Vector3.FORWARD
var _life: float = LIFETIME


func _ready() -> void:
	add_to_group("projectile")
	body_entered.connect(_on_body_entered)

	var mesh := MeshInstance3D.new()
	var m := SphereMesh.new()
	m.radius = 0.12
	m.height = 0.4
	mesh.mesh = m
	var mat := StandardMaterial3D.new()
	mat.albedo_color = Color(1.0, 0.85, 0.2)
	mat.emission_enabled = true
	mat.emission = Color(1.0, 0.6, 0.1)
	mat.emission_energy_multiplier = 1.2
	mesh.material_override = mat
	add_child(mesh)

	var col := CollisionShape3D.new()
	var s := SphereShape3D.new()
	s.radius = 0.18
	col.shape = s
	add_child(col)


func setup(origin: Vector3, dir: Vector3) -> void:
	global_position = origin
	_dir = dir.normalized()
	look_at(global_position + _dir)


func _process(delta: float) -> void:
	global_position += _dir * SPEED * delta
	_life -= delta
	if _life <= 0.0:
		queue_free()


func _on_body_entered(body: Node) -> void:
	if body.is_in_group("enemy"):
		body.take_damage(1)
		queue_free()
