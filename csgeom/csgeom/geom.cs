using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csgeom {
    public struct Plane {
        //of the form ax + by + cz = d
        //|<a, b, c>| will be 1 unless the plane is degenerate
        //g_planes DO have a direction they are facing, it is the direction <a, b, c>
        //flipping the sign of every component yields a g_plane facing the opposite direction
        //this information will be used for ccw checking if a point is inside a triangle or complex polygon
        public double a, b, c, d;

        /// <summary>
        ///     Finds an intersection between this plane and a specified infinite line.
        /// </summary>
        /// <param name="p0">A point lying on an infinite line</param>
        /// <param name="p1">A point lying on an infinite line</param>
        /// <returns>The intersection between this plane and the specified infinite line</returns>
        public gvec3 UnconstrainedIntersection(gvec3 p0, gvec3 p1) {
            //math overview:
            //ax + by + cz = d
            //dot(<a, b, c>, <x, y, z>) = d
            //<x, y, z> = v0 + dir * t
            //dot(<a, b, c>, (v0 + dir * t)) = d
            //dot(<a, b, c>, v0) + dot(<a, b, c>, dir) * t = d
            //t = (d - dot(<a, b, c>, v0)) / dot(<a, b, c>, dir)

            gvec3 dir = p1 - p0;
            double num = d - a * p0.x - b * p0.y - c * p0.z;
            double div = a * dir.x + b * dir.y + c * dir.z;
            double t = num / div;
            return p0 + dir * t;
        }

        /// <summary>
        ///     Finds an intersection between this plane and a specified finite line.
        /// </summary>
        /// <param name="p0">A point lying on an finite line</param>
        /// <param name="p1">A point lying on an finite line</param>
        /// <param name="intersection">The intersection between this plane and the specified finite line, if it exists</param>
        /// <returns>Whether or not there was an intersection</returns>
        public bool ConstrainedIntersection(gvec3 p0, gvec3 p1, ref gvec3 intersection) {
            gvec3 dir = p1 - p0;
            double num = d - a * p0.x - b * p0.y - c * p0.z;
            double div = a * dir.x + b * dir.y + c * dir.z;
            double t = num / div;
            if (t < 0) return false;
            if (t < (p1 - p0).Length) {
                intersection = p0 + dir * t;
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        ///     Transforms a point into plane space with x and y being the
        ///     position on the plane, and z being distance from the plane.
        ///     <para/>
        ///     This can be used for performing accelerated operations, such as
        ///     triangulation, insideness checks, and clipping, on 2D geometry.
        ///     This can be reversed with unprojectPoint.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public gvec3 ProjectPoint(gvec3 p) {
            //Console.WriteLine("Warning: very inefficient function projectPoint");
            //dmat4 mat = new dmat4(dquat.FromAxisAngle(0.0, new dvec3(a, b, c)));
            //return new g_vert3((dvec3)(mat * new dvec4(p.x, p.y, p.z, 1.0)));
            throw new NotImplementedException();
        }
        public gvec3 UnprojectPoint(gvec3 p) {
            throw new NotImplementedException();
        }

        public static Plane CCW(gvec3 v0, gvec3 v1, gvec3 v2) {
            gvec3 cross = gvec3.Cross(v1 - v0, v2 - v0).Normalized;
            return new Plane {
                a = cross.x,
                b = cross.y,
                c = cross.z,
                d = gvec3.Dot(cross, v0)
            };
        }
    }
}