# world.gd - Chunk streaming, procedural terrain, and voxel raycasting.
extends Node3D

const CHUNK_SIZE := 16
const RENDER_RADIUS := 4
const VERTICAL_MIN := 0
const VERTICAL_MAX := 3
const SEA_LEVEL := 18
const MAX_BUILD_PER_FRAME := 3
const MAX_REMESH_PER_FRAME := 4

const CHUNK_SCRIPT = preload("res://scripts/chunk.gd")

var chunks: Dictionary = {}   # Vector3i -> Chunk
var gen_queue: Array = []     # Vector3i list (chunk coords awaiting terrain + mesh)
var remesh_queue: Array = []  # Vector3i list (existing chunks awaiting remesh)

var height_noise: FastNoiseLite
var tree_noise: FastNoiseLite

var player: CharacterBody3D


func _ready() -> void:
	_setup_environment()
	_setup_noise()
	_spawn_player()
	_update_chunks_around(_world_to_chunk(player.position))


func _setup_environment() -> void:
	var env := Environment.new()
	env.background_mode = Environment.BG_COLOR
	env.background_color = Color(0.55, 0.78, 0.95)
	env.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	env.ambient_light_color = Color(0.55, 0.65, 0.80)
	env.ambient_light_energy = 0.8
	env.fog_enabled = true
	env.fog_light_color = Color(0.60, 0.78, 0.95)
	env.fog_density = 0.015
	var cam_env := WorldEnvironment.new()
	cam_env.environment = env
	add_child(cam_env)

	var light := DirectionalLight3D.new()
	light.rotation = Vector3(deg_to_rad(-45), deg_to_rad(30), 0)
	light.light_energy = 0.9
	add_child(light)

	# Minimal HUD: crosshair + help text.
	var layer := CanvasLayer.new()
	add_child(layer)
	var cross := ColorRect.new()
	cross.color = Color(1, 1, 1, 0.85)
	cross.size = Vector2(3, 3)
	cross.set_anchors_preset(Control.PRESET_CENTER)
	cross.position -= cross.size / 2.0
	layer.add_child(cross)
	var help := Label.new()
	help.text = "WASD move | Space jump | Shift sprint | F fly | LMB break | RMB place | 1-7 select | Esc free mouse"
	help.position = Vector2(10, 10)
	help.add_theme_color_override("font_color", Color(0.05, 0.05, 0.05))
	layer.add_child(help)


func _setup_noise() -> void:
	height_noise = FastNoiseLite.new()
	height_noise.seed = 1337
	height_noise.frequency = 0.012
	height_noise.noise_type = FastNoiseLite.TYPE_SIMPLEX
	tree_noise = FastNoiseLite.new()
	tree_noise.seed = 99
	tree_noise.frequency = 0.6
	tree_noise.noise_type = FastNoiseLite.TYPE_VALUE


func _spawn_player() -> void:
	player = CharacterBody3D.new()
	player.set_script(preload("res://scripts/player.gd"))
	player.position = Vector3(8.0, 48.0, 8.0)
	add_child(player)


func _process(_delta: float) -> void:
	var pc := _world_to_chunk(player.position)
	_update_chunks_around(pc)
	_process_queues()


func _world_to_chunk(wp: Vector3) -> Vector3i:
	return Vector3i(int(floor(wp.x / CHUNK_SIZE)), int(floor(wp.y / CHUNK_SIZE)), int(floor(wp.z / CHUNK_SIZE)))


func _update_chunks_around(center: Vector3i) -> void:
	var wanted: Dictionary = {}
	for dz in range(-RENDER_RADIUS, RENDER_RADIUS + 1):
		for dx in range(-RENDER_RADIUS, RENDER_RADIUS + 1):
			for cy in range(VERTICAL_MIN, VERTICAL_MAX + 1):
				var cp := Vector3i(center.x + dx, cy, center.z + dz)
				wanted[cp] = true
				if not chunks.has(cp) and not gen_queue.has(cp):
					gen_queue.append(cp)

	# Unload chunks that have left the render area.
	var to_remove: Array = []
	for cp in chunks.keys():
		if not wanted.has(cp):
			to_remove.append(cp)
	for cp in to_remove:
		var c = chunks[cp]
		c.queue_free()
		chunks.erase(cp)


