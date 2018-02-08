using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GlmSharp;
using CSGeom;
using CSGeom.D2;
using OpenTK.Input;

namespace csgeom_test {
    class HUDGeom : HUDItem {
        public Polygon poly;
        public WeaklySimplePolygon other;

        Loop currentLoop = new Loop();

        public bool Dragging { get; private set; }
        public vec3 LastCursor { get; private set; }

        public bool RenderSolid = true;

        void DrawCursor() {
            vec3 pos = new vec3();

            if (CastCursor(ref pos)) {
                Model m = new Model(Mesh.ColoredRectangle(new vec2(0.05f, 0.05f), new vec3(0, 0, 1)));

                Program.g.DrawModel(m, mat4.Translate(pos - new vec3(0.025f, 0.025f, 0)), Program.colorShader);

                m.Destroy();
            }
        }

        void DrawGeom() {
           
        }

        bool CastCursor(ref vec3 res) {
            vec3 pos = new vec3();

            if (Util.Plane.CCW(vec3.Zero, new vec3(1, 0, 0), new vec3(0, 1, 0)).ConstrainedIntersection(Program.cam.getNearPoint(Program.win.Mouse), Program.cam.getFarPoint(Program.win.Mouse), ref pos)) {
                res = pos;
                return true;
            } else {
                return false;
            }
        }
        

        public override bool Hitbox(vec2 point) {
            
            return false;
            //return poly.verts.IsInside(point.csgeom());
        }

        public override void DoMouseDown(MouseButtonEventArgs bu) {
            Console.WriteLine("abc");
            if (Root.Hovered == Root) {
                if (bu.Button == MouseButton.Left) {
                    Dragging = true;
                    vec3 pos = new vec3();

                    if (CastCursor(ref pos)) {
                        LastCursor = pos;
                        currentLoop.Add(new vec2(pos).csgeom());
                    }
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
                    if ((LastCursor - pos).Length > 0.1f) {
                        currentLoop.Add(new vec2(pos).csgeom());
                        LastCursor = pos;
                    }
                }
            }
        }

        public override void DoDraw() {
            DrawCursor();



            DrawGeom();
        }

        public HUDGeom(string name, HUDItem parent) : base(name, parent) {
            new HUDRect("test", this) {
                Text = "TEST",
                Color = Util.Color(100, 100, 100),
                mode = AlignMode.leftAlign | AlignMode.topAlign,
                LocalPos = new vec2(-1, 1),
                MouseDown = (self, args) => { if(self.Root.Hovered == self) self.Text = self.Text + "1"; }
            };
        }
    }
}
