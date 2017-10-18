﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csgeom {
    public static class Math2 {
        public static bool SegmentsIntersecting(gvec2 p0, gvec2 p1, gvec2 pa, gvec2 pb, ref gvec2 intersection) {
            gvec2 dir0 = p1 - p0;
            gvec2 dir1 = pb - pa;

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
        
        public static bool IsCCW(gvec2 v0, gvec2 v1, gvec2 v2) => (v1.x - v0.x) * (v1.y + v0.y) + (v2.x - v1.x) * (v2.y + v1.y) + (v0.x - v2.x) * (v0.y + v2.y) <= 0;

        public static bool PointInCCWTriangle(gvec2 pt, gvec2 v0, gvec2 v1, gvec2 v2) => IsCCW(pt, v0, v1) && IsCCW(pt, v1, v2) && IsCCW(pt, v2, v0);
    }
}
