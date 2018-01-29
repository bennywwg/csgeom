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
        public static RenderPass g;

        public static Texture consolas;

        public static Shader colorShader;
        public static Shader shadeless;
        public static Shader textShader;

        public static HUDBase hud;

        public static Camera cam;


        public static void Initialize() {
            try {
                win = new Window(750, 750, "Circles");
                Console.WriteLine("Window initialized with OpenGL version 3.3");
            } catch {
                Console.WriteLine("Window failed to initialize");
            }

            Vao.Initialize();

            try {
                consolas = Texture.color("../../resources/consolas1.bmp");
                Console.WriteLine("Consolas font loaded from consolas1.bmp");
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

            try {
                textShader = new Shader("../../resources/shaders/text/s.vert", "../../resources/shaders/text/s.frag");
                Console.WriteLine("Shader 'text' loaded");
            } catch {
                Console.WriteLine("Shader 'text' failed to load");
            }

            g = new RenderPass() {
                depthEnabled = true
            };

            ui = new RenderPass() {
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
            hud = new HUDBase(win);

            win.MouseDown = new Action<OpenTK.Input.MouseButtonEventArgs>(ev => {
                hud.DoMouseDown(ev);
            });

            win.MouseUp = new Action<OpenTK.Input.MouseButtonEventArgs>(ev => {
                hud.DoMouseUp(ev);
            });

            win.KeyDown = new Action<OpenTK.Input.KeyboardKeyEventArgs>(ev => {
                if (ev.Key == OpenTK.Input.Key.Escape) {
                    win.Close();
                    return;
                }
                hud.DoKeyDown(ev);
            });

            win.KeyUp = new Action<OpenTK.Input.KeyboardKeyEventArgs>(ev => {
                hud.DoKeyUp(ev);
            });

            win.MouseMove = new Action<OpenTK.Input.MouseMoveEventArgs>(ev => {
                
            });

            cam = new Camera() {
                ncp = 0.01f,
                fcp = 1000.0f,
                fov = 90.0f
            };

            HUDCameraController cameraRotater = new HUDCameraController("Camera Controller", hud);

            HUDGeom ge = new HUDGeom("Geometry Interface", hud);
        }



        public static void Main(string[] stdin) {

            CSGeom.D2.Polygon p = new CSGeom.D2.Polygon();

            p.InsertLoop(new CSGeom.D2.LineLoop(new CSGeom.gvec2[] {
                new CSGeom.gvec2(0.0, 0.0),
                new CSGeom.gvec2(4.0, 0.0),
                new CSGeom.gvec2(4.0, 4.0),
                new CSGeom.gvec2(0.0, 4.0),
            }));
            p.InsertLoop(new CSGeom.D2.LineLoop(new CSGeom.gvec2[] {
                new CSGeom.gvec2(1.0, 1.0),
                new CSGeom.gvec2(1.0, 3.0),
                new CSGeom.gvec2(3.0, 3.0),
                new CSGeom.gvec2(3.0, 1.0),
            }));

            List<CSGeom.D2.WeaklySimplePolygon> output = p.Simplify();



            

            Initialize();
            SetupUI();


            cam.Position.z = 1;
            //cam.AngleY = 00.0f / 180.0f * (float)Math.PI;

            double lastDelaT = 0;

            int count = 0;
            Stopwatch outer = Stopwatch.StartNew();

            while (!win.Closed) {
                count++;

                Stopwatch st = Stopwatch.StartNew();

                hud.Update((float)lastDelaT * 1000, win.Mouse);


                //cam.AngleY += ((float)Math.PI * 2) * 0.01f;
                //cam.Position.x += (1 ) * 0.01f;

                g.pv = cam.ViewProjection;
                //cam.Position += cam.Direction * 0.01f;



                //GL.ClearColor(1, 1, 1, 1);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                hud.DrawRecurse();

                win.Flush();

                lastDelaT = st.ElapsedMilliseconds / (double)Stopwatch.Frequency;

                if(outer.ElapsedMilliseconds > 1000) {
                    count = 0;
                    outer = Stopwatch.StartNew();
                }
            }

            Cleanup();
        }
    }
}
