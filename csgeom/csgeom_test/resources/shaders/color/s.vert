#version 330 core

layout(location = 0) in vec3 in_position;
layout(location = 3) in vec3 in_color;

uniform mat4 hlg_model;
uniform mat4 hlg_mvp;

out vec3 inter_color;

void main() {
	gl_Position = hlg_mvp * vec4(in_position, 1);
	inter_color = in_color;
}