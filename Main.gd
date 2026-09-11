extends Node2D
## Entry point. Spawns the player, draws the HUD, and periodically
## spawns customers who want to buy something off your supermarket-arm.

const PlayerScene := preload("res://Player.tscn")
const NPCScene := preload("res://NPC.tscn")

const NPC_SPAWN_INTERVAL := 3.5
const MAX_NPCS := 4

var player: CharacterBody2D
var score_label: Label
var hint_label: Label
var score := 0
var npc_count := 0
var npc_spawn_timer := 0.0

func _ready() -> void:
	# Player
	player = PlayerScene.instantiate()
	player.position = Vector2(512, 300)
	add_child(player)

	# HUD: score
	score_label = Label.new()
	score_label.position = Vector2(16, 12)
	score_label.add_theme_font_size_override("font_size", 24)
	score_label.text = "Sales: 0"
	add_child(score_label)

	# HUD: hint
	hint_label = Label.new()
	hint_label.position = Vector2(16, 556)
	hint_label.text = "WASD / Arrows: walk     SPACE: hand an item to nearest customer\nYour LEFT ARM is a SUPERMARKET — items restock on the shelves automatically."
	add_child(hint_label)

func _process(delta: float) -> void:
	npc_spawn_timer += delta
	if npc_spawn_timer >= NPC_SPAWN_INTERVAL and npc_count < MAX_NPCS:
		npc_spawn_timer = 0.0
		_spawn_npc()

func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventKey and event.pressed and not event.echo:
		if event.keycode == KEY_SPACE or event.is_action_pressed("ui_accept"):
			player.try_give_item()

func _spawn_npc() -> void:
	var npc := NPCScene.instantiate()
	var angle := randf() * TAU
	var dist := 520.0
	npc.position = player.position + Vector2(cos(angle), sin(angle)) * dist
	add_child(npc)
	npc.player = player
	npc.satisfied.connect(_on_npc_satisfied)
	npc_count += 1

func _on_npc_satisfied(_item: String) -> void:
	score += 1
	score_label.text = "Sales: %d" % score
	npc_count -= 1
