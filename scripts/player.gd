# player.gd - First-person controller with block break/place and fly mode.
extends CharacterBody3D

const WALK_SPEED := 6.0
const SPRINT_SPEED := 10.0
const FLY_SPEED := 14.0
const JUMP_VELOCITY := 7.0
const SENSITIVITY := 0.0025
const EYE_HEIGHT := 1.6
const REACH := 6.0

var gravity: float = ProjectSettings.get_setting("physics/3d/default_gravity")

var camera: Camera3D
var mouse_captured: bool = true
var fly_mode: bool = false

# Palette selected by number keys 1..7 (air and water excluded).
var palette: Array = [
	Blocks.ID.GRASS,
	Blocks.ID.DIRT,
	Blocks.ID.STONE,
	Blocks.ID.SAND,
	Blocks.ID.WOOD,
	Blocks.ID.LEAVES,
	Blocks.ID.BEDROCK,
]
var selected_block: int = Blocks.ID.GRASS


func _ready() -> void:
	var col := CollisionShape3D.new()
	var shape := CapsuleShape3D.new()
	shape.radius = 0.35
	shape.height = 1.7
	col.shape = shape
	add_child(col)

	camera = Camera3D.new()
	camera.position = Vector3(0, EYE_HEIGHT, 0)
	add_child(camera)
	camera.current = true

	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)


func _input(event: InputEvent) -> void:
	if event.is_action_pressed("toggle_mouse") and not event.is_echo():
		mouse_captured = not mouse_captured
		Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED if mouse_captured else Input.MOUSE_MODE_VISIBLE)

	if event.is_action_pressed("toggle_fly") and not event.is_echo():
		fly_mode = not fly_mode
		velocity = Vector3.ZERO

	if event is InputEventMouseMotion and mouse_captured:
		var m := event as InputEventMouseMotion
		rotate_y(-m.relative.x * SENSITIVITY)
		camera.rotate_x(-m.relative.y * SENSITIVITY)
		camera.rotation.x = clamp(camera.rotation.x, deg_to_rad(-85), deg_to_rad(85))

	if event is InputEventMouseButton and event.pressed and mouse_captured:
		var mb := event as InputEventMouseButton
		if mb.button_index == MOUSE_BUTTON_LEFT:
			_break_block()
		elif mb.button_index == MOUSE_BUTTON_RIGHT:
			_place_block()

	if event is InputEventKey and event.pressed and not event.is_echo():
		var k = event.keycode
		if k >= KEY_1 and k <= KEY_7:
			var i = k - KEY_1
			if i < palette.size():
				selected_block = palette[i]


func _physics_process(delta: float) -> void:
	var forward := -global_transform.basis.z
	forward.y = 0.0
	forward = forward.normalized()
	var right := global_transform.basis.x
	right.y = 0.0
	right = right.normalized()

	var dir := Vector3.ZERO
	if Input.is_action_pressed("move_forward"): dir += forward
	if Input.is_action_pressed("move_back"):    dir -= forward
	if Input.is_action_pressed("move_right"):   dir += right
	if Input.is_action_pressed("move_left"):    dir -= right
	dir = dir.normalized()

	if fly_mode:
		var v := dir * FLY_SPEED
		if Input.is_action_pressed("jump"):   v.y += FLY_SPEED
		if Input.is_action_pressed("sprint"): v.y -= FLY_SPEED
		velocity = v
	else:
		var speed := SPRINT_SPEED if Input.is_action_pressed("sprint") else WALK_SPEED
		var horiz := dir * speed
		velocity.x = horiz.x
		velocity.z = horiz.z
		velocity.y -= gravity * delta
		if is_on_floor() and Input.is_action_pressed("jump"):
			velocity.y = JUMP_VELOCITY

	move_and_slide()


func _break_block() -> void:
	var world: Node = get_parent()
	var hit: Dictionary = world.raycast_voxel(camera.global_position, -camera.global_transform.basis.z, REACH)
	if hit.has("pos"):
		world.set_block(hit.pos, Blocks.ID.AIR)


func _place_block() -> void:
	var world: Node = get_parent()
	var hit: Dictionary = world.raycast_voxel(camera.global_position, -camera.global_transform.basis.z, REACH)
	if hit.has("pos"):
		var target: Vector3i = hit.pos + hit.normal
		# Don't place a block inside the player.
		if (Vector3(target) + Vector3(0.5, 0.5, 0.5)).distance_to(camera.global_position) < 1.2:
			return
		if world.get_block(target) == Blocks.ID.AIR:
			world.set_block(target, selected_block)
