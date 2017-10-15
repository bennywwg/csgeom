using System;
using System.Collections.Generic;
using System.Linq;
using GlmSharp;
using OpenTK.Graphics.OpenGL;

namespace csgeom_test {
    
    public class model {
        public readonly indexBuffer indices;
        
        public readonly vbuffer positions;
        public readonly vbuffer normals;
        public readonly vbuffer uvs;
        public readonly vbuffer colors;
        public bool hasNormals => normals != null;
        public bool hasUVs => uvs != null;
        public bool hasColors => colors != null;
        public readonly vbuffer[] genericBuffers;
        public int bufferCount => 1 + (hasNormals ? 1 : 0) + (hasUVs ? 1 : 0) + (hasColors ? 1 : 0) + genericBuffers.Length;
        public vbuffer[] allBuffers {
            get {
                vbuffer[] res = new vbuffer[bufferCount];
                res[0] = positions;
                int current = 1;
                if(hasNormals) {
                    res[current] = normals;
                    current++;
                }
                if(hasUVs) {
                    res[current] = uvs;
                    current++;
                }
                if(hasColors) {
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

        public readonly texture albedo;
        public readonly texture bump;
        public bool hasAlbedo => albedo != null;
        public bool hasBump => bump != null;
        public readonly texture[] genericTextures;
        public int textureCount => (hasAlbedo ? 1 : 0) + (hasBump ? 1 : 0) + genericTextures.Length;
        public texture[] allTextures {
            get {
                texture[] res = new texture[textureCount];
                int current = 0;
                if(hasAlbedo) {
                    res[current] = albedo;
                    current++;
                }
                if(hasBump) {
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


        void genericDraw(PolygonMode mode) {
            indices.bind();
            foreach (texture tx in allTextures) tx.bind();
            foreach (vbuffer buf in allBuffers) buf.bindToAttrib();

            GL.PolygonMode(MaterialFace.FrontAndBack, mode);
            indices.drawElements();

            foreach (vbuffer buf in allBuffers) buf.unbindFromAttrib();
            foreach (texture tx in allTextures) tx.unbind();
            indices.unbind();
        }
        public void draw(PolygonMode mode = PolygonMode.Fill) {
            genericDraw(mode);
        }

        private model(indexBuffer indices, vbuffer[] buffers, texture[] textures) {
            this.indices = indices;

            List<vbuffer> otherBuffers = new List<vbuffer>();
            for (int i = 0; i < buffers.Length; i++) {
                vbuffer buf = buffers[i];
                if (buf != null) {//this makes life easier elsewhere
                    if (buf.isPositionAttrib) {
                        positions = buf;
                    } else if (buf.isNormalAttrib) {
                        normals = buf;
                    } else if (buf.isUVAttrib) {
                        uvs = buf;
                    } else if (buf.isColorAttrib) {
                        colors = buf;
                    } else {
                        otherBuffers.Add(buf);
                    }
                }
            }
            if (positions == null) throw new Exception("No position buffer");
            this.genericBuffers = otherBuffers.ToArray();

            List<texture> otherTextures = new List<texture>();
            for (int i = 0; i < textures.Length; i++) {
                texture tx = textures[i];
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

        public model(mesh m) : this(
            new indexBuffer(m.indexData),
            new vbuffer[] {
                vbuffer.positionAttrib(m.positionData),
                m.normalData != null ? vbuffer.normalAttrib(m.normalData) : null,
                m.uvData != null ? vbuffer.uvAttrib(m.uvData) : null,
                m.colorData != null ? vbuffer.colorAttrib(m.colorData) : null
            },
            new texture[] { }) {
        }

        public model(mesh m, texture tx) : this(
            new indexBuffer(m.indexData),
            new vbuffer[] {
                vbuffer.positionAttrib(m.positionData),
                m.normalData != null ? vbuffer.normalAttrib(m.normalData) : null,
                m.uvData != null ? vbuffer.uvAttrib(m.uvData) : null,
                m.colorData != null ? vbuffer.colorAttrib(m.colorData) : null
            },
            new texture[] {
                tx
            }) {
        }
        
        public static void streamDraw(mesh m, shader sh, texture tx, PolygonMode mode = PolygonMode.Fill) {
            model mv = new model(m, tx);
            sh.use();
            mv.draw(mode);
            mv.destroy();
        }

        public void destroy() {
            foreach (vbuffer buf in allBuffers) buf.destroy();
            indices.destroy();
        }
    }

    public static class modelExtensions {
        public static model generateModel(this mesh m, texture tx = null) {
            return new model(m, tx);
        }
    }
    
    public class renderPass {

        private shader main;
        //private framebuffer frame;
        private shader post;

        private mat4 _view;
        private mat4 _projection;

        public void beginPass(mat4 view, mat4 projection) {
            _view = view;
            _projection = projection;
            //main.setUniform("view", view);
            //main.setUniform("projection", projection);
            //main.setUniform("viewProjection", viewProjection);
        }

        public void enableDepth() {
            GL.Enable(EnableCap.DepthTest);
        }
        public void disableDepth() {
            GL.Disable(EnableCap.DepthTest);
        }

        public bool depthEnabled = true;

        public void drawModel(model m, mat4 model, PolygonMode mode = PolygonMode.Fill) {
            main.use();

            main.setUniform("hlg_model", model);
            //main.setUniform("hlg_mvp", _projection * _view * model);
            if(m.hasAlbedo) main.setUniform("hlg_albedo", m.albedo.ptr);

            if (depthEnabled) enableDepth(); else disableDepth();
            m.draw(mode);
        }

        public renderPass(shader main, int width, int height) : this(main, null, width, height) {
        }
        public renderPass(shader main, shader post, int width, int height) {
            this.main = main;
            this.post = post;

            //this.frame = new framebuffer(width, height);
        }
    }

    public static class hlg {
        public static void drawBox(vec2 pos, vec2 size, vec3 color) {

        }
    }

    public class multiPassCompositor {

    }
}
