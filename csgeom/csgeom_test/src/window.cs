using System;
using OpenTK;
using OpenTK.Graphics;
using GlmSharp;

namespace csgeom_test {
    public enum MouseButton {
        left,
        right
    }

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

        private vec2 _mouse;
        public vec2 Mouse {
            get {
                return _mouse;
            }
        }

        public Window(int width, int height, string title) {
            win = new NativeWindow(width, height, title, GameWindowFlags.FixedWindow, GraphicsMode.Default, DisplayDevice.Default);
            ctx = new GraphicsContext(GraphicsMode.Default, win.WindowInfo, 4, 4, GraphicsContextFlags.Default);

            ctx.MakeCurrent(win.WindowInfo);
            ctx.LoadAll();
            win.Visible = true;

            win.MouseMove += (sender, args) => _mouse = new vec2(args.X / (float)win.Width * 2 - 1, -args.Y / (float)win.Height * 2 + 1);

            win.Closed += (sender, args) => _closed = true;
        }

        public void Flush() {
            if (!Closed) {
                ctx.SwapBuffers();
                win.ProcessEvents();
            }
        }

        public void Close() {
            ctx.Dispose();
            win.Close();
            win.Dispose();
        }
    }
}
