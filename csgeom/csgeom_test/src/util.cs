using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmSharp;

namespace csgeom_test {
    public static class Util {
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
        public static vec2 glm(this CSGeom.gvec2 v) {
            return new vec2((float)v.x, (float)v.y);
        }
        public static vec3 glm(this CSGeom.gvec3 v) {
            return new vec3((float)v.x, (float)v.y, (float)v.z);
        }
        public static CSGeom.gvec2 csgeom(this vec2 v) {
            return new CSGeom.gvec2 { x = v.x, y = v.y };
        }
        public static CSGeom.gvec3 csgeom(this vec3 v) {
            return new CSGeom.gvec3 { x = v.x, y = v.y, z = v.z };
        }

        public static vec3 Color(string hex) {
            if(hex.StartsWith("#")) hex = hex.Remove(0, 1);
            int len = hex.Length <= 3 ? 3 : 6;
            hex = hex.PadRight(len, '0');
            if(hex.Length > 6) hex = hex.Remove(len);
            if (len == 3) {
                return Color(
                    int.Parse(hex.Substring(0, 1), System.Globalization.NumberStyles.HexNumber),
                    int.Parse(hex.Substring(1, 1), System.Globalization.NumberStyles.HexNumber),
                    int.Parse(hex.Substring(2, 1), System.Globalization.NumberStyles.HexNumber)
                );
            } else {
                return Color(
                    int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
                    int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                    int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber)
                );
            }
        }
        //public static vec3 Color(int hex) {
        //    # cee3f8
        //}
        public static vec3 Color(int r, int g, int b) {
            return new vec3(r / 255.0f, g / 255.0f, b / 255.0f);
        }
        public static vec3 Gray(float brightness) {
            return new vec3(brightness);
        }
        public static readonly vec3 Red     = Color(255, 0, 0);
        public static readonly vec3 Green   = Color(0, 255, 0);
        public static readonly vec3 Blue    = Color(0, 0, 255);
        public static readonly vec3 White   = Color(255, 255, 255);
        public static readonly vec3 Black   = Color(0, 0, 0);

        public static int IntPow(int x, uint pow) {
            int ret = 1;
            while (pow != 0) {
                if ((pow & 1) == 1)
                    ret *= x;
                x *= x;
                pow >>= 1;
            }
            return ret;
        }

        public static string Round(this double value, int decimals) {
            int mul = IntPow(10, (uint)decimals);
            return ((int)(value * mul) / (double)mul).ToString();
        }
        public static string Round(this float value, int decimals) {
            int mul = IntPow(10, (uint)decimals);
            return ((int)(value * mul) / (float)mul).ToString();
        }

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

        public static void Transform(this CSGeom.D2.Loop loop, dmat4 transform) {
            for (int i = 0; i < loop.Count; i++) {
                dvec2 tmp = new dvec2(transform * new dvec4(loop[i].x, loop[i].y, 0, 1));
                loop[i] = new CSGeom.gvec2(tmp.x, tmp.y);
            }
        }
    }
}
