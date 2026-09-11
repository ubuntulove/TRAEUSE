extends CharacterBody2D
## Top-down mover for the Repair Realm.

const SPEED := 230.0


func _physics_process(_delta: float) -> void:
	var v := Vector2.ZERO
	v.x = Input.get_axis("move_left", "move_right")
	v.y = Input.get_axis("move_up", "move_down")
	velocity = v.normalized() * SPEED
	move_and_slide()
