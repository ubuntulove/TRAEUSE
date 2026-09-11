extends Node2D
## Title screen. Pick which cartridge to insert.
## Shows whether Game A is currently corrupted.

func _ready() -> void:
	var layer := CanvasLayer.new()
	add_child(layer)

	var bg := ColorRect.new()
	bg.color = Color(0.05, 0.06, 0.10)
	bg.position = Vector2.ZERO
	bg.size = get_viewport_rect().size
	bg.mouse_filter = Control.MOUSE_FILTER_IGNORE
	layer.add_child(bg)

	var title := Label.new()
	title.text = "CROSS-GAME DEATH"
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.position = Vector2(0, 70)
	title.size = Vector2(960, 60)
	title.add_theme_font_size_override("font_size", 40)
	layer.add_child(title)

	var sub := Label.new()
	sub.text = "Dying in one game corrupts it forever.\nThe only way to repair it is to play another game."
	sub.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	sub.position = Vector2(0, 140)
	sub.size = Vector2(960, 60)
	sub.add_theme_color_override("font_color", Color(0.8, 0.8, 0.85))
	layer.add_child(sub)

	var status := Label.new()
	status.text = _status_text()
	status.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	status.position = Vector2(0, 220)
	status.size = Vector2(960, 30)
	status.add_theme_color_override("font_color", Color(1.0, 0.85, 0.3))
	status.add_theme_font_size_override("font_size", 20)
	layer.add_child(status)

	var cx := 330
	_add_button(layer, "1. Enter the Trial  (Game A)", Vector2(cx, 280), "_on_trial")
	_add_button(layer, "2. Enter the Repair Realm  (Game B)", Vector2(cx, 340), "_on_repair")
	_add_button(layer, "Reset all progress", Vector2(cx, 400), "_on_reset")

	var hint := Label.new()
	hint.text = "Controls: WASD / Arrows to move, Space to jump. ESC returns here."
	hint.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	hint.position = Vector2(0, 480)
	hint.size = Vector2(960, 30)
	hint.add_theme_color_override("font_color", Color(0.6, 0.6, 0.65))
	layer.add_child(hint)


func _status_text() -> String:
	if GameState.is_broken():
		return "[ STATUS:  GAME A IS CORRUPTED  --  repair it in Game B ]"
	return "[ STATUS:  Game A is functional ]"


func _add_button(parent: Node, text: String, pos: Vector2, method: String) -> void:
	var b := Button.new()
	b.text = text
	b.position = pos
	b.size = Vector2(300, 46)
	parent.add_child(b)
	b.pressed.connect(Callable(self, method))


func _on_trial() -> void:
	get_tree().change_scene_to_file("res://broken_game/BrokenGame.tscn")


func _on_repair() -> void:
	get_tree().change_scene_to_file("res://fixer_game/FixerGame.tscn")


func _on_reset() -> void:
	GameState.reset_state()
	get_tree().reload_current_scene()
