using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Highpoint.Math;
using Highpoint.Misc;
using UnityEngine;

namespace Highpoint {
    /*
     * General information about our grid representation:
     * - Moving solids are currently not supported.
     * - Free surfaces (i.e. the boundary between fluid and air) and air cells in general
     *   are not specially marked.
     * - Everything outside of the grid is treated as air.
     */
    public class MacGrid {
        // Size is in fluid-space and should be a multiple of StepSize.
        public Vector3 Size { get; }
        public float StepSize { get; }

        // GridSize is the amount of grid cells we have going in each direction.
        public Vec3Int GridSize { get; }

        // Pressure is sampled at cell centers.
        private float[] pressure;

        // Velocity components are sampled at the center of each cell face in the
        // respective directions (specifically, the normal components are stored).
        // Implementation-wise, we treat this as if we had values at each cell center,
        // shift them left by half a unit and add another element at the end of the
        // respective direction (to ensure we have values for all 6 faces of every cell).
        private float[] velocityX;
        private float[] velocityY;
        private float[] velocityZ;

        // FREE is cells which are not in the simulation space.
        private const int CTYPE_FREE = -1;

        private const int CTYPE_SIMULATE = 0;
        private const int CTYPE_SOLID = 1;
        private const int CTYPE_SOURCE = 2;
        private const int CTYPE_SINK = 3;

        private byte[] cellType;

        private Dictionary<Vec3Int, Source> sources;

        public MacGrid(Vector3 size, float step) {
            Size = size;
            StepSize = step;

            GridSize = Vec3Int.FloorToInt(Size / StepSize);

            pressure = new float[GridSize.x * GridSize.y * GridSize.z];
            velocityX = new float[(GridSize.x + 1) * GridSize.y * GridSize.z];
            velocityY = new float[GridSize.x * (GridSize.y + 1) * GridSize.z];
            velocityZ = new float[GridSize.x * GridSize.y * (GridSize.z + 1)];
            cellType = new byte[GridSize.x * GridSize.y * GridSize.z];

            sources = new Dictionary<Vec3Int, Source>();
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float PressureAtCenter(Vec3Int p) {
            if (ValidIndex(p)) return pressure[GetArrayIndex(p)];
            return 0f;
        }


        // VelocityRight here refers to the normal component of the velocity of the fluid flowing through
        // the right face of the given cell. Positive if flow is going out of the cell / to the right, negative in
        // the other direction.
        // The same applies to the other direction, where flow to the right/up/forward is always positive and flow to
        // the left/down/back is always negative.

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float VelocityRight(Vec3Int p) {
            if (ValidIndexStaggered(p, Axis.X)) return pressure[GetArrayIndex(p + Vec3Int.Right)];
            return 0f;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float VelocityLeft(Vec3Int p) {
            if (ValidIndexStaggered(p, Axis.X)) return velocityX[GetArrayIndex(p)];
            return 0f;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float VelocityUp(Vec3Int p) {
            if (ValidIndexStaggered(p, Axis.Y)) return velocityY[GetArrayIndex(p + Vec3Int.Up)];
            return 0f;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float VelocityDown(Vec3Int p) {
            if (ValidIndexStaggered(p, Axis.Y)) return velocityY[GetArrayIndex(p)];
            return 0f;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float VelocityForward(Vec3Int p) {
            if (ValidIndexStaggered(p, Axis.Z)) return velocityZ[GetArrayIndex(p + Vec3Int.Forward)];
            return 0f;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float VelocityBackward(Vec3Int p) {
            if (ValidIndexStaggered(p, Axis.Z)) return velocityZ[GetArrayIndex(p)];
            return 0f;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CellType(Vec3Int p) {
            if (ValidIndex(p)) return cellType[GetArrayIndex(p)];
            return CTYPE_FREE;
        }

        public void MarkSolidCells(Predicate<Vector3> markerFunction) {
            for (var x = 0; x < GridSize.x; x++) {
                for (var y = 0; y < GridSize.y; y++) {
                    for (var z = 0; z < GridSize.z; z++) {
                        if (markerFunction(new Vector3(x * StepSize, y * StepSize, z * StepSize)))
                            cellType[GetArrayIndex(new Vec3Int(x, y, z))] = CTYPE_SOLID;
                    }
                }
            }
        }

        public void AddSource(Source source) {
            var p = Vec3Int.FloorToInt(source.Position / StepSize);
            if (!ValidIndex(p)) throw new IndexOutOfRangeException($"pos {source.Position} => p {p} is out of bounds");
            cellType[GetArrayIndex(p)] = CTYPE_SOURCE;
            sources.Add(p, source);
        }

        public void AddSink(Vector3 pos) {
            var p = Vec3Int.FloorToInt(pos / StepSize);
            if (!ValidIndex(p)) throw new IndexOutOfRangeException($"pos {pos} => p {p} is out of bounds");
            cellType[GetArrayIndex(p)] = CTYPE_SINK;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetArrayIndex(Vec3Int p) {
            return p.x + GridSize.x * (p.y + GridSize.y * p.z);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ValidIndex(Vec3Int p) {
            return p.x >= 0 && p.x < GridSize.x &&
                   p.y >= 0 && p.y < GridSize.y &&
                   p.z >= 0 && p.z < GridSize.z;
        }

        // For values that are staggered on the grid (i.e. stored for each face, not for each
        // cell center), for each axis an extra value is valid.
        private bool ValidIndexStaggered(Vec3Int p, Axis axis) {
            var maxX = axis == Axis.X ? GridSize.x : GridSize.x + 1;
            var maxY = axis == Axis.Y ? GridSize.y : GridSize.y + 1;
            var maxZ = axis == Axis.Z ? GridSize.z : GridSize.z + 1;
            return p.x >= 0 && p.x < maxX &&
                   p.y >= 0 && p.y < maxY &&
                   p.z >= 0 && p.z < maxZ;
        }

        public override string ToString() {
            return $"MAC Grid: Size {Size} in {GridSize} elements for step size {StepSize}";
        }
    }
}