using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csgeom {
#pragma warning disable IDE1006 // Naming Styles
    public struct gvec2 : IEquatable<gvec2> {
#pragma warning restore IDE1006 // Naming Styles
        public double x, y;

        public double Length => Math.Sqrt(Length2);
        public double Length2 => x * x + y * y;
        public gvec2 Normalized => this / Length;

        public static double Dot(gvec2 lhs, gvec2 rhs) {
            return lhs.x * rhs.x + lhs.y * rhs.y;
        }
        public static double Cross(gvec2 lhs, gvec2 rhs) {
            return lhs.x * rhs.y - lhs.y * rhs.x;
        }

        public static gvec2 Interpolate(gvec2 a, gvec2 b, double a_to_b) {
            if (a_to_b > 1.0 || a_to_b < 0.0) throw new Exception("Interpolation range must be 0 to 1 inclusive");
            return new gvec2 { x = a.x + (b.x - a.x) * a_to_b, y = a.y + (b.y - a.y) * a_to_b };
        }
        public gvec2 InterpolateTo(gvec2 b, double a_to_b) {
            return Interpolate(this, b, a_to_b);
        }

        public bool Identical(gvec2 other) {
            return x == other.x && y == other.y;
        }

        public static gvec2 zero = new gvec2(0, 0);
        public static gvec2 i = new gvec2(1, 0);
        public static gvec2 j = new gvec2(0, 1);
        public static gvec2 right = i;
        public static gvec2 up = j;
        public static gvec2 left = -i;
        public static gvec2 down = -j;
        
        public static gvec2 operator +(gvec2 lhs, gvec2 rhs) {
            return new gvec2 { x = lhs.x + rhs.x, y = lhs.y + rhs.y };
        }
        public static gvec2 operator -(gvec2 lhs, gvec2 rhs) {
            return new gvec2 { x = lhs.x - rhs.x, y = lhs.y - rhs.y };
        }
        public static gvec2 operator *(gvec2 lhs, double val) {
            return new gvec2 { x = lhs.x * val, y = lhs.y * val };
        }
        public static gvec2 operator /(gvec2 lhs, double val) {
            return lhs * (1.0 / val);
        }
        public static gvec2 operator -(gvec2 rhs) {
            return new gvec2 { x = -rhs.x, y = -rhs.y };
        }

        public static bool operator ==(gvec2 lhs, gvec2 rhs) => lhs.Equals(rhs);
        public static bool operator !=(gvec2 lhs, gvec2 rhs) => !(lhs == rhs);

        public gvec2(double x, double y) {
            this.x = x;
            this.y = y;
        }

        public override string ToString() {
            return "<" + x.ToString("#####0.00") + "," + y.ToString("#####0.00") + ">";
        }

        public override bool Equals(object obj) {
            return obj is gvec2 && Equals((gvec2)obj);
        }

        public bool Equals(gvec2 other) {
            return x == other.x &&
                   y == other.y;
        }
    }

#pragma warning disable IDE1006 // Naming Styles
    public struct gvec3 {
#pragma warning restore IDE1006 // Naming Styles
        public double x, y, z;

        public double Length => Math.Sqrt(Length2);
        public double Length2 => x * x + y * y + z * z;
        public gvec3 Normalized => this / Length;

        public static double Dot(gvec3 lhs, gvec3 rhs) {
            return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z;
        }
        public static gvec3 Cross(gvec3 lhs, gvec3 rhs) {
            return new gvec3 { x = lhs.y * rhs.z - lhs.z * rhs.y, y = lhs.z * rhs.x - lhs.x * rhs.z, z = lhs.x * rhs.y - lhs.y * rhs.x };
        }

        public static gvec3 Interpolate(gvec3 a, gvec3 b, double a_to_b) {
            if (a_to_b > 1.0 || a_to_b < 0.0) throw new Exception("Interpolation range must be 0 to 1 inclusive");
            return new gvec3 { x = a.x + (b.x - a.x) * a_to_b, y = a.y + (b.y - a.y) * a_to_b, z = a.z + (b.z - a.z) * a_to_b };
        }
        public gvec3 InterpolateTo(gvec3 b, double a_to_b) {
            return Interpolate(this, b, a_to_b);
        }

        public bool Identical(gvec3 other) {
            return x == other.x && y == other.y && z == other.z;
        }

        public static gvec3 zero = new gvec3(0, 0, 0);
        public static gvec3 i = new gvec3(1, 0, 0);
        public static gvec3 j = new gvec3(0, 1, 0);
        public static gvec3 k = new gvec3(0, 0, 1);
        public static gvec3 right = i;
        public static gvec3 up = j;
        public static gvec3 left = -i;
        public static gvec3 down = -j;
        public static gvec3 forward = k;
        public static gvec3 backward = -k;

        public static gvec3 operator +(gvec3 lhs, gvec3 rhs) {
            return new gvec3 { x = lhs.x + rhs.x, y = lhs.y + rhs.y, z = lhs.z + rhs.z };
        }
        public static gvec3 operator -(gvec3 lhs, gvec3 rhs) {
            return new gvec3 { x = lhs.x - rhs.x, y = lhs.y - rhs.y, z = lhs.z - rhs.z };
        }
        public static gvec3 operator *(gvec3 lhs, double val) {
            return new gvec3 { x = lhs.x * val, y = lhs.y * val, z = lhs.z * val };
        }
        public static gvec3 operator /(gvec3 lhs, double val) {
            return lhs * (1.0 / val);
        }
        public static gvec3 operator -(gvec3 rhs) {
            return new gvec3(rhs.x, rhs.y, rhs.z);
        }

        public gvec3(double x, double y, double z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override string ToString() {
            return "<" + x.ToString("#####0.00") + "," + y.ToString("#####0.00") + "," + z.ToString("#####0.00") + ">";
        }
    }
}