func _process_queues() -> void:
	var built := 0
	while gen_queue.size() > 0 and built < MAX_BUILD_PER_FRAME:
		var cp: Vector3i = gen_queue.pop_front()
		if chunks.has(cp):
			continue
		_generate_chunk(cp)
		built += 1
	var remeshed := 0
	while remesh_queue.size() > 0 and remeshed < MAX_REMESH_PER_FRAME:
		var cp: Vector3i = remesh_queue.pop_front()
		if chunks.has(cp):
			chunks[cp].build_mesh()
		remeshed += 1


func _generate_chunk(cp: Vector3i) -> void:
	var c = CHUNK_SCRIPT.new()
	c.chunk_pos = cp
	c.world = self
	add_child(c)
	chunks[cp] = c
	_fill_terrain(c)
	c.generated = true
	c.build_mesh()
	# Remesh existing neighbors so border faces clean up now that data is present.
	for d in [Vector3i(1, 0, 0), Vector3i(-1, 0, 0), Vector3i(0, 1, 0), Vector3i(0, -1, 0), Vector3i(0, 0, 1), Vector3i(0, 0, -1)]:
		_queue_remesh(cp + d)


func _fill_terrain(c) -> void:
	var base_x := c.chunk_pos.x * CHUNK_SIZE
	var base_y := c.chunk_pos.y * CHUNK_SIZE
	var base_z := c.chunk_pos.z * CHUNK_SIZE
	for lz in CHUNK_SIZE:
		for lx in CHUNK_SIZE:
			var wx := base_x + lx
			var wz := base_z + lz
			var h := _column_height(wx, wz)
			for ly in CHUNK_SIZE:
				var wy := base_y + ly
				var id := Blocks.ID.AIR
				if wy == 0:
					id = Blocks.ID.BEDROCK
				elif wy > h:
					if wy <= SEA_LEVEL:
						id = Blocks.ID.WATER
					else:
						id = Blocks.ID.AIR
				elif wy == h:
					if h <= SEA_LEVEL + 1:
						id = Blocks.ID.SAND
					else:
						id = Blocks.ID.GRASS
				elif wy >= h - 3:
					id = Blocks.ID.DIRT
				else:
					id = Blocks.ID.STONE
				if id != Blocks.ID.AIR:
					c.set_block_local(lx, ly, lz, id)

			# Trees: only when the surface sits inside this chunk and the canopy fits.
			var surface_ly := h - base_y
			if h > SEA_LEVEL + 1 and surface_ly >= 0 and surface_ly < CHUNK_SIZE:
				var top_ly := surface_ly + 4
				if top_ly < CHUNK_SIZE and lx >= 3 and lx <= CHUNK_SIZE - 4 and lz >= 3 and lz <= CHUNK_SIZE - 4:
					var tv := tree_noise.get_noise_2d(float(wx), float(wz))
					if tv > 0.82:
						_plant_tree(c, lx, surface_ly, lz)


func _plant_tree(c, lx: int, surface_ly: int, lz: int) -> void:
	var trunk_h := 4
	var top_ly := surface_ly + trunk_h
	for i in range(1, trunk_h + 1):
		var ly := surface_ly + i
		if ly >= 0 and ly < CHUNK_SIZE:
			c.set_block_local(lx, ly, lz, Blocks.ID.WOOD)
	# Rounded canopy around the trunk top.
	for dy in range(-1, 2):
		for dx in range(-2, 3):
			for dz in range(-2, 3):
				if abs(dx) + abs(dz) > 3:
					continue
				var ly := top_ly - 1 + dy
				if ly < 0 or ly >= CHUNK_SIZE:
					continue
				if c.get_block_local(lx + dx, ly, lz + dz) == Blocks.ID.AIR:
					c.set_block_local(lx + dx, ly, lz + dz, Blocks.ID.LEAVES)


func _column_height(wx: int, wz: int) -> int:
	var n := height_noise.get_noise_2d(float(wx), float(wz)) # -1..1
	var h := SEA_LEVEL + 2 + int(n * 14.0) # ~6..34
	if h < 1:
		h = 1
	return h


