using System;
namespace CSGeom.D2 {
    public enum WindingDir {
        ccw,
        cw
    }

    public enum TraversalMode {
        entering,
        exiting,
        inline
    }

    public struct Primitive2 {
        public gvec2 v0, v1, v2;
        public Primitive2(gvec2 v0, gvec2 v1, gvec2 v2) {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
        }
    }
}
