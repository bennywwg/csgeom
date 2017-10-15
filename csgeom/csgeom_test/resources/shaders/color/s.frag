#version 440 core

in vec3 inter_color;
out vec3 color;

void main(){
	color = inter_color;
}