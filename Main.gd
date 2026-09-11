extends Node2D

# =======================================================================
#  TWIN ARMS
#  A co-op game where two players each control ONE ARM of the protagonist.
#  Inspired by @petermolydeux:
#  "What if there were a co-op game where both you and your friend control
#   one arm each of the game's protagonist?"
#
#  The body only walks when BOTH players hold their walk key. The heavy
#  crate can be dragged along the ground by a single hand, but needs BOTH
#  hands to be lifted. Get it onto the pressure plate to escape.
# =======================================================================

const GROUND_Y := 600.0
const ROOM_W := 1280.0
const ROOM_H := 720.0
const PEDESTAL_X := 1230.0
const PEDESTAL_TOP_Y := 420.0
const PEDESTAL_W := 100.0
const PLATE_Y := 420.0
const BOX_HALF := 37.0
const BODY_MIN_X := 60.0
const BODY_MAX_X := 1130.0
const GRAVITY := 900.0

var player: Player
var box: Box
var won := false
var win_t := 0.0
var instr_label: Label
var win_label: Label
var hint_label: Label


func _ready() -> void:
	player = preload("res://Player.gd").new()
	player.body_pos = Vector2(300.0, GROUND_Y)
	add_child(player)

	box = preload("res://Box.gd").new()
	box.position = Vector2(150.0, GROUND_Y - BOX_HALF)
	add_child(box)

	_setup_ui()


func _setup_ui() -> void:
	var layer = CanvasLayer.new()
	add_child(layer)

	instr_label = Label.new()
	instr_label.position = Vector2(20, 14)
	instr_label.size = Vector2(1240, 130)
	instr_label.text = (
		"TWIN ARMS — you and a friend each control ONE ARM of the protagonist.\n\n" +
		"P1 LEFT ARM  (blue):  WASD = aim hand   Q = grab/release   hold E = walk\n" +
		"P2 RIGHT ARM (red):   Arrows = aim hand   M = grab/release   hold . = walk\n" +
		"The body only moves when BOTH hold walk. The crate is heavy: one hand drags it, TWO hands lift it. Get it onto the plate!   (R = restart)"
	)
	instr_label.add_theme_color_override("font_color", Color(1, 1, 1))
	instr_label.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	instr_label.add_theme_constant_override("outline_size", 5)
	layer.add_child(instr_label)

	hint_label = Label.new()
	hint_label.position = Vector2(0, 660)
	hint_label.size = Vector2(1280, 40)
	hint_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	hint_label.add_theme_color_override("font_color", Color(1, 0.95, 0.6))
	hint_label.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	hint_label.add_theme_constant_override("outline_size", 5)
	layer.add_child(hint_label)

	win_label = Label.new()
	win_label.position = Vector2(0, 270)
	win_label.size = Vector2(1280, 140)
	win_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	win_label.text = "ESCAPED!"
	win_label.add_theme_font_size_override("font_size", 96)
	win_label.add_theme_color_override("font_color", Color(1, 0.88, 0.2))
	win_label.add_theme_color_override("font_outline_color", Color(0, 0, 0))
	win_label.add_theme_constant_override("outline_size", 10)
	win_label.visible = false
	layer.add_child(win_label)


func _process(dt: float) -> void:
	if Input.is_key_pressed(KEY_R):
		get_tree().reload_current_scene()
		return

	player.update_player(dt, [box], BODY_MIN_X, BODY_MAX_X)

	# --- Resolve crate position from whichever hands hold it ---
	var lh = player.left_grab == box
	var rh = player.right_grab == box
	if lh and rh:
		# Two hands: full cooperative control — crate follows the midpoint.
		box.position = (player.get_left_hand() + player.get_right_hand()) * 0.5
	elif lh:
		# One hand can only drag the crate along the ground.
		box.position.x = player.get_left_hand().x
		box.position.y = GROUND_Y - BOX_HALF
	elif rh:
		box.position.x = player.get_right_hand().x
		box.position.y = GROUND_Y - BOX_HALF
	else:
		# Released: apply gravity, rest on ground or on the pedestal top.
		box.position.y += GRAVITY * dt
		var rest_y = GROUND_Y - BOX_HALF
		if box.position.x > PEDESTAL_X - PEDESTAL_W * 0.5 and box.position.x < PEDESTAL_X + PEDESTAL_W * 0.5:
			rest_y = PEDESTAL_TOP_Y - BOX_HALF
		if box.position.y > rest_y:
			box.position.y = rest_y

	box.position.x = clamp(box.position.x, 40.0, ROOM_W - 40.0)

	# --- Win check: crate resting on the plate ---
	var on_plate = abs(box.position.x - PEDESTAL_X) < 55.0 and box.position.y < PLATE_Y + 12.0
	if on_plate and not won:
		won = true
		win_label.visible = true

	if won:
		win_t += dt
		hint_label.text = "Press R to play again."
	else:
		_update_hint(lh, rh)

	queue_redraw()


