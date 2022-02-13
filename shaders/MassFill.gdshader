shader_type canvas_item;

uniform vec4 fill_colour : hint_color = vec4(1.0);
uniform vec4 outline_colour : hint_color = vec4(1.0);
uniform float outline_thickness : hint_range(0, 10) = 1.0;
uniform float uniform_scale = 1.0;

// From https://godotshaders.com/snippet/scale/
vec2 scale(vec2 uv, float x, float y)
{
	mat2 scale = mat2(vec2(x, 0.0), vec2(0.0, y));
	
	uv -= 0.5;
	uv = uv * scale;
	uv += 0.5;
	return uv;
}

// Adapted from https://godotshaders.com/shader/2d-outline-stroke/
void fragment() {
	vec2 sUV = scale(UV, uniform_scale, uniform_scale);
	
	vec2 size = TEXTURE_PIXEL_SIZE * outline_thickness * uniform_scale;
	
	float outline = texture(TEXTURE, sUV + vec2(-size.x, 0)).a;
	outline += texture(TEXTURE, sUV + vec2(0, size.y)).a;
	outline += texture(TEXTURE, sUV + vec2(size.x, 0)).a;
	outline += texture(TEXTURE, sUV + vec2(0, -size.y)).a;
	outline += texture(TEXTURE, sUV + vec2(-size.x, size.y)).a;
	outline += texture(TEXTURE, sUV + vec2(size.x, size.y)).a;
	outline += texture(TEXTURE, sUV + vec2(-size.x, -size.y)).a;
	outline += texture(TEXTURE, sUV + vec2(size.x, -size.y)).a;
	outline = min(outline, 1.0);
	
	vec4 color = texture(TEXTURE, sUV) * fill_colour;
	COLOR = mix(color, outline_colour, outline - color.a);
}