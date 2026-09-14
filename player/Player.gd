extends CharacterBody3D
## Player character. Handles movement, jumping, shooting and collecting.
## The camera is a child hierarchy (CamYaw -> CamPivot -> Camera) that is
## repositioned for each perspective: First-person, Third-person, Top-down.
## Mouse-look is enabled for First/Third person; in Top-down the body faces
## its movement direction and the camera looks straight down.

const SPEED := 6.5
const SPRINT := 9.5
const JUMP_VELOCITY := 8.0
const MOUSE_SENS := 0.0022
const PITCH_LIMIT := 1.3
const SHOOT_COOLDOWN := 0.18

var CamYaw: Node3D
var CamPivot: Node3D
var Camera: Camera3D
var VisualYaw: Node3D
var Muzzle: Marker3D

var _yaw_angle: float = 0.0
var _pitch_angle: float = 0.0
var _shoot_cd: float = 0.0
var _look_enabled: bool = true
var _facing: float = 0.0


func _ready() -> void:
	add_to_group("player")
	GameManager.player = self

	# --- Visual body (capsule + simple head) ---
	VisualYaw = Node3D.new()
	VisualYaw.name = "VisualYaw"
	add_child(VisualYaw)

	var mesh := MeshInstance3D.new()
	var capsule := CapsuleMesh.new()
	capsule.radius = 0.35
	capsule.height = 1.6
	mesh.mesh = capsule
	var mat := StandardMaterial3D.new()
	mat.albedo_color = Color(0.25, 0.55, 0.95)
	mat.roughness = 0.6
	mesh.material_override = mat
	mesh.position = Vector3(0, 0.8, 0)
	VisualYaw.add_child(mesh)

	# Small "nose" to indicate facing.
	var nose := MeshInstance3D.new()
	var box := BoxMesh.new()
	box.size = Vector3(0.12, 0.12, 0.35)
	nose.mesh = box
	var nmat := StandardMaterial3D.new()
	nmat.albedo_color = Color(0.95, 0.85, 0.2)
	nose.material_override = nmat
	nose.position = Vector3(0, 0.9, -0.42)
	VisualYaw.add_child(nose)

	# --- Physics collision ---
	var col := CollisionShape3D.new()
	var shape := CapsuleShape3D.new()
	shape.radius = 0.35
	shape.height = 1.6
	col.shape = shape
	col.position = Vector3(0, 0.8, 0)
	add_child(col)

	# --- Camera hierarchy ---
	CamYaw = Node3D.new()
	CamYaw.name = "CamYaw"
	CamYaw.position = Vector3(0, 0.62, 0) # eye height: pitch orbits here, not the feet
	add_child(CamYaw)

	CamPivot = Node3D.new()
	CamPivot.name = "CamPivot"
	CamYaw.add_child(CamPivot)

	Camera = Camera3D.new()
	Camera.name = "Camera3D"
	Camera.fov = 70.0
	Camera.far = 200.0
	Camera.current = true
	CamPivot.add_child(Camera)

	# Muzzle used for First/Third-person shooting (rides the camera).
	Muzzle = Marker3D.new()
	Muzzle.name = "Muzzle"
	Muzzle.position = Vector3(0, 0, -0.4)
	CamPivot.add_child(Muzzle)

	GameManager.perspective_changed.connect(_on_perspective_changed)
	GameManager.game_type_changed.connect(_on_game_type_changed)
	_on_perspective_changed(GameManager.perspective)
	_on_game_type_changed(GameManager.game_type)


func _on_perspective_changed(p: int) -> void:
	match p:
		GameManager.Perspective.FIRST_PERSON:
			Camera.position = Vector3(0, 0.0, 0.0)
			Camera.fov = 75.0
			VisualYaw.visible = false
			_set_mouse_captured(true)
		GameManager.Perspective.THIRD_PERSON:
			Camera.position = Vector3(0, 0.9, 4.8)
			Camera.fov = 70.0
			VisualYaw.visible = true
			_set_mouse_captured(true)
		GameManager.Perspective.TOP_DOWN:
			CamYaw.rotation.y = 0.0
			CamPivot.rotation.x = -PI / 2.0
			Camera.position = Vector3(0, 0.0, 22.0)
			Camera.fov = 60.0
			VisualYaw.visible = true
			_set_mouse_captured(false)
	_apply_camera_angles()


func _on_game_type_changed(_t: int) -> void:
	# Reset transient combat state on mode switch.
	_shoot_cd = 0.0


