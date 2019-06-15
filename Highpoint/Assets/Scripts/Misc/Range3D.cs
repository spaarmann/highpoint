using System;
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

        public bool Contains(Vector3 p) {
            return Contains(p.x, p.y, p.z);
        }
    }
}