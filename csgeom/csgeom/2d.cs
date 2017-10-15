using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace csgeom {
    public static class Math2 {
        public static bool SegmentsIntersecting(vert2 p0, vert2 p1, vert2 pa, vert2 pb, ref vert2 intersection) {
            vert2 dir0 = p1 - p0;
            vert2 dir1 = pb - pa;

            double t0 =
                ((dir1.x) * (p0.y - pa.y) - (dir1.y) * (p0.x - pa.x)) /
                ((dir1.y) * (dir0.x) - (dir1.x) * (dir0.y));

            if (t0 < 0 || t0 >= 1) return false;

            double t1 =
                ((dir0.x) * (p0.y - pa.y) - (dir0.y) * (p0.x - pa.x)) /
                ((dir1.y) * (dir0.x) - (dir1.x) * (dir0.y));

            if (t1 < 0 || t1 >= 1) return false;

            intersection = dir0 * t0 + p0;

            return true;
        }
    }

    public struct vert2 {
        public double x, y;

        public double length => Math.Sqrt(x * x + y * y);
        public double length2 => x * x + y * y;
        public vert2 normalized => this / length;

        public static double dot(vert2 lhs, vert2 rhs) {
            return lhs.x * rhs.x + lhs.y * rhs.y;
        }
        public static double cross(vert2 lhs, vert2 rhs) {
            return lhs.x * rhs.y - lhs.y * rhs.x;
        }

        public static vert2 interpolate(vert2 a, vert2 b, double a_to_b) {
            if (a_to_b > 1.0 || a_to_b < 0.0) throw new Exception("Interpolation range must be 0 to 1 inclusive");
            return new vert2 { x = a.x + (b.x - a.x) * a_to_b, y = a.y + (b.y - a.y) * a_to_b };
        }
        public vert2 interpolateTo(vert2 b, double a_to_b) {
            return interpolate(this, b, a_to_b);
        }

        public bool Identical(vert2 other) {
            return x == other.x && y == other.y;
        }

        public static vert2 operator +(vert2 lhs, vert2 rhs) {
            return new vert2 { x = lhs.x + rhs.x, y = lhs.y + rhs.y };
        }
        public static vert2 operator -(vert2 lhs, vert2 rhs) {
            return new vert2 { x = lhs.x - rhs.x, y = lhs.y - rhs.y };
        }
        public static vert2 operator *(vert2 lhs, double val) {
            return new vert2 { x = lhs.x * val, y = lhs.y * val };
        }
        public static vert2 operator /(vert2 lhs, double val) {
            return lhs * (1.0 / val);
        }

        public override string ToString() {
            return "<" + x.ToString("#####0.00") + "," + y.ToString("#####0.00") + ">";
        }
    }

    public struct primitive2 {
        public vert2 v0, v1, v2;
        public primitive2(vert2 v0, vert2 v1, vert2 v2) {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
        }
    }

    public enum triangulationCode {
        operationSuccess = 0,
        operationFailed = 1 << 0,
        insufficientVertices = 1 << 1,
        incorrectWinding = 1 << 2,
        notSimple = 1 << 3,
        robustnessFailure = 1 << 4
    }

    public enum windingDir {
        ccw,
        cw
    }

    public struct triangle2 {
        public vert2 v0, v1, v2;
        public vert2 this[int index] {
            get {
                if (!(index >= 0 && index <= 2)) {
                    return (index == 0) ? v0 : ((index == 1) ? v1 : v2);
                } else {
                    throw new Exception("Triangle bracket accessor out of bounds, only 0, 1, 2, allowed");
                }
            }
        }
        public const int Count = 3;
        public windingDir getWinding() {
            return (v1.x - v0.x) * (v0.y + v1.y) + (v2.x - v1.x) * (v1.y + v2.y) + (v0.x - v2.x) * (v2.y + v0.y) < 0 ? windingDir.ccw : windingDir.cw;
        }
    }

    public struct triangulationResult2 {
        public primitive2[] data;
        public triangulationCode code;
    }

    public class LineLoop2 {
        List<vert2> data;
        public double integrateccw() {
            double sum = 0;
            for (int i = 0; i < Count; i++) {
                vert2 v0 = this[i];
                vert2 v1 = this[(i + 1) % Count];
                sum += (v1.x - v0.x) * (v1.y + v0.y);
            }
            return sum;
        }

        public vert2 this[int index] => data[index];
        public void Add(vert2 vert) {
            data.Add(vert);
        }
        public void Insert(vert2 vert, int index) {
            data.Insert(index, vert);
        }
        public void Remove(int index) {
            data.RemoveAt(index);
        }
        public void Clear() {
            data.Clear();
        }
        public int Count => (data == null) ? 0 : data.Count;
        public windingDir CalculateWinding() {
            return integrateccw() < 0 ? windingDir.ccw : windingDir.cw;
        }


        /// <summary>
        ///     Connects two loops with an infinitesimal line, adding two duplicated vertices in the process
        /// </summary>
        public static LineLoop2 PseudoSimpleJoin(LineLoop2 loop0, int index0, LineLoop2 loop1, int index1, bool reverse0, bool reverse1) {
            LineLoop2 res = new LineLoop2();
            res.data = new List<vert2>(loop0.Count + loop1.Count + 2);
            for (int i = reverse0 ? loop0.Count - 1 : 0; reverse0 ? i >= 0 : i <= loop0.Count; i += reverse0 ? -1 : 1) {
                res.Add(loop0[(i + index0) % loop0.Count]);
            }
            for (int i = reverse1 ? loop1.Count : 0; reverse1 ? i >= 0 : i <= loop1.Count; i += reverse1 ? -1 : 1) {
                res.Add(loop1[(i + index1) % loop1.Count]);
            }
            return res;
        }

        public bool IntersectsAny(vert2 p0, vert2 p1, int skipIndex = -1) {
            for (int i0 = 0; i0 < Count; i0++) {
                int i1 = (i0 + 1) % Count;
                if (i0 != skipIndex && i1 != skipIndex) {
                    vert2 res = new vert2();
                    if (SimplePolygon.SegmentsIntersecting(p0, p1, this[i0], this[i1], ref res)) {
                        return true;
                    }
                }
            }
            return false;
        }
        public bool IntersectsAny(vert2 p0, int thisIndex) {
            vert2 p1 = this[thisIndex];
            for (int i0 = 0; i0 < Count; i0++) {
                int i1 = (i0 + 1) % Count;
                if (i0 != thisIndex && i1 != thisIndex) {
                    vert2 res = new vert2();
                    if (SimplePolygon.SegmentsIntersecting(p0, p1, this[i0], this[i1], ref res)) {
                        return true;
                    }
                }
            }
            return false;
        }

        public LineLoop2 Reversed() {
            List<vert2> rev = data.ToList();
            rev.Reverse();
            return new LineLoop2 { data = rev };
        }

        public LineLoop2 Clone() {
            return new LineLoop2(data);
        }

        public LineLoop2() {
            this.data = new List<vert2>();
        }
        public LineLoop2(vert2[] data) {
            if (data != null) this.data = new List<vert2>(data);
            else this.data = new List<vert2>();
        }
        public LineLoop2(IEnumerable<vert2> data) {
            if (data != null) this.data = data.ToList();
            else this.data = new List<vert2>();
        }
    }

    public class SimplePolygon {
        public LineLoop2 verts = new LineLoop2(null);

        

        /// <summary>
        ///     Checks if the winding of a triangle is counterclockwise
        /// </summary>
        /// <param name="v0">v0</param>
        /// <param name="v1">v1</param>
        /// <param name="v2">v2</param>
        public static bool IsCCW(vert2 v0, vert2 v1, vert2 v2) {
            //return (v0.x - v2.x) * (v1.y - v2.y) - (v1.x - v2.x) * (v0.y - v2.y) >= 0;
            //return v2.x * v0.y - v2.x * v1.y - v1.x * v0.y + v1.x * v1.y + v2.y * v1.x - v2.y * v0.x - v1.y * v1.x + v1.y * v0.x >= 0;
            return (v1.x - v0.x) * (v1.y + v0.y) + (v2.x - v1.x) * (v2.y + v1.y) + (v0.x - v2.x) * (v0.y + v2.y) <= 0;
        }

        /// <summary>
        ///     Returns whether or not a point is inside a ccw wound triangle
        /// </summary>
        /// <param name="pt">The point to test</param>
        /// <param name="v0">v0</param>
        /// <param name="v1">v1</param>
        /// <param name="v2">v2</param>
        public static bool InsideCCW(vert2 pt, vert2 v0, vert2 v1, vert2 v2) {
            bool a = IsCCW(pt, v0, v1);
            bool b = IsCCW(pt, v1, v2);
            bool c = IsCCW(pt, v2, v0);
            return (a && b && c);
        }

        /// <summary>
        ///     Triangulates the polygon
        /// </summary>
        /// <returns>The results of the triangulation including triangles and status codes</returns>
        public triangulationResult2 Triangulate() {
            //trivial check
            if (verts.Count < 3) {
                return new triangulationResult2 { code = triangulationCode.insufficientVertices };
            }

            for(int i = 0; i < verts.Count; i++) {
                int iw0 = i % verts.Count;
                int iw1 = (i + 1) % verts.Count;

                for (int j = 0; j < verts.Count; j++) {
                    int jw0 = j % verts.Count;
                    int jw1 = (j + 1) % verts.Count;
                    //don't check adjacent or the same segment
                    if(iw0 != jw0 && iw0 != jw1 && iw1 != jw0) {
                        vert2 vi0 = verts[iw0];
                        vert2 vi1 = verts[iw1];
                        vert2 vj0 = verts[jw0];
                        vert2 vj1 = verts[jw1];

                        //this is hacky and is only used because of reducing simple with holes to simple
                        if (!vi0.Identical(vj0) && !vi0.Identical(vj1) && !vj0.Identical(vi0) && !vj1.Identical(vi1)) {
                            vert2 res = new vert2();
                            if (SegmentsIntersecting(verts[iw0], verts[iw1], verts[jw0], verts[jw1], ref res)) {
                                return new triangulationResult2 { code = triangulationCode.notSimple };
                            }
                        }
                    }
                }
            }
            

            if (verts.CalculateWinding() != windingDir.ccw) {
                return new triangulationResult2 { code = triangulationCode.incorrectWinding };
            } else {
                if (verts.Count == 3) {
                    return new triangulationResult2 { data = new primitive2[] { new primitive2(verts[0], verts[1], verts[2]) } };
                } else {
                    LineLoop2 _verts = verts.Clone();
                    List<primitive2> _tris = new List<primitive2>(_verts.Count - 2);

                    int lastClippedIndex = -1;
                    int index = 0;

                    while (_verts.Count > 3) {
                        if (index == lastClippedIndex) return new triangulationResult2 { code = triangulationCode.robustnessFailure };

                        //wrap indices around
                        int v0Index = (index - 1 + _verts.Count) % _verts.Count;
                        int v1Index = (index + 0) % _verts.Count;
                        int v2Index = (index + 1) % _verts.Count;

                        vert2 v0 = _verts[v0Index];
                        vert2 v1 = _verts[v1Index];
                        vert2 v2 = _verts[v2Index];

                        
                        if (((v1.x - v0.x) * (v1.y + v0.y) + (v2.x - v1.x) * (v2.y + v1.y) + (v0.x - v2.x) * (v0.y + v2.y)) <= 0) {
                            bool anyInside = false;
                            for (int j = 0; j < _verts.Count; j++) {
                                if (j != v0Index && j != v1Index && j != v2Index) {

                                    //this is hacky and is only used because of reducing simple with holes to simple
                                    vert2 vj = _verts[j];
                                    if (!vj.Identical(v0) && !vj.Identical(v1) && !vj.Identical(v2)) {
                                        if (InsideCCW(_verts[j], v0, v1, v2)) {
                                            anyInside = true;
                                            break;
                                        }
                                    }
                                }
                            }

                            if (!anyInside) {
                                _tris.Add(new primitive2(v0, v1, v2));
                                _verts.Remove(index % _verts.Count);
                                lastClippedIndex = index;
                            }
                        }

                        index = (index + 1) % _verts.Count;
                    }
                    _tris.Add(new primitive2(_verts[0], _verts[1], _verts[2]));
                    return new triangulationResult2 { data = _tris.ToArray(), code = triangulationCode.operationSuccess };
                }
            }
            //code after this point cannot execute
        }
    }

    public class SimplePolygonWithHoles {
        public LineLoop2 verts;
        public List<LineLoop2> holes;

        bool mainToHoleIntersectsAnyHole(int mainIndex, int holeIndex, int holeVertexIndex) {
            vert2 mainVert = verts[mainIndex];
            vert2 holeVert = holes[holeIndex][holeVertexIndex];
            for(int i = 0; i < holes.Count; i++) {
                LineLoop2 hole = holes[i];

                if(i == holeIndex) {
                    if (hole.IntersectsAny(mainVert, holeVertexIndex)) return true; //check by index explicitly
                } else {
                    if (hole.IntersectsAny(mainVert, holeVert)) return true;
                }
            }
            return false;
        }
        bool holeToHoleIntersectsAnyHole(int hole0, int hole0Vertex, int hole1, int hole1Vertex) {
            vert2 p0 = holes[hole0][hole0Vertex];
            vert2 p1 = holes[hole1][hole1Vertex];
            for (int h = 0; h < holes.Count; h++) {
                LineLoop2 hole = holes[h];

                int skip = (h == hole0) ? hole0Vertex : ((h == hole1) ? hole1Vertex : -1);

                if (hole.IntersectsAny(p0, p1, skip)) return true;
            }
            return false;
        }

        public SimplePolygonWithHoles Clone() {
            SimplePolygonWithHoles res = new SimplePolygonWithHoles();
            res.verts = verts.Clone();
            res.holes = new List<LineLoop2>();
            foreach(LineLoop2 hole in holes) {
                res.holes.Add(hole.Clone());
            }
            return res;
        }

        public SimplePolygon simplify() {
            SimplePolygon res = new SimplePolygon();
            res.verts = verts.Clone();

            //each hole must be eliminated
            for (int currentHoleIndex = 0; currentHoleIndex < holes.Count; currentHoleIndex++) {
                LineLoop2 currentHole = holes[currentHoleIndex];

                bool holeDone = false;
                int foundVertexIndex = -1;
                int foundHoleIndex = -1;
                int foundOtherVertexIndex = -1;

                //check all the vertices of this hole
                for (int currentHoleVertexIndex = 0; currentHoleVertexIndex < currentHole.Count; currentHoleVertexIndex++) {
                    vert2 currentHoleVertex = currentHole[currentHoleVertexIndex];

                    //first check outer loop vertices
                    for(int mainVertexIndex = 0; mainVertexIndex < verts.Count; mainVertexIndex++) {
                        if(!mainToHoleIntersectsAnyHole(mainVertexIndex, currentHoleIndex, currentHoleVertexIndex) &&
                           !verts.IntersectsAny(currentHoleVertex, mainVertexIndex)) {
                            //we've found a connection between a hole and the outer loop!
                            holeDone = true;
                            foundVertexIndex = currentHoleVertexIndex;
                            foundHoleIndex = -1; //not a hole, its the main loop
                            foundOtherVertexIndex = mainVertexIndex;
                            break;
                        }
                    }

                    if (!holeDone) {
                        //check all the other holes
                        for (int otherHoleIndex = 0; otherHoleIndex < holes.Count; otherHoleIndex++) {
                            if (otherHoleIndex == currentHoleIndex) continue;
                            LineLoop2 otherHole = holes[otherHoleIndex];
                            
                            
                            for (int otherHoleVertexIndex = 0; otherHoleVertexIndex < otherHole.Count; otherHoleVertexIndex++) {
                                vert2 otherHoleVertex = otherHole[otherHoleVertexIndex];

                                if(!verts.IntersectsAny(currentHoleVertex, otherHoleVertex) &&
                                   !holeToHoleIntersectsAnyHole(currentHoleIndex, currentHoleVertexIndex, otherHoleIndex, otherHoleVertexIndex)) {
                                    holeDone = true;
                                    foundVertexIndex = currentHoleVertexIndex;
                                    foundHoleIndex = otherHoleIndex;
                                    foundOtherVertexIndex = otherHoleVertexIndex;
                                    break;
                                }
                            }

                            if (holeDone) break;
                        }
                    }

                    if (holeDone) break;
                }

                Console.WriteLine("holeDone: " + holeDone);
                Console.WriteLine("foundVertexIndex: " + foundVertexIndex);
                Console.WriteLine("foundHoleIndex: " + (foundHoleIndex == -1 ? "main" : foundVertexIndex.ToString()));
                Console.WriteLine("foundHoleVertexIndex: " + foundOtherVertexIndex);

                if(foundHoleIndex == -1) {
                    res.verts = LineLoop2.PseudoSimpleJoin(res.verts, foundOtherVertexIndex, currentHole, foundVertexIndex, false, false);
                    holes.RemoveAt(currentHoleIndex);
                    currentHoleIndex--;
                }

                //currentHoleIndex--;
            }
            return res;
        }

        public SimplePolygonWithHoles() {
            verts = new LineLoop2();
            holes = new List<LineLoop2>();
        }
    }
}
