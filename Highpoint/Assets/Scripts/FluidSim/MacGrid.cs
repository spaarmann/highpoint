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
        public Vec3Int GridSize;

        // Pressure is sampled at cell centers.
        public float[] Pressure;

        // Velocity components are sampled at the center of each cell face in the
        // respective directions (specifically, the normal components are stored).
        // Implementation-wise, we treat this as if we had values at each cell center,
        // shift them left by half a unit and add another element at the end of the
        // respective direction (to ensure we have values for all 6 faces of every cell).
        public float[] VelocityX;
        public float[] VelocityY;
        public float[] VelocityZ;

        // FREE is cells which are not in the simulation space.
        private const int CTYPE_FREE = -1;

        private const int CTYPE_SIMULATE = 0;
        private const int CTYPE_SOLID = 1;
        private const int CTYPE_SOURCE = 2;
        private const int CTYPE_SINK = 3;

        public byte[] CellType;

        // We want to calculate the next time-step based on the values at the current time step
        // so for now, to make things easy, keep two copies of everything and make it easy to switch
        // them around.
        // We rely on the simulation setting *every* value each time step right now! The values are not
        // copied between the buffers when swapping them.

        // Getters act on current set of values while setters act on the "next" set.

        public float[] PressureNext;
        public float[] VelocityNextX;
        public float[] VelocityNextY;
        public float[] VelocityNextZ;

        private Dictionary<Vec3Int, Source> sources;

        public MacGrid(Vector3 size, float step) {
            Size = size;
            StepSize = step;

            GridSize = Vec3Int.FloorToInt(Size / StepSize);

            Pressure = new float[GridSize.x * GridSize.y * GridSize.z];
            VelocityX = new float[(GridSize.x + 1) * GridSize.y * GridSize.z];
            VelocityY = new float[GridSize.x * (GridSize.y + 1) * GridSize.z];
            VelocityZ = new float[GridSize.x * GridSize.y * (GridSize.z + 1)];
            CellType = new byte[GridSize.x * GridSize.y * GridSize.z];

            sources = new Dictionary<Vec3Int, Source>();

            PressureNext = new float[GridSize.x * GridSize.y * GridSize.z];
            VelocityNextX = new float[(GridSize.x + 1) * GridSize.y * GridSize.z];
            VelocityNextY = new float[GridSize.x * (GridSize.y + 1) * GridSize.z];
            VelocityNextZ = new float[GridSize.x * GridSize.y * (GridSize.z + 1)];
        }

        // These accessors are relatively simple averaging, just have to be careful
        // to get the indices correct.
        public Vector3 VelocityAtCenter(int x, int y, int z) {
            return new Vector3(
                (VelocityX[Idx(x, y, z)] + VelocityX[Idx(x + 1, y, z)]) / 2f,
                (VelocityY[Idx(x, y, z)] + VelocityY[Idx(x, y + 1, z)]) / 2f,
                (VelocityZ[Idx(x, y, z)] + VelocityZ[Idx(x, y, z + 1)]) / 2f
            );
        }
        public Vector3 VelocityAtStaggeredX(int x, int y, int z) {
            return new Vector3(
                VelocityX[Idx(x, y, z)],
                (VelocityY[Idx(x - 1, y, z)] + VelocityY[Idx(x - 1, y + 1, z)]
                                             + VelocityY[Idx(x, y, z)] + VelocityY[Idx(x, y + 1, z)]) / 4f,
                (VelocityZ[Idx(x - 1, y, z)] + VelocityZ[Idx(x - 1, y, z + 1)]
                                             + VelocityZ[Idx(x, y, z)] + VelocityZ[Idx(x, y, z + 1)]) / 4f
            );
        }
        public Vector3 VelocityAtStaggeredY(int x, int y, int z) {
            return new Vector3(
                (VelocityX[Idx(x, y - 1, z)] + VelocityX[Idx(x + 1, y - 1, z)]
                                             + VelocityX[Idx(x, y, z)] + VelocityX[Idx(x + 1, y, z)]) / 4f,
                VelocityY[Idx(x, y, z)],
                (VelocityZ[Idx(x, y - 1, z)] + VelocityZ[Idx(x, y - 1, z + 1)]
                                             + VelocityZ[Idx(x, y, z)] + VelocityZ[Idx(x, y, z + 1)]) / 4f
            );
        }
        public Vector3 VelocityAtStaggeredZ(int x, int y, int z) {
            return new Vector3(
                (VelocityX[Idx(x, y, z - 1)] + VelocityX[Idx(x + 1, y, z - 1)]
                                             + VelocityX[Idx(x, y, z)] + VelocityX[Idx(x + 1, y, z)]) / 4f,
                (VelocityY[Idx(x, y, z - 1)] + VelocityY[Idx(x, y + 1, z - 1)]
                 + VelocityY[Idx(x, y, z)] + VelocityY[Idx(x, y + 1, z)]) / 4f,
                VelocityZ[Idx(x, y, z)]
            );
        }

        public void SwapBuffers(bool pressure = false, bool velocity = false,
            bool velocityX = false, bool velocityY = false, bool velocityZ = false) {
            float[] tmp;
            if (pressure) {
                tmp = Pressure;
                Pressure = PressureNext;
                PressureNext = tmp;
            }

            if (velocity || velocityX) {
                tmp = VelocityX;
                VelocityX = VelocityNextX;
                VelocityNextX = tmp;
            }

            if (velocity || velocityY) {
                tmp = VelocityY;
                VelocityY = VelocityNextY;
                VelocityNextX = tmp;
            }

            if (velocity || velocityZ) {
                tmp = VelocityZ;
                VelocityZ = VelocityNextZ;
                VelocityNextZ = tmp;
            }
        }

        public void MarkSolidCells(Predicate<Vector3> markerFunction) {
            for (var x = 0; x < GridSize.x; x++) {
                for (var y = 0; y < GridSize.y; y++) {
                    for (var z = 0; z < GridSize.z; z++) {
                        if (markerFunction(new Vector3(x * StepSize, y * StepSize, z * StepSize)))
                            CellType[Idx(new Vec3Int(x, y, z))] = CTYPE_SOLID;
                    }
                }
            }
        }

        public void AddSource(Source source) {
            var p = Vec3Int.FloorToInt(source.Position / StepSize);
            if (!ValidIndex(p)) throw new IndexOutOfRangeException($"pos {source.Position} => p {p} is out of bounds");
            CellType[Idx(p)] = CTYPE_SOURCE;
            sources.Add(p, source);
        }

        public void AddSink(Vector3 pos) {
            var p = Vec3Int.FloorToInt(pos / StepSize);
            if (!ValidIndex(p)) throw new IndexOutOfRangeException($"pos {pos} => p {p} is out of bounds");
            CellType[Idx(p)] = CTYPE_SINK;
        }

        public float MetersToGrid(float m) {
            return m / StepSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Idx(Vec3Int p) {
            return p.x + GridSize.x * (p.y + GridSize.y * p.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Idx(int x, int y, int z) {
            return x + GridSize.x * (y + GridSize.y * z);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ValidIndex(Vec3Int p) {
            var valid = p.x >= 0 && p.x < GridSize.x &&
                   p.y >= 0 && p.y < GridSize.y &&
                   p.z >= 0 && p.z < GridSize.z;

            if (!valid) Debug.LogError("Invalid Index!");
            return valid;
        }

        // For values that are staggered on the grid (i.e. stored for each face, not for each
        // cell center), for each axis an extra value is valid.
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ValidIndexStaggered(Vec3Int p, Axis axis) {
            var maxX = axis == Axis.X ? GridSize.x + 1: GridSize.x;
            var maxY = axis == Axis.Y ? GridSize.y + 1: GridSize.y;
            var maxZ = axis == Axis.Z ? GridSize.z + 1: GridSize.z;
            var valid = p.x >= 0 && p.x < maxX &&
                   p.y >= 0 && p.y < maxY &&
                   p.z >= 0 && p.z < maxZ;

            if (!valid) Debug.LogError("Invalid Staggered Index!");
            return valid;
        }

        public override string ToString() {
            return $"MAC Grid: Size {Size} in {GridSize} elements for step size {StepSize}";
        }
    }
}