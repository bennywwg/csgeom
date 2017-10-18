using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmSharp;

namespace csgeom_test {
    public static class util {
        public static bool powerOf2(this ulong x) {
            return (x != 0) && ((x & (x - 1)) == 0);
        }
        public static bool powerOf2(this int x) {
            return (x != 0) && ((x & (x - 1)) == 0);
        }
        public static vec3 transformPos(vec3 p, mat4 trans) {
            return new vec3(trans * new vec4(p, 1.0f));
        }
        public static vec3 transformDir(vec3 d, mat4 trans) {
            return new vec3(trans * new vec4(d, 0.0f));
        }

        //glm has more support functions than csgeom vertices
        public static vec2 glm(this csgeom.gvec2 v) {
            return new vec2((float)v.x, (float)v.y);
        }
        public static vec3 glm(this csgeom.gvec3 v) {
            return new vec3((float)v.x, (float)v.y, (float)v.z);
        }
        public static csgeom.gvec2 csgeom(this vec2 v) {
            return new csgeom.gvec2 { x = v.x, y = v.y };
        }
        public static csgeom.gvec3 csgeom(this vec3 v) {
            return new csgeom.gvec3 { x = v.x, y = v.y, z = v.z };
        }
    }
}
