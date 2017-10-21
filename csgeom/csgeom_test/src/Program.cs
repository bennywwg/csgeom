﻿using System;
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
                win = new Window(900, 900, "Circles");
                Console.WriteLine("Window initialized with OpenGL version 3.3");
            } catch {
                Console.WriteLine("Window failed to initialize");
            }

            Vao.Initialize();

            try {
                consolas = Texture.color("../../resources/consolas.png");
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
            Initialize();
            SetupUI();

            

            for (int i = 0; i < 4; i++) {
                for (int u = 0; u < 4; u++) {
                    HUDRect test = new HUDRect("test", hud) {
                        Color = util.RGB(128, 50, 50),
                        LocalPos = new vec2(-0.05f + i * 0.25f, -0.05f + u * 0.25f),
                        Size = new vec2(0.1f, 0.1f),
                        Text = "Text"
                    };
                }
            }


            cam.Position.z = 1;
            //cam.AngleY = 00.0f / 180.0f * (float)Math.PI;

            double lastDelaT = 0;

            while (!win.Closed) {

                Stopwatch st = Stopwatch.StartNew();

                hud.Update((float)lastDelaT * 1000);


                //cam.AngleY += ((float)Math.PI * 2) * 0.01f;
                //cam.Position.x += (1 ) * 0.01f;

                g.pv = cam.ViewProjection;
                //cam.Position += cam.Direction * 0.01f;
                
                


                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                hud.DrawRecurse(g);

                win.Flush();

                lastDelaT = (double)st.ElapsedMilliseconds / (double)Stopwatch.Frequency;

                //Thread.Sleep(20);
            }

            Cleanup();
        }
    }
}
