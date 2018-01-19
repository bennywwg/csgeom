using System;
using System.Collections.Generic;
using System.Linq;

namespace CSGeom.D2 {
    public class WeaklySimplePolygon {
        public LineLoop verts;
        public List<LineLoop> holes;

        static bool AnyIntersections(LineLoop[] loops, int vert0LoopIndex, int vert0Index, int vert1LoopIndex, int vert1Index) {
            gvec2 p0 = loops[vert0LoopIndex][vert0Index];
            gvec2 p1 = loops[vert1LoopIndex][vert1Index];
            for (int i = 0; i < loops.Length; i++) {
                if (i == vert0LoopIndex) {
                    if (loops[i].IntersectsAny(p1, vert0Index)) return true;
                } else if (i == vert1LoopIndex) {
                    if (loops[i].IntersectsAny(p0, vert1Index)) return true;
                } else {
                    if (loops[i].IntersectsAny(p0, p1)) return true;
                }
            }
            return false;
        }
        public LineLoop Simplify() {
            List<LineLoop> remainingLoops = holes.ToList();
            remainingLoops.Insert(0, verts.Clone());

            //iterate over each hole, trying to find a line that cuts the hole out without intersecting anything else
            while (remainingLoops.Count > 1) {
                LineLoop hole = remainingLoops[1];

                bool holeDone = false;
                int foundVertexIndex = -1;
                int foundLoopIndex = -1;
                int foundOtherVertexIndex = -1;

                //we have go into each loop including the main loop at index 0 
                for (int loopIndex = 0; loopIndex < remainingLoops.Count; loopIndex++) {
                    if (loopIndex == 1) continue; //

                    LineLoop loop = remainingLoops[loopIndex];

                    //check each vertex in this hole...
                    for (int p0Index = 0; p0Index < hole.Count; p0Index++) {
                        gvec2 p0 = hole[p0Index];


                        //and make a segment with each vertex in this loop...
                        for (int p1Index = 0; p1Index < loop.Count; p1Index++) {
                            gvec2 p1 = loop[p1Index];

                            //we have now obtained our line segment that must be checked for collision against every pre-existing line segment
                            //this operation is important enough to have its own function
                            if (!AnyIntersections(remainingLoops.ToArray(), 1, p0Index, loopIndex, p1Index)) {

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
                                    double an0 = Math.Atan2(n0.y, n0.x) + Math.PI * 2; if (an0 < ap0n0) an0 += Math.PI * 2;
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
                    remainingLoops[0] = LineLoop.PseudoSimpleJoin(remainingLoops[0], foundOtherVertexIndex, hole, foundVertexIndex, false, false);
                    remainingLoops.RemoveAt(1);
                } else {
                    remainingLoops[1] = LineLoop.PseudoSimpleJoin(remainingLoops[foundLoopIndex], foundOtherVertexIndex, hole, foundVertexIndex, false, false);
                    remainingLoops.RemoveAt(foundLoopIndex);
                }
            }

            return remainingLoops[0];
        }

        //this is used internally
        public class IntersectionInfo {
            public gvec2 vert;
            public int lhsIndex;
            public int rhsIndex;
            public int lhsSegment;
            public int rhsSegment;
            public double lhsParam;
            public double rhsParam;
            public TraversalMode lhsMode;
            public TraversalMode rhsMode;
            public double lhsDist => lhsSegment + lhsParam; //basically, how far it is along the loop if each segment = 1 unit
            public double rhsDist => rhsSegment + rhsParam; //basically, how far it is along the loop if each segment = 1 unit
        }
        public static List<IntersectionInfo> GetIntersectionInfo(WeaklySimplePolygon lhs, WeaklySimplePolygon rhs) {
            List<IntersectionInfo> intersections = new List<IntersectionInfo>();
            //-1 just refers to verts and not an index into holes
            for (int i = -1; i < lhs.holes.Count; i++) {
                LineLoop lhsLoop = (i == -1) ? lhs.verts : lhs.holes[i];
                for (int j = -1; j < rhs.holes.Count; j++) {
                    LineLoop rhsLoop = (j == -1) ? rhs.verts : rhs.holes[j];
                    List<LineLoop.LoopLoopIntersection> theseIntersections = LineLoop.AllIntersections(lhsLoop, rhsLoop);

                    intersections.AddRange(theseIntersections.Select(info => {
                        gvec2 lhsDir = lhsLoop[info.lhsIndex + 1] - lhsLoop[info.lhsIndex];
                        gvec2 rhsDir = rhsLoop[info.rhsIndex + 1] - rhsLoop[info.rhsIndex];

                        TraversalMode lhsMode = gvec2.Dot(lhsDir, rhsDir.RotatedCCW90()) > 0 ? TraversalMode.entering : TraversalMode.exiting;
                        TraversalMode rhsMode = gvec2.Dot(rhsDir, lhsDir.RotatedCCW90()) > 0 ? TraversalMode.entering : TraversalMode.exiting;

                        return new IntersectionInfo {
                            vert = info.position,
                            lhsIndex = i,
                            rhsIndex = j,
                            lhsSegment = info.lhsIndex,
                            rhsSegment = info.rhsIndex,
                            lhsParam = info.lhsParam,
                            rhsParam = info.rhsParam,
                            lhsMode = lhsMode,
                            rhsMode = rhsMode
                        };
                    }));
                }
            }

            return intersections;
        }
        public struct SegmentInfo {
            public gvec2 vert;
            public int originalSegment;
            public double param;
            public bool done;
            public int otherLoop;
            public int otherSegment;
        }
        public static WeaklySimplePolygon Union(WeaklySimplePolygon lhs, WeaklySimplePolygon rhs) {
            List<IntersectionInfo> intersections = GetIntersectionInfo(lhs, rhs);

            List<IntersectionInfo> currentLoop = new List<IntersectionInfo>();
            while(intersections.Count != 0) {
                if (currentLoop.Count == 0) {
                    currentLoop.Add(intersections[0]);
                    intersections.RemoveAt(0);
                }

                IntersectionInfo lastIntersection = currentLoop.Last();
                IntersectionInfo nearestIntersection = null;
                TraversalMode mode = (lastIntersection.lhsMode == TraversalMode.exiting) ? TraversalMode.lhs : TraversalMode.rhs;

                //find the next intersection and store it in nearestIntersection
                foreach (IntersectionInfo info in intersections) {
                    if (info.lhsIndex == lastIntersection.lhsIndex && info.rhsIndex == lastIntersection.rhsIndex) {
                        if ((mode & TraversalMode.lhs) != 0) {
                            //traversing lhs
                            if (nearestIntersection == null) {
                                nearestIntersection = info;
                            } else {
                                if (info.lhsDist < nearestIntersection.lhsDist) {
                                    if (nearestIntersection.lhsDist < lastIntersection.lhsDist) {
                                        nearestIntersection = info;
                                    } else {
                                        if (info.lhsDist < lastIntersection.lhsDist) {
                                            nearestIntersection = info;
                                        }
                                    }
                                }
                            }
                        } else {
                            //traversing rhs
                            if (nearestIntersection == null) {
                                nearestIntersection = info;
                            } else {
                                if (info.rhsDist < nearestIntersection.rhsDist) {
                                    if (nearestIntersection.rhsDist < lastIntersection.rhsDist) {
                                        nearestIntersection = info;
                                    } else {
                                        if (info.rhsDist < lastIntersection.rhsDist) {
                                            nearestIntersection = info;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if(nearestIntersection == currentLoop.First()) {
                    break; //loop is done
                } else {
                    currentLoop.Add(nearestIntersection);
                    intersections.Remove(nearestIntersection);
                }
            }











            lhs = lhs.Clone();
            rhs = rhs.Clone();

            //set up lists of metadata for the new loops
            List<SegmentInfo> lhsVerts = new List<SegmentInfo>(lhs.verts.Count);
            for (int i = 0; i < lhs.verts.Count; i++) {
                lhsVerts[i] = new SegmentInfo {
                    vert = lhs.verts[i],
                    originalSegment = i,
                    done = false,
                    otherLoop = -1,
                    otherSegment = -1
                };
            }
            List<List<SegmentInfo>> lhsHoles = new List<List<SegmentInfo>>(lhs.holes.Count);
            for (int u = 0; u < lhs.holes.Count; u++) {
                LineLoop verts = lhs.holes[u];
                lhsHoles[u] = new List<SegmentInfo>(verts.Count);
                for (int i = 0; i < verts.Count; i++) {
                    lhsVerts[i] = new SegmentInfo {
                        vert = verts[i],
                        originalSegment = i,
                        done = false,
                        otherLoop = -1,
                        otherSegment = -1
                    };
                }
            }
            List<SegmentInfo> rhsVerts = new List<SegmentInfo>(rhs.verts.Count);
            for (int i = 0; i < rhs.verts.Count; i++) {
                rhsVerts[i] = new SegmentInfo {
                    vert = rhs.verts[i],
                    originalSegment = i,
                    done = false,
                    otherLoop = -1,
                    otherSegment = -1
                };
            }
            List<List<SegmentInfo>> rhsHoles = new List<List<SegmentInfo>>(rhs.holes.Count);
            for (int u = 0; u < rhs.holes.Count; u++) {
                LineLoop verts = rhs.holes[u];
                rhsHoles[u] = new List<SegmentInfo>(verts.Count);
                for (int i = 0; i < verts.Count; i++) {
                    rhsVerts[i] = new SegmentInfo {
                        vert = verts[i],
                        originalSegment = i,
                        done = false,
                        otherLoop = -1,
                        otherSegment = -1
                    };
                }
            }

            //insert the intersections into the new loop
            for (int i = 0; i < intersections.Count; i++) {
                IntersectionInfo item = intersections[i];
                List<SegmentInfo> lhsList = (item.lhsIndex == -1) ? lhsVerts : lhsHoles[item.lhsIndex];
                List<SegmentInfo> rhsList = (item.rhsIndex == -1) ? rhsVerts : rhsHoles[item.rhsIndex];

                //these loops do two things, find the correct seg.originalIndex
                //and insert the intersection correctly based on seg.param
                if(true) {
                    int lastFoundIndex = -1;
                    for (int u = 0; u < lhsList.Count; u++) {
                        SegmentInfo seg = lhsList[u];
                        if (item.lhsSegment == seg.originalSegment) {
                            if (item.lhsParam >= seg.param) {
                                lastFoundIndex = u;
                            } else {
                                break;
                            }
                        }
                    }
                    lhsList.Insert(lastFoundIndex, new SegmentInfo {
                        vert = item.vert,
                        originalSegment = item.lhsSegment,
                        param = item.lhsParam,
                        done = false,
                        otherLoop = item.rhsIndex,
                        otherSegment = item.rhsSegment
                    });
                }
                if(true) {
                    int lastFoundIndex = -1;
                    for (int u = 0; u < rhsList.Count; u++) {
                        SegmentInfo seg = rhsList[u];
                        if (item.rhsSegment == seg.originalSegment) {
                            if (item.rhsParam >= seg.param) {
                                lastFoundIndex = u;
                            } else {
                                break;
                            }
                        }
                    }
                    rhsList.Insert(lastFoundIndex, new SegmentInfo {
                        vert = item.vert,
                        originalSegment = item.rhsSegment,
                        param = item.rhsParam,
                        done = false,
                        otherLoop = item.lhsIndex,
                        otherSegment = item.lhsSegment
                    });
                }

            }



            return null;
        }

        public WeaklySimplePolygon Clone() {
            WeaklySimplePolygon res = new WeaklySimplePolygon {
                verts = verts.Clone(),
                holes = new List<LineLoop>()
            };
            foreach (LineLoop hole in holes) {
                res.holes.Add(hole.Clone());
            }
            return res;
        }
        public WeaklySimplePolygon() {
            verts = new LineLoop();
            holes = new List<LineLoop>();
        }
    }
}
