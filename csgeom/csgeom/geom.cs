using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csgeom {
    public enum geomCode {

    }

    public struct plane {
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
        public vert3 unconstrainedIntersection(vert3 p0, vert3 p1) {
            //math overview:
            //ax + by + cz = d
            //dot(<a, b, c>, <x, y, z>) = d
            //<x, y, z> = v0 + dir * t
            //dot(<a, b, c>, (v0 + dir * t)) = d
            //dot(<a, b, c>, v0) + dot(<a, b, c>, dir) * t = d
            //t = (d - dot(<a, b, c>, v0)) / dot(<a, b, c>, dir)

            vert3 dir = p1 - p0;
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
        public bool constrainedIntersection(vert3 p0, vert3 p1, ref vert3 intersection) {
            vert3 dir = p1 - p0;
            double num = d - a * p0.x - b * p0.y - c * p0.z;
            double div = a * dir.x + b * dir.y + c * dir.z;
            double t = num / div;
            if (t < 0) return false;
            if (t < (p1 - p0).length) {
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
        public vert3 projectPoint(vert3 p) {
            //Console.WriteLine("Warning: very inefficient function projectPoint");
            //dmat4 mat = new dmat4(dquat.FromAxisAngle(0.0, new dvec3(a, b, c)));
            //return new g_vert3((dvec3)(mat * new dvec4(p.x, p.y, p.z, 1.0)));
            throw new NotImplementedException();
        }
        public vert3 unprojectPoint(vert3 p) {
            throw new NotImplementedException();
        }

        public static plane ccw(vert3 v0, vert3 v1, vert3 v2) {
            vert3 cross = vert3.cross(v1 - v0, v2 - v0).normalized;
            return new plane {
                a = cross.x,
                b = cross.y,
                c = cross.z,
                d = vert3.dot(cross, v0)
            };
        }
    }

    /*
        it won't kill ya (chainsmokers), one time (jb), flashing lights (kanye), T-shirt (migos)
             
            */



    public struct vert3 {
        public double x, y, z;

        public double length => Math.Sqrt(x * x + y * y + z * z);
        public vert3 normalized => this / length;

        public static double dot(vert3 lhs, vert3 rhs) {
            return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z;
        }
        public static vert3 cross(vert3 lhs, vert3 rhs) {
            return new vert3 { x = lhs.y * rhs.z - lhs.z * rhs.y, y = lhs.z * rhs.x - lhs.x * rhs.z, z = lhs.x * rhs.y - lhs.y * rhs.x };
        }

        public static vert3 interpolate(vert3 a, vert3 b, double a_to_b) {
            if (a_to_b > 1.0 || a_to_b < 0.0) throw new Exception("Interpolation range must be 0 to 1 inclusive");
            return new vert3 { x = a.x + (b.x - a.x) * a_to_b, y = a.y + (b.y - a.y) * a_to_b, z = a.z + (b.z - a.z) * a_to_b };
        }
        public vert3 interpolateTo(vert3 b, double a_to_b) {
            return interpolate(this, b, a_to_b);
        }

        public static vert3 operator +(vert3 lhs, vert3 rhs) {
            return new vert3 { x = lhs.x + rhs.x, y = lhs.y + rhs.y, z = lhs.z + rhs.z };
        }
        public static vert3 operator -(vert3 lhs, vert3 rhs) {
            return new vert3 { x = lhs.x - rhs.x, y = lhs.y - rhs.y, z = lhs.z - rhs.z };
        }
        public static vert3 operator *(vert3 lhs, double val) {
            return new vert3 { x = lhs.x * val, y = lhs.y * val, z = lhs.z * val };
        }
        public static vert3 operator /(vert3 lhs, double val) {
            return lhs * (1.0 / val);
        }

        public override string ToString() {
            return "<" + x.ToString("#####0.00") + "," + y.ToString("#####0.00") + "," + z.ToString("#####0.00") + ">";
        }
    }



    public class polymesh {
        public struct edge {
            public int a, b;
        }

        public struct orderedEdge {
            public int e;
            public bool flipped;
        }

        public struct indexedOrderedEdgeLoop {
            public List<orderedEdge> path;
            public indexedOrderedEdgeLoop(params orderedEdge[] path) {
                this.path = path.ToList();
            }
        }

        public struct face {
            public List<indexedOrderedEdgeLoop> loops;
            public face(params indexedOrderedEdgeLoop[] loops) {
                this.loops = loops.ToList();
            }
        }

        public struct featureIndex {
            public int face, faceLoop, loopEdge;
        }

        public struct rawLoop {
            public List<vert3> verts;
            public vert3 this[int index] => verts[index];
            public rawLoop(params vert3[] verts) {
                this.verts = verts.ToList();
            }
        }

        public List<vert3> vertices;
        public List<edge> edges;
        public List<face> faces;

        int getEdgeIndex(edge e) {
            for (int i = 0; i < edges.Count; i++) {
                if ((e.a == edges[i].a && e.b == edges[i].b) || (e.a == edges[i].b && e.b == edges[i].a)) {
                    return i;
                }
            }
            throw new Exception("Degenerate mesh; Cannot find edge.");
        }
        featureIndex[] getEdgeFacesIndices(edge e) {
            return getEdgeFacesIndices(getEdgeIndex(e));
        }
        featureIndex[] getEdgeFacesIndices(int globalEdgeIndex) {
            featureIndex[] features = new featureIndex[2];
            int currentFeature = 0;

            for (int i = 0; i < faces.Count; i++) {
                if (currentFeature == 2) break;

                face f = faces[i];

                bool doneWithFace = false;
                for (int u = 0; u < f.loops.Count; u++) {
                    if (doneWithFace) break;

                    List<orderedEdge> faceEdges = f.loops[u].path;

                    for (int v = 0; v < faceEdges.Count; v++) {
                        if (faceEdges[v].e == globalEdgeIndex) {
                            features[currentFeature].face = i;
                            features[currentFeature].faceLoop = u;
                            features[currentFeature].loopEdge = v;

                            currentFeature++;
                            doneWithFace = true;
                            break;
                        }
                    }
                }
            }

            if (currentFeature == 0) throw new Exception("Degenerate mesh; An edge has no associated faces.");
            else if (currentFeature == 1) throw new Exception("Degenerate mesh; An edge only has one associated face.");
            else return features;
        }

        rawLoop getFaceVertLoop(int globalFaceIndex, int faceLoopIndex) {
            indexedOrderedEdgeLoop path = faces[globalFaceIndex].loops[faceLoopIndex];
            rawLoop loop = new rawLoop { verts = new List<vert3>(path.path.Count) };
            foreach (orderedEdge e in path.path) {
                loop.verts.Add(vertices[e.flipped ? edges[e.e].b : edges[e.e].a]);
            }
            return loop;
        }

        //this only works for faces that are triangles
        //public mesh toMesh() {
        //    mesh m = new mesh();
        //    for (int i = 0; i < faces.Count; i++) {
        //        rawLoop l = getFaceVertLoop(i, 0);
        //        vec3 v0 = (vec3)l[0];
        //        vec3 v1 = (vec3)l[1];
        //        vec3 v2 = (vec3)l[2];
        //        vec3 faceNormal = vec3.Cross(v1 - v0, v2 - v0).Normalized;

        //        int startIndex = m.verts.Count;

        //        m.verts.Add(v0);
        //        m.normals.Add(faceNormal);
        //        m.uvs.Add(v0.xy);
        //        m.indices.Add(startIndex);

        //        m.verts.Add(v1);
        //        m.normals.Add(faceNormal);
        //        m.uvs.Add(v1.xy);
        //        m.indices.Add(startIndex + 1);

        //        m.verts.Add(v2);
        //        m.normals.Add(faceNormal);
        //        m.uvs.Add(v2.xy);
        //        m.indices.Add(startIndex + 2);
        //    }
        //    return m;
        //}

        //simplest tetrahedron
        public polymesh() {
            vertices = new List<vert3>(new vert3[] {
                new vert3 {x = 0, y = 0, z = 0 },
                new vert3 {x = 1, y = 0, z = 0 },
                new vert3 {x = 0, y = 1, z = 0 },
                new vert3 {x = 0, y = 0, z = 1 }
            });

            edges = new List<edge>(new edge[] {
                new edge { a = 0, b = 1 },
                new edge { a = 0, b = 2 },
                new edge { a = 0, b = 3 },
                new edge { a = 1, b = 2 },
                new edge { a = 1, b = 3 },
                new edge { a = 2, b = 3 }
            });

            faces = new List<face>(new face[] {
                new face(
                    new indexedOrderedEdgeLoop(
                        new orderedEdge { e = 0, flipped = true },
                        new orderedEdge { e = 1, flipped = false },
                        new orderedEdge { e = 4, flipped = true }
                    )
                ),
                new face(
                    new indexedOrderedEdgeLoop(
                            new orderedEdge { e = 2, flipped = false },
                            new orderedEdge { e = 5, flipped = false },
                            new orderedEdge { e = 1, flipped = true }
                    )
                ),
                new face(
                    new indexedOrderedEdgeLoop(
                            new orderedEdge { e = 0, flipped = false },
                            new orderedEdge { e = 3, flipped = false },
                            new orderedEdge { e = 2, flipped = true }
                    )
                ),
                new face(
                    new indexedOrderedEdgeLoop(
                            new orderedEdge { e = 4, flipped = false },
                            new orderedEdge { e = 5, flipped = true },
                            new orderedEdge { e = 3, flipped = true }
                    )
                ),
            });
        }
    }
}