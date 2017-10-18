using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using GlmSharp;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;

namespace csgeom_test {
    public static class Program {
        public static Window win;

        public static RenderPass ui;

        public static HLGfont consolas;

        public static Shader colorShader;
        public static Shader shadeless;

        public static HUDBase hud;


        public static void Initialize() {
            try {
                win = new Window(900, 900, "Circles");
                Console.WriteLine("Window initialized with OpenGL version 3.3");
            } catch {
                Console.WriteLine("Window failed to initialize");
            }

            Vao.Initialize();

            try {
                consolas = new HLGfont("../../resources/consolas.png", "../../resources/shaders/text/s.vert", "../../resources/shaders/text/s.frag") {
                    name = "Consolas"
                };
                Console.WriteLine("Consolas font loaded from consolas.png");
            } catch {
                Console.WriteLine("Consolas font failed to load");
            }

            try {
                colorShader = new Shader("../../resources/shaders/color/s.vert", "../../resources/shaders/color/s.frag");
                Console.WriteLine("Shader 'color' loaded");
            } catch {
                Console.WriteLine("Shader 'color' failed to load");
            }

            try {
                shadeless = new Shader("../../resources/shaders/shadeless/s.vert", "../../resources/shaders/shadeless/s.frag");
                Console.WriteLine("Shader 'shadeless' loaded");
            } catch {
                Console.WriteLine("Shader 'shadeless' failed to load");
            }
            
            ui = new RenderPass(colorShader, 0, 0) {
                depthEnabled = false
            };
        }

        public static void Cleanup() {
            consolas.destroy();

            colorShader.destroy();

            shadeless.destroy();

            Vao.Destroy();
        }

        public static void SetupUI() {
            hud = new HUDBase(consolas, ui) {
                MouseDown = (self, ba, bu) => {
                    if (bu == MouseButton.left) {
                        poly.verts.Add(new csgeom.gvec2 { x = win.Mouse.x, y = win.Mouse.y });
                    } else {
                        currentHole.Add(new csgeom.gvec2 { x = win.Mouse.x, y = win.Mouse.y });
                    }
                },
                Draw = (self, ba) => {
                    for (int i = 0; i < poly.verts.Count; i++) {
                        vec2 pos = poly.verts[i].glm();
                        //ba.text(i.ToString(), pos.x - i.ToString().Length * 0.0125f, pos.y - 0.025f, 0.05f);
                    }
                }
            };

            HUDItem clearAllButton = new HUDItem("clear selection", 0.3f, 0.1f, b) {
                localX = -1,
                localY = -0.9f
            };
            clearAllButton.Draw = (self, ba) => {
                ba.Rect(self.X, self.Y, 0.3f, 0.1f, new vec3(0.3f, 0.3f, 0.3f));
                ba.Text("clear", self.X, self.Y, 0.08f);
            };
            clearAllButton.MouseDown = (self, ba, bu) => {
                poly.verts.Clear();
                poly.holes.Clear();
                currentHole = new csgeom.LineLoop2();
            };

            HUDItem holeButton = new HUDItem("next hole", 0.45f, 0.1f, b) {
                localX = -1,
                localY = -0.8f
            };
            holeButton.Draw = (self, ba) => {
                ba.Rect(self.X, self.Y, self.Width, self.Height, new vec3(0.3f, 0.3f, 0.3f));
                ba.Text("next hole", self.X, self.Y, 0.08f);
            };
            holeButton.MouseDown = (self, ba, bu) => {
                poly.holes.Add(currentHole);
                currentHole = new csgeom.LineLoop2();
            };

            int index = 0;

            HUDItem nextButton = new HUDItem("next", 0.45f, 0.1f, b) {
                localX = -1,
                localY = -0.6f
            };
            nextButton.Draw = (self, ba) => {
                ba.Rect(self.X, self.Y, self.Width, self.Height, new vec3(0.3f, 0.3f, 0.3f));
                ba.Text("next", self.X, self.Y, 0.08f);
            };
            nextButton.MouseDown = (self, ba, bu) => {
                index++;
            };
            HUDItem prevButton = new HUDItem("prev", 0.45f, 0.1f, b) {
                localX = -1,
                localY = -0.7f
            };
            prevButton.Draw = (self, ba) => {
                ba.Rect(self.X, self.Y, self.Width, self.Height, new vec3(0.3f, 0.3f, 0.3f));
                ba.Text("prev", self.X, self.Y, 0.08f);
            };
            prevButton.MouseDown = (self, ba, bu) => {
                index--;
            };
        }



