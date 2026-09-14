extends Node
## Game entry point. Wires the level, player, HUD and a dynamic entity
## container, sets up lighting, and spawns/respawns entities based on the
## active game type (Shooter / Platformer / Collector).

const ENEMY_TARGET := 5
const COLLECTOR_WAVE := 12

var level: Node3D
var entity_container: Node3D
var _maintain_timer: Timer


func _ready() -> void:
	_setup_lighting()

	level = $Level
	entity_container = $EntityContainer
	GameManager.entity_container = entity_container

	var player_scene: PackedScene = load("res://player/Player.tscn")
	var player: Node3D = player_scene.instantiate()
	add_child(player)
	player.global_position = level.player_start

	GameManager.game_type_changed.connect(_on_type_changed)
	_spawn_for_type(GameManager.game_type)

	_maintain_timer = Timer.new()
	_maintain_timer.wait_time = 2.0
	_maintain_timer.timeout.connect(_maintain)
	add_child(_maintain_timer)
	_maintain_timer.start()


func _setup_lighting() -> void:
	var sun := DirectionalLight3D.new()
	sun.name = "Sun"
	sun.rotation = Vector3(-0.95, -0.7, 0.0)
	sun.light_energy = 1.05
	sun.shadow_enabled = true
	sun.shadow_normal_bias = 1.5
	add_child(sun)

	var env := Environment.new()
	env.background_mode = Environment.BG_COLOR
	env.background_color = Color(0.52, 0.66, 0.85)
	env.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	env.ambient_light_color = Color(0.55, 0.66, 0.82)
	env.ambient_light_energy = 0.7
	env.fog_enabled = true
	env.fog_light_color = Color(0.55, 0.66, 0.82)
	env.fog_density = 0.018
	var we := WorldEnvironment.new()
	we.environment = env
	add_child(we)


func _on_type_changed(_t: int) -> void:
	_spawn_for_type(GameManager.game_type)


func _spawn_for_type(type_value: int) -> void:
	_clear_entities()
	match type_value:
		GameManager.GameType.SHOOTER:
			for i in ENEMY_TARGET:
				_spawn_enemy()
			GameManager.status_message.emit("Shooter: shoot the enemies!")
		GameManager.GameType.PLATFORMER:
			for top in level.platform_tops:
				_spawn_pickup(top)
			GameManager.status_message.emit("Platformer: collect gems on the platforms!")
		GameManager.GameType.COLLECTOR:
			_spawn_collector_wave()
			GameManager.status_message.emit("Collector: grab every gem!")


func _maintain() -> void:
	match GameManager.game_type:
		GameManager.GameType.SHOOTER:
			var n := get_tree().get_nodes_in_group("enemy").size()
			for i in range(n, ENEMY_TARGET):
				_spawn_enemy()
		GameManager.GameType.COLLECTOR:
			if get_tree().get_nodes_in_group("pickup").size() == 0:
				_spawn_collector_wave()
				GameManager.status_message.emit("Wave cleared! New wave incoming...")
		GameManager.GameType.PLATFORMER:
			if get_tree().get_nodes_in_group("pickup").size() == 0:
				for top in level.platform_tops:
					_spawn_pickup(top)
				GameManager.status_message.emit("All gems collected! Respawning gems...")


func _spawn_collector_wave() -> void:
	var pts := (level.ground_points as Array).duplicate()
	pts.shuffle()
	var count: int = min(pts.size(), COLLECTOR_WAVE)
	for i in count:
		_spawn_pickup(pts[i])


func _spawn_enemy() -> void:
	var pts := (level.ground_points as Array).duplicate()
	pts.shuffle()
	var p := pts[0] as Vector3
	var scene: PackedScene = load("res://entities/Enemy.tscn")
	var e: Node3D = scene.instantiate()
	entity_container.add_child(e)
	e.global_position = Vector3(p.x, 1.0, p.z)


func _spawn_pickup(at: Vector3) -> void:
	var scene: PackedScene = load("res://entities/Pickup.tscn")
	var pk: Node3D = scene.instantiate()
	entity_container.add_child(pk)
	pk.global_position = Vector3(at.x, at.y, at.z)


func _clear_entities() -> void:
	for c in entity_container.get_children():
		c.queue_free()
