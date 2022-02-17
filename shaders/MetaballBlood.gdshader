shader_type canvas_item;

uniform float threshhold = 1.;
uniform float contrast = 1.5;
uniform vec4 base_colour : hint_color = vec4(1.);

void fragment()
{
	vec4 viewport_sample = texture(TEXTURE, UV);
	if(viewport_sample.r < threshhold)
		discard;
	
	COLOR = vec4(base_colour.rgb, clamp(viewport_sample.r * contrast, 0., 1.));	
}