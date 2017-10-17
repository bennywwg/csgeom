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

        public vert2(double x, double y) {
            this.x = x;
            this.y = y;
        }

        public static vert2 zero = new vert2(0, 0);
        public static vert2 i = new vert2(1, 0);
        public static vert2 j = new vert2(0, 1);
        public static vert2 right = i;
        public static vert2 up = j;
        public static vert2 left = -i;
        public static vert2 down = -j;

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
        public static vert2 operator -(vert2 rhs) {
            return new vert2 { x = -rhs.x, y = -rhs.y };
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
        
        bool _integralccw_needsUpdate;
        double _integralccw;
        public double Integral_y_dx {
            get {
                if(_integralccw_needsUpdate) {
                    _integralccw = Integrate_y_dx();
                    _integralccw_needsUpdate = false;
                }
                return _integralccw;
            }
        }
        double Integrate_y_dx() {
            double sum = 0;
            for (int i = 0; i < Count; i++) {
                vert2 v0 = this[i];
                vert2 v1 = this[(i + 1) % Count];
                sum += (v1.x - v0.x) * (v1.y + v0.y);
            }
            return sum;
        }
        
        public windingDir Winding => Integral_y_dx < 0 ? windingDir.ccw : windingDir.cw;

        public double Area => -Integral_y_dx * 0.5;

        public vert2 this[int index] {
            get {
                return data[index];
            }
            set {
                data[index] = value;
                _integralccw_needsUpdate = true;
            }
        }
        public void Add(vert2 vert) {
            data.Add(vert);
            _integralccw_needsUpdate = true;
        }
        public void Insert(vert2 vert, int index) {
            data.Insert(index, vert);
            _integralccw_needsUpdate = true;
        }
        public void Remove(int index) {
            data.RemoveAt(index);
            _integralccw_needsUpdate = true;
        }
        public void Clear() {
            data.Clear();
            _integralccw_needsUpdate = false;
            _integralccw = 0;
        }
        public int Count => data.Count;

        public List<vert2> Data() {
            return data.ToList();
        }
        
        /// <summary>
        ///     Connects two loops with an infinitesimal bridge, adding two duplicated vertices in the process
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

        public bool IntersectsAny(vert2 p0, vert2 p1) {
            for (int i0 = 0; i0 < Count; i0++) {
                int i1 = (i0 + 1) % Count;
                vert2 res = new vert2();
                if (Math2.SegmentsIntersecting(p0, p1, this[i0], this[i1], ref res)) {
                    return true;
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
                    if (Math2.SegmentsIntersecting(p0, p1, this[i0], this[i1], ref res)) {
                        return true;
                    }
                }
            }
            return false;
        }
        public bool IntersectsAny(int thisIndex0, int thisIndex1) {
            vert2 p0 = this[thisIndex0];
            vert2 p1 = this[thisIndex1];
            for (int i0 = 0; i0 < Count; i0++) {
                int i1 = (i0 + 1) % Count;
                if ((i0 != thisIndex0) && (i1 != thisIndex0) && (i0 != thisIndex1) && (i1 != thisIndex1)) {
                    vert2 res = new vert2();
                    if (Math2.SegmentsIntersecting(p0, p1, this[i0], this[i1], ref res)) {
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

        /// <summary>
        ///     Doesn't copy the data given to it, so it's faster
        /// </summary>
        /// <param name="rawData">The actual List to be used for this LineLoop, not copied</param>
        public static LineLoop2 Raw(List<vert2> rawData) {
            return new LineLoop2((rawData != null) ? rawData : new List<vert2>(), true, 0);
        }

        /// <summary>
        ///     Raw constructor
        /// </summary>
        LineLoop2(List<vert2> data, bool _integralccw_needsUpdate, double _integralccw) {
            this.data = data;
            this._integralccw_needsUpdate = _integralccw_needsUpdate;
            this._integralccw = _integralccw;
        }
        public LineLoop2() : this(new List<vert2>(), false, 0) {
        }
        public LineLoop2(vert2[] data) : this((data != null) ? new List<vert2>(data) : new List<vert2>(), true, 0) {
        }
        public LineLoop2(IEnumerable<vert2> data) : this((data != null) ? data.ToList() : new List<vert2>(), true, 0) {
        }
    }

    public class SimplePolygon {
        public LineLoop2 verts = new LineLoop2();

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
                            if (Math2.SegmentsIntersecting(verts[iw0], verts[iw1], verts[jw0], verts[jw1], ref res)) {
                                return new triangulationResult2 { code = triangulationCode.notSimple };
                            }
                        }
                    }
                }
            }
            

            if (verts.Winding != windingDir.ccw) {
                return new triangulationResult2 { code = triangulationCode.incorrectWinding };
            } else {
                if (verts.Count == 3) {
                    return new triangulationResult2 { data = new primitive2[] { new primitive2(verts[0], verts[1], verts[2]) } };
                } else {
                    LineLoop2 _verts = verts.Clone();
                    List<primitive2> _tris = new List<primitive2>(_verts.Count - 2);

                    int index = 0;

                    int attemptedVertices = 0;
                    while (_verts.Count > 3) {
                        attemptedVertices++;

                        //wrap indices around
                        int v0Index = (index - 1 + _verts.Count) % _verts.Count;
                        int v1Index = (index + 0) % _verts.Count;
                        int v2Index = (index + 1) % _verts.Count;

                        vert2 v0 = _verts[v0Index];
                        vert2 v1 = _verts[v1Index];
                        vert2 v2 = _verts[v2Index];


                        bool anyInside = false;
                        if (((v1.x - v0.x) * (v1.y + v0.y) + (v2.x - v1.x) * (v2.y + v1.y) + (v0.x - v2.x) * (v0.y + v2.y)) <= 0) {
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
                                attemptedVertices = 0;
                            }
                        }
                        
                        if (attemptedVertices == _verts.Count) return new triangulationResult2 { code = triangulationCode.robustnessFailure };
                        
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
        
        static bool AnyIntersections(LineLoop2[] loops, int vert0LoopIndex, int vert0Index, int vert1LoopIndex, int vert1Index) {
            vert2 p0 = loops[vert0LoopIndex][vert0Index];
            vert2 p1 = loops[vert1LoopIndex][vert1Index];
            for(int i = 0; i < loops.Length; i++) {
                if (i == vert0LoopIndex) {
                    if (loops[i].IntersectsAny(p1, vert0Index)) return true;
                } else if(i == vert1LoopIndex) {
                    if (loops[i].IntersectsAny(p0, vert1Index)) return true;
                } else {
                    if (loops[i].IntersectsAny(p0, p1)) return true;
                }
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

        public SimplePolygon Simplify() {
            List<LineLoop2> remainingLoops = holes.ToList();
            remainingLoops.Insert(0, verts.Clone());
            
            //iterate over each hole, trying to find a line that cuts the hole out without intersecting anything else
            while(remainingLoops.Count > 1) {
                LineLoop2 hole = remainingLoops[1];

                bool holeDone = false;
                int foundVertexIndex = -1;
                int foundLoopIndex = -1;
                int foundOtherVertexIndex = -1;

                //we have go into each loop including the main loop at index 0 
                for (int loopIndex = 0; loopIndex < remainingLoops.Count; loopIndex++) {
                    if (loopIndex == 1) continue;

                    LineLoop2 loop = remainingLoops[loopIndex];

                    //check each vertex in this hole...
                    for (int p0Index = 0; p0Index < hole.Count; p0Index++) {
                    vert2 p0 = hole[p0Index];


                        //and make a segment with each vertex in this loop...
                        for(int p1Index = 0; p1Index < loop.Count; p1Index++) {
                            vert2 p1 = loop[p1Index];
                            
                            //we have now obtained our line segment that must be checked for collision against every pre-existing line segment
                            //this operation is important enough to have its own function
                            if(!AnyIntersections(remainingLoops.ToArray(), 1, p0Index, loopIndex, p1Index)) {

                                //we're almost done, just make sure it isn't the wrong side of a hole cutting edge
                                bool valid = true;

                                {
                                    vert2 n0 = (p0 - p1);
                                    vert2 p0n0 = (loop[(p1Index - 1 + loop.Count) % loop.Count] - p1);
                                    vert2 p0n1 = (loop[(p1Index + 1) % loop.Count] - p1);

                                    double ap0n0 = Math.Atan2(p0n0.y, p0n0.x) + Math.PI * 2;
                                    double an0 = Math.Atan2(n0.y, n0.x) + Math.PI * 2; if (an0 < ap0n0) an0 += Math.PI * 2;
                                    double ap0n1 = Math.Atan2(p0n1.y, p0n1.x) + Math.PI * 2; if (ap0n1 < ap0n0) ap0n1 += Math.PI * 2;

                                    if (an0 > ap0n0 && an0 < ap0n1) valid = false;

                                }

                                if (valid) {
                                    vert2 n0 = (p1 - p0);
                                    vert2 p0n0 = (hole[(p1Index - 1 + hole.Count) % hole.Count] - p0);
                                    vert2 p0n1 = (hole[(p1Index + 1) % hole.Count] - p0);

                                    double ap0n0 = Math.Atan2(p0n0.y, p0n0.x) + Math.PI * 2;
                                    double an0 = Math.Atan2(n0.y, n0.x) + Math.PI * 2; if(an0 < ap0n0) an0 += Math.PI * 2;
                                    double ap0n1 = Math.Atan2(p0n1.y, p0n1.x) + Math.PI * 2; if (ap0n1 < ap0n0) ap0n1 += Math.PI * 2;

                                    if (an0 > ap0n0 && an0 < ap0n1) valid = false;
                                }

                                
                                if (valid) {
                                    holeDone = true;
                                    foundVertexIndex = p0Index;
                                    foundLoopIndex = loopIndex;
                                    foundOtherVertexIndex = p1Index;
                                    break;
                                }
                            }
                        }

                        if (holeDone) break;
                    }

                    if (holeDone) break;
                }

                if (!holeDone) throw new Exception("Found no way out for a hole");

                Console.WriteLine("holeDone: " + holeDone);
                Console.WriteLine("foundVertexIndex: " + foundVertexIndex);
                Console.WriteLine("foundHoleIndex: " + (foundLoopIndex == -1 ? "main" : foundVertexIndex.ToString()));
                Console.WriteLine("foundHoleVertexIndex: " + foundOtherVertexIndex);

                //we need this conditional to account for the main loop winding ccw and holes winding cw
                if (foundLoopIndex == 0) {
                    remainingLoops[0] = LineLoop2.PseudoSimpleJoin(remainingLoops[0], foundOtherVertexIndex, hole, foundVertexIndex, false, false);
                    remainingLoops.RemoveAt(1);
                } else {
                    remainingLoops[1] = LineLoop2.PseudoSimpleJoin(remainingLoops[foundLoopIndex], foundOtherVertexIndex, hole, foundVertexIndex, false, false);
                    remainingLoops.RemoveAt(foundLoopIndex);
                }
            }

            SimplePolygon res = new SimplePolygon();
            res.verts = remainingLoops[0];
            return res;
        }

        public SimplePolygonWithHoles() {
            verts = new LineLoop2();
            holes = new List<LineLoop2>();
        }
    }
}
