extends CharacterBody2D
## A wandering customer who wants an item from the supermarket-arm.
## They walk toward the player; once served, they leave satisfied.

signal satisfied(item_type: String)

var player: Node2D
var wanted_item := ""
var speed := 90.0
const DESIRED_DIST := 95.0

func _ready() -> void:
	var possible := ["apple", "bread", "milk", "cola", "chips", "banana"]
	wanted_item = possible[randi() % possible.size()]
	add_to_group("npcs")
	queue_redraw()

func _physics_process(_delta: float) -> void:
	if player == null:
		return
	var to_player := player.position - position
	var d := to_player.length()
	if d > DESIRED_DIST:
		velocity = to_player.normalized() * speed
	else:
		velocity = Vector2.ZERO
	move_and_slide()

func receive_item(item_type: String) -> bool:
	# Customer is lenient — accepts any item you hand them.
	satisfied.emit(item_type)
	queue_free()
	return true

func _draw() -> void:
	# Body
	draw_circle(Vector2.ZERO, 22, Color(0.40, 0.55, 0.85))
	draw_circle(Vector2(-7, -4), 3, Color(0.10, 0.10, 0.10))
	draw_circle(Vector2(7, -4), 3, Color(0.10, 0.10, 0.10))
	# Slight frown (anticipating goods)
	draw_arc(Vector2(0, 12), 6, PI, TAU, 8, Color(0.20, 0.10, 0.10), 1.5)
	# Thought bubble (small -> large) showing what they want
	draw_circle(Vector2(10, -22), 3, Color(1, 1, 1, 0.95))
	draw_circle(Vector2(16, -28), 5, Color(1, 1, 1, 0.95))
	draw_circle(Vector2(24, -36), 16, Color(1, 1, 1, 0.95))
	draw_arc(Vector2(24, -36), 16, 0, TAU, 24, Color(0.20, 0.20, 0.20), 1.0)
	_draw_item_icon(Vector2(24, -36), wanted_item)

func _draw_item_icon(pos: Vector2, item_type: String) -> void:
	match item_type:
		"apple":
			draw_circle(pos, 8, Color(0.85, 0.15, 0.15))
		"bread":
			draw_rect(Rect2(pos - Vector2(9, 4), Vector2(18, 8)), Color(0.75, 0.55, 0.30))
		"milk":
			draw_rect(Rect2(pos - Vector2(5, 9), Vector2(10, 18)), Color(0.95, 0.95, 0.95))
		"cola":
			draw_rect(Rect2(pos - Vector2(6, 9), Vector2(12, 18)), Color(0.20, 0.10, 0.10))
		"chips":
			draw_rect(Rect2(pos - Vector2(7, 9), Vector2(14, 18)), Color(0.90, 0.85, 0.30))
		"banana":
			draw_arc(pos, 8, 0.2, PI - 0.2, 10, Color(1.0, 0.85, 0.2), 3.0)
