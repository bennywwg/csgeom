using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GlmSharp;
using csgeom;
using OpenTK.Input;

namespace csgeom_test {
    class HUDGeom : HUDItem {

        public LineLoop2 loop;

        public bool Dragging { get; private set; }
        public vec3 LastCursor { get; private set; }

        void DrawCursor(RenderPass g) {
            vec3 pos = new vec3();

            if (CastCursor(ref pos)) {
                Model m = new Model(Mesh.ColoredRectangle(new vec2(0.05f, 0.05f), new vec3(0, 0, 1)));

                g.DrawModel(m, mat4.Translate(pos - new vec3(0.025f, 0.025f, 0)), Program.colorShader);

                m.Destroy();
            }
        }

        void DrawGeom(RenderPass g) {
            Random r = new Random(5);

            TriangulationResult2 res = loop.Triangulate();

            if (res.data != null) {
                Mesh tri = new Mesh(MeshComponent.colors);
                tri.RandomColoredTriangles(res.data.Select(ts => new vec2[] { ts.v0.glm(), ts.v1.glm(), ts.v2.glm() }).ToList());
                Model m = new Model(tri);
                g.DrawModel(m, mat4.Identity, Program.colorShader);
                m.Destroy();
            }
        }

        bool CastCursor(ref vec3 res) {
            vec3 pos = new vec3();

            if (util.Plane.CCW(vec3.Zero, new vec3(1, 0, 0), new vec3(0, 1, 0)).ConstrainedIntersection(Program.cam.getNearPoint(Program.win.Mouse), Program.cam.getFarPoint(Program.win.Mouse), ref pos)) {
                res = pos;
                return true;
            } else {
                return false;
            }
        }

        public override void DoMouseDown(MouseButtonEventArgs bu) {
            if (bu.Button == MouseButton.Left) {
                Dragging = true;
                vec3 pos = new vec3();

                if (CastCursor(ref pos)) {
                    LastCursor = pos;
                }
            }
        }

        public override void DoMouseUp(MouseButtonEventArgs bu) {
            if (bu.Button == MouseButton.Left) {
                Dragging = false;
            }
        }

        public override void Update(float deltaT) {
            if (Dragging) {
                vec3 pos = new vec3();

                if (CastCursor(ref pos)) {
                    if ((LastCursor - pos).Length > 0.05f) {
                        loop.Add(new vec2(pos).csgeom());
                        LastCursor = pos;
                    }
                }
            }
        }

        public override void DoDraw(RenderPass g) {
            DrawCursor(g);

            DrawGeom(g);
            
        }

        public override void DoKeyDown(KeyboardKeyEventArgs args) {
            if (args.Key == Key.Q) loop.Clear();
        }

        public HUDGeom(string name, HUDItem parent) : base(name, parent) {
            loop = new LineLoop2();
            loop.Add(new gvec2(0, 0));
            loop.Add(new gvec2(1, 0));
            loop.Add(new gvec2(2, 0));
            loop.Add(new gvec2(3, 0));
            loop.Add(new gvec2(3, 1));
            loop.Add(new gvec2(0, 1));
        }
    }
}
