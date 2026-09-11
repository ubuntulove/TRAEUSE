extends CharacterBody2D
## The character whose LEFT ARM is a SUPERMARKET.
## Walk around with WASD/Arrows. Pluck items off your own
## arm-shelves and hand them to the nearest customer with SPACE.

const SPEED := 220.0
const ARM_OFFSET := Vector2(-95, -8)   # where the supermarket sits relative to body
const SM_SIZE := Vector2(82, 104)
const SHELF_SIZE := 4
const RESTOCK_INTERVAL := 4.0
const GIVE_RANGE := 150.0

var shelf_items: Array[String] = ["apple", "bread", "milk", "cola"]
var restock_timer := 0.0

func _physics_process(delta: float) -> void:
	var dir := Vector2.ZERO
	# Arrows
	dir.x = Input.get_axis("ui_left", "ui_right")
	dir.y = Input.get_axis("ui_up", "ui_down")
	# WASD
	if Input.is_key_pressed(KEY_A): dir.x -= 1.0
	if Input.is_key_pressed(KEY_D): dir.x += 1.0
	if Input.is_key_pressed(KEY_W): dir.y -= 1.0
	if Input.is_key_pressed(KEY_S): dir.y += 1.0
	velocity = dir.normalized() * SPEED
	move_and_slide()

	# Keep player on-screen, leaving room on the left for the supermarket arm
	var s := get_viewport_rect().size
	position.x = clamp(position.x, 160.0, s.x - 50.0)
	position.y = clamp(position.y, 50.0, s.y - 50.0)

	restock_timer += delta
	if restock_timer >= RESTOCK_INTERVAL:
		restock_timer = 0.0
		_restock()
	queue_redraw()

func _restock() -> void:
	var possible := ["apple", "bread", "milk", "cola", "chips", "banana"]
	for i in range(shelf_items.size()):
		if shelf_items[i] == "":
			shelf_items[i] = possible[randi() % possible.size()]

func grab_item() -> String:
	for i in range(shelf_items.size()):
		if shelf_items[i] != "":
			var t := shelf_items[i]
			shelf_items[i] = ""
			return t
	return ""

func _return_item(item: String) -> void:
	for i in range(shelf_items.size()):
		if shelf_items[i] == "":
			shelf_items[i] = item
			return

func try_give_item() -> void:
	# Pluck the first available item off your own supermarket-arm
	var item := grab_item()
	if item == "":
		return
	# Find nearest customer in give range
	var nearest = null
	var nearest_d := GIVE_RANGE
	for npc in get_tree().get_nodes_in_group("npcs"):
		var d := (npc.position - position).length()
		if d < nearest_d:
			nearest_d = d
			nearest = npc
	if nearest != null:
		nearest.receive_item(item)
	else:
		_return_item(item)  # nobody nearby; put it back on the shelf

func _draw() -> void:
	# --- Body (head-ish blob) ---
	draw_circle(Vector2.ZERO, 30, Color(0.85, 0.70, 0.55))
	draw_circle(Vector2(-10, -8), 4, Color(0.1, 0.1, 0.1))
	draw_circle(Vector2(10, -8), 4, Color(0.1, 0.1, 0.1))
	draw_arc(Vector2(0, 8), 8, 0.0, PI, 12, Color(0.2, 0.1, 0.1), 2.0)

	# --- Right arm (a normal arm) ---
	draw_line(Vector2(20, 0), Vector2(58, 26), Color(0.85, 0.70, 0.55), 8)
	draw_circle(Vector2(58, 26), 6, Color(0.85, 0.70, 0.55))

	# --- LEFT ARM = SUPERMARKET ---
	# Connecting "arm" from shoulder to supermarket
	draw_line(Vector2(-20, -5), ARM_OFFSET + Vector2(SM_SIZE.x / 2.0 - 5.0, 0.0), Color(0.85, 0.70, 0.55), 8)

	# Roof (triangle)
	var roof_pts := PackedVector2Array([
		ARM_OFFSET + Vector2(-SM_SIZE.x / 2.0 - 10.0, -SM_SIZE.y / 2.0),
		ARM_OFFSET + Vector2(0.0, -SM_SIZE.y / 2.0 - 28.0),
		ARM_OFFSET + Vector2(SM_SIZE.x / 2.0 + 10.0, -SM_SIZE.y / 2.0),
	])
	draw_colored_polygon(roof_pts, Color(0.60, 0.30, 0.25))

	# Building wall + outline
	var sm_rect := Rect2(ARM_OFFSET - SM_SIZE / 2.0, SM_SIZE)
	draw_rect(sm_rect, Color(0.95, 0.92, 0.85))
	draw_rect(sm_rect, Color(0.30, 0.25, 0.20), false, 2.0)

	# Sign strip (green bar; the word SHOP is implied by the colors of the awning)
	var sign_rect := Rect2(ARM_OFFSET.x - SM_SIZE.x / 2.0, ARM_OFFSET.y - SM_SIZE.y / 2.0 + 4.0, SM_SIZE.x, 14.0)
	draw_rect(sign_rect, Color(0.20, 0.50, 0.20))
	# Awning stripes
	var awning_y := sign_rect.position.y + 18.0
	for i in range(6):
		var c := Color(0.9, 0.85, 0.3) if i % 2 == 0 else Color(0.6, 0.3, 0.25)
		var stripe := Rect2(ARM_OFFSET.x - SM_SIZE.x / 2.0 + i * (SM_SIZE.x / 6.0), awning_y, SM_SIZE.x / 6.0, 8.0)
		draw_rect(stripe, c)

	# Shelves (2x2 grid)
	var shelf_origin := ARM_OFFSET + Vector2(-SM_SIZE.x / 2.0 + 10.0, -SM_SIZE.y / 2.0 + 36.0)
	var slot_size := Vector2(28, 28)
	for i in range(SHELF_SIZE):
		var col := i % 2
		var row := i / 2
		var slot_pos := shelf_origin + Vector2(float(col) * (slot_size.x + 4.0), float(row) * (slot_size.y + 4.0))
		draw_rect(Rect2(slot_pos, slot_size), Color(0.70, 0.65, 0.55))
		draw_rect(Rect2(slot_pos, slot_size), Color(0.30, 0.25, 0.20), false, 1.0)
		if shelf_items[i] != "":
			_draw_item_icon(slot_pos + slot_size / 2.0, shelf_items[i])

func _draw_item_icon(pos: Vector2, item_type: String) -> void:
	match item_type:
		"apple":
			draw_circle(pos, 9, Color(0.85, 0.15, 0.15))
			draw_circle(pos - Vector2(3, 3), 3, Color(1.0, 0.6, 0.6))
		"bread":
			draw_rect(Rect2(pos - Vector2(10, 5), Vector2(20, 10)), Color(0.75, 0.55, 0.30))
		"milk":
			draw_rect(Rect2(pos - Vector2(6, 10), Vector2(12, 20)), Color(0.95, 0.95, 0.95))
			draw_rect(Rect2(pos - Vector2(6, 10), Vector2(12, 20)), Color(0.30, 0.30, 0.30), false, 1.0)
		"cola":
			draw_rect(Rect2(pos - Vector2(7, 10), Vector2(14, 20)), Color(0.20, 0.10, 0.10))
		"chips":
			draw_rect(Rect2(pos - Vector2(8, 10), Vector2(16, 20)), Color(0.90, 0.85, 0.30))
		"banana":
			draw_arc(pos, 9, 0.2, PI - 0.2, 10, Color(1.0, 0.85, 0.2), 4.0)
