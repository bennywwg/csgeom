﻿using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using GlmSharp;

namespace csgeom_test {
    public class shader {
        private int _ptr;
        public int ptr => _ptr;

        private Dictionary<string, int> _uniformMap;

        private void loadFile(string vertPath, string fragPath) {
            //load source
            string vertexCode = System.IO.File.ReadAllText(vertPath).Replace("\r\n\r", "\n");
            string fragmentCode = System.IO.File.ReadAllText(fragPath).Replace("\r\n\r", "\n");

            //create shaders
            int vertexPtr = GL.CreateShader(ShaderType.VertexShader);
            int fragmentPtr = GL.CreateShader(ShaderType.FragmentShader);

            //assign and compile shaders
            GL.ShaderSource(vertexPtr, vertexCode);
            GL.ShaderSource(fragmentPtr, fragmentCode);
            GL.CompileShader(vertexPtr);
            GL.CompileShader(fragmentPtr);
            string vertexLog = GL.GetShaderInfoLog(vertexPtr);
            string fragmentLog = GL.GetShaderInfoLog(fragmentPtr);

            //attach and link program
            _ptr = GL.CreateProgram();
            GL.AttachShader(_ptr, vertexPtr);
            GL.AttachShader(_ptr, fragmentPtr);
            GL.LinkProgram(_ptr);

            string programLog = GL.GetProgramInfoLog(3);

            //clean up
            GL.DetachShader(_ptr, vertexPtr);
            GL.DetachShader(_ptr, fragmentPtr);

            GL.DeleteShader(vertexPtr);
            GL.DeleteShader(fragmentPtr);
        }

        public void use() {
            GL.UseProgram(ptr);
        }

        public int uniformLocation(string name) {
            if (_uniformMap.ContainsKey(name)) {
                return _uniformMap[name];
            } else {
                int index = GL.GetUniformLocation(_ptr, name);
                _uniformMap.Add(name, index);
                return index;
            }
        }

        public bool hasUniform(string name) {
            return uniformLocation(name) != -1;
        }

        public void setUniform(string name, int val) {
            int index = uniformLocation(name);
            if (index != -1) GL.ProgramUniform1(_ptr, index, val);
        }
        public void setUniform(string name, float val) {
            int index = uniformLocation(name);
            if (index != -1) GL.ProgramUniform1(_ptr, index, val);
        }
        public void setUniform(string name, vec2 val) {
            int index = uniformLocation(name);
            if (index != -1) GL.ProgramUniform2(_ptr, index, val.x, val.y);
        }
        public void setUniform(string name, vec3 val) {
            int index = uniformLocation(name);
            if (index != -1) GL.ProgramUniform3(_ptr, index, val.x, val.y, val.z);
        }
        public void setUniform(string name, mat4 val) {
            int index = uniformLocation(name);
            if (index != -1) GL.ProgramUniformMatrix4(ptr, index, 1, false, val.Values1D);
        }

        public void destroy() {
            GL.DeleteProgram(ptr);
        }

        public shader(string vertPath, string fragPath) {
            _uniformMap = new Dictionary<string, int>();
            loadFile(vertPath, fragPath);
        }
    }
}
