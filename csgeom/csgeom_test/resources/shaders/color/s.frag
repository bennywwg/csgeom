#version 440 core

in vec3 inter_color;
out vec3 color;

uniform float time;

void main(){
	if(length(inter_color.xy) < 0.01f)
		color = vec3(
			cos(inter_color.z + time + 3.1415968f * 2.0f / 3.0f) * 0.5f + 0.5f,
			cos(inter_color.z + time + 3.1415968f * 4.0f / 3.0f) * 0.5f + 0.5f,
			cos(inter_color.z + time + 3.1415968f * 6.0f / 3.0f) * 0.5f + 0.5f
		);
	else
		color = inter_color;
}