        public static void Main(string[] args) {
            
            
            double lastTime = 0.0;



            csgeom.WeaklySimplePolygon poly = new csgeom.WeaklySimplePolygon();
            csgeom.LineLoop2 currentHole = new csgeom.LineLoop2();
            csgeom.TriangulationCode code = csgeom.TriangulationCode.insufficientVertices;
            

            

            win.MouseDown = new Action<OpenTK.Input.MouseButtonEventArgs>(ev => {
                if (ev.Button == OpenTK.Input.MouseButton.Left) {
                    b.Click(win.Mouse.x, win.Mouse.y);
                } else {
                    b.RightClick(win.Mouse.x, win.Mouse.y);
                }
            });

            while (!win.Closed) {
                GL.ClearColor(142.0f/511.0f, 188.0f/511.0f, 229.0f/511.0f, 1.0f);

                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                GL.Enable(EnableCap.DepthTest);

                Stopwatch sw = Stopwatch.StartNew();
                csgeom.LineLoop2 spoly = poly.Simplify();
                csgeom.TriangulationResult2 res = spoly.Triangulate();
                lastTime = (sw.ElapsedTicks / (double)Stopwatch.Frequency);

                Console.Clear();
                code = res.code;
                Console.WriteLine("Triangulation took " + ((int)(lastTime * 100000)) / 100.0 + " ms");
                Console.WriteLine("Status: " + code.ToString());
                Console.WriteLine("Area: " + poly.verts.Area);

                code = res.code;

                List<vec2[]> edges = new List<vec2[]>();
                List<vec2[]> edgesAll = new List<vec2[]>();
                List<vec2> glms = spoly.Data().Select(vert => vert.glm()).ToList();
                for (int i = 0; i < glms.Count; i++) {
                    if (i == index) edges.Add(new vec2[] { glms[i], glms[i], glms[(i + 1) % glms.Count]});
                    edgesAll.Add(new vec2[] { glms[i], glms[i], glms[(i + 1) % glms.Count] });
                }

                if (glms.Count != 0) {
                    if (index < 0) index = (index + glms.Count) % glms.Count;
                    if (index >= glms.Count) index = index % glms.Count;
                }

                Random r = new Random(5);

                if (res.data != null) {
                    Mesh tri = new Mesh(MeshComponent.colors);
                    tri.RandomColoredTriangles(res.data.Select(ts => new vec2[] { ts.v0.glm(), ts.v1.glm(), ts.v2.glm() }).ToList());
                    Model m = new Model(tri);
                    ui.DrawModel(m, mat4.Identity, PolygonMode.Fill);
                    m.Destroy();
                }


                Mesh edgeMeshAll = new Mesh(MeshComponent.colors);
                edgeMeshAll.Triangles(edgesAll, vec3.Zero);
                Model edgeModelAll = new Model(edgeMeshAll);
                ui.DrawModel(edgeModelAll, mat4.Identity, PolygonMode.Line);
                edgeModelAll.Destroy();

                Mesh edgeMesh = new Mesh(MeshComponent.colors);
                edgeMesh.Triangles(edges, vec3.Ones);
                Model edgeModel = new Model(edgeMesh);
                ui.DrawModel(edgeModel, mat4.Identity, PolygonMode.Line, 3);
                edgeModel.Destroy();


                b.DrawAll();

                win.Flush();

                Thread.Sleep(20);
            }

            HLGfont.consolas.destroy();

            Vao.Destroy();
        }
    }
}
