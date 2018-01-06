#version 440 core

in vec3 normal;
in vec2 uv;

out vec4 color;

uniform sampler2D texSampler;

void main(){
	vec3 outColor = texture(texSampler, uv).rgb;
	if(outColor == vec3(0,0,0)) discard;
	color = vec4(outColor, length(outColor));
}