#version 440 core

in vec3 normal;
in vec2 uv;

out vec3 color;

uniform sampler2D texSampler;

void main(){
	color = texture(texSampler, uv).rgb;
}