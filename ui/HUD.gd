extends CanvasLayer
## Heads-up display: shows the current game type & perspective, the score,
## transient status messages, a controls hint, and buttons to switch modes.
## Everything is built in code so the scene stays minimal.

var _type_label: Label
var _view_label: Label
var _score_label: Label
var _status_label: Label
var _hint_label: Label
var _msg_timer: Timer


func _ready() -> void:
	var root := Control.new()
	root.set_anchors_preset(Control.PRESET_FULL_RECT)
	root.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(root)

	var margin := MarginContainer.new()
	margin.set_anchors_preset(Control.PRESET_FULL_RECT)
	margin.mouse_filter = Control.MOUSE_FILTER_IGNORE
	margin.add_theme_constant_override("margin_left", 18)
	margin.add_theme_constant_override("margin_top", 14)
	margin.add_theme_constant_override("margin_right", 18)
	margin.add_theme_constant_override("margin_bottom", 14)
	root.add_child(margin)

	var vbox := VBoxContainer.new()
	vbox.mouse_filter = Control.MOUSE_FILTER_PASS
	vbox.add_theme_constant_override("separation", 8)
	margin.add_child(vbox)

	var info := HBoxContainer.new()
	info.mouse_filter = Control.MOUSE_FILTER_PASS
	info.add_theme_constant_override("separation", 18)
	vbox.add_child(info)

	_type_label = _make_label(info, "Game: Shooter")
	_view_label = _make_label(info, "View: Third-Person")
	_score_label = _make_label(info, "Score: 0")

	var buttons := HBoxContainer.new()
	buttons.mouse_filter = Control.MOUSE_FILTER_PASS
	buttons.add_theme_constant_override("separation", 10)
	vbox.add_child(buttons)
	_make_button(buttons, "Switch Game Type (G)", _on_switch_type)
	_make_button(buttons, "Switch View (V)", _on_switch_view)
	_make_button(buttons, "Reset Score", _on_reset_score)

	_status_label = Label.new()
	_status_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_status_label.add_theme_font_size_override("font_size", 18)
	vbox.add_child(_status_label)

	_hint_label = Label.new()
	_hint_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	vbox.add_child(_hint_label)

	_msg_timer = Timer.new()
	_msg_timer.wait_time = 2.6
	_msg_timer.one_shot = true
	_msg_timer.timeout.connect(_on_msg_timeout)
	add_child(_msg_timer)

	GameManager.game_type_changed.connect(_on_type_changed)
	GameManager.perspective_changed.connect(_on_view_changed)
	GameManager.score_changed.connect(_on_score_changed)
	GameManager.status_message.connect(_on_status)

	_on_type_changed(GameManager.game_type)
	_on_view_changed(GameManager.perspective)
	_on_score_changed(GameManager.score)
	_update_hint()


func _make_label(parent: Control, text: String) -> Label:
	var l := Label.new()
	l.text = text
	l.mouse_filter = Control.MOUSE_FILTER_IGNORE
	l.add_theme_font_size_override("font_size", 20)
	parent.add_child(l)
	return l


func _make_button(parent: Control, text: String, callable: Callable) -> void:
	var b := Button.new()
	b.text = text
	b.add_theme_font_size_override("font_size", 16)
	b.pressed.connect(callable)
	parent.add_child(b)


func _on_switch_type() -> void:
	GameManager.cycle_game_type()
	_update_hint()


func _on_switch_view() -> void:
	GameManager.cycle_perspective()
	_update_hint()


func _on_reset_score() -> void:
	GameManager.reset_score()
	_status_label.text = "Score reset."


func _on_type_changed(t: int) -> void:
	_type_label.text = "Game: %s" % GameManager.TYPE_NAMES[t]
	_update_hint()


func _on_view_changed(p: int) -> void:
	_view_label.text = "View: %s" % GameManager.PERSPECTIVE_NAMES[p]
	_update_hint()


func _on_score_changed(s: int) -> void:
	_score_label.text = "Score: %d" % s


func _on_status(text: String) -> void:
	_status_label.text = text
	_msg_timer.start()


func _on_msg_timeout() -> void:
	_status_label.text = ""


func _update_hint() -> void:
	var t := GameManager.game_type
	var p := GameManager.perspective
	var hint := "WASD/Arrows: move | Space: jump"
	match t:
		GameManager.GameType.SHOOTER:
			hint += " | Left-click: shoot"
		GameManager.GameType.COLLECTOR:
			hint += " | Walk into gems to collect"
		GameManager.GameType.PLATFORMER:
			hint += " | Jump onto platforms for gems"
	hint += " | G: game type | V: view"
	if p != GameManager.Perspective.TOP_DOWN:
		hint += " | Esc: free cursor (click world to re-lock)"
	_hint_label.text = hint
