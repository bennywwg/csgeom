﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GlmSharp;
using csgeom;
using OpenTK.Input;

namespace csgeom_test {
    class HUDGeom : HUDItem {

        public WeaklySimplePolygon poly;

        public bool Dragging { get; private set; }
        public vec3 LastCursor { get; private set; }

        TriangulationResult2 resultCache;
        Model solidCache;
        Model outlineCache;

        HUDRect ClearButton;
        HUDRect TransformTools;
        HUDRect GeomInfo;
        HUDRect ToggleSolid;
        HUDRect GeomResultInfo;

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
            Random r = new Random(5);

            if (solidCache != null && RenderSolid) {
                Program.g.DrawModel(solidCache, mat4.Identity, Program.colorShader);
            }
            if(outlineCache != null) {
                Program.g.DrawModel(outlineCache, mat4.Identity, Program.colorShader, OpenTK.Graphics.OpenGL.PolygonMode.Line, 4);
            }
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

        void UpdateModels() {
            DestroyAndClearModels();

            resultCache = poly.verts.Triangulate();


            if (resultCache.data != null) {
                Mesh tri = new Mesh(MeshComponent.colors);
                tri.Triangles(resultCache.data.Select(ts => new vec2[] { ts.v0.glm(), ts.v1.glm(), ts.v2.glm() }).ToList(), Util.Color("#cee3f8"));
                Model m = new Model(tri);
                m.Destroy();

                if (solidCache != null) {
                    solidCache.Destroy();
                }
                solidCache = new Model(tri);
            }

            List<vec2[]> lines = new List<vec2[]>();
            for (int i = 0; i < poly.verts.Count; i++) {
                lines.Add(new vec2[] { poly.verts[i].glm(), poly.verts[i].glm(), poly.verts[(i + 1) % poly.verts.Count].glm() });
            }
            Mesh l = new Mesh(MeshComponent.colors);
            l.Triangles(lines, Util.Color(resultCache.data != null ? "#5f99cf" : "#ff99cf"));

            if(outlineCache != null) {
                outlineCache.Destroy();
            }
            outlineCache = new Model(l);
        }
        void DestroyAndClearModels() {
            if(solidCache != null) {
                solidCache.Destroy();
                solidCache = null;
            }
            if(outlineCache != null) {
                outlineCache.Destroy();
                outlineCache = null;
            }
        }

        public override void DoMouseDown(MouseButtonEventArgs bu) {
            if (Root.Hovered == Root) {
                if (bu.Button == MouseButton.Left) {
                    Dragging = true;
                    vec3 pos = new vec3();

                    if (CastCursor(ref pos)) {
                        LastCursor = pos;
                        poly.verts.Add(new vec2(pos).csgeom());
                        UpdateModels();
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
                        poly.verts.Add(new vec2(pos).csgeom());
                        UpdateModels();
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
            poly = new WeaklySimplePolygon();
            poly.verts.Add(new gvec2(0, 0));
            poly.verts.Add(new gvec2(1, 0));
            poly.verts.Add(new gvec2(2, 0));
            poly.verts.Add(new gvec2(3, 0));
            poly.verts.Add(new gvec2(3, 1));
            poly.verts.Add(new gvec2(0, 1));

            UpdateModels();



            ClearButton = new HUDRect("Clear", this) {
                Text = "Clear",
                LocalPos = new vec2(-1, -1),
                Color = Util.Gray(0.1f),
                MouseDown = (item, args) => { if (Root.Hovered == item) { poly.verts.Clear(); DestroyAndClearModels(); } }
            };

            GeomInfo = new HUDRect("Info", this) {
                Text = "Info",
                LocalPos = new vec2(-1, -1 + TextUnit),
                Color = Util.Gray(0.1f),
            };

            ToggleSolid = new HUDRect("Toggle Solid", this) {
                Text = "Solid",
                LocalPos = new vec2(-1, -1 + TextUnit * 2),
                Color = Util.Gray(0.1f),
                MouseDown = (item, args) => {
                    if (Root.Hovered == item)
                        RenderSolid = !RenderSolid;
                },
                DynamicUpdate = (item, args) => {
                    if (resultCache.code != TriangulationCode.operationSuccess) {
                        item.Color = Root.Hovered != item ? new vec3(0.5f, 0.3f, 0.3f) : new vec3(0.4f, 0.3f, 0.3f);
                        item.Text = "Failure";
                    } else {
                        if (Root.Hovered != item) {
                            item.Color = RenderSolid ? new vec3(0.3f, 0.5f, 0.3f) : new vec3(0.7f, 0.5f, 0.5f);
                        } else {
                            item.Color = RenderSolid ? new vec3(0.3f, 0.7f, 0.3f) : new vec3(1f, 0.5f, 0.5f);
                        }
                        item.Text = RenderSolid ? "Solid" : "Outline";
                    }
                },
                
            };

            GeomResultInfo = new HUDRect("Result Info", this) {
                Text = "Solid",
                LocalPos = new vec2(-1, -1 + TextUnit * 3),
                Color = Util.Gray(0.1f),
                MouseDown = (item, args) => {
                    if (Root.Hovered == item) {

                        WeaklySimplePolygon p1 = new WeaklySimplePolygon();
                        p1.verts.Add(new gvec2(0, 0));
                        p1.verts.Add(new gvec2(1, 0));
                        p1.verts.Add(new gvec2(1, 1));
                        p1.verts.Add(new gvec2(0, 1));
                        p1.verts.Transform(dmat4.Translate(-0.5, -0.5, 0));

                        poly = poly.Union(p1);

                        Console.WriteLine("abc");


                        //poly.verts.Transform(dmat4.RotateZ(Math.PI * 0.1f));
                        UpdateModels();
                    }
                },
                DynamicUpdate = (item, args) => {
                    item.Text = resultCache.code.ToString();
                },
            };
        }
    }
}
