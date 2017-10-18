using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using GlmSharp;

namespace csgeom_test {
    public class Vao {
        private static int _ptr;

        public static void Initialize() {
            _ptr = GL.GenVertexArray();
            GL.BindVertexArray(_ptr);
        }

        public static void Destroy() {
            GL.DeleteVertexArray(_ptr);
        }
    }

    public static class VbufferStatus {
        public static Vbuffer currentBound;
        public static Dictionary<int, Vbuffer> currentEnabledLayouts = new Dictionary<int, Vbuffer>();
    }

    public class Vbuffer {
        public readonly int ptr;

        public readonly int layoutLocation;
        public readonly bool normalized;
        public readonly int elementSize;
        public readonly string elementType;
        public readonly int count;

        public bool IsPositionAttrib => layoutLocation == 0;
        public bool IsNormalAttrib => layoutLocation == 1;
        public bool IsUVAttrib => layoutLocation == 2;
        public bool IsColorAttrib => layoutLocation == 3;
        public bool IsGenericAttrib => layoutLocation > 3;

        static bool IsValidAndNotReservedLayout(int layoutLocation) {
            //0 = position
            //1 = normal
            //2 = uv
            //3 = color
            return layoutLocation > 3;
        }

        public void BindToAttrib() {
            Bind();
            GL.EnableVertexAttribArray(layoutLocation);
            VbufferStatus.currentEnabledLayouts[layoutLocation] = this;
            GL.VertexAttribPointer(layoutLocation, elementSize, VertexAttribPointerType.Float, normalized, 0, 0);
            Unbind();
        }
        public void UnbindFromAttrib() {
            VbufferStatus.currentEnabledLayouts.Remove(layoutLocation);
            GL.DisableVertexAttribArray(layoutLocation);
        }

        public void Bind() {
            VbufferStatus.currentBound = this;
            GL.BindBuffer(BufferTarget.ArrayBuffer, ptr);
        }
        public void Unbind() {
            Vbuffer.GlobalUnbind();
        }

        public static void GlobalUnbind() {
            VbufferStatus.currentBound = null;
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public static Vbuffer PositionAttrib(float[] data) {
            return new Vbuffer(0, 3, false, data);
        }
        public static Vbuffer NormalAttrib(float[] data) {
            return new Vbuffer(1, 3, true, data);
        }
        public static Vbuffer UVAttrib(float[] data) {
            return new Vbuffer(2, 2, false, data);
        }
        public static Vbuffer ColorAttrib(float[] data) {
            return new Vbuffer(3, 3, false, data);

        }
        public static Vbuffer FloatAttrib(int layoutLocation, float[] data) {
            if (!IsValidAndNotReservedLayout(layoutLocation)) throw new Exception("Can't use reserved layout location " + layoutLocation);
            return new Vbuffer(layoutLocation, 1, false, data);
        }
        public static Vbuffer Vec2Attrib(int layoutLocation, bool normalized, vec2[] data) {
            if (!IsValidAndNotReservedLayout(layoutLocation)) throw new Exception("Can't use reserved layout location " + layoutLocation);
            return new Vbuffer(layoutLocation, 2, normalized, data.RawData());
        }
        public static Vbuffer Vec3Attrib(int layoutLocation, bool normalized, vec3[] data) {
            if (!IsValidAndNotReservedLayout(layoutLocation)) throw new Exception("Can't use reserved layout location " + layoutLocation);
            return new Vbuffer(layoutLocation, 3, normalized, data.RawData());
        }
        public static Vbuffer IntAttrib(int layoutLocation, int[] data) {
            if (!IsValidAndNotReservedLayout(layoutLocation)) throw new Exception("Can't use reserved layout location " + layoutLocation);
            return new Vbuffer(layoutLocation, data);
        }

        private Vbuffer(int layoutLocation, int elementSize, bool normalized, float[] data) {
            this.layoutLocation = layoutLocation;
            this.normalized = normalized;
            this.elementSize = elementSize;
            this.elementType = (elementSize == 1) ? "float" : ("vec" + elementSize);
            this.count = data.Length / elementSize;
            ptr = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, ptr);
            GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, BufferUsageHint.StaticDraw);
        }
        private Vbuffer(int layoutLocation, int[] data) {
            this.layoutLocation = layoutLocation;
            this.normalized = false;
            this.elementSize = 1;
            this.elementType = "int";
            this.count = data.Length;
            ptr = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, ptr);
            GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(int), data, BufferUsageHint.StaticDraw);
        }

        public void Destroy() {
            GL.DeleteBuffer(ptr);
        }

        public override string ToString() {
            return elementType + " buffer, attrib = " + layoutLocation + ", " + count + " elements";
        }
    }

    public static class IndexBufferStatus {
        public static IndexBuffer currentBound;
    }

    public class IndexBuffer {
        readonly int ptr;

        public readonly int count;

        public void Bind() {
            IndexBufferStatus.currentBound = this;
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ptr);
        }
        public void Unbind() {
            GlobalUnbind();
        }

        public static void GlobalUnbind() {
            IndexBufferStatus.currentBound = null;
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        /// <summary>
        ///     This won't do anything if you haven't bound the actual data buffers
        /// </summary>
        public void DrawElements() {
            GL.DrawElements(PrimitiveType.Triangles, count, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }

        public IndexBuffer(int[] data) {
            ptr = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ptr);
            GL.BufferData(BufferTarget.ElementArrayBuffer, data.Length * sizeof(int), data, BufferUsageHint.StaticDraw);
            count = data.Length;
        }

        public void Destroy() {
            GL.DeleteBuffer(ptr);
        }
    }
}
