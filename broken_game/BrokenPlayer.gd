extends CharacterBody2D
## Simple platformer player. Movement is killed the instant the game corrupts.

const SPEED := 260.0
const JUMP_VELOCITY := -540.0
const GRAVITY := 1400.0

var alive := true


func _physics_process(delta: float) -> void:
	if not alive:
		return
	if not is_on_floor():
		velocity.y += GRAVITY * delta
	if Input.is_action_just_pressed("jump") and is_on_floor():
		velocity.y = JUMP_VELOCITY
	var dir := Input.get_axis("move_left", "move_right")
	velocity.x = dir * SPEED
	move_and_slide()
