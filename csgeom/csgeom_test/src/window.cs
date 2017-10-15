using System;
using OpenTK;
using OpenTK.Graphics;
using GlmSharp;

namespace csgeom_test {
    public enum mouseButton {
        left,
        right
    }

    public class window {
        private readonly NativeWindow win;
        private readonly GraphicsContext ctx;

        private bool _closed;
        public bool closed => _closed;

        private EventHandler<OpenTK.Input.KeyboardKeyEventArgs> _keyDown;
        public Action<OpenTK.Input.KeyboardKeyEventArgs> keyDown {
            set {
                win.KeyDown -= _keyDown;
                _keyDown = (sender, args) => value(args);
                win.KeyDown += _keyDown;
            }
        }
        private EventHandler<OpenTK.Input.KeyboardKeyEventArgs> _keyUp;
        public Action<OpenTK.Input.KeyboardKeyEventArgs> keyUp {
            set {
                win.KeyUp -= _keyUp;
                _keyUp = (sender, args) => value(args);
                win.KeyUp += _keyUp;
            }
        }

        private EventHandler<OpenTK.Input.MouseButtonEventArgs> _mouseDown;
        public Action<OpenTK.Input.MouseButtonEventArgs> mouseDown {
            set {
                win.MouseDown -= _mouseDown;
                _mouseDown = (sender, args) => value(args);
                win.MouseDown += _mouseDown;
            }
        }
        private EventHandler<OpenTK.Input.MouseButtonEventArgs> _mouseUp;
        public Action<OpenTK.Input.MouseButtonEventArgs> mouseUp {
            set {
                win.MouseUp -= _mouseUp;
                _mouseUp = (sender, args) => value(args);
                win.MouseUp += _mouseUp;
            }
        }

        private vec2 _mouse;
        public vec2 mouse {
            get {
                return _mouse;
            }
        }

        public window(int width, int height, string title) {
            win = new NativeWindow(width, height, title, GameWindowFlags.FixedWindow, GraphicsMode.Default, DisplayDevice.Default);
            ctx = new GraphicsContext(GraphicsMode.Default, win.WindowInfo, 4, 4, GraphicsContextFlags.Default);

            ctx.MakeCurrent(win.WindowInfo);
            ctx.LoadAll();
            win.Visible = true;

            win.MouseMove += (sender, args) => _mouse = new vec2(args.X / (float)win.Width * 2 - 1, -args.Y / (float)win.Height * 2 + 1);

            win.Closed += (sender, args) => _closed = true;
        }

        public void flush() {
            if (!closed) {
                ctx.SwapBuffers();
                win.ProcessEvents();
            }
        }

        public void close() {
            ctx.Dispose();
            win.Close();
            win.Dispose();
        }
    }
}
