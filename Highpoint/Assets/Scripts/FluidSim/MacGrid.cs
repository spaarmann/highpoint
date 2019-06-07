using UnityEngine;

namespace Highpoint {
    public class MacGrid {
        // Size is in fluid-space and should be a multiple of StepSize.
        public Vector3 Size { get; }
        public float StepSize { get; }

        // GridSize is the amount of grid cells we have going in each direction.
        public Vector3Int GridSize { get; }

        // Pressure is sampled at cell centers.
        private readonly float[] pressure;

        // Velocity components are sampled at the center of each cell face in the
        // respective directions (specifically, the normal components are stored).
        // Implementation-wise, we treat this as if we had values at each cell center,
        // shift them left by half a unit and add another element at the end of the
        // respective direction (to ensure we have values for all 6 faces of every cell).
        private float[] velocityX;
        private float[] velocityY;
        private float[] velocityZ;

        public MacGrid(Vector3 size, float step) {
            Size = size;
            StepSize = step;

            GridSize = Vector3Int.FloorToInt(Size / StepSize);

            pressure = new float[GridSize.x * GridSize.y * GridSize.z];
            velocityX = new float[(GridSize.x + 1) * GridSize.y * GridSize.z];
            velocityY = new float[GridSize.x * (GridSize.y + 1) * GridSize.z];
            velocityZ = new float[GridSize.x * GridSize.y * (GridSize.z + 1)];
        }

        public float PressureAtCenter(int x, int y, int z) {
            return pressure[x + GridSize.x * (y + GridSize.y * z)];
        }

        public float VelocityRight(int x, int y, int z) {
            return pressure[x + 1 + GridSize.x * (y + GridSize.y * z)];
        }

        public float VelocityLeft(int x, int y, int z) {
            return velocityX[x + GridSize.x * (y + GridSize.y * z)];
        }

        public float VelocityUp(int x, int y, int z) {
            return velocityY[x + GridSize.x * (y + 1 + GridSize.y * z)];
        }

        public float VelocityDown(int x, int y, int z) {
            return velocityY[x + GridSize.x * (y + GridSize.y * z)];
        }

        public float VelocityForward(int x, int y, int z) {
            return velocityZ[x + GridSize.x * (y + GridSize.y * (z + 1))];
        }

        public float VelocityBackward(int x, int y, int z) {
            return velocityZ[x + GridSize.x * (y + GridSize.y * z)];
        }

        public override string ToString() {
            return $"MAC Grid: Size {Size} in {GridSize} elements for step size {StepSize}";
        }
    }
}