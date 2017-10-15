using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using GlmSharp;

namespace csgeom_test {
    public class vao {
        private static int _ptr;

        public static void initialize() {
            _ptr = GL.GenVertexArray();
            GL.BindVertexArray(_ptr);
        }

        public static void destroy() {
            GL.DeleteVertexArray(_ptr);
        }
    }

    public static class vbufferStatus {
        public static vbuffer currentBound;
        public static Dictionary<int, vbuffer> currentEnabledLayouts = new Dictionary<int, vbuffer>();
    }

    public class vbuffer {
        public readonly int ptr;

        public readonly int layoutLocation;
        public readonly bool normalized;
        public readonly int elementSize;
        public readonly string elementType;
        public readonly int count;

        public bool isPositionAttrib => layoutLocation == 0;
        public bool isNormalAttrib => layoutLocation == 1;
        public bool isUVAttrib => layoutLocation == 2;
        public bool isColorAttrib => layoutLocation == 3;
        public bool isGenericAttrib => layoutLocation > 3;

        static bool isValidAndNotReservedLayout(int layoutLocation) {
            //0 = position
            //1 = normal
            //2 = uv
            //3 = color
            return layoutLocation > 3;
        }

        public void bindToAttrib() {
            bind();
            GL.EnableVertexAttribArray(layoutLocation);
            vbufferStatus.currentEnabledLayouts[layoutLocation] = this;
            GL.VertexAttribPointer(layoutLocation, elementSize, VertexAttribPointerType.Float, normalized, 0, 0);
            unbind();
        }
        public void unbindFromAttrib() {
            vbufferStatus.currentEnabledLayouts.Remove(layoutLocation);
            GL.DisableVertexAttribArray(layoutLocation);
        }

        public void bind() {
            vbufferStatus.currentBound = this;
            GL.BindBuffer(BufferTarget.ArrayBuffer, ptr);
        }
        public void unbind() {
            vbuffer.globalUnbind();
        }

        public static void globalUnbind() {
            vbufferStatus.currentBound = null;
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public static vbuffer positionAttrib(float[] data) {
            return new vbuffer(0, 3, false, data);
        }
        public static vbuffer normalAttrib(float[] data) {
            return new vbuffer(1, 3, true, data);
        }
        public static vbuffer uvAttrib(float[] data) {
            return new vbuffer(2, 2, false, data);
        }
        public static vbuffer colorAttrib(float[] data) {
            return new vbuffer(3, 3, false, data);

        }
        public static vbuffer floatAttrib(int layoutLocation, float[] data) {
            if (!isValidAndNotReservedLayout(layoutLocation)) throw new Exception("Can't use reserved layout location " + layoutLocation);
            return new vbuffer(layoutLocation, 1, false, data);
        }
        public static vbuffer vec2Attrib(int layoutLocation, bool normalized, vec2[] data) {
            if (!isValidAndNotReservedLayout(layoutLocation)) throw new Exception("Can't use reserved layout location " + layoutLocation);
            return new vbuffer(layoutLocation, 2, normalized, data.rawData());
        }
        public static vbuffer vec3Attrib(int layoutLocation, bool normalized, vec3[] data) {
            if (!isValidAndNotReservedLayout(layoutLocation)) throw new Exception("Can't use reserved layout location " + layoutLocation);
            return new vbuffer(layoutLocation, 3, normalized, data.rawData());
        }
        public static vbuffer intAttrib(int layoutLocation, int[] data) {
            if (!isValidAndNotReservedLayout(layoutLocation)) throw new Exception("Can't use reserved layout location " + layoutLocation);
            return new vbuffer(layoutLocation, data);
        }

        private vbuffer(int layoutLocation, int elementSize, bool normalized, float[] data) {
            this.layoutLocation = layoutLocation;
            this.normalized = normalized;
            this.elementSize = elementSize;
            this.elementType = (elementSize == 1) ? "float" : ("vec" + elementSize);
            this.count = data.Length / elementSize;
            ptr = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, ptr);
            GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, BufferUsageHint.StaticDraw);
        }
        private vbuffer(int layoutLocation, int[] data) {
            this.layoutLocation = layoutLocation;
            this.normalized = false;
            this.elementSize = 1;
            this.elementType = "int";
            this.count = data.Length;
            ptr = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, ptr);
            GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(int), data, BufferUsageHint.StaticDraw);
        }

        public void destroy() {
            GL.DeleteBuffer(ptr);
        }

        public override string ToString() {
            return elementType + " buffer, attrib = " + layoutLocation + ", " + count + " elements";
        }
    }

    public static class indexBufferStatus {
        public static indexBuffer currentBound;
    }

    public class indexBuffer {
        readonly int ptr;

        public readonly int count;

        public void bind() {
            indexBufferStatus.currentBound = this;
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ptr);
        }
        public void unbind() {
            globalUnbind();
        }

        public static void globalUnbind() {
            indexBufferStatus.currentBound = null;
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        /// <summary>
        ///     This won't do anything if you haven't bound the actual data buffers
        /// </summary>
        public void drawElements() {
            GL.DrawElements(PrimitiveType.Triangles, count, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }

        public indexBuffer(int[] data) {
            ptr = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ptr);
            GL.BufferData(BufferTarget.ElementArrayBuffer, data.Length * sizeof(int), data, BufferUsageHint.StaticDraw);
            count = data.Length;
        }

        public void destroy() {
            GL.DeleteBuffer(ptr);
        }
    }
}
