using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using csgeom;
using GlmSharp;

namespace csgeom_test {
    public class EntityManager {
        public vec3 position;
        public quat orientation;

        WeaklySimplePolygon poly;
        
        HUDItem item;

        public EntityManager Union(EntityManager other) {
            return new EntityManager(item.root) {
                poly = poly.Clone()
            };
        }

        public void Draw() {
            item.root.Rect(item.X, item.Y, item.Width, item.Height, new vec3(1, 1, 1));
            item.root.sh.BeginPass
        }

        public EntityManager(HUDBase itemRoot) {
            item = new HUDItem("Entity Manager", 1, 1, itemRoot);
        }
    }
}
