shader_type canvas_item;

uniform bool IsFlashing = false;
uniform vec4 FlashColor = vec4(1.0, 1.0, 1.0, 1.0); // white

void fragment()
{
	vec4 result = texture(TEXTURE, UV);
	
	if(IsFlashing)
	{
		result = FlashColor * result.a;
	}	
	
	COLOR = result;
}

