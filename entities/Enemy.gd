extends CharacterBody3D
## Simple chasing enemy for Shooter mode. Walks toward the player on the XZ
## plane, deals contact damage, and takes damage from projectiles.

const SPEED := 3.1
const MAX_HEALTH := 3
const CONTACT_RANGE := 1.3
const HIT_COOLDOWN := 0.6

var health: int = MAX_HEALTH
var _flash: float = 0.0
var _hit_cd: float = 0.0
var _mesh: MeshInstance3D
var _base_color := Color(0.85, 0.25, 0.25)


func _ready() -> void:
	add_to_group("enemy")
	_mesh = MeshInstance3D.new()
	var m := CapsuleMesh.new()
	m.radius = 0.4
	m.height = 1.4
	_mesh.mesh = m
	_mesh.material_override = StandardMaterial3D.new()
	(_mesh.material_override as StandardMaterial3D).albedo_color = _base_color
	_mesh.position = Vector3(0, 0.7, 0)
	add_child(_mesh)

	var col := CollisionShape3D.new()
	var s := CapsuleShape3D.new()
	s.radius = 0.4
	s.height = 1.4
	col.shape = s
	col.position = Vector3(0, 0.7, 0)
	add_child(col)


func _physics_process(delta: float) -> void:
	_flash = max(0.0, _flash - delta)
	_hit_cd = max(0.0, _hit_cd - delta)
	if _mesh and _mesh.material_override:
		var mat := _mesh.material_override as StandardMaterial3D
		mat.albedo_color = _base_color.lerp(Color(1.6, 1.6, 1.6), _flash / 0.12)

	var p := GameManager.player
	if p:
		var to := p.global_position - global_position
		to.y = 0.0
		var d := to.normalized()
		velocity.x = d.x * SPEED
		velocity.z = d.z * SPEED
		if is_on_floor():
			velocity.y = 0.0
		else:
			velocity.y -= 24.0 * delta
		move_and_slide()

		# Face the player.
		if d.length_squared() > 0.01:
			rotation.y = atan2(-d.x, -d.z)

		# Contact damage.
		if to.length() < CONTACT_RANGE and _hit_cd <= 0.0:
			if p.has_method("take_hit"):
				p.call("take_hit")
			_hit_cd = HIT_COOLDOWN


func take_damage(amount: int) -> void:
	health -= amount
	_flash = 0.12
	if health <= 0:
		GameManager.add_score(10)
		GameManager.status_message.emit("Enemy down! +10")
		queue_free()
