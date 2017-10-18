using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace csgeom_test {
    public static class textureStatus {
        public static Dictionary<TextureUnit, Texture> currentBound = new Dictionary<TextureUnit, Texture>();
    }

    public class Texture {
        public readonly int ptr;

        public readonly TextureUnit textureUnit;

        public bool isAlbedo => textureUnit == getTextureUnit(0);
        public bool isBump => textureUnit == getTextureUnit(1);


        static TextureUnit getTextureUnit(int textureUnit) {
            return TextureUnit.Texture0 + textureUnit;
        }
        static bool isValidAndNotReservedUnit(int textureUnit) {
            //0 = albedo
            //1 = bump
            //2 = tangent (maybe not needed, never used normal mapping before)
            //3 = specular
            //4 = anisotropy (also mabye not needed)
            return textureUnit > 4;
        }

        public void bind() {
            textureStatus.currentBound[textureUnit] = this;
            GL.ActiveTexture(textureUnit);
            GL.BindTexture(TextureTarget.Texture2D, ptr);
            GL.ActiveTexture(TextureUnit.Texture0);
        }

        public void unbind() {
            textureStatus.currentBound.Remove(textureUnit);
            GL.ActiveTexture(textureUnit);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
        }

        public void globalUnbind() {
            Texture[] allTextures = textureStatus.currentBound.Select(kvp => kvp.Value).ToArray();
            foreach (Texture tx in allTextures) {
                tx.unbind();
            }
        }

        private Texture(int width, int height, TextureUnit textureUnit) {
            this.textureUnit = textureUnit;

            ptr = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, ptr);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, width, height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        }

        private Texture(string path, TextureUnit textureUnit) {
            this.textureUnit = textureUnit;

            Image img = Image.FromFile(path);

            if (!(img.Width.powerOf2() && img.Height.powerOf2())) {
                throw new Exception("Image must be power of 2 sized");
            }

            ptr = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, ptr);

            Bitmap bmp = new Bitmap(img);
            var raw = bmp.LockBits(new Rectangle(0, 0, img.Width, img.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, img.Width, img.Height, 0, PixelFormat.Bgr, PixelType.UnsignedByte, raw.Scan0);
            bmp.UnlockBits(raw);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        }

        public static Texture color(string path) {
            return new Texture(path, getTextureUnit(0));
        }
        public static Texture generic(string path, int textureUnit) {
            if (!isValidAndNotReservedUnit(textureUnit)) throw new Exception("Can't use reserved texture unit " + textureUnit);
            return new Texture(path, getTextureUnit(textureUnit));
        }
        public static Texture empty(int width, int height, int textureUnit) {
            return new Texture(width, height, getTextureUnit(textureUnit));
        }

        public void destroy() {
            GL.DeleteTexture(ptr);
        }
    }

    public class framebuffer {
        private int _fbptr;
        private int _dbptr;

        public int width { get; private set; }
        public int height { get; private set; }

        private Texture tx;

        public void bind() {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbptr);
            GL.Viewport(0, 0, width, height);
        }

        public framebuffer(int width, int height) {
            this.width = width;
            this.height = height;

            tx = Texture.empty(width, height, 8);

            _fbptr = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbptr);

            _dbptr = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _dbptr);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, width, height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _dbptr);

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, tx.ptr, 0);

            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
        }

        public void destroy() {
            GL.DeleteRenderbuffer(_dbptr);
            GL.DeleteFramebuffer(_fbptr);
            tx.destroy();
        }
    }
}
