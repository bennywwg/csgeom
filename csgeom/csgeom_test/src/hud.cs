using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmSharp;

namespace csgeom_test {
    public class hudItem {
        public readonly string name;
        public float width { get; protected set; }
        public float height {get; protected set; }
        hudItem parent;

        public float localX, localY, localZ;

        public virtual float x => localX + parent.x;
        public virtual float y => localY + parent.y;
        public vec2 pos => new vec2(x, y);

        private List<hudItem> _children;
        public List<hudItem> children => _children.ToList();

        private Action<hudBase, mouseButton> _mouseDown;
        public Action<hudBase, mouseButton> mouseDown {
            set {
                _mouseDown = value;
            }
        }
        public void doMouseDown(hudBase b, mouseButton bu) {
            if (_mouseDown != null) _mouseDown(b, bu);
        }

        private Action<hudBase> _draw;
        public Action<hudBase> draw {
            set {
                _draw = value;
            }
        }
        public void doDraw(hudBase b) {
            if (_draw != null) _draw(b);
        }
        
        protected void drawRecurse(hudBase b) {
            doDraw(b);
            foreach(hudItem child in children) {
                child.drawRecurse(b);
            }
        }

        public hudItem(string name, float width, float height, hudItem parent) {
            localY = 0;
            localY = 0;
            this.width = width;
            this.height = height;
            this.name = name;

            this.parent = parent;

            _children = new List<hudItem>();
            if(parent != null) parent._children.Add(this);
        }

        public void remove() {
            parent._children.Remove(this);
        }
    }

    public class hudBase : hudItem {
        public readonly hlg_font font;
        public readonly renderPass sh;

        public override float x => 0;
        public override float y => 0;
        /// <summary>
        ///     Returns the highest global z object that 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public hudItem over(float x, float y) {
            hudItem current = this;
            while(true) {
                hudItem next = null;
                foreach (hudItem child in current.children) {
                    if (x >= child.x && x < child.x + child.width && y >= child.y && y < child.y + child.height) {
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

        public void click(float x, float y) {
            hudItem item = over(x, y);
            item.doMouseDown(this, mouseButton.left);
        }
        public void rightClick(float x, float y) {
            hudItem item = over(x, y);
            item.doMouseDown(this, mouseButton.right);
        }

        public void text(string text, float x, float y, float scale) {
            font.drawText(text, x, y, scale);
        }

        public void rect(float x, float y, float width, float height, vec3 color) {
            model m = new model(mesh.coloredRectangle(new vec2(width, height), color));
            sh.drawModel(m, mat4.Translate(x, y, 0));
            m.destroy();
        }

        public void drawAll() {
            sh.disableDepth();
            drawRecurse(this);
        }

        public hudBase(hlg_font font, renderPass color) : base("base " + DateTime.UtcNow.ToLongTimeString() + " " + DateTime.UtcNow.ToLongDateString(), 2, 2, null) {
            this.font = font;
            this.sh = color;
        }
    }
}
