using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGeom {


    public class Polymesh {
        public struct Edge {
            public int a, b;
        }

        public struct OrderedEdge {
            public int e;
            public bool flipped;
        }

        public struct IndexedOrderedEdgeLoop {
            public List<OrderedEdge> path;
            public IndexedOrderedEdgeLoop(params OrderedEdge[] path) {
                this.path = path.ToList();
            }
        }

        public struct Face {
            public List<IndexedOrderedEdgeLoop> loops;
            public Face(params IndexedOrderedEdgeLoop[] loops) {
                this.loops = loops.ToList();
            }
        }

        public struct FeatureIndex {
            public int face, faceLoop, loopEdge;
        }

        public struct RawLoop {
            public List<gvec3> verts;
            public gvec3 this[int index] => verts[index];
            public RawLoop(params gvec3[] verts) {
                this.verts = verts.ToList();
            }
        }

        public List<gvec3> vertices;
        public List<Edge> edges;
        public List<Face> faces;

        int GetEdgeIndex(Edge e) {
            for (int i = 0; i < edges.Count; i++) {
                if ((e.a == edges[i].a && e.b == edges[i].b) || (e.a == edges[i].b && e.b == edges[i].a)) {
                    return i;
                }
            }
            throw new Exception("Degenerate mesh; Cannot find edge.");
        }
        FeatureIndex[] GetEdgeFacesIndices(Edge e) {
            return GetEdgeFacesIndices(GetEdgeIndex(e));
        }
        FeatureIndex[] GetEdgeFacesIndices(int globalEdgeIndex) {
            FeatureIndex[] features = new FeatureIndex[2];
            int currentFeature = 0;

            for (int i = 0; i < faces.Count; i++) {
                if (currentFeature == 2) break;

                Face f = faces[i];

                bool doneWithFace = false;
                for (int u = 0; u < f.loops.Count; u++) {
                    if (doneWithFace) break;

                    List<OrderedEdge> faceEdges = f.loops[u].path;

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

        RawLoop GetFaceVertLoop(int globalFaceIndex, int faceLoopIndex) {
            IndexedOrderedEdgeLoop path = faces[globalFaceIndex].loops[faceLoopIndex];
            RawLoop loop = new RawLoop { verts = new List<gvec3>(path.path.Count) };
            foreach (OrderedEdge e in path.path) {
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
        public Polymesh() {
            vertices = new List<gvec3>(new gvec3[] {
                new gvec3 {x = 0, y = 0, z = 0 },
                new gvec3 {x = 1, y = 0, z = 0 },
                new gvec3 {x = 0, y = 1, z = 0 },
                new gvec3 {x = 0, y = 0, z = 1 }
            });

            edges = new List<Edge>(new Edge[] {
                new Edge { a = 0, b = 1 },
                new Edge { a = 0, b = 2 },
                new Edge { a = 0, b = 3 },
                new Edge { a = 1, b = 2 },
                new Edge { a = 1, b = 3 },
                new Edge { a = 2, b = 3 }
            });

            faces = new List<Face>(new Face[] {
                new Face(
                    new IndexedOrderedEdgeLoop(
                        new OrderedEdge { e = 0, flipped = true },
                        new OrderedEdge { e = 1, flipped = false },
                        new OrderedEdge { e = 4, flipped = true }
                    )
                ),
                new Face(
                    new IndexedOrderedEdgeLoop(
                            new OrderedEdge { e = 2, flipped = false },
                            new OrderedEdge { e = 5, flipped = false },
                            new OrderedEdge { e = 1, flipped = true }
                    )
                ),
                new Face(
                    new IndexedOrderedEdgeLoop(
                            new OrderedEdge { e = 0, flipped = false },
                            new OrderedEdge { e = 3, flipped = false },
                            new OrderedEdge { e = 2, flipped = true }
                    )
                ),
                new Face(
                    new IndexedOrderedEdgeLoop(
                            new OrderedEdge { e = 4, flipped = false },
                            new OrderedEdge { e = 5, flipped = true },
                            new OrderedEdge { e = 3, flipped = true }
                    )
                ),
            });
        }
    }
}
