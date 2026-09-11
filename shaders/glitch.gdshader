shader_type canvas_item;

// Glitch / corruption overlay applied over the frozen game when Game A is broken.
uniform float strength : hint_range(0.0, 1.0) = 1.0;

float rand(vec2 co) {
	return fract(sin(dot(co, vec2(12.9898, 78.233))) * 43758.5453);
}

void fragment() {
	vec2 uv = SCREEN_UV;
	float t = TIME;

	// horizontal jitter, per scanline
	float j = sin(uv.y * 80.0 + t * 8.0) * 0.008 * strength;
	j += (rand(vec2(floor(uv.y * 200.0), t)) - 0.5) * 0.02 * strength;
	vec2 uvr = uv + vec2(j, 0.0);

	// RGB channel split
	float split = 0.005 * strength;
	vec3 col;
	col.r = texture(SCREEN_TEXTURE, uvr + vec2(split, 0.0)).r;
	col.g = texture(SCREEN_TEXTURE, uvr).g;
	col.b = texture(SCREEN_TEXTURE, uvr - vec2(split, 0.0)).b;

	// scanlines + flicker
	col *= 0.8 + 0.2 * sin(uv.y * 1500.0);
	col *= 0.9 + 0.1 * sin(t * 30.0);

	// occasional dark bands
	float band = step(0.6, rand(vec2(floor(uv.y * 8.0), floor(t * 4.0))));
	col *= 1.0 - band * 0.3 * strength;

	COLOR = vec4(col, 1.0);
}
