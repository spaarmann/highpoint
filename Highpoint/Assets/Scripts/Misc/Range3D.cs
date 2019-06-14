using System;
using System.Diagnostics;
using UnityEngine;

namespace Highpoint.Misc {
    [Serializable]
    public struct Range3D {
        public Vector3 Min;
        public Vector3 Max;

        public bool Contains(float x, float y, float z) {
            return Min.x <= x && Min.y <= y && Min.z <= z
                   && x < Max.x && y < Max.y && z < Max.z;
        }

        public bool Contains((float x, float y, float z) t) {
            return Contains(t.x, t.y, t.z);
        }
    }
}