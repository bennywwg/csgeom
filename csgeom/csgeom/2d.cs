using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace csgeom {
    public struct Primitive2 {
        public gvec2 v0, v1, v2;
        public Primitive2(gvec2 v0, gvec2 v1, gvec2 v2) {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
        }
    }

    public enum TriangulationCode {
        operationSuccess = 0,
        operationFailed = 1 << 0,
        insufficientVertices = 1 << 1,
        incorrectWinding = 1 << 2,
        notSimple = 1 << 3,
        robustnessFailure = 1 << 4
    }

    public enum WindingDir {
        ccw,
        cw
    }

    public struct TriangulationResult2 {
        public Primitive2[] data;
        public TriangulationCode code;
    }

    public class LineLoop2 {
        List<gvec2> data;
        
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

        public List<gvec2> Data() {
            return data.ToList();
        }
        
        public static LineLoop2 PseudoSimpleJoin(LineLoop2 loop0, int index0, LineLoop2 loop1, int index1, bool reverse0, bool reverse1) {
            List<gvec2> res = new List<gvec2>(loop0.Count + loop1.Count + 2);
            for (int i = reverse0 ? loop0.Count - 1 : 0; reverse0 ? i >= 0 : i <= loop0.Count; i += reverse0 ? -1 : 1) {
                res.Add(loop0[(i + index0) % loop0.Count]);
            }
            for (int i = reverse1 ? loop1.Count : 0; reverse1 ? i >= 0 : i <= loop1.Count; i += reverse1 ? -1 : 1) {
                res.Add(loop1[(i + index1) % loop1.Count]);
            }
            return new LineLoop2(res, true, 0);
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

        public LineLoop2 Reversed() {
            List<gvec2> rev = data.ToList();
            rev.Reverse();
            return new LineLoop2 { data = rev };
        }

        public LineLoop2 Clone() {
            return new LineLoop2(data);
        }
        
        public static LineLoop2 Raw(List<gvec2> rawData) {
            return new LineLoop2(rawData ?? new List<gvec2>(), true, 0);
        }

        public TriangulationResult2 Triangulate() {
            //trivial check
            if (Count < 3) {
                return new TriangulationResult2 { code = TriangulationCode.insufficientVertices };
            }

            
            if(!IsSimple()) {
                return new TriangulationResult2 { code = TriangulationCode.notSimple };
            }

            if (this.Winding != WindingDir.ccw) {
                return new TriangulationResult2 { code = TriangulationCode.incorrectWinding };
            } else {
                if (this.Count == 3) {
                    return new TriangulationResult2 { data = new Primitive2[] { new Primitive2(this[0], this[1], this[2]) } };
                } else {
                    LineLoop2 _verts = Clone();
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
        
        LineLoop2(List<gvec2> data, bool _integralccw_needsUpdate, double _integralccw) {
            this.data = data;
            this._integralccw_needsUpdate = _integralccw_needsUpdate;
            this._integralccw = _integralccw;
        }
        public LineLoop2() : this(new List<gvec2>(), false, 0) {
        }
        public LineLoop2(gvec2[] data) : this((data != null) ? new List<gvec2>(data) : new List<gvec2>(), true, 0) {
        }
        public LineLoop2(IEnumerable<gvec2> data) : this((data != null) ? data.ToList() : new List<gvec2>(), true, 0) {
        }
    }

    public class WeaklySimplePolygon {
        public LineLoop2 verts;
        public List<LineLoop2> holes;
        
        static bool AnyIntersections(LineLoop2[] loops, int vert0LoopIndex, int vert0Index, int vert1LoopIndex, int vert1Index) {
            gvec2 p0 = loops[vert0LoopIndex][vert0Index];
            gvec2 p1 = loops[vert1LoopIndex][vert1Index];
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

        public WeaklySimplePolygon Clone() {
            WeaklySimplePolygon res = new WeaklySimplePolygon {
                verts = verts.Clone(),
                holes = new List<LineLoop2>()
            };
            foreach (LineLoop2 hole in holes) {
                res.holes.Add(hole.Clone());
            }
            return res;
        }

        public LineLoop2 Simplify() {
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
                    gvec2 p0 = hole[p0Index];


                        //and make a segment with each vertex in this loop...
                        for(int p1Index = 0; p1Index < loop.Count; p1Index++) {
                            gvec2 p1 = loop[p1Index];
                            
                            //we have now obtained our line segment that must be checked for collision against every pre-existing line segment
                            //this operation is important enough to have its own function
                            if(!AnyIntersections(remainingLoops.ToArray(), 1, p0Index, loopIndex, p1Index)) {

                                //we're almost done, just make sure it isn't the wrong side of a hole cutting edge
                                bool valid = true;

                                {
                                    gvec2 n0 = (p0 - p1);
                                    gvec2 p0n0 = (loop[(p1Index - 1 + loop.Count) % loop.Count] - p1);
                                    gvec2 p0n1 = (loop[(p1Index + 1) % loop.Count] - p1);

                                    double ap0n0 = Math.Atan2(p0n0.y, p0n0.x) + Math.PI * 2;
                                    double an0 = Math.Atan2(n0.y, n0.x) + Math.PI * 2; if (an0 < ap0n0) an0 += Math.PI * 2;
                                    double ap0n1 = Math.Atan2(p0n1.y, p0n1.x) + Math.PI * 2; if (ap0n1 < ap0n0) ap0n1 += Math.PI * 2;

                                    if (an0 > ap0n0 && an0 < ap0n1) valid = false;

                                }

                                if (valid) {
                                    gvec2 n0 = (p1 - p0);
                                    gvec2 p0n0 = (hole[(p1Index - 1 + hole.Count) % hole.Count] - p0);
                                    gvec2 p0n1 = (hole[(p1Index + 1) % hole.Count] - p0);

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
            
            return remainingLoops[0];
        }

        public WeaklySimplePolygon() {
            verts = new LineLoop2();
            holes = new List<LineLoop2>();
        }
    }
}
