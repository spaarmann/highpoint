using System;
using Highpoint.Math;
using UnityEngine;

namespace Highpoint {
    [Serializable]
    public struct Source {
        public Vector3 Position; // In Fluid space
        public Vec3Int GridPosition; // Grid coordinates
        public float FlowRight, FlowLeft,
            FlowUp, FlowDown,
            FlowForward, FlowBackward;
    }
}