func _update_hint(lh: bool, rh: bool) -> void:
	if not lh and not rh:
		hint_label.text = "Aim a hand onto the crate and grab it  (Q = left, M = right)."
	elif not (lh and rh):
		hint_label.text = "One hand drags the crate along the ground. Walk together (hold E AND .) to haul it over — then grab with the OTHER hand and LIFT onto the plate!"
	else:
		hint_label.text = "Both hands hold the crate! Aim both hands up onto the plate to lift it home."


func _draw() -> void:
	# sky
	draw_rect(Rect2(0, 0, ROOM_W, ROOM_H), Color(0.55, 0.72, 0.92), true)
	# back wall
	draw_rect(Rect2(0, 0, ROOM_W, GROUND_Y), Color(0.7, 0.83, 0.95), true)
	# ground
	draw_rect(Rect2(0, GROUND_Y, ROOM_W, ROOM_H - GROUND_Y), Color(0.27, 0.47, 0.24), true)
	draw_line(Vector2(0, GROUND_Y), Vector2(ROOM_W, GROUND_Y), Color(0.18, 0.36, 0.17), 4)
	# side walls
	draw_rect(Rect2(0, 0, 30, GROUND_Y), Color(0.2, 0.2, 0.25), true)
	draw_rect(Rect2(ROOM_W - 30, 0, 30, GROUND_Y), Color(0.2, 0.2, 0.25), true)

	# pedestal
	draw_rect(Rect2(PEDESTAL_X - PEDESTAL_W * 0.5, PEDESTAL_TOP_Y, PEDESTAL_W, GROUND_Y - PEDESTAL_TOP_Y), Color(0.55, 0.55, 0.6), true)
	draw_rect(Rect2(PEDESTAL_X - PEDESTAL_W * 0.5, PEDESTAL_TOP_Y, PEDESTAL_W, 12), Color(0.42, 0.42, 0.47), true)
	# plate
	var plate_col = Color(0.85, 0.25, 0.25) if not won else Color(0.25, 0.85, 0.3)
	draw_rect(Rect2(PEDESTAL_X - 46, PEDESTAL_TOP_Y - 10, 92, 12), plate_col, true)
	draw_rect(Rect2(PEDESTAL_X - 46, PEDESTAL_TOP_Y - 10, 92, 12), Color(0.2, 0.1, 0.1), false, 2)
	if not won:
		# plate target glow
		draw_arc(Vector2(PEDESTAL_X, PEDESTAL_TOP_Y - 4), 30, 0, TAU, 28, Color(1, 0.6, 0.2, 0.5), 2)

	# victory gate on the right wall
	var gate_x = ROOM_W - 30
	var gate_col = Color(0.3, 0.2, 0.1) if not won else Color(0.2, 0.7, 0.3)
	draw_rect(Rect2(gate_x - 54, GROUND_Y - 180, 54, 180), gate_col, true)
	draw_rect(Rect2(gate_x - 54, GROUND_Y - 180, 54, 180), Color(0.12, 0.08, 0.04), false, 3)
	if won:
		# glowing opening
		var glow = 0.4 + 0.3 * sin(win_t * 4.0)
		draw_rect(Rect2(gate_x - 54, GROUND_Y - 180, 54, 180), Color(1, 0.85, 0.3, glow), true)

	# grab-range highlight on the crate when a hand is near
	if not won:
		var ln = player.get_left_hand().distance_to(box.position) < Player.GRAB_RADIUS
		var rn = player.get_right_hand().distance_to(box.position) < Player.GRAB_RADIUS
		if ln or rn:
			draw_arc(box.position, 50, 0, TAU, 32, Color(1, 1, 0.35, 0.85), 2)
