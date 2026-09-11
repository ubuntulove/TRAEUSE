# blocks.gd - Block type registry + shared materials (autoload singleton "Blocks")
extends Node

# Block ids
enum ID { AIR, GRASS, DIRT, STONE, SAND, WOOD, LEAVES, WATER, BEDROCK }

# Definition of a single block type.
class BlockType:
	var name: String
	var solid: bool        # blocks movement / can be raycast-targeted
	var transparent: bool  # neighbors still render their faces against it
	var liquid: bool       # water-like: no collision, transparent
	var colors: Dictionary # optional per-face color override; falls back to "all"

	func _init(p_name: String, p_solid: bool = true, p_transparent: bool = false, p_liquid: bool = false, p_colors: Dictionary = {}):
		name = p_name
		solid = p_solid
		transparent = p_transparent
		liquid = p_liquid
		colors = p_colors

var types: Array = []

# Shared materials (unshaded, vertex-color driven for a clean voxel look).
var opaque_material: StandardMaterial3D
var water_material: StandardMaterial3D


func _ready() -> void:
	types.resize(ID.size())
	types[ID.AIR]     = BlockType.new("air",     false, true,  false, {})
	types[ID.GRASS]   = BlockType.new("grass",   true,  false, false, {"top": Color(0.45, 0.74, 0.31), "side": Color(0.50, 0.42, 0.27), "bottom": Color(0.50, 0.42, 0.27)})
	types[ID.DIRT]    = BlockType.new("dirt",    true,  false, false, {"all": Color(0.50, 0.40, 0.26)})
	types[ID.STONE]   = BlockType.new("stone",   true,  false, false, {"all": Color(0.50, 0.50, 0.52)})
	types[ID.SAND]    = BlockType.new("sand",    true,  false, false, {"all": Color(0.85, 0.80, 0.56)})
	types[ID.WOOD]    = BlockType.new("wood",    true,  false, false, {"top": Color(0.55, 0.42, 0.25), "side": Color(0.40, 0.30, 0.20), "bottom": Color(0.55, 0.42, 0.25)})
	types[ID.LEAVES]  = BlockType.new("leaves",  true,  true,  false, {"all": Color(0.26, 0.55, 0.22)})
	types[ID.WATER]   = BlockType.new("water",   false, true,  true,  {"all": Color(0.20, 0.38, 0.78, 0.65)})
	types[ID.BEDROCK] = BlockType.new("bedrock", true,  false, false, {"all": Color(0.15, 0.15, 0.17)})

	opaque_material = StandardMaterial3D.new()
	opaque_material.vertex_color_use_as_albedo = true
	opaque_material.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED

	water_material = StandardMaterial3D.new()
	water_material.vertex_color_use_as_albedo = true
	water_material.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	water_material.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	water_material.cull_mode = BaseMaterial3D.CULL_DISABLED


func is_solid(id: int) -> bool:
	if id < 0 or id >= types.size():
		return false
	return types[id].solid


func is_transparent(id: int) -> bool:
	if id < 0 or id >= types.size():
		return true
	return types[id].transparent


func is_liquid(id: int) -> bool:
	if id < 0 or id >= types.size():
		return false
	return types[id].liquid


func get_color(id: int, key: String) -> Color:
	var t = types[id]
	if t.colors.has(key):
		return t.colors[key]
	return t.colors.get("all", Color.WHITE)
