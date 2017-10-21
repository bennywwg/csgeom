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

        public static vec3 RGB(int r, int g, int b) {
            return new vec3(r / 255.0f, g / 255.0f, b / 255.0f);
        }
        public static vec3 Gray(float brightness) {
            return new vec3(brightness);
        }
        public static readonly vec3 Red     = RGB(255, 0, 0);
        public static readonly vec3 Green   = RGB(0, 255, 0);
        public static readonly vec3 Blue    = RGB(0, 0, 255);
        public static readonly vec3 White   = RGB(255, 255, 255);
        public static readonly vec3 Black   = RGB(0, 0, 0);

        public struct Plane {
            //of the form ax + by + cz = d
            //|<a, b, c>| will be 1 unless the plane is degenerate
            //g_planes DO have a direction they are facing, it is the direction <a, b, c>
            //flipping the sign of every component yields a g_plane facing the opposite direction
            //this information will be used for ccw checking if a point is inside a triangle or complex polygon
            public float a, b, c, d;

            /// <summary>
            ///     Finds an intersection between this plane and a specified infinite line.
            /// </summary>
            /// <param name="p0">A point lying on an infinite line</param>
            /// <param name="p1">A point lying on an infinite line</param>
            /// <returns>The intersection between this plane and the specified infinite line</returns>
            public vec3 UnconstrainedIntersection(vec3 p0, vec3 p1) {
                //math overview:
                //ax + by + cz = d
                //dot(<a, b, c>, <x, y, z>) = d
                //<x, y, z> = v0 + dir * t
                //dot(<a, b, c>, (v0 + dir * t)) = d
                //dot(<a, b, c>, v0) + dot(<a, b, c>, dir) * t = d
                //t = (d - dot(<a, b, c>, v0)) / dot(<a, b, c>, dir)

                vec3 dir = p1 - p0;
                float num = d - a * p0.x - b * p0.y - c * p0.z;
                float div = a * dir.x + b * dir.y + c * dir.z;
                float t = num / div;
                return p0 + dir * t;
            }

            /// <summary>
            ///     Finds an intersection between this plane and a specified finite line.
            /// </summary>
            /// <param name="p0">A point lying on an finite line</param>
            /// <param name="p1">A point lying on an finite line</param>
            /// <param name="intersection">The intersection between this plane and the specified finite line, if it exists</param>
            /// <returns>Whether or not there was an intersection</returns>
            public bool ConstrainedIntersection(vec3 p0, vec3 p1, ref vec3 intersection) {
                vec3 dir = p1 - p0;
                float num = d - a * p0.x - b * p0.y - c * p0.z;
                float div = a * dir.x + b * dir.y + c * dir.z;
                float t = num / div;
                if (t < 0) return false;
                if (t < (p1 - p0).Length) {
                    intersection = p0 + dir * t;
                    return true;
                } else {
                    return false;
                }
            }

            public static Plane CCW(vec3 v0, vec3 v1, vec3 v2) {
                vec3 cross = vec3.Cross(v1 - v0, v2 - v0).Normalized;
                return new Plane {
                    a = cross.x,
                    b = cross.y,
                    c = cross.z,
                    d = vec3.Dot(cross, v0)
                };
            }
        }
    }
}