func _chunk_coord(wp: Vector3i) -> Vector3i:
	return Vector3i(int(floor(float(wp.x) / CHUNK_SIZE)), int(floor(float(wp.y) / CHUNK_SIZE)), int(floor(float(wp.z) / CHUNK_SIZE)))


func get_block(wp: Vector3i) -> int:
	var cp := _chunk_coord(wp)
	if not chunks.has(cp):
		return Blocks.ID.AIR
	var c = chunks[cp]
	var lx := wp.x - cp.x * CHUNK_SIZE
	var ly := wp.y - cp.y * CHUNK_SIZE
	var lz := wp.z - cp.z * CHUNK_SIZE
	return c.data[c.idx(lx, ly, lz)]


func set_block(wp: Vector3i, id: int) -> void:
	var cp := _chunk_coord(wp)
	if not chunks.has(cp):
		return
	var c = chunks[cp]
	var lx := wp.x - cp.x * CHUNK_SIZE
	var ly := wp.y - cp.y * CHUNK_SIZE
	var lz := wp.z - cp.z * CHUNK_SIZE
	c.set_block_local(lx, ly, lz, id)
	_queue_remesh(cp)
	if lx == 0: _queue_remesh(cp + Vector3i(-1, 0, 0))
	if lx == CHUNK_SIZE - 1: _queue_remesh(cp + Vector3i(1, 0, 0))
	if ly == 0: _queue_remesh(cp + Vector3i(0, -1, 0))
	if ly == CHUNK_SIZE - 1: _queue_remesh(cp + Vector3i(0, 1, 0))
	if lz == 0: _queue_remesh(cp + Vector3i(0, 0, -1))
	if lz == CHUNK_SIZE - 1: _queue_remesh(cp + Vector3i(0, 0, 1))


func _queue_remesh(cp: Vector3i) -> void:
	if chunks.has(cp) and not remesh_queue.has(cp):
		remesh_queue.append(cp)


# Amanatides-Woo voxel ray traversal. Returns {pos, normal} or empty dict.
func raycast_voxel(origin: Vector3, dir: Vector3, max_dist: float) -> Dictionary:
	dir = dir.normalized()
	var ix := int(floor(origin.x))
	var iy := int(floor(origin.y))
	var iz := int(floor(origin.z))
	var sx := 0
	var sy := 0
	var sz := 0
	if dir.x > 1e-6: sx = 1
	elif dir.x < -1e-6: sx = -1
	if dir.y > 1e-6: sy = 1
	elif dir.y < -1e-6: sy = -1
	if dir.z > 1e-6: sz = 1
	elif dir.z < -1e-6: sz = -1

	var t_max := Vector3(INF, INF, INF)
	var t_delta := Vector3(INF, INF, INF)
	if sx != 0:
		var boundary := float(ix) + (float(sx) + 1.0) * 0.5
		t_max.x = (boundary - origin.x) / dir.x
		t_delta.x = abs(1.0 / dir.x)
	if sy != 0:
		var boundary := float(iy) + (float(sy) + 1.0) * 0.5
		t_max.y = (boundary - origin.y) / dir.y
		t_delta.y = abs(1.0 / dir.y)
	if sz != 0:
		var boundary := float(iz) + (float(sz) + 1.0) * 0.5
		t_max.z = (boundary - origin.z) / dir.z
		t_delta.z = abs(1.0 / dir.z)

	var normal := Vector3i.ZERO
	var t := 0.0
	while t <= max_dist:
		var id := get_block(Vector3i(ix, iy, iz))
		if id != Blocks.ID.AIR and not Blocks.is_liquid(id):
			return {"pos": Vector3i(ix, iy, iz), "normal": normal}
		if t_max.x < t_max.y and t_max.x < t_max.z:
			ix += sx
			t = t_max.x
			t_max.x += t_delta.x
			normal = Vector3i(-sx, 0, 0)
		elif t_max.y < t_max.z:
			iy += sy
			t = t_max.y
			t_max.y += t_delta.y
			normal = Vector3i(0, -sy, 0)
		else:
			iz += sz
			t = t_max.z
			t_max.z += t_delta.z
			normal = Vector3i(0, 0, -sz)
	return {}
