﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace CSGeom.D2 {
    public class WeaklySimplePolygon : DiscreteBooleanSpace {
        public LineLoop verts;
        public List<LineLoop> holes;

        public bool IsPointInside(gvec2 point) {
            if(point.IsInside(verts)) {
                foreach(LineLoop loop in holes) {
                    if (point.IsInside(loop)) return false;
                }
                return true;
            }
            return false;
        }

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
        public class IntersectionPair {
            public gvec2 vert;
            public IntersectionInfo lhs, rhs;
        }
        public class IntersectionInfo {
            public int index;
            public int segment;
            public double param;
            public TraversalMode mode;
            public double dist => segment + param; //basically, how far it is along the loop if each segment = 1 unit
        }
        public class IntersectionResolution {
            public List<IntersectionPair> intersections;
            public Dictionary<int, List<int>> lhs;
            public Dictionary<int, List<int>> rhs;
        }
        public static IntersectionResolution GetIntersectionInfo(WeaklySimplePolygon lhs, WeaklySimplePolygon rhs) {
            IntersectionResolution res = new IntersectionResolution {
                intersections = new List<IntersectionPair>(),
                lhs = new Dictionary<int, List<int>>(),
                rhs = new Dictionary<int, List<int>>()
            };

            //-1 just refers to verts and not an index into holes
            for (int i = -1; i < lhs.holes.Count; i++) {
                LineLoop lhsLoop = (i == -1) ? lhs.verts : lhs.holes[i];
                for (int j = -1; j < rhs.holes.Count; j++) {
                    LineLoop rhsLoop = (j == -1) ? rhs.verts : rhs.holes[j];
                    List<LineLoop.LoopLoopIntersection> theseIntersections = LineLoop.AllIntersections(lhsLoop, rhsLoop);

                    foreach(LineLoop.LoopLoopIntersection info in theseIntersections) {
                        gvec2 lhsDir = lhsLoop[info.lhsIndex + 1] - lhsLoop[info.lhsIndex];
                        gvec2 rhsDir = rhsLoop[info.rhsIndex + 1] - rhsLoop[info.rhsIndex];

                        TraversalMode lhsMode = gvec2.Dot(lhsDir, rhsDir.RotatedCCW90()) > 0 ? TraversalMode.entering : TraversalMode.exiting;
                        TraversalMode rhsMode = gvec2.Dot(rhsDir, lhsDir.RotatedCCW90()) > 0 ? TraversalMode.entering : TraversalMode.exiting;

                        res.intersections.Add(new IntersectionPair {
                            vert = info.position,
                            lhs = new IntersectionInfo {
                                index = i,
                                segment = info.lhsIndex,
                                param = info.lhsParam,
                                mode = lhsMode
                            },
                            rhs = new IntersectionInfo {
                                index = j,
                                segment = info.rhsIndex,
                                param = info.rhsParam,
                                mode = rhsMode
                            }
                        });

                        if(!res.lhs.ContainsKey(i)) res.lhs[i] = new List<int>();
                        res.lhs[i].Add(res.intersections.Count - 1);
                        if (!res.rhs.ContainsKey(j)) res.rhs[j] = new List<int>();
                        res.rhs[j].Add(res.intersections.Count - 1);
                    }
                }
            }

            return res;
        }
        public static WeaklySimplePolygon Union(WeaklySimplePolygon lhs, WeaklySimplePolygon rhs) {
            WeaklySimplePolygon res = new WeaklySimplePolygon();

            IntersectionResolution ir = GetIntersectionInfo(lhs, rhs);

            //preprocessing
            for (int i = -1; i < lhs.holes.Count; i++) {
                if(i == -1 && (!ir.lhs.ContainsKey(-1)) && !lhs.verts[0].IsInside(rhs)) {

                }
                //if(ir.lhs.ContainsKey()
            }


            List<IntersectionPair> currentLoop = new List<IntersectionPair>();

            //this loop finds the next IntersectionPair in the loop
            //and when a loop is complete, outputs it into res
            while(ir.intersections.Count != 0) {
                //start off a random pair if we need to
                if (currentLoop.Count == 0) {
                    currentLoop.Add(ir.intersections[0]);
                }

                //find the next intersection and store it in nearestIntersection
                IntersectionPair nearestIntersection = null;
                IntersectionPair lastIntersection = currentLoop.Last();
                TraversalMode mode = (lastIntersection.lhs.mode == TraversalMode.exiting) ? TraversalMode.lhs : TraversalMode.rhs;
                //Dictionary<int, int> nextQueue =

                foreach (IntersectionPair info in ir.intersections) {
                    if (info.lhs.index == lastIntersection.lhs.index && info.rhs.index == lastIntersection.rhs.index && info != lastIntersection) {
                        if (mode == TraversalMode.lhs) {
                            //traversing lhs
                            if (nearestIntersection == null) {
                                nearestIntersection = info;
                            } else {
                                if(nearestIntersection.lhs.dist < lastIntersection.lhs.dist) {
                                    if(info.lhs.dist > lastIntersection.lhs.dist || info.lhs.dist < nearestIntersection.lhs.dist) {
                                        nearestIntersection = info;
                                    }
                                } else {
                                    if(info.lhs.dist < nearestIntersection.lhs.dist && info.lhs.dist > lastIntersection.lhs.dist) {
                                        nearestIntersection = info;
                                    }
                                }
                            }
                        } else {
                            //traversing rhs
                            if (nearestIntersection == null) {
                                nearestIntersection = info;
                            } else {
                                if (nearestIntersection.rhs.dist < lastIntersection.rhs.dist) {
                                    if (info.rhs.dist > lastIntersection.rhs.dist || info.rhs.dist < nearestIntersection.rhs.dist) {
                                        nearestIntersection = info;
                                    }
                                } else {
                                    if (info.rhs.dist < nearestIntersection.rhs.dist && info.rhs.dist > lastIntersection.rhs.dist) {
                                        nearestIntersection = info;
                                    }
                                }
                            }
                        }
                    }
                }

                //do what we must with this intersection, we might be done with the loop
                if (nearestIntersection == currentLoop.First()) {
                    //loop is done, create the loop with actual segments and such
                    LineLoop loop = new LineLoop();

                    //segment mode
                    TraversalMode segMode = (currentLoop.First().lhs.mode == TraversalMode.exiting) ? TraversalMode.lhs : TraversalMode.rhs;
                    for (int i = 0; i < currentLoop.Count; i++) {
                        IntersectionPair info = currentLoop[i];
                        IntersectionPair nextInfo = currentLoop[(i + 1) % currentLoop.Count];

                        loop.Add(info.vert);

                        //select the correct polygon and loop
                        WeaklySimplePolygon opPoly = (segMode == TraversalMode.lhs) ? lhs : rhs;
                        int loopIndex = (segMode == TraversalMode.lhs) ? info.lhs.index : info.rhs.index;
                        LineLoop opLoop = (loopIndex == -1) ? opPoly.verts : opPoly.holes[loopIndex];
                        int startSegment = ((segMode == TraversalMode.lhs) ? (info.lhs.segment + 1) : info.rhs.segment + 1) % opLoop.Count;
                        int endSegment = (segMode == TraversalMode.lhs) ? nextInfo.lhs.segment : nextInfo.rhs.segment;
                        int endSegmentPlusOneMod = (endSegment + 1) % opLoop.Count;

                        bool first = (segMode == TraversalMode.lhs) ? (info.lhs.dist > nextInfo.lhs.dist) : (info.rhs.dist > nextInfo.rhs.dist);
                        for(int currentSegment = startSegment;
                            (currentSegment != endSegmentPlusOneMod) || first;
                            currentSegment = (currentSegment + 1) % opLoop.Count) {
                            loop.Add(opLoop[currentSegment]);
                            if (first) first = false;
                        }

                        if (segMode == TraversalMode.lhs) segMode = TraversalMode.rhs; else segMode = TraversalMode.lhs;
                    }

                    res.holes.Add(loop);

                    foreach (IntersectionPair info in currentLoop) ir.intersections.Remove(info);
                    currentLoop.Clear();
                } else {
                    currentLoop.Add(nearestIntersection);
                }
            }

            return res;
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

    public class Polygon {
        public class PolygonNode {
            public PolygonNode parent;
            public readonly LineLoop loop;
            public readonly List<PolygonNode> children;

            public bool IsWellFormed(WindingDir correctDir) {
                if(loop.Winding == correctDir) {
                    WindingDir nextDir = (correctDir == WindingDir.ccw) ? WindingDir.cw : WindingDir.ccw;

                    foreach(PolygonNode n in children) {
                        if (!n.IsWellFormed(nextDir)) return false;
                    }

                    return true;
                } else {
                    return false;
                }
            }
            public void AccumulateWeaklySimplyPolygons(List<WeaklySimplePolygon> accumulator) {
                if(loop.Winding == WindingDir.ccw) {
                    WeaklySimplePolygon p = new WeaklySimplePolygon();
                    p.verts = loop;
                    foreach (PolygonNode n in children) {
                        p.holes.Add(n.loop);
                    }
                    accumulator.Add(p);
                }
                foreach(PolygonNode n in children) {
                    n.AccumulateWeaklySimplyPolygons(accumulator);
                }
            }

            public List<PolygonNode> FindHighestNodeEnclosedBy(LineLoop l) {
                List<PolygonNode> found = new List<PolygonNode>();
                foreach (PolygonNode n in children) {
                    if (n.loop.IsInsideOther(l)) {
                        found.Add(n);
                    }
                }
                if (found.Count != 0) {
                    return found;
                } else {
                    foreach(PolygonNode n in children) {
                        found = n.FindHighestNodeEnclosedBy(l);
                        if (found != null) return found;
                    }
                }
                return null;
            }
            public PolygonNode FindLowestNodeEnclosing(LineLoop l) {
                if (l.IsInsideOther(loop)) {
                    foreach(PolygonNode n in children) {
                        PolygonNode lowest = n.FindLowestNodeEnclosing(l);
                        if (lowest != null) return lowest;
                    }
                    return this;
                } else {
                    return null;
                }
            }

            public bool ContainsLoop(LineLoop l) {
                if (loop == l) return true;
                foreach (PolygonNode n in children) if (n.ContainsLoop(l)) return true;
                return false;
            }
            public bool AnyIntersects(LineLoop l) {
                if (l.IntersectsOther(loop)) {
                    return true;
                } else {
                    foreach (PolygonNode n in children) {
                        if (n.AnyIntersects(l)) return true;
                    }
                    return false;
                }
            }

            public void ChangeParent(PolygonNode newParent) {
                if(parent != null) parent.children.Remove(this);
                if(newParent != null) newParent.children.Add(this);
                parent = newParent;
            }

            public PolygonNode(LineLoop loop, PolygonNode parent) {
                this.loop = loop;
                this.parent = parent;
                this.children = new List<PolygonNode>();
            }
        }

        private List<PolygonNode> FindHighestNodeEnclosedBy(LineLoop l) {
            List<PolygonNode> found = new List<PolygonNode>();
            foreach (PolygonNode n in nodes) {
                if (n.loop.IsInsideOther(l)) {
                    found.Add(n);
                }
            }
            if (found.Count != 0) {
                return found;
            } else {
                foreach (PolygonNode n in nodes) {
                    found = n.FindHighestNodeEnclosedBy(l);
                    if (found != null) return found;
                }
                return null;
            }
        }
        private PolygonNode FindLowestNodeEnclosing(LineLoop l) {
            foreach (PolygonNode n in nodes) {
                PolygonNode lowest = n.FindLowestNodeEnclosing(l);
                if (lowest != null) return lowest;
            }
            return null;
        }

        public bool ContainsLoop(LineLoop l) {
            foreach (PolygonNode n in nodes) if (n.ContainsLoop(l)) return true;
            return false;
        }
        public bool AnyIntersects(LineLoop l) {
            foreach (PolygonNode n in nodes) {
                if (n.AnyIntersects(l)) return true;
            }
            return false;
        }

        public void InsertLoop(LineLoop l) {
            if (!AnyIntersects(l) && !ContainsLoop(l)) {
                PolygonNode lowestEnclosingLoop = FindLowestNodeEnclosing(l);
                List<PolygonNode> highestEnclosedByLoop = FindHighestNodeEnclosedBy(l);
                if (lowestEnclosingLoop == null) {
                    nodes.Add(new PolygonNode(l, null));
                } else {
                    lowestEnclosingLoop.children.Add(new PolygonNode(l, lowestEnclosingLoop));
                }
                if (highestEnclosedByLoop != null) {
                    foreach (PolygonNode n in highestEnclosedByLoop) {
                        n.ChangeParent(null);
                    }
                }
            } else {
                throw new Exception("Loop to be added intersects pre-existing loops");
            }
        }

        public readonly List<PolygonNode> nodes;


        public bool IsWellFormed {
            get {
                foreach(PolygonNode n in nodes) {
                    if (!n.IsWellFormed(WindingDir.ccw)) return false;
                }

                return true;
            }
        }

        public List<WeaklySimplePolygon> Simplify() {
            if (IsWellFormed) {
                List<WeaklySimplePolygon> res = new List<WeaklySimplePolygon>();
                foreach (PolygonNode n in nodes) {
                    n.AccumulateWeaklySimplyPolygons(res);
                }
                return res;
            } else {
                throw new Exception("Can't Simplify an ill-formed Polygon");
            }
        }


        public Polygon() {
            nodes = new List<PolygonNode>();
        }
    }
}
