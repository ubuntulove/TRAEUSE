extends Node2D
## GAME A -- "The Trial".
## A small platformer. If the player touches a spike (or otherwise dies) the
## game writes broken=true to the SHARED state file and freezes itself behind
## a glitchy "FATAL ERROR" screen. The cartridge then refuses to boot normally
## until Game B repairs it.

const LEVEL_WIDTH := 2400.0

var player: CharacterBody2D
var camera: Camera2D
var hud_label: Label
var broken := false


func _ready() -> void:
	broken = GameState.is_broken()
	_build_level()
	_build_player()
	_build_camera()
	_build_hud()
	if broken:
		player.alive = false
		hud_label.text = "GAME A -- CORRUPTED. This cartridge will not boot. (ESC: menu)"
		_show_corrupted()


func _process(_delta: float) -> void:
	if Input.is_action_just_pressed("ui_cancel"):
		get_tree().change_scene_to_file("res://MainMenu.tscn")
		return
	if player and not broken:
		var target := player.global_position + Vector2(0, -60)
		camera.global_position = camera.global_position.lerp(target, 0.12)


# ------------------------------------------------------------------- world build
func _build_level() -> void:
	var bg := Polygon2D.new()
	bg.polygon = PackedVector2Array([
		Vector2(0, 0), Vector2(LEVEL_WIDTH, 0),
		Vector2(LEVEL_WIDTH, 540), Vector2(0, 540)])
	bg.color = Color(0.05, 0.06, 0.10)
	add_child(bg)

	# ground: two segments with a spike pit between them
	_add_platform(0.0, 420.0, 900.0, 120.0, Color(0.20, 0.22, 0.28))
	_add_platform(1100.0, 420.0, LEVEL_WIDTH - 1100.0, 120.0, Color(0.20, 0.22, 0.28))
	# stepping platform over the pit
	_add_platform(940.0, 340.0, 120.0, 20.0, Color(0.20, 0.22, 0.28))
	# decorative ledge
	_add_platform(1500.0, 320.0, 140.0, 20.0, Color(0.20, 0.22, 0.28))
	# hazards
	_add_spikes(900.0, 500.0, 200.0, 40.0)   # the pit
	_add_spikes(1700.0, 380.0, 60.0, 40.0)  # obstacle on the ground
	# goal flag at the end
	_add_goal(2280.0, 340.0, 40.0, 80.0)


func _add_platform(x: float, y: float, w: float, h: float, color: Color) -> void:
	var body := StaticBody2D.new()
	body.position = Vector2(x, y)
	add_child(body)
	var col := CollisionShape2D.new()
	var shape := RectangleShape2D.new()
	shape.size = Vector2(w, h)
	col.shape = shape
	col.position = Vector2(w / 2.0, h / 2.0)
	body.add_child(col)
	var poly := Polygon2D.new()
	poly.polygon = PackedVector2Array([
		Vector2(0, 0), Vector2(w, 0), Vector2(w, h), Vector2(0, h)])
	poly.color = color
	body.add_child(poly)


func _add_spikes(x: float, y: float, w: float, h: float) -> void:
	var area := Area2D.new()
	area.position = Vector2(x, y)
	add_child(area)
	var col := CollisionShape2D.new()
	var shape := RectangleShape2D.new()
	shape.size = Vector2(w, h)
	col.shape = shape
	col.position = Vector2(w / 2.0, h / 2.0)
	area.add_child(col)
	# row of triangular spikes
	var tri := Polygon2D.new()
	var pts := PackedVector2Array()
	var n := int(w / 20.0)
	for i in n:
		var x0 := float(i) * 20.0
		pts.append(Vector2(x0, h))
		pts.append(Vector2(x0 + 10.0, 0.0))
		pts.append(Vector2(x0 + 20.0, h))
	tri.polygon = pts
	tri.color = Color(0.9, 0.3, 0.4)
	area.add_child(tri)
	area.body_entered.connect(_on_hazard_entered)


func _add_goal(x: float, y: float, w: float, h: float) -> void:
	var area := Area2D.new()
	area.position = Vector2(x, y)
	add_child(area)
	var col := CollisionShape2D.new()
	var shape := RectangleShape2D.new()
	shape.size = Vector2(w, h)
	col.shape = shape
	col.position = Vector2(w / 2.0, h / 2.0)
	area.add_child(col)
	var pole := Polygon2D.new()
	pole.polygon = PackedVector2Array([
		Vector2(0, 0), Vector2(6, 0), Vector2(6, h), Vector2(0, h)])
	pole.color = Color(0.9, 0.9, 0.2)
	area.add_child(pole)
	var flag := Polygon2D.new()
	flag.polygon = PackedVector2Array([
		Vector2(6, 4), Vector2(40, 12), Vector2(6, 20)])
	flag.color = Color(0.3, 1.0, 0.4)
	area.add_child(flag)
	area.body_entered.connect(_on_goal_entered)


