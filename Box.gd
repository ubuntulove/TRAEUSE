extends Node2D
class_name Box

# A heavy crate. Too heavy for one hand to lift — a single hand can only
# drag it along the ground. Two hands together can lift and carry it freely.
# Position is driven by Main based on which hands are grabbing it.

var size := Vector2(74, 74)


func _draw() -> void:
	var r = Rect2(-size * 0.5, size)
	# body
	draw_rect(r, Color(0.66, 0.49, 0.27), true)
	# border
	draw_rect(r, Color(0.38, 0.26, 0.16), false, 4)
	# plank lines
	var mid_y = r.position.y + size.y * 0.5
	draw_line(Vector2(r.position.x, mid_y), Vector2(r.position.x + size.x, mid_y), Color(0.4, 0.3, 0.18), 3)
	# X bracing
	draw_line(r.position, r.position + size, Color(0.42, 0.31, 0.2), 2)
	draw_line(Vector2(r.position.x, r.position.y + size.y), Vector2(r.position.x + size.x, r.position.y), Color(0.42, 0.31, 0.2), 2)
	# "HEAVY" hint mark
	var mark = Rect2(-8, -8, 16, 16)
	draw_rect(mark, Color(0.3, 0.2, 0.12), true)
