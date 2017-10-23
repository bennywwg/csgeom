using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmSharp;
using OpenTK;
using OpenTK.Input;

namespace csgeom_test {
    public class HUDItem {
        public readonly Window Win;
        public readonly string Name;
        public readonly HUDBase Root;
        public readonly HUDItem Parent;

        readonly List<HUDItem> _children;
        public List<HUDItem> Children => _children.ToList();

        public virtual void DoMouseDown(MouseButtonEventArgs bu) {
        }
        public virtual void DoMouseUp(MouseButtonEventArgs bu) {
        }

        public virtual void DoKeyDown(KeyboardKeyEventArgs args) {
        }
        public virtual void DoKeyUp(KeyboardKeyEventArgs args) {
        }

        public virtual bool Hitbox(vec2 point) => false;

        public virtual void Update(float deltaT) {
        }

        public virtual void DoDraw(RenderPass g) {
        }
        
        public void DrawRecurse(RenderPass g) {
            foreach (HUDItem child in Children) {
                child.DoDraw(g);
            }
            foreach (HUDItem child in Children) {
                child.DrawRecurse(g);
            }
        }
        public List<HUDItem> AllSubChildren {
            get {
                List<HUDItem> accum = Children;
                foreach(List<HUDItem> childsChildren in Children.Select(child => child.AllSubChildren)) {
                    foreach (HUDItem subChild in childsChildren) accum.Add(subChild);
                }
                return accum;
            }
        }

        protected HUDItem(Window win, string name, HUDBase root, HUDItem parent, List<HUDItem> children) {
            Win = win;
            Name = name;
            Root = root;
            Parent = parent;
            _children = children;
        }
        public HUDItem(string name, HUDItem parent) : this(parent.Win, name, parent.Root, parent, new List<HUDItem>()) {
            Parent._children.Add(this);
        }

        public void Remove() {
            Parent._children.Remove(this);
        }
    }

    public class HUDRect : HUDItem {
        public vec2 Size;
        public vec2 LocalPos;
        public vec2 Pos => (Parent is HUDRect) ? (Parent as HUDRect).Pos + LocalPos : LocalPos;
        public vec3 Color = util.White;
        public string Text = "";

        public override bool Hitbox(vec2 point) {
            return point.x > Pos.x && point.y > Pos.y && point.x < Pos.x + Size.x && point.y < Pos.y + Size.y;
        }

        private Action<HUDRect, MouseButtonEventArgs> _mouseDown;
        public Action<HUDRect, MouseButtonEventArgs> MouseDown {
            set {
                _mouseDown = value;
            }
        }
        public override void DoMouseDown(MouseButtonEventArgs bu) {
            _mouseDown?.Invoke(this, bu);
        }

        private Action<HUDRect, MouseButtonEventArgs> _mouseUp;
        public Action<HUDRect, MouseButtonEventArgs> MouseUp {
            set {
                _mouseUp = value;
            }
        }
        public override void DoMouseUp(MouseButtonEventArgs bu) {
            _mouseUp?.Invoke(this, bu);
        }

        private Action<HUDRect, KeyboardKeyEventArgs> _keyDown;
        public Action<HUDRect, KeyboardKeyEventArgs> KeyDown {
            set {
                _keyDown = value;
            }
        }
        public override void DoKeyDown(KeyboardKeyEventArgs args) {
            _keyDown?.Invoke(this, args);
        }

        private Action<HUDRect, KeyboardKeyEventArgs> _keyUp;
        public Action<HUDRect, KeyboardKeyEventArgs> KeyUp {
            set {
                _keyUp = value;
            }
        }
        public override void DoKeyUp(KeyboardKeyEventArgs args) {
            _keyUp?.Invoke(this, args);
        }

        public override void DoDraw(RenderPass g) {
            Model m = new Model(Mesh.ColoredRectangle(Size, Color));
            g.DrawModel(m, mat4.Translate(Pos.x, Pos.y, 0), Program.colorShader);
            m.Destroy();
            if (Text.Length != 0) {
                Model t = new Model(Mesh.Text(Text, 0, 0, Size.x), Program.consolas);
                Program.g.DrawModel(t, mat4.Translate(Pos.x, Pos.y, 0.01f), Program.textShader);
                t.Destroy();
            }
        }

        public HUDRect(string name, HUDItem parent) : base(name, parent) {
        }
    }

    public class HUDDynamic : HUDItem {
        public object Custom;

        public override bool Hitbox(vec2 point) => false;

        private Action<HUDDynamic, RenderPass> _draw;
        private Action<HUDDynamic, RenderPass> Draw {
            set {
                _draw = value;
            }
        }
        public override void DoDraw(RenderPass g) {
            _draw?.Invoke(this, g);
        }

        private Action<HUDDynamic, MouseButtonEventArgs> _mouseDown;
        public Action<HUDDynamic, MouseButtonEventArgs> MouseDown {
            set {
                _mouseDown = value;
            }
        }
        public override void DoMouseDown(MouseButtonEventArgs bu) {
            _mouseDown?.Invoke(this, bu);
        }

