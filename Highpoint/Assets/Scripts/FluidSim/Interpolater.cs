using Highpoint.Math;
using Vector3 = UnityEngine.Vector3;

namespace Highpoint {
    public static class Interpolater {
        public static float LerpCenters(MacGrid g, float[] q, Vector3 pos) {
            Vec3Int p0 = Vec3Int.FloorToInt(pos);
            // Because we're using grid coordinates here, not fluid space, we can rely on the
            // fact the spacing (on one axis) between two samples values is always 1.
            Vector3 alpha = pos - p0;

            // This is just simple trilinear interpolation, written out in full.
            float q00 = (1f - alpha.x) * q[g.Idx(p0.x, p0.y, p0.z)] + alpha.x * q[g.Idx(p0.x + 1, p0.y, p0.z)];
            float q10 = (1f - alpha.x) * q[g.Idx(p0.x, p0.y + 1, p0.z)] + alpha.x * q[g.Idx(p0.x + 1, p0.y + 1, p0.z)];
            float q01 = (1f - alpha.x) * q[g.Idx(p0.x, p0.y, p0.z + 1)] + alpha.x * q[g.Idx(p0.x + 1, p0.y, p0.z + 1)];
            float q11 = (1f - alpha.x) * q[g.Idx(p0.x, p0.y + 1, p0.z + 1)] + alpha.x * q[g.Idx(p0.x + 1, p0.y + 1, p0.z + 1)];

            float q0 = (1f - alpha.y) * q00 + alpha.y * q10;
            float q1 = (1f - alpha.y) * q01 + alpha.y * q11;

            return (1f - alpha.z) * q0 + alpha.z * q1;
        }

        // Only one of the stagger flags can be set!
        public static float Lerp(MacGrid g, float[] q, Vector3 pos,
            bool staggerX = false, bool staggerY = false, bool staggerZ = false) {

            Vec3Int staggerAdjustment;
            if (staggerX) staggerAdjustment = Vec3Int.right;
            else if (staggerY) staggerAdjustment = Vec3Int.up;
            else if (staggerZ) staggerAdjustment = Vec3Int.forward;
            else staggerAdjustment = Vec3Int.zero;

            // p0 gives us the array-coordinates of lower-back-left sample point next to the given pos.
            Vec3Int p0 = Vec3Int.FloorToInt(pos - staggerAdjustment * 0.5f) + staggerAdjustment;
            // The actual position in grid coordinates is as above but with 0.5f * staggerAdjustment added on the end:
            // p0 is correct for indexing to the array, but the staggering causes a mismatch between indices and coordinates.
            // Because we're using grid based coordinates here, not fluid space, we can rely on the
            // fact the spacing (on one axis) between two samples values is always 1.
            Vector3 alpha = pos - p0 - staggerAdjustment * 0.5f;

            // This is just simple trilinear interpolation, written out in full.
            float q00 = (1f - alpha.x) * q[g.Idx(p0.x, p0.y, p0.z)] + alpha.x * q[g.Idx(p0.x + 1, p0.y, p0.z)];
            float q10 = (1f - alpha.x) * q[g.Idx(p0.x, p0.y + 1, p0.z)] + alpha.x * q[g.Idx(p0.x + 1, p0.y + 1, p0.z)];
            float q01 = (1f - alpha.x) * q[g.Idx(p0.x, p0.y, p0.z + 1)] + alpha.x * q[g.Idx(p0.x + 1, p0.y, p0.z + 1)];
            float q11 = (1f - alpha.x) * q[g.Idx(p0.x, p0.y + 1, p0.z + 1)] + alpha.x * q[g.Idx(p0.x + 1, p0.y + 1, p0.z + 1)];

            float q0 = (1f - alpha.y) * q00 + alpha.y * q10;
            float q1 = (1f - alpha.y) * q01 + alpha.y * q11;

            return (1f - alpha.z) * q0 + alpha.z * q1;
        }
    }
}