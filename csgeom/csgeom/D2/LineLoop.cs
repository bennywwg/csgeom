using System;
using System.Linq;
using System.Collections.Generic;

namespace CSGeom.D2 {
    public enum TriangulationCode {
        operationSuccess = 0,
        operationFailed = 1 << 0,
        insufficientVertices = 1 << 1,
        incorrectWinding = 1 << 2,
        notSimple = 1 << 3,
        robustnessFailure = 1 << 4
    }

    public struct TriangulationResult2 {
        public Primitive2[] data;
        public TriangulationCode code;
    }

    public class LineLoop {
        List<gvec2> data;

        bool _integralccw_needsUpdate;
        double _integralccw;
        public double Integral_y_dx {
            get {
                if (_integralccw_needsUpdate) {
                    _integralccw = Integrate_y_dx();
                    _integralccw_needsUpdate = false;
                }
                return _integralccw;
            }
        }
        double Integrate_y_dx() {
            double sum = 0;
            for (int i = 0; i < Count; i++) {
                gvec2 v0 = this[i];
                gvec2 v1 = this[(i + 1) % Count];
                sum += (v1.x - v0.x) * (v1.y + v0.y);
            }
            return sum;
        }

        public WindingDir Winding => Integral_y_dx < 0 ? WindingDir.ccw : WindingDir.cw;

        public double Area => -Integral_y_dx * 0.5;

