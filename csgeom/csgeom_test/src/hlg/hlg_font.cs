using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmSharp;

namespace csgeom_test {
    public class HLGfont {
        Texture tx;
        Shader s;

        RenderPass pass;

        public string name;
        public readonly string path;
        public readonly float charWidth;

        string _status;
        public string Status => _status;

        public void drawText(string text, float x, float y, float scale) {
            Model m = new Model(Mesh.Text(text, 0, 0, scale, charWidth), tx);
            pass.DrawModel(m, mat4.Translate(new vec3(x, y, 0)));
            m.Destroy();
        }

        public HLGfont(string fontPath, string vertPath, string fragPath, float charWidth = 0.5f) {
            tx = Texture.color(fontPath);
            s = new Shader(vertPath, fragPath);
            pass = new RenderPass(s, 0, 0);
            this.charWidth = charWidth;
            this.path = fontPath;
        }

        public void destroy() {
            tx.destroy();
            s.destroy();
        }
    }
}
