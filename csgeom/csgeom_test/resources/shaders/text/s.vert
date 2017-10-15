#version 440 core

layout(location = 0) in vec3 in_position;
layout(location = 1) in vec3 in_normal;
layout(location = 2) in vec2 in_uv;

uniform mat4 hlg_model;

out vec3 normal;
out vec2 uv;

void main() {
	gl_Position = hlg_model * vec4(in_position, 1);
	uv = in_uv;
	normal = in_normal;
}