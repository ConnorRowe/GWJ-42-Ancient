shader_type canvas_item;

uniform float clip :hint_range(0., 1.) = 0.;

void fragment()
{
	if(UV.y > clip)
		discard;
}