        public bool IsSimple() {
            for (int i = 0; i < Count; i++) {
                int iw0 = i % Count;
                int iw1 = (i + 1) % Count;

                for (int j = 0; j < Count; j++) {
                    int jw0 = j % Count;
                    int jw1 = (j + 1) % Count;

                    if (iw0 != jw0 && iw0 != jw1 && iw1 != jw0) {
                        gvec2 vi0 = this[iw0];
                        gvec2 vi1 = this[iw1];
                        gvec2 vj0 = this[jw0];
                        gvec2 vj1 = this[jw1];

                        if (!vi0.Identical(vj0) && !vi0.Identical(vj1) && !vj0.Identical(vi0) && !vj1.Identical(vi1)) {
                            gvec2 res = new gvec2();
                            if (Math2.SegmentsIntersecting(vi0, vi1, vj0, vj1, ref res)) {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        public gvec2 this[int index] {
            get {
                if (index == Count) return data[0];
                return data[index];
            }
            set {
                data[index] = value;
                _integralccw_needsUpdate = true;
            }
        }
        public void Add(gvec2 vert) {
            data.Add(vert);
            _integralccw_needsUpdate = true;
        }
        public void Insert(gvec2 vert, int index) {
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

        public bool IsInside(gvec2 point) {
            gvec2 endOfRay = point + new gvec2(10000.0, 10000.0);
            bool inside = false;
            for (int i = 0; i < Count; i++) {
                gvec2 ignoreThis = gvec2.zero;
                if (Math2.SegmentsIntersecting(this[i], this[(i + 1 == Count) ? 0 : i + 1], point, endOfRay, ref ignoreThis)) {
                    inside = !inside;
                }
            }
            return inside;
        }

        public List<gvec2> Data() {
            return data.ToList();
        }

        public static LineLoop PseudoSimpleJoin(LineLoop loop0, int index0, LineLoop loop1, int index1, bool reverse0, bool reverse1) {
            List<gvec2> res = new List<gvec2>(loop0.Count + loop1.Count + 2);
            for (int i = reverse0 ? loop0.Count - 1 : 0; reverse0 ? i >= 0 : i <= loop0.Count; i += reverse0 ? -1 : 1) {
                res.Add(loop0[(i + index0) % loop0.Count]);
            }
            for (int i = reverse1 ? loop1.Count : 0; reverse1 ? i >= 0 : i <= loop1.Count; i += reverse1 ? -1 : 1) {
                res.Add(loop1[(i + index1) % loop1.Count]);
            }
            return new LineLoop(res, true, 0);
        }

        public bool IntersectsAny(gvec2 p0, gvec2 p1) {
            for (int i0 = 0; i0 < Count; i0++) {
                int i1 = (i0 + 1) % Count;
                gvec2 res = new gvec2();
                if (Math2.SegmentsIntersecting(p0, p1, this[i0], this[i1], ref res)) {
                    return true;
                }
            }
            return false;
        }
        public bool IntersectsAny(gvec2 p0, int thisIndex) {
            gvec2 p1 = this[thisIndex];
            for (int i0 = 0; i0 < Count; i0++) {
                int i1 = (i0 + 1) % Count;
                if (i0 != thisIndex && i1 != thisIndex) {
                    gvec2 res = new gvec2();
                    if (Math2.SegmentsIntersecting(p0, p1, this[i0], this[i1], ref res)) {
                        return true;
                    }
                }
            }
            return false;
        }
        public bool IntersectsAny(int thisIndex0, int thisIndex1) {
            gvec2 p0 = this[thisIndex0];
            gvec2 p1 = this[thisIndex1];
            for (int i0 = 0; i0 < Count; i0++) {
                int i1 = (i0 + 1) % Count;
                if ((i0 != thisIndex0) && (i1 != thisIndex0) && (i0 != thisIndex1) && (i1 != thisIndex1)) {
                    gvec2 res = new gvec2();
                    if (Math2.SegmentsIntersecting(p0, p1, this[i0], this[i1], ref res)) {
                        return true;
                    }
                }
            }
            return false;
        }
        public List<KeyValuePair<int, gvec2>> AllIntersectionsBySegment(gvec2 p0, gvec2 p1) {
            List<KeyValuePair<int, gvec2>> res = new List<KeyValuePair<int, gvec2>>();
            for (int i = 0; i < Count; i++) {
                gvec2 this0 = this[i];
                gvec2 this1 = this[(i + 1) % Count];
                gvec2 intersection = new gvec2();
                if (Math2.SegmentsIntersecting(p0, p1, this0, this1, ref intersection)) {
                    res.Add(new KeyValuePair<int, gvec2>(i, intersection));
                }
            }
            return res;
        }
        public List<Tuple<int, bool, double, gvec2>> AllIntersectionsByParameter(gvec2 p0, gvec2 p1) {
            //index, in/out, parameter, position
            List<Tuple<int, bool, double, gvec2>> res = new List<Tuple<int, bool, double, gvec2>>();
            for (int i = 0; i < Count; i++) {
                gvec2 this0 = this[i];
                gvec2 this1 = this[(i + 1) % Count];
                gvec2 intersection = new gvec2();
                double t0 = 0, t1 = 0;
                if (Math2.SegmentsIntersecting(p0, p1, this0, this1, ref intersection, ref t0, ref t1)) {
                    gvec2 a = p1 - p0;
                    gvec2 b = this1 - this0;
                    res.Add(new Tuple<int, bool, double, gvec2>(i, a.y * b.x - a.x * b.y > 0, t0, intersection));
                }
            }
            return res.OrderByDescending(tuple => -tuple.Item3).ToList();
        }


        public struct LoopLoopIntersection {
            public gvec2 position;
            public int lhsIndex;
            public int rhsIndex;
            public double lhsParam;
            public double rhsParam;
        }
        public static List<LoopLoopIntersection> AllIntersections(LineLoop lhs, LineLoop rhs) {
            List<LoopLoopIntersection> res = new List<LoopLoopIntersection>();
            for (int i = 0; i < lhs.Count; i++) {
                gvec2 a0 = lhs[i];
                gvec2 a1 = lhs[((i + 1) == lhs.Count) ? 0 : i + 1];
                for (int j = 0; j < rhs.Count; j++) {
                    gvec2 b0 = rhs[j];
                    gvec2 b1 = rhs[((j + 1) == rhs.Count) ? 0 : j + 1];


                    double param0 = 0, param1 = 0;
                    gvec2 intersection = new gvec2();
                    if(Math2.SegmentsIntersecting(a0, a1, b0, b1, ref intersection, ref param0, ref param1)) {
                        res.Add(new LoopLoopIntersection {
                            position = intersection,
                            lhsIndex = i,
                            rhsIndex = j,
                            lhsParam = param0,
                            rhsParam = param1
                        });
                    }
                }
            }
            return res;
        }

        public void Reverse() {
            data.Reverse();
        }
        public LineLoop Reversed() {
            List<gvec2> rev = data.ToList();
            rev.Reverse();
            return new LineLoop { data = rev };
        }

        public LineLoop Clone() {
            return new LineLoop(data);
        }

        public static LineLoop Raw(List<gvec2> rawData) {
            return new LineLoop(rawData ?? new List<gvec2>(), true, 0);
        }

        public TriangulationResult2 Triangulate() {

            //trivial check
            if (Count < 3) {
                return new TriangulationResult2 { code = TriangulationCode.insufficientVertices };
            }

            if (!IsSimple()) {
                return new TriangulationResult2 { code = TriangulationCode.notSimple };
            }

            if (this.Winding != WindingDir.ccw) {
                return new TriangulationResult2 { code = TriangulationCode.incorrectWinding };
            } else {
                if (this.Count == 3) {
                    return new TriangulationResult2 { data = new Primitive2[] { new Primitive2(this[0], this[1], this[2]) } };
                } else {
                    LineLoop _verts = Clone();
                    List<Primitive2> _tris = new List<Primitive2>(_verts.Count - 2);

                    int index = 0;

                    int attemptedVertices = 0;
                    while (_verts.Count > 3) {
                        attemptedVertices++;

                        //wrap indices around
                        int v0Index = (index - 1 + _verts.Count) % _verts.Count;
                        int v1Index = (index + 0) % _verts.Count;
                        int v2Index = (index + 1) % _verts.Count;

                        gvec2 v0 = _verts[v0Index];
                        gvec2 v1 = _verts[v1Index];
                        gvec2 v2 = _verts[v2Index];


                        bool anyInside = false;
                        if (((v1.x - v0.x) * (v1.y + v0.y) + (v2.x - v1.x) * (v2.y + v1.y) + (v0.x - v2.x) * (v0.y + v2.y)) <= 0) {
                            for (int j = 0; j < _verts.Count; j++) {
                                if (j != v0Index && j != v1Index && j != v2Index) {

                                    //this is hacky and is only used because of reducing simple with holes to simple
                                    gvec2 vj = _verts[j];
                                    if (!vj.Identical(v0) && !vj.Identical(v1) && !vj.Identical(v2)) {
                                        if (Math2.PointInCCWTriangle(_verts[j], v0, v1, v2)) {
                                            anyInside = true;
                                            break;
                                        }
                                    }
                                }
                            }

                            if (!anyInside) {
                                _tris.Add(new Primitive2(v0, v1, v2));
                                _verts.Remove(index % _verts.Count);
                                attemptedVertices = 0;
                            }
                        }

                        if (attemptedVertices == _verts.Count) return new TriangulationResult2 { code = TriangulationCode.robustnessFailure };

                        index = (index + 1) % _verts.Count;
                    }
                    _tris.Add(new Primitive2(_verts[0], _verts[1], _verts[2]));
                    return new TriangulationResult2 { data = _tris.ToArray(), code = TriangulationCode.operationSuccess };
                }
            }
        }

        LineLoop(List<gvec2> data, bool _integralccw_needsUpdate, double _integralccw) {
            this.data = data;
            this._integralccw_needsUpdate = _integralccw_needsUpdate;
            this._integralccw = _integralccw;
        }
        public LineLoop() : this(new List<gvec2>(), false, 0) {
        }
        public LineLoop(gvec2[] data) : this((data != null) ? new List<gvec2>(data) : new List<gvec2>(), true, 0) {
        }
        public LineLoop(IEnumerable<gvec2> data) : this((data != null) ? data.ToList() : new List<gvec2>(), true, 0) {
        }
    }
}