func _set_mouse_captured(captured: bool) -> void:
	_look_enabled = captured
	Input.mouse_mode = Input.MOUSE_MODE_CAPTURED if captured else Input.MOUSE_MODE_VISIBLE


func _apply_camera_angles() -> void:
	if GameManager.perspective == GameManager.Perspective.TOP_DOWN:
		return
	CamYaw.rotation.y = _yaw_angle
	CamPivot.rotation.x = _pitch_angle


func _unhandled_input(event: InputEvent) -> void:
	# Hotkeys to switch game type / perspective.
	if Input.is_action_just_pressed("switch_type"):
		GameManager.cycle_game_type()
		return
	if Input.is_action_just_pressed("switch_view"):
		GameManager.cycle_perspective()
		return

	# Toggle cursor with Esc (First/Third person only).
	if Input.is_action_just_pressed("toggle_mouse") and GameManager.perspective != GameManager.Perspective.TOP_DOWN:
		_set_mouse_captured(not _look_enabled)
		return

	# Click in the world to recapture after using the HUD.
	if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and event.pressed:
		if GameManager.perspective != GameManager.Perspective.TOP_DOWN and not _look_enabled:
			_set_mouse_captured(true)
			get_viewport().set_input_as_handled()
			return

	# Mouse look (First/Third person).
	if _look_enabled and event is InputEventMouseMotion:
		_yaw_angle -= event.relative.x * MOUSE_SENS
		_pitch_angle -= event.relative.y * MOUSE_SENS
		_pitch_angle = clamp(_pitch_angle, -PITCH_LIMIT, PITCH_LIMIT)
		_apply_camera_angles()

	# Shoot (Shooter mode).
	if Input.is_action_just_pressed("shoot") and GameManager.game_type == GameManager.GameType.SHOOTER:
		_try_shoot()


func _physics_process(delta: float) -> void:
	_shoot_cd = max(0.0, _shoot_cd - delta)

	# Gravity & jump.
	if not is_on_floor():
		velocity.y -= 24.0 * delta
	else:
		velocity.y = -1.0
	if Input.is_action_just_pressed("jump") and is_on_floor():
		velocity.y = JUMP_VELOCITY

	# Movement relative to the camera yaw (world-aligned in Top-down).
	var input_vec := Input.get_vector("move_left", "move_right", "move_forward", "move_back")
	var forward := -CamYaw.global_basis.z
	forward.y = 0.0
	forward = forward.normalized()
	var right := CamYaw.global_basis.x
	right.y = 0.0
	right = right.normalized()
	var move_dir := right * input_vec.x + forward * -input_vec.y

	var speed := SPRINT if Input.is_physical_key_pressed(KEY_SHIFT) else SPEED
	var target := move_dir * speed
	velocity.x = target.x
	velocity.z = target.z

	move_and_slide()

	# Facing.
	match GameManager.perspective:
		GameManager.Perspective.FIRST_PERSON, GameManager.Perspective.THIRD_PERSON:
			VisualYaw.rotation.y = _yaw_angle
		GameManager.Perspective.TOP_DOWN:
			if move_dir.length_squared() > 0.01:
				_facing = atan2(-move_dir.x, -move_dir.z)
				VisualYaw.rotation.y = lerp_angle(VisualYaw.rotation.y, _facing, 12.0 * delta)

	# Respawn if the player falls out of the arena.
	if global_position.y < -8.0:
		global_position = Vector3(0, 1.5, 0)
		velocity = Vector3.ZERO


func _try_shoot() -> void:
	if _shoot_cd > 0.0:
		return
	_shoot_cd = SHOOT_COOLDOWN
	var proj_scene: PackedScene = load("res://entities/Projectile.tscn")
	var proj: Projectile = proj_scene.instantiate() as Projectile
	GameManager.entity_container.add_child(proj)

	var origin: Vector3
	var dir: Vector3
	if GameManager.perspective == GameManager.Perspective.TOP_DOWN:
		origin = global_position + Vector3(0, 0.9, 0)
		dir = -VisualYaw.global_basis.z
		dir.y = 0.0
		dir = dir.normalized()
	else:
		origin = Muzzle.global_position
		dir = -Camera.global_basis.z
	proj.setup(origin, dir)


func take_hit() -> void:
	GameManager.status_message.emit("You took a hit! -5")
	GameManager.add_score(-5)
	# Small hop so the hit is felt.
	if is_on_floor():
		velocity.y = 4.0
