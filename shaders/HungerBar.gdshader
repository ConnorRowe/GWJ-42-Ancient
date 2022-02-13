shader_type canvas_item;

uniform float fill_percent: hint_range(0.0, 1.0) = 1.0;
uniform sampler2D fill_mask;
uniform vec4 fill_colour: hint_color = vec4(1.0);
uniform float strength = 6.0;

void fragment()
{
	vec4 fill = texture(fill_mask, UV);
	fill.a -= fill.r / fill_percent;
	fill.a *= pow(strength, fill_percent);
	fill.a = clamp(fill.a, 0.0, 1.0);
	
	COLOR = texture(TEXTURE, UV);
	COLOR = mix(COLOR, fill_colour, fill.a);
}