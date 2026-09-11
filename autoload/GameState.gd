extends Node
## Cross-game persistent state.
##
## Both games read/write the SAME save file in user://. When the player dies
## in Game A, `broken` becomes true and Game A refuses to boot normally.
## Game B repairs it by collecting Light Shards, which sets `broken` false.
##
## This is the whole point of the concept: the death "breaks the cartridge",
## and the fix lives in a *different* game.

const STATE_PATH := "user://cross_game_state.json"

var _state: Dictionary = {
	"broken": false,
	"broken_token": 0,
	"deaths": 0,
	"repairs": 0,
}


func _ready() -> void:
	load_state()
	_setup_input()


# ------------------------------------------------------------------ input map
# Defined in code so the project has no fragile [input] section in project.godot.
func _setup_input() -> void:
	_add_key_action("move_left", [KEY_A, KEY_LEFT])
	_add_key_action("move_right", [KEY_D, KEY_RIGHT])
	_add_key_action("move_up", [KEY_W, KEY_UP])
	_add_key_action("move_down", [KEY_S, KEY_DOWN])
	_add_key_action("jump", [KEY_SPACE, KEY_W, KEY_UP])
	if not InputMap.has_action("ui_cancel"):
		InputMap.add_action("ui_cancel")
		var ev := InputEventKey.new()
		ev.physical_keycode = KEY_ESCAPE
		InputMap.action_add_event("ui_cancel", ev)


func _add_key_action(action_name: String, keycodes: Array) -> void:
	if not InputMap.has_action(action_name):
		InputMap.add_action(action_name)
	else:
		InputMap.action_erase_events(action_name)
	for kc in keycodes:
		var ev := InputEventKey.new()
		ev.physical_keycode = kc
		InputMap.action_add_event(action_name, ev)


# ------------------------------------------------------------------ persistence
func load_state() -> void:
	if FileAccess.file_exists(STATE_PATH):
		var f := FileAccess.open(STATE_PATH, FileAccess.READ)
		if f:
			var text := f.get_as_text()
			f.close()
			var parsed = JSON.parse_string(text)
			if typeof(parsed) == TYPE_DICTIONARY:
				_state.merge(parsed, true)


func save_state() -> void:
	var f := FileAccess.open(STATE_PATH, FileAccess.WRITE)
	if f:
		f.store_string(JSON.stringify(_state, "  "))
		f.close()


# ------------------------------------------------------------------ public API
func is_broken() -> bool:
	return bool(_state.get("broken", false))


func get_broken_token() -> int:
	return int(_state.get("broken_token", 0))


func break_game() -> void:
	_state["broken"] = true
	_state["broken_token"] = get_broken_token() + 1
	_state["deaths"] = int(_state.get("deaths", 0)) + 1
	save_state()


func repair_game() -> void:
	_state["broken"] = false
	_state["repairs"] = int(_state.get("repairs", 0)) + 1
	save_state()


func get_deaths() -> int:
	return int(_state.get("deaths", 0))


func get_repairs() -> int:
	return int(_state.get("repairs", 0))


func reset_state() -> void:
	_state = {
		"broken": false,
		"broken_token": 0,
		"deaths": 0,
		"repairs": 0,
	}
	save_state()
