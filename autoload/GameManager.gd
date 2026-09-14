extends Node
## Central game state. Switches the game type (Shooter / Platformer / Collector)
## and the camera perspective (First-person / Third-person / Top-down).
## Emits signals that the player, HUD and spawner listen to.

enum GameType { SHOOTER, PLATFORMER, COLLECTOR }
enum Perspective { FIRST_PERSON, THIRD_PERSON, TOP_DOWN }

signal game_type_changed(new_type: int)
signal perspective_changed(new_perspective: int)
signal score_changed(new_score: int)
signal status_message(text: String)

## Current state
var game_type: int = GameType.SHOOTER
var perspective: int = Perspective.THIRD_PERSON
var score: int = 0

## Player reference (set by Main on ready)
var player: Node3D = null
## Entity container (set by Main on ready) — spawned entities live here.
var entity_container: Node = null

const TYPE_NAMES := {
	GameType.SHOOTER: "Shooter",
	GameType.PLATFORMER: "Platformer",
	GameType.COLLECTOR: "Collector",
}
const PERSPECTIVE_NAMES := {
	Perspective.FIRST_PERSON: "First-Person",
	Perspective.THIRD_PERSON: "Third-Person",
	Perspective.TOP_DOWN: "Top-Down",
}


func _ready() -> void:
	randomize()


func set_game_type(type_value: int) -> void:
	if type_value == game_type:
		return
	game_type = type_value
	game_type_changed.emit(game_type)
	status_message.emit("Game type: %s" % TYPE_NAMES[game_type])


func set_perspective(p: int) -> void:
	if p == perspective:
		return
	perspective = p
	perspective_changed.emit(perspective)
	status_message.emit("View: %s" % PERSPECTIVE_NAMES[perspective])


func cycle_game_type() -> void:
	set_game_type((game_type + 1) % 3)


func cycle_perspective() -> void:
	set_perspective((perspective + 1) % 3)


func add_score(amount: int) -> void:
	score += amount
	score_changed.emit(score)


func reset_score() -> void:
	score = 0
	score_changed.emit(score)


func game_type_name() -> String:
	return TYPE_NAMES[game_type]


func perspective_name() -> String:
	return PERSPECTIVE_NAMES[perspective]
