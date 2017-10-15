using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmSharp;

namespace csgeom_test {
    public class hlg_font {
        public static hlg_font consolas;

        texture tx;
        shader s;

        renderPass pass;

        public string name;
        public readonly string path;
        public readonly float charWidth;

        public void drawText(string text, float x, float y, float scale) {
            model m = new model(mesh.text(text, 0, 0, scale, charWidth), tx);
            pass.drawModel(m, mat4.Translate(new vec3(x, y, 0)));
            m.destroy();
        }

        public hlg_font(string fontPath, string vertPath, string fragPath, float charWidth = 0.5f) {
            tx = texture.color(fontPath);
            s = new shader(vertPath, fragPath);
            pass = new renderPass(s, 0, 0);
            this.charWidth = charWidth;
            this.path = fontPath;
        }

        public void destroy() {
            tx.destroy();
            s.destroy();
        }
    }
}
