using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmSharp;

namespace csgeom_test {
    public class HUDItem {
        public readonly string name;
        public float Width { get; protected set; }
        public float Height { get; protected set; }
        public readonly HUDBase root;
        HUDItem parent;

        public float localX, localY, localZ;

        public virtual float X => localX + parent.X;
        public virtual float Y => localY + parent.Y;
        public vec2 Pos => new vec2(X, Y);

        private List<HUDItem> _children;
        public List<HUDItem> Children => _children.ToList();

        private Action<HUDItem, HUDBase, MouseButton> _mouseDown;
        public Action<HUDItem, HUDBase, MouseButton> MouseDown {
            set {
                _mouseDown = value;
            }
        }
        public void DoMouseDown(HUDBase b, MouseButton bu) {
            _mouseDown?.Invoke(this, b, bu);
        }

        private Action<HUDItem, HUDBase> _draw;
        public Action<HUDItem, HUDBase> Draw {
            set {
                _draw = value;
            }
        }
        public void DoDraw(HUDBase b) {
            _draw?.Invoke(this, b);
        }
        
        protected void DrawRecurse(HUDBase b) {
            DoDraw(b);
            foreach(HUDItem child in Children) {
                child.DrawRecurse(b);
            }
        }

        public HUDItem(string name, float width, float height, HUDItem parent) {
            localY = 0;
            localY = 0;
            this.Width = width;
            this.Height = height;
            this.name = name;

            this.root = parent.root;
            this.parent = parent;

            _children = new List<HUDItem>();
            if(parent != null) parent._children.Add(this);
        }

        public void Remove() {
            parent._children.Remove(this);
        }
    }

    public class HUDBase : HUDItem {
        public readonly HLGfont font;
        public readonly RenderPass sh;

        public override float X => 0;
        public override float Y => 0;
        /// <summary>
        ///     Returns the highest global z object that 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public HUDItem Over(float x, float y) {
            HUDItem current = this;
            while(true) {
                HUDItem next = null;
                foreach (HUDItem child in current.Children) {
                    if (x >= child.X && x < child.X + child.Width && y >= child.Y && y < child.Y + child.Height) {
                        if(next == null) {
                            next = child;
                        } else if(child.localZ >= next.localZ) {
                            next = child;
                        }
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

        public void Click(float x, float y) {
            HUDItem item = Over(x, y);
            item.DoMouseDown(this, MouseButton.left);
        }
        public void RightClick(float x, float y) {
            HUDItem item = Over(x, y);
            item.DoMouseDown(this, MouseButton.right);
        }

        public void Text(string text, float x, float y, float scale) {
            font.drawText(text, x, y, scale);
        }

        public void Rect(float x, float y, float width, float height, vec3 color) {
            Model m = new Model(Mesh.ColoredRectangle(new vec2(width, height), color));
            sh.DrawModel(m, mat4.Translate(x, y, 0));
            m.Destroy();
        }

        public void DrawAll() {
            sh.DisableDepth();
            DrawRecurse(this);
        }

        public HUDBase(HLGfont font, RenderPass color) : base("base " + DateTime.UtcNow.ToLongTimeString() + " " + DateTime.UtcNow.ToLongDateString(), 2, 2, null) {
            this.font = font;
            this.sh = color;
        }
    }
}
