using System;
using System.Collections.Generic;
using System.Linq;
using GlmSharp;
using OpenTK.Graphics.OpenGL;

namespace csgeom_test {
    
    public class Model {
        public readonly IndexBuffer indices;
        
        public readonly Vbuffer positions;
        public readonly Vbuffer normals;
        public readonly Vbuffer uvs;
        public readonly Vbuffer colors;
        public bool HasNormals => normals != null;
        public bool HasUVs => uvs != null;
        public bool HasColors => colors != null;
        public readonly Vbuffer[] genericBuffers;
        public int BufferCount => 1 + (HasNormals ? 1 : 0) + (HasUVs ? 1 : 0) + (HasColors ? 1 : 0) + genericBuffers.Length;
        public Vbuffer[] AllBuffers {
            get {
                Vbuffer[] res = new Vbuffer[BufferCount];
                res[0] = positions;
                int current = 1;
                if(HasNormals) {
                    res[current] = normals;
                    current++;
                }
                if(HasUVs) {
                    res[current] = uvs;
                    current++;
                }
                if(HasColors) {
                    res[current] = colors;
                    current++;
                }
                for (int i = 0; i < genericBuffers.Length; i++) {
                    res[current] = genericBuffers[i];
                    current++;
                }
                return res;
            }
        }

        public readonly Texture albedo;
        public readonly Texture bump;
        public bool HasAlbedo => albedo != null;
        public bool HasBump => bump != null;
        public readonly Texture[] genericTextures;
        public int TextureCount => (HasAlbedo ? 1 : 0) + (HasBump ? 1 : 0) + genericTextures.Length;
        public Texture[] AllTextures {
            get {
                Texture[] res = new Texture[TextureCount];
                int current = 0;
                if(HasAlbedo) {
                    res[current] = albedo;
                    current++;
                }
                if(HasBump) {
                    res[current] = bump;
                    current++;
                }
                for (int i = 0; i < genericTextures.Length; i++) {
                    res[current] = genericTextures[i];
                    current++;
                }
                return res;
            }
        }


        void GenericDraw(PolygonMode mode, float lineWidth = 1) {
            indices.Bind();
            foreach (Texture tx in AllTextures) tx.bind();
            foreach (Vbuffer buf in AllBuffers) buf.BindToAttrib();

            GL.LineWidth(lineWidth);

            GL.PolygonMode(MaterialFace.FrontAndBack, mode);
            indices.DrawElements();

            foreach (Vbuffer buf in AllBuffers) buf.UnbindFromAttrib();
            foreach (Texture tx in AllTextures) tx.unbind();
            indices.Unbind();
        }
        public void Draw(PolygonMode mode = PolygonMode.Fill, float lineWidth = 1) {
            GenericDraw(mode, lineWidth);
        }

        private Model(IndexBuffer indices, Vbuffer[] buffers, Texture[] textures) {
            this.indices = indices;

            List<Vbuffer> otherBuffers = new List<Vbuffer>();
            for (int i = 0; i < buffers.Length; i++) {
                Vbuffer buf = buffers[i];
                if (buf != null) {//this makes life easier elsewhere
                    if (buf.IsPositionAttrib) {
                        positions = buf;
                    } else if (buf.IsNormalAttrib) {
                        normals = buf;
                    } else if (buf.IsUVAttrib) {
                        uvs = buf;
                    } else if (buf.IsColorAttrib) {
                        colors = buf;
                    } else {
                        otherBuffers.Add(buf);
                    }
                }
            }
            if (positions == null) throw new Exception("No position buffer");
            this.genericBuffers = otherBuffers.ToArray();

            List<Texture> otherTextures = new List<Texture>();
            for (int i = 0; i < textures.Length; i++) {
                Texture tx = textures[i];
                if (tx != null) {
                    if (tx.isAlbedo) {
                        albedo = tx;
                    } else if (tx.isBump) {
                        bump = tx;
                    } else {
                        otherTextures.Add(tx);
                    }
                }
            }
            this.genericTextures = otherTextures.ToArray();
        }

        public Model(Mesh m) : this(
            new IndexBuffer(m.IndexData),
            new Vbuffer[] {
                Vbuffer.PositionAttrib(m.PositionData),
                m.NormalData != null ? Vbuffer.NormalAttrib(m.NormalData) : null,
                m.UVData != null ? Vbuffer.UVAttrib(m.UVData) : null,
                m.ColorData != null ? Vbuffer.ColorAttrib(m.ColorData) : null
            },
            new Texture[] { }) {
        }

        public Model(Mesh m, Texture tx) : this(
            new IndexBuffer(m.IndexData),
            new Vbuffer[] {
                Vbuffer.PositionAttrib(m.PositionData),
                m.NormalData != null ? Vbuffer.NormalAttrib(m.NormalData) : null,
                m.UVData != null ? Vbuffer.UVAttrib(m.UVData) : null,
                m.ColorData != null ? Vbuffer.ColorAttrib(m.ColorData) : null
            },
            new Texture[] {
                tx
            }) {
        }
        
        public static void StreamDraw(Mesh m, Shader sh, Texture tx, PolygonMode mode = PolygonMode.Fill) {
            Model mv = new Model(m, tx);
            sh.use();
            mv.Draw(mode);
            mv.Destroy();
        }

        public void Destroy() {
            foreach (Vbuffer buf in AllBuffers) buf.Destroy();
            indices.Destroy();
        }
    }

    public static class ModelExtensions {
        public static Model GenerateModel(this Mesh m, Texture tx = null) {
            return new Model(m, tx);
        }
    }
    
    public class RenderPass {

        private Shader main;
        //private framebuffer frame;
        private Shader post;

        private mat4 _view;
        private mat4 _projection;

        public void BeginPass(mat4 view, mat4 projection) {
            _view = view;
            _projection = projection;
            //main.setUniform("view", view);
            //main.setUniform("projection", projection);
            //main.setUniform("viewProjection", viewProjection);
        }

        public void EnableDepth() {
            GL.Enable(EnableCap.DepthTest);
        }
        public void DisableDepth() {
            GL.Disable(EnableCap.DepthTest);
        }

        public bool depthEnabled = true;

        public void DrawModel(Model m, mat4 model, PolygonMode mode = PolygonMode.Fill, float lineWidth = 1) {
            main.use();

            float time = (float)(DateTime.UtcNow - DateTime.Today).TotalSeconds * 3.0f;

            main.setUniform("time", time);

            main.setUniform("hlg_model", model);
            //main.setUniform("hlg_mvp", _projection * _view * model);
            if(m.HasAlbedo) main.setUniform("hlg_albedo", m.albedo.ptr);

            if (depthEnabled) EnableDepth(); else DisableDepth();
            m.Draw(mode, lineWidth);
        }

        public RenderPass(Shader main, int width, int height) : this(main, null, width, height) {
        }
        public RenderPass(Shader main, Shader post, int width, int height) {
            this.main = main;
            this.post = post;

            //this.frame = new framebuffer(width, height);
        }
    }

    public static class HLG {
        public static void DrawBox(vec2 pos, vec2 size, vec3 color) {

        }
    }

    public class MultiPassCompositor {

    }
}
