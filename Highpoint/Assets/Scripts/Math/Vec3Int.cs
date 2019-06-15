using System;
using UnityEngine;

namespace Highpoint.Math {
    public struct Vec3Int : IEquatable<Vec3Int> {
        public int x;
        public int y;
        public int z;

        public Vec3Int(int x, int y, int z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vec3Int operator +(Vec3Int lhs, Vec3Int rhs) {
            return new Vec3Int(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z);
        }

        public static Vec3Int operator -(Vec3Int lhs, Vec3Int rhs) {
            return new Vec3Int(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z);
        }

        public static Vec3Int operator *(Vec3Int lhs, Vec3Int rhs) {
            return new Vec3Int(lhs.x * rhs.x, lhs.y * rhs.y, lhs.z * rhs.z);
        }

        public static Vec3Int operator *(Vec3Int lhs, int rhs) {
            return new Vec3Int(lhs.x * rhs, lhs.y * rhs, lhs.z * rhs);
        }

        public static bool operator ==(Vec3Int lhs, Vec3Int rhs) {
            return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
        }

        public static bool operator !=(Vec3Int lhs, Vec3Int rhs) {
            return !(lhs == rhs);
        }

        public override bool Equals(object obj) {
            if (!(obj is Vec3Int)) return false;

            return Equals((Vec3Int) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = x;
                hashCode = (hashCode * 397) ^ y;
                hashCode = (hashCode * 397) ^ z;
                return hashCode;
            }
        }

        public bool Equals(Vec3Int v) {
            return this == v;
        }

        public static explicit operator Vector3(Vec3Int v) {
            return new Vector3(v.x, v.y, v.z);
        }

        public static Vec3Int FloorToInt(Vector3 v) {
            return new Vec3Int(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y), Mathf.FloorToInt(v.z));
        }

        public override string ToString() {
            return $"({x}, {y}, {z})";
        }

        public static readonly Vec3Int Right = new Vec3Int(1, 0, 0);
        public static readonly Vec3Int Left = new Vec3Int(-1, 0, 0);
        public static readonly Vec3Int Up = new Vec3Int(0, 1, 0);
        public static readonly Vec3Int Down = new Vec3Int(0, -1, 0);
        public static readonly Vec3Int Forward = new Vec3Int(0, 0, 1);
        public static readonly Vec3Int Backward = new Vec3Int(0, 0, -1);
        public static readonly Vec3Int One = new Vec3Int(1, 1, 1);
        public static readonly Vec3Int Zero = new Vec3Int(0, 0, 0);
    }
}