extends Node2D
## GAME B -- "The Repair Realm".
## A separate, top-down game. Collecting 3 Light Shards here writes broken=false
## back to the SHARED state file, which is what Game A reads on boot.
##
## That is the entire mechanic: the fix for one game literally lives inside
## another game. If Game A is not currently broken, there is nothing to repair.

const SHARD_COUNT := 3

var player: CharacterBody2D
var collected := 0
var done := false
var hud_label: Label


func _ready() -> void:
	_build_room()
	_build_player()
	_build_shards()
	_build_hud()
	if not GameState.is_broken():
		# Nothing to repair. Freeze the player and tell them to come back later.
		player.set_physics_process(false)
		_show_idle_overlay()


func _process(_delta: float) -> void:
	if Input.is_action_just_pressed("ui_cancel"):
		get_tree().change_scene_to_file("res://MainMenu.tscn")


# ------------------------------------------------------------------- world build
func _build_room() -> void:
	var floor_poly := Polygon2D.new()
	floor_poly.polygon = PackedVector2Array([
		Vector2(0, 0), Vector2(960, 0), Vector2(960, 540), Vector2(0, 540)])
	floor_poly.color = Color(0.06, 0.07, 0.11)
	add_child(floor_poly)
	# walls
	_add_wall(0.0, 0.0, 960.0, 20.0)
	_add_wall(0.0, 520.0, 960.0, 20.0)
	_add_wall(0.0, 0.0, 20.0, 540.0)
	_add_wall(940.0, 0.0, 20.0, 540.0)


func _add_wall(x: float, y: float, w: float, h: float) -> void:
	var b := StaticBody2D.new()
	b.position = Vector2(x, y)
	add_child(b)
	var c := CollisionShape2D.new()
	var s := RectangleShape2D.new()
	s.size = Vector2(w, h)
	c.shape = s
	c.position = Vector2(w / 2.0, h / 2.0)
	b.add_child(c)
	var p := Polygon2D.new()
	p.polygon = PackedVector2Array([
		Vector2(0, 0), Vector2(w, 0), Vector2(w, h), Vector2(0, h)])
	p.color = Color(0.28, 0.30, 0.38)
	b.add_child(p)


func _build_player() -> void:
	player = CharacterBody2D.new()
	player.position = Vector2(480, 300)
	player.set_script(load("res://fixer_game/FixerPlayer.gd"))
	add_child(player)
	var c := CollisionShape2D.new()
	var s := RectangleShape2D.new()
	s.size = Vector2(20, 20)
	c.shape = s
	player.add_child(c)
	var p := Polygon2D.new()
	p.polygon = PackedVector2Array([
		Vector2(-10, -10), Vector2(10, -10), Vector2(10, 10), Vector2(-10, 10)])
	p.color = Color(1.0, 0.9, 0.4)
	player.add_child(p)


func _build_shards() -> void:
	var positions := [Vector2(180, 150), Vector2(760, 420), Vector2(480, 120)]
	for pos in positions:
		var shard := Area2D.new()
		shard.position = pos
		add_child(shard)
		var c := CollisionShape2D.new()
		var s := RectangleShape2D.new()
		s.size = Vector2(30, 30)
		c.shape = s
		shard.add_child(c)
		var d := Polygon2D.new()
		d.polygon = PackedVector2Array([
			Vector2(0, -16), Vector2(12, 0), Vector2(0, 16), Vector2(-12, 0)])
		d.color = Color(0.5, 1.0, 0.85)
		shard.add_child(d)
		shard.body_entered.connect(_on_shard_entered.bind(shard))


func _build_hud() -> void:
	var layer := CanvasLayer.new()
	layer.layer = 5
	add_child(layer)
	hud_label = Label.new()
	hud_label.text = "GAME B -- REPAIR REALM   |   Light Shards: 0/3   |   ESC: menu"
	hud_label.position = Vector2(12, 10)
	layer.add_child(hud_label)


# ------------------------------------------------------------------- game events
func _on_shard_entered(body: Node, shard: Area2D) -> void:
	if body != player or done:
		return
	if not GameState.is_broken():
		return
	collected += 1
	shard.queue_free()
	hud_label.text = "GAME B -- REPAIR REALM   |   Light Shards: %d/3   |   ESC: menu" % collected
	if collected >= SHARD_COUNT:
		_complete()


func _complete() -> void:
	done = true
	GameState.repair_game()
	hud_label.text = "REPAIR COMPLETE -- Game A restored! Returning..."
	await get_tree().create_timer(2.5).timeout
	get_tree().change_scene_to_file("res://MainMenu.tscn")


func _show_idle_overlay() -> void:
	var layer := CanvasLayer.new()
	layer.layer = 50
	add_child(layer)
	var panel := Panel.new()
	panel.position = Vector2(230, 200)
	panel.size = Vector2(500, 160)
	layer.add_child(panel)
	var l := Label.new()
	l.text = "The Trial still functions.\nNothing to repair here yet.\nCome back after dying in Game A."
	l.set_anchors_preset(Control.PRESET_FULL_RECT)
	l.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	l.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	l.add_theme_color_override("font_color", Color(0.9, 0.9, 0.9))
	panel.add_child(l)
	var b := Button.new()
	b.text = "Back to Menu"
	b.position = Vector2(380, 330)
	b.size = Vector2(200, 40)
	layer.add_child(b)
	b.pressed.connect(func() -> void: get_tree().change_scene_to_file("res://MainMenu.tscn"))
