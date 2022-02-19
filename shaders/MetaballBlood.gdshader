shader_type canvas_item;

uniform float threshhold = 1.;
uniform float contrast = 1.5;
uniform float border_threshhold = .05f;
uniform vec4 border_colour : hint_color = vec4(0.,0.,0.,1.);

void fragment()
{
	vec4 viewport_sample = texture(TEXTURE, UV);
	
	if(viewport_sample.a < threshhold - border_threshhold)
		discard;
		
	if(viewport_sample.a > threshhold)
		COLOR = vec4(viewport_sample.rgb, clamp(viewport_sample.a * contrast, 0., 1.));	
	else
		COLOR = border_colour;
}