        private Action<HUDDynamic, MouseButtonEventArgs> _mouseUp;
        public Action<HUDDynamic, MouseButtonEventArgs> MouseUp {
            set {
                _mouseUp = value;
            }
        }
        public override void DoMouseUp(MouseButtonEventArgs bu) {
            _mouseUp?.Invoke(this, bu);
        }

        private Action<HUDDynamic, KeyboardKeyEventArgs> _keyDown;
        public Action<HUDDynamic, KeyboardKeyEventArgs> KeyDown {
            set {
                _keyDown = value;
            }
        }
        public override void DoKeyDown(KeyboardKeyEventArgs args) {
            _keyDown?.Invoke(this, args);
        }

        private Action<HUDDynamic, KeyboardKeyEventArgs> _keyUp;
        public Action<HUDDynamic, KeyboardKeyEventArgs> KeyUp {
            set {
                _keyUp = value;
            }
        }
        public override void DoKeyUp(KeyboardKeyEventArgs args) {
            _keyUp?.Invoke(this, args);
        }

        public HUDDynamic(string name, HUDItem parent) : base(name, parent) {
        }
    }

    public class HUDBase : HUDItem {
        HUDItem _focused;
        public HUDItem Focused => _focused;

        public HUDItem Over(vec2 pos) {
            HUDItem current = this;
            while(true) {
                HUDItem next = null;
                foreach (HUDItem child in current.Children) {
                    if(child.Hitbox(pos)) {
                        next = child;
                    }
                }
                if(next != null) {
                    current = next;
                } else {
                    break;
                }
            }
            return current;
        }
        
        public override void DoMouseDown(MouseButtonEventArgs bu) {
            _focused = Over(Win.Mouse);
            foreach (HUDItem item in AllSubChildren) {
                item.DoMouseDown(bu);
            }
        }

        public override void DoMouseUp(MouseButtonEventArgs bu) {
            foreach (HUDItem item in AllSubChildren) {
                item.DoMouseUp(bu);
            }
        }

        public override void DoKeyDown(KeyboardKeyEventArgs args) {
            foreach (HUDItem item in AllSubChildren) {
                item.DoKeyDown(args);
            }
        }

        public override void DoKeyUp(KeyboardKeyEventArgs args) {
            foreach(HUDItem item in AllSubChildren) {
                item.DoKeyUp(args);
            }
        }

        public override void Update(float deltaT) {
            foreach (HUDItem child in AllSubChildren) child.Update(deltaT);
        }

        public override bool Hitbox(vec2 point) => true;

        public HUDBase(Window win) : base(win, "base " + DateTime.UtcNow.ToLongTimeString() + " " + DateTime.UtcNow.ToLongDateString(), null, null, new List<HUDItem>()) {
        }
    }

    public class HUDCameraController : HUDItem {
        public Camera CameraStart;
        public vec2 MouseStart;
        public bool panning = false;

        public vec3 Motion = vec3.Zero;

        public bool Mode = false;

        public override void DoMouseDown(MouseButtonEventArgs bu) {
            if (bu.Button == MouseButton.Right) {
                CameraStart = Program.cam;
                MouseStart = Win.Mouse;
                panning = true;
            }
        }
        public override void DoMouseUp(MouseButtonEventArgs bu) {
            if (bu.Button == MouseButton.Right) {
                panning = false;
            }
        }

        public override void DoKeyDown(KeyboardKeyEventArgs args) {
            if(args.Key == Key.W) {
                Motion.z = -1f;
            } else if (args.Key == Key.A) {
                Motion.x = -1f;
            } else if (args.Key == Key.S) {
                Motion.z = 1f;
            } else if (args.Key == Key.D) {
                Motion.x = 1f;
            }
        }
        public override void DoKeyUp(KeyboardKeyEventArgs args) {
            if (args.Key == Key.W) {
                Motion.z = 0f;
            } else if (args.Key == Key.A) {
                Motion.x = 0f;
            } else if (args.Key == Key.S) {
                Motion.z = 0f;
            } else if (args.Key == Key.D) {
                Motion.x = 0f;
            }
        }

        public override void Update(float deltaT) {
            if (Mode) {
                if (panning) {
                    Program.cam.AngleY = CameraStart.AngleY + MouseStart.x - Win.Mouse.x;
                    Program.cam.AngleX = CameraStart.AngleX - (MouseStart.y - Win.Mouse.y);
                }
                Program.cam.Position += new vec3(Program.cam.ViewRot.Inverse * new vec4(Motion, 1)) * deltaT * 10.0f;
            } else {
                if (panning) {
                    Program.cam.Position.x = CameraStart.Position.x + MouseStart.x - Win.Mouse.x;
                    Program.cam.Position.y = CameraStart.Position.y + (MouseStart.y - Win.Mouse.y);
                }
                //Program.cam.Position += new vec3(Program.cam.ViewRot.Inverse * new vec4(Motion, 1)) * deltaT * 10.0f;
            }
        }


        public HUDCameraController(string name, HUDItem parent) : base(name, parent) {
        }
    }
}
