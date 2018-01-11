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
        public HUDBase Root;
        public readonly HUDItem Parent;

        public float TextUnit => (float)(512.0 / 16.0 / Win.Size.y * 2);

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
        public HUDItem Over(vec2 point) {
            foreach(HUDItem child in Children) {
                HUDItem found = child.Over(point);
                if (found != null) return found;
            }
            if (Hitbox(point)) return this;
            return null;
        }

        public virtual void Update(float deltaT) {
        }

        public virtual void DoDraw() {
        }
        
        public void DrawRecurse() {
            foreach (HUDItem child in Children) {
                child.DoDraw();
            }
            foreach (HUDItem child in Children) {
                child.DrawRecurse();
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

    [Flags]
    public enum AlignMode {
        leftAlign = 0,
        rightAlign = 1,
        //horizontalCenter = 4,
        bottomAlign = 0,
        topAlign = 2,
        //verticalCenter = 32
    }

    public class HUDRect : HUDItem {
        public AlignMode mode = AlignMode.bottomAlign | AlignMode.leftAlign;
        public vec2 LocalPos;
        public vec2 Pos => (Parent is HUDRect) ? (Parent as HUDRect).Pos + LocalPos : LocalPos;
        public vec3 Color = Util.White;
        public string Text = "";
        public float CharHeight => TextUnit;
        public float CharWidth => CharHeight * 0.3f;
        public float TotalWidth => CharWidth * Text.Length;

        public vec2 LowerLeftPos {
            get {
                float x = 0;
                float y = 0;
                if ((mode & AlignMode.rightAlign) != 0) {
                    x = Pos.x - TotalWidth;
                } else {
                    x = Pos.x;
                }
                if ((mode & AlignMode.topAlign) != 0) {
                    y = Pos.y - CharHeight;
                } else {
                    y = Pos.y;
                }
                return new vec2(x, y);
            }
        }

        public override bool Hitbox(vec2 point) {
            return point.x > LowerLeftPos.x && point.y > LowerLeftPos.y && point.x < LowerLeftPos.x + TotalWidth && point.y < LowerLeftPos.y + CharHeight;
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

        private Action<HUDRect, float> _update;
        public Action<HUDRect, float> DynamicUpdate {
            set {
                _update = value;
            }
        }

        public override void Update(float deltaT) {
            _update?.Invoke(this, deltaT);
        }

        public override void DoDraw() {
            if (Text.Length != 0) {
                Model m = new Model(Mesh.ColoredRectangle(new vec2(TotalWidth, CharHeight), Color));
                Program.ui.DrawModel(m, mat4.Translate(LowerLeftPos.x, LowerLeftPos.y, 0), Program.colorShader);
                m.Destroy();
                Model t = new Model(Mesh.Text(Text, 0, 0, CharHeight), Program.consolas);
                Program.ui.DrawModel(t, mat4.Translate(LowerLeftPos.x, LowerLeftPos.y, 0.01f), Program.textShader, OpenTK.Graphics.OpenGL.PolygonMode.Fill, 1, true);
                t.Destroy();
            }
        }

        public HUDRect(string name, HUDItem parent) : base(name, parent) {
        }

        public override string ToString() {
            return Name + ": " + Text;
        }
    }

    public class HUDDynamic : HUDItem {
        public object Custom;

        public override bool Hitbox(vec2 point) => false;

        private Action<HUDDynamic> _draw;
        private Action<HUDDynamic> Draw {
            set {
                _draw = value;
            }
        }
        public override void DoDraw() {
            _draw?.Invoke(this);
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
        HUDItem _hovered;
        public HUDItem Hovered => _hovered;
        
        public override void DoMouseDown(MouseButtonEventArgs bu) {
            _hovered = Over(Win.Mouse);
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

        public void UpdateHovered(vec2 pos) {
            _hovered = Over(pos);
        }

        public void Update(float deltaT, vec2 mousePos) {
            UpdateHovered(mousePos);
            foreach (HUDItem child in AllSubChildren) child.Update(deltaT);
        }

        public override bool Hitbox(vec2 point) => true;

        public HUDBase(Window win) : base(win, "base " + DateTime.UtcNow.ToLongTimeString() + " " + DateTime.UtcNow.ToLongDateString(), null, null, new List<HUDItem>()) {
            Root = this;
        }
    }

    public class HUDCameraController : HUDItem {
        public Camera CameraStart;
        public vec2 MouseStart;
        public bool panning = false;

        public vec3 Motion = vec3.Zero;

        public bool Mode = false;

        HUDRect coords;

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
                    //Program.cam.Position.x = CameraStart.Position.x + MouseStart.x - Win.Mouse.x;
                    //Program.cam.Position.y = CameraStart.Position.y + (MouseStart.y - Win.Mouse.y);
                }
                //Program.cam.Position += new vec3(Program.cam.ViewRot.Inverse * new vec4(Motion, 1)) * deltaT * 10.0f;
            }

            UpdateOverlay();
        }

        void UpdateOverlay() {
            string coordsText = "<" + Program.cam.Position.x.Round(1) + "," + Program.cam.Position.y.Round(1) + ">";
            coords.Text = coordsText;
        }


        public HUDCameraController(string name, HUDItem parent) : base(name, parent) {
            coords = new HUDRect("coords", this) {
                Color = Util.Color(30, 0, 0),
                mode = AlignMode.rightAlign | AlignMode.bottomAlign,
                LocalPos = new vec2(1, -1)
            };

        }
    }
}
