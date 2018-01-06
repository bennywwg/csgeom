using System;
using OpenTK;
using OpenTK.Graphics;
using GlmSharp;

namespace csgeom_test {
    public class Window {
        private readonly NativeWindow win;
        private readonly GraphicsContext ctx;

        private bool _closed;
        public bool Closed => _closed;

        private EventHandler<OpenTK.Input.KeyboardKeyEventArgs> _keyDown;
        public Action<OpenTK.Input.KeyboardKeyEventArgs> KeyDown {
            set {
                win.KeyDown -= _keyDown;
                _keyDown = (sender, args) => value(args);
                win.KeyDown += _keyDown;
            }
        }
        private EventHandler<OpenTK.Input.KeyboardKeyEventArgs> _keyUp;
        public Action<OpenTK.Input.KeyboardKeyEventArgs> KeyUp {
            set {
                win.KeyUp -= _keyUp;
                _keyUp = (sender, args) => value(args);
                win.KeyUp += _keyUp;
            }
        }

        private EventHandler<OpenTK.Input.MouseButtonEventArgs> _mouseDown;
        public Action<OpenTK.Input.MouseButtonEventArgs> MouseDown {
            set {
                win.MouseDown -= _mouseDown;
                _mouseDown = (sender, args) => value(args);
                win.MouseDown += _mouseDown;
            }
        }
        private EventHandler<OpenTK.Input.MouseButtonEventArgs> _mouseUp;
        public Action<OpenTK.Input.MouseButtonEventArgs> MouseUp {
            set {
                win.MouseUp -= _mouseUp;
                _mouseUp = (sender, args) => value(args);
                win.MouseUp += _mouseUp;
            }
        }

        private EventHandler<OpenTK.Input.MouseMoveEventArgs> _mouseMove;
        public Action<OpenTK.Input.MouseMoveEventArgs> MouseMove {
            set {
                win.MouseMove -= _mouseMove;
                _mouseMove = (sender, args) => value(args);
                win.MouseMove += _mouseMove;
            }
        }
        
        
        private ivec2 _mousepx;
        public ivec2 Mousepx {
            get {
                return new ivec2(win.X, win.Y);
            }
        }
        public vec2 Mouse {
            get {
                return new vec2((float)_mousepx.x / (float)Size.x * 2.0f - 1.0f, 1.0f - (float)_mousepx.y / (float)Size.y * 2.0f);
            }
        }

        public ivec2 Position => new ivec2(win.X, win.Y);
        public ivec2 Size => new ivec2(win.ClientRectangle.Width, win.ClientRectangle.Height);

        public Window(int width, int height, string title) {
            win = new NativeWindow(width, height, title, GameWindowFlags.FixedWindow, GraphicsMode.Default, DisplayDevice.Default);
            ctx = new GraphicsContext(new GraphicsMode(32, 24, 0, 8), win.WindowInfo, 4, 4, GraphicsContextFlags.Default);

            ctx.MakeCurrent(win.WindowInfo);
            ctx.LoadAll();
            win.Visible = true;

            win.MouseMove += (sender, args) => _mousepx = new ivec2(args.X, args.Y);

            win.Closed += (sender, args) => _closed = true;
        }

        public void Flush() {
            if (!Closed) {
                ctx.SwapBuffers();
                win.ProcessEvents();
            } else {
                ctx.Dispose();
                win.Close();
                win.Dispose();
            }
        }

        public void Close() {
            _closed = true;
        }
    }
}
