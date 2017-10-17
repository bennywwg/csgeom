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
        public static renderPass ui;

        public static void Main(string[] args) {
            window win = new window(900, 900, "Circles");
            
            double lastTime = 0.0;

            vao.initialize();

            hlg_font.consolas = new hlg_font("../../resources/consolas.png", "../../resources/shaders/text/s.vert", "../../resources/shaders/text/s.frag");
            hlg_font.consolas.name = "Consolas";



            csgeom.SimplePolygonWithHoles poly = new csgeom.SimplePolygonWithHoles();
            csgeom.LineLoop2 currentHole = new csgeom.LineLoop2();
            csgeom.triangulationCode code = csgeom.triangulationCode.insufficientVertices;

            

            texture colors = texture.color("../../resources/colors.png");

            shader colorShader = new shader("../../resources/shaders/color/s.vert", "../../resources/shaders/color/s.frag");
            shader shadeless = new shader("../../resources/shaders/shadeless/s.vert", "../../resources/shaders/shadeless/s.frag");

            ui = new renderPass(colorShader, 0, 0);
            ui.depthEnabled = false;

            hudBase b = new hudBase(hlg_font.consolas, ui);
            b.mouseDown = (ba, bu) => {
                if (bu == mouseButton.left) {
                    poly.verts.Add(new csgeom.vert2 { x = win.mouse.x, y = win.mouse.y });
                } else {
                    currentHole.Add(new csgeom.vert2 { x = win.mouse.x, y = win.mouse.y });
                }
            };
            b.draw = ba => {
                for (int i = 0; i < poly.verts.Count; i++) {
                    vec2 pos = poly.verts[i].glm();
                    //ba.text(i.ToString(), pos.x - i.ToString().Length * 0.0125f, pos.y - 0.025f, 0.05f);
                }
            };

            hudItem clearAllButton = new hudItem("clear selection", 0.3f, 0.1f, b);
            clearAllButton.localX = -1;
            clearAllButton.localY = -0.9f;
            clearAllButton.draw = ba => {
                ba.rect(clearAllButton.x, clearAllButton.y, 0.3f, 0.1f, new vec3(0.3f, 0.3f, 0.3f));
                ba.text("clear", clearAllButton.x, clearAllButton.y, 0.08f);
            };
            clearAllButton.mouseDown = (ba, bu) => {
                poly.verts.Clear();
                poly.holes.Clear();
                currentHole = new csgeom.LineLoop2();
            };

            hudItem holeButton = new hudItem("next hole", 0.45f, 0.1f, b);
            holeButton.localX = -1;
            holeButton.localY = -0.8f;
            holeButton.draw = ba => {
                ba.rect(holeButton.x, holeButton.y, holeButton.width, holeButton.height, new vec3(0.3f, 0.3f, 0.3f));
                ba.text("next hole", holeButton.x, holeButton.y, 0.08f);
            };
            holeButton.mouseDown = (ba, bu) => {
                poly.holes.Add(currentHole);
                currentHole = new csgeom.LineLoop2();
            };

            int index = 0;

            hudItem nextButton = new hudItem("next", 0.45f, 0.1f, b);
            nextButton.localX = -1;
            nextButton.localY = -0.6f;
            nextButton.draw = ba => {
                ba.rect(nextButton.x, nextButton.y, nextButton.width, nextButton.height, new vec3(0.3f, 0.3f, 0.3f));
                ba.text("next", nextButton.x, nextButton.y, 0.08f);
            };
            nextButton.mouseDown = (ba, bu) => {
                index++;
            };
            hudItem prevButton = new hudItem("prev", 0.45f, 0.1f, b);
            prevButton.localX = -1;
            prevButton.localY = -0.7f;
            prevButton.draw = ba => {
                ba.rect(prevButton.x, prevButton.y, prevButton.width, prevButton.height, new vec3(0.3f, 0.3f, 0.3f));
                ba.text("prev", prevButton.x, prevButton.y, 0.08f);
            };
            prevButton.mouseDown = (ba, bu) => {
                index--;
            };

            win.mouseDown = new Action<OpenTK.Input.MouseButtonEventArgs>(ev => {
                if (ev.Button == OpenTK.Input.MouseButton.Left) {
                    b.click(win.mouse.x, win.mouse.y);
                } else {
                    b.rightClick(win.mouse.x, win.mouse.y);
                }
            });

            while (!win.closed) {
                GL.ClearColor(142.0f/511.0f, 188.0f/511.0f, 229.0f/511.0f, 1.0f);

                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                GL.Enable(EnableCap.DepthTest);

                Stopwatch sw = Stopwatch.StartNew();
                csgeom.SimplePolygon spoly = poly.Simplify();
                csgeom.triangulationResult2 res = spoly.Triangulate();
                lastTime = (sw.ElapsedTicks / (double)Stopwatch.Frequency);

                Console.Clear();
                code = res.code;
                Console.WriteLine("Triangulation took " + ((int)(lastTime * 100000)) / 100.0 + " ms");
                Console.WriteLine("Status: " + code.ToString());
                Console.WriteLine("Area: " + poly.verts.Area);

                code = res.code;

                List<vec2[]> edges = new List<vec2[]>();
                List<vec2[]> edgesAll = new List<vec2[]>();
                List<vec2> glms = spoly.verts.Data().Select(vert => vert.glm()).ToList();
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
                    mesh tri = new mesh(meshComponent.colors);
                    tri.randomColoredTriangles(res.data.Select(ts => new vec2[] { ts.v0.glm(), ts.v1.glm(), ts.v2.glm() }).ToList());
                    model m = new model(tri);
                    ui.drawModel(m, mat4.Identity, PolygonMode.Fill);
                    m.destroy();
                }


                mesh edgeMeshAll = new mesh(meshComponent.colors);
                edgeMeshAll.triangles(edgesAll, vec3.Zero);
                model edgeModelAll = new model(edgeMeshAll);
                ui.drawModel(edgeModelAll, mat4.Identity, PolygonMode.Line);
                edgeModelAll.destroy();

                mesh edgeMesh = new mesh(meshComponent.colors);
                edgeMesh.triangles(edges, vec3.Ones);
                model edgeModel = new model(edgeMesh);
                ui.drawModel(edgeModel, mat4.Identity, PolygonMode.Line, 3);
                edgeModel.destroy();


                b.drawAll();

                win.flush();

                Thread.Sleep(20);
            }

            hlg_font.consolas.destroy();

            vao.destroy();
        }
    }
}
