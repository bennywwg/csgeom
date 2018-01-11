using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CSGeom.D2;
using GlmSharp;

namespace csgeom_test {
    public class EntityManager {
        public vec3 position;
        public quat orientation;

        WeaklySimplePolygon poly;
        
        HUDItem item;

        public EntityManager Union(EntityManager other) {
            return new EntityManager(item.Root) {
                poly = poly.Clone()
            };
        }

        public void Draw(RenderPass g, HUDRect item) {

        }

        public EntityManager(HUDBase itemRoot) {
            item = new HUDItem("Entity Manager", itemRoot);
        }
    }
}
