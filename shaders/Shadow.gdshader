// Adapted from https://inspirnathan.com/posts/48-shadertoy-tutorial-part-2/

shader_type canvas_item;

uniform vec4 shadow_colour : hint_color = vec4(0,0,0,.25);

vec4 sdfCircle(vec2 uv, float r) {
    float x = uv.x;
    float y = uv.y;
    
    float d = length(vec2(x, y)) - r;
    
    return d > 0. ? vec4(0) : shadow_colour;
}

void fragment()
{
	COLOR = sdfCircle(UV - .5, .49);
}