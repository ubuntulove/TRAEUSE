extends Node2D
class_name Player

# The protagonist: a single body with TWO independently-controlled arms.
# Player 1 (left arm, blue wristband): WASD move hand, Q grab, E walk.
# Player 2 (right arm, red wristband): Arrows move hand, M grab, . walk.
# The body only walks when BOTH players hold their walk key — that is the
# whole point: two minds must agree to move one body.

const GROUND_Y := 600.0
const LEFT_SHOULDER := Vector2(-30, -150)
const RIGHT_SHOULDER := Vector2(30, -150)
const ARM_REACH := 340.0
const HAND_SPEED := 520.0
const WALK_SPEED := 135.0
const GRAB_RADIUS := 58.0

const BAND_LEFT := Color(0.25, 0.45, 0.95)   # blue = Player 1
const BAND_RIGHT := Color(0.95, 0.3, 0.3)    # red  = Player 2
const SKIN := Color(0.96, 0.82, 0.7)
const SKIN_CLENCH := Color(0.92, 0.7, 0.45)

# Body
var body_pos := Vector2(300.0, GROUND_Y)

# Left arm (P1)
var left_offset := Vector2(-110, 110)
var left_grab: Node2D = null
# Right arm (P2)
var right_offset := Vector2(110, 110)
var right_grab: Node2D = null

# Edge detection for grab keys (so holding doesn't repeat-toggle)
var _prev_left_grab := false
var _prev_right_grab := false


func get_left_shoulder() -> Vector2:
	return body_pos + LEFT_SHOULDER


func get_right_shoulder() -> Vector2:
	return body_pos + RIGHT_SHOULDER


func get_left_hand() -> Vector2:
	return get_left_shoulder() + left_offset


func get_right_hand() -> Vector2:
	return get_right_shoulder() + right_offset


func update_player(dt: float, grabbables: Array, min_x: float, max_x: float) -> void:
	# --- P1: left hand aim ---
	var li := Vector2.ZERO
	if Input.is_key_pressed(KEY_A): li.x -= 1.0
	if Input.is_key_pressed(KEY_D): li.x += 1.0
	if Input.is_key_pressed(KEY_W): li.y -= 1.0
	if Input.is_key_pressed(KEY_S): li.y += 1.0
	if li != Vector2.ZERO:
		left_offset += li.normalized() * HAND_SPEED * dt
	if left_offset.length() > ARM_REACH:
		left_offset = left_offset.normalized() * ARM_REACH

	# --- P2: right hand aim ---
	var ri := Vector2.ZERO
	if Input.is_key_pressed(KEY_LEFT): ri.x -= 1.0
	if Input.is_key_pressed(KEY_RIGHT): ri.x += 1.0
	if Input.is_key_pressed(KEY_UP): ri.y -= 1.0
	if Input.is_key_pressed(KEY_DOWN): ri.y += 1.0
	if ri != Vector2.ZERO:
		right_offset += ri.normalized() * HAND_SPEED * dt
	if right_offset.length() > ARM_REACH:
		right_offset = right_offset.normalized() * ARM_REACH

	# --- Grab toggles (edge detected) ---
	var lg = Input.is_key_pressed(KEY_Q)
	if lg and not _prev_left_grab:
		if left_grab:
			left_grab = null
		else:
			left_grab = _find_nearest(get_left_hand(), grabbables)
	_prev_left_grab = lg

	var rg = Input.is_key_pressed(KEY_M)
	if rg and not _prev_right_grab:
		if right_grab:
			right_grab = null
		else:
			right_grab = _find_nearest(get_right_hand(), grabbables)
	_prev_right_grab = rg

	# --- Walk: only when BOTH players hold their walk key ---
	var p1_walk = Input.is_key_pressed(KEY_E)
	var p2_walk = Input.is_key_pressed(KEY_PERIOD)
	if p1_walk and p2_walk:
		body_pos.x += WALK_SPEED * dt
	# (if only one holds walk, the body refuses to budge — cooperation required)
	body_pos.x = clamp(body_pos.x, min_x, max_x)
	body_pos.y = GROUND_Y
	queue_redraw()


func _find_nearest(hand: Vector2, grabbables: Array) -> Node2D:
	var best: Node2D = null
	var best_d := GRAB_RADIUS
	for g in grabbables:
		if g == null:
			continue
		var d = hand.distance_to(g.position)
		if d < best_d:
			best_d = d
			best = g
	return best


func _draw() -> void:
	# Legs
	draw_line(body_pos + Vector2(-10, -34), body_pos, Color(0.18, 0.18, 0.26), 14)
	draw_line(body_pos + Vector2(10, -34), body_pos, Color(0.18, 0.18, 0.26), 14)
	# Torso
	var torso_top = body_pos + Vector2(0, -150)
	draw_rect(Rect2(torso_top + Vector2(-24, -2), Vector2(48, 118)), Color(0.22, 0.34, 0.82), true)
	draw_rect(Rect2(torso_top + Vector2(-24, -2), Vector2(48, 118)), Color(0.12, 0.2, 0.55), false, 3)
	# Head
	draw_circle(torso_top + Vector2(0, -22), 21, Color(0.95, 0.8, 0.7))
	draw_circle(torso_top + Vector2(-7, -25), 3.5, Color(0.1, 0.1, 0.12))
	draw_circle(torso_top + Vector2(7, -25), 3.5, Color(0.1, 0.1, 0.12))
	# worried mouth (two players steering him is stressful)
	draw_arc(torso_top + Vector2(0, -14), 6, PI, TAU, 8, Color(0.1, 0.1, 0.12), 2)

	# Arms
	_draw_arm(get_left_shoulder(), get_left_hand(), left_grab != null, BAND_LEFT)
	_draw_arm(get_right_shoulder(), get_right_hand(), right_grab != null, BAND_RIGHT)


func _draw_arm(shoulder: Vector2, hand: Vector2, grabbing: bool, band: Color) -> void:
	# Two-segment limb with a soft downward bend at the elbow.
	var elbow = (shoulder + hand) * 0.5 + Vector2(0, 22)
	draw_line(shoulder, elbow, SKIN, 15)
	draw_line(elbow, hand, SKIN, 15)
	# joints
	draw_circle(shoulder, 8, Color(0.12, 0.2, 0.55))
	draw_circle(elbow, 7, SKIN)
	# wristband = which player owns this arm
	draw_circle(hand, 14, band)
	# hand
	var hand_col = SKIN_CLENCH if grabbing else SKIN
	draw_circle(hand, 10, hand_col)
	draw_arc(hand, 10, 0, TAU, 24, Color(0.2, 0.15, 0.1), 2)