func _build_player() -> void:
	player = CharacterBody2D.new()
	player.position = Vector2(100, 360)
	player.set_script(load("res://broken_game/BrokenPlayer.gd"))
	add_child(player)
	var col := CollisionShape2D.new()
	var shape := RectangleShape2D.new()
	shape.size = Vector2(24, 40)
	col.shape = shape
	player.add_child(col)
	var body := Polygon2D.new()
	body.polygon = PackedVector2Array([
		Vector2(-12, -20), Vector2(12, -20), Vector2(12, 20), Vector2(-12, 20)])
	body.color = Color(0.35, 0.85, 1.0)
	player.add_child(body)


func _build_camera() -> void:
	camera = Camera2D.new()
	camera.limit_left = 0
	camera.limit_top = 0
	camera.limit_right = int(LEVEL_WIDTH)
	camera.limit_bottom = 540
	camera.global_position = player.global_position + Vector2(0, -60)
	add_child(camera)
	camera.make_current()


func _build_hud() -> void:
	var layer := CanvasLayer.new()
	layer.layer = 5
	add_child(layer)
	hud_label = Label.new()
	hud_label.text = "GAME A -- THE TRIAL   |   Reach the flag. Avoid the spikes.   |   ESC: menu"
	hud_label.position = Vector2(12, 10)
	hud_label.add_theme_color_override("font_color", Color.WHITE)
	layer.add_child(hud_label)


# ------------------------------------------------------------------- game events
func _on_hazard_entered(body: Node) -> void:
	if body != player or broken:
		return
	_die()


func _on_goal_entered(body: Node) -> void:
	if body != player or broken:
		return
	_win()


func _die() -> void:
	broken = true
	if player:
		player.alive = false
	GameState.break_game()
	hud_label.text = "YOU DIED -- the cartridge is corrupted."
	_show_corrupted()


func _win() -> void:
	if player:
		player.alive = false
	broken = true
	_show_victory()


# ------------------------------------------------------------------- end states
func _show_corrupted() -> void:
	var layer := CanvasLayer.new()
	layer.layer = 100
	add_child(layer)

	var rect := ColorRect.new()
	rect.color = Color(1, 1, 1, 1)
	rect.position = Vector2.ZERO
	rect.size = get_viewport_rect().size
	rect.mouse_filter = Control.MOUSE_FILTER_IGNORE
	var mat := ShaderMaterial.new()
	mat.shader = load("res://shaders/glitch.gdshader")
	rect.material = mat
	layer.add_child(rect)

	var msg := Label.new()
	msg.text = "FATAL ERROR\nGAME CORRUPTED\n\nThe Trial has died.\nThis cartridge can no longer boot.\n\nOpen the Repair Realm (Game B)\nand collect 3 Light Shards to restore it.\n\nESC to return to the menu."
	msg.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	msg.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	msg.position = Vector2.ZERO
	msg.size = get_viewport_rect().size
	msg.add_theme_color_override("font_color", Color(1.0, 0.35, 0.35))
	msg.add_theme_font_size_override("font_size", 26)
	layer.add_child(msg)


func _show_victory() -> void:
	var layer := CanvasLayer.new()
	layer.layer = 100
	add_child(layer)

	var dim := ColorRect.new()
	dim.color = Color(0, 0, 0, 0.6)
	dim.position = Vector2.ZERO
	dim.size = get_viewport_rect().size
	dim.mouse_filter = Control.MOUSE_FILTER_IGNORE
	layer.add_child(dim)

	var msg := Label.new()
	msg.text = "TRIAL COMPLETE\n\nYou survived without dying.\nThe cartridge remains intact."
	msg.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	msg.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	msg.position = Vector2.ZERO
	msg.size = get_viewport_rect().size
	msg.add_theme_color_override("font_color", Color(0.4, 1.0, 0.5))
	msg.add_theme_font_size_override("font_size", 28)
	layer.add_child(msg)

	await get_tree().create_timer(3.0).timeout
	get_tree().change_scene_to_file("res://MainMenu.tscn")
