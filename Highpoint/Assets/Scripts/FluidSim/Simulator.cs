using Highpoint.Math;
using Highpoint.Misc;
using UnityEngine;

namespace Highpoint {
    public class Simulator {
        private Fluid fluid;
        private MacGrid grid;

        public Simulator(Fluid f) {
            fluid = f;
            grid = fluid.Grid;
        }

        public void SimulateStep(float dt) {
            // We split the full differential equation we're solving and approximate
            // each summand separately.
            // TODO: Argument accuracy of this.
            AdvectStaggeredX(dt, grid.VelocityX, grid.VelocityNextX);
            AdvectStaggeredY(dt, grid.VelocityY, grid.VelocityNextY);
            AdvectStaggeredZ(dt, grid.VelocityZ, grid.VelocityNextZ);
            grid.SwapBuffers(velocity: true);
            AddBodyForces(dt);
            grid.SwapBuffers(velocityY: true);
            ProjectVelocity(dt);
            grid.SwapBuffers(velocity: true);
        }

        // We always advect through the velocity field given by the grid's current values.
        // current should be the current values of the quantity we're advecting, while next is
        // where we write the resulting field.
        private void AdvectCenters(float dt, float[] current, float[] next) {
            /*
             * Semi-lagrangian approach to advection:
             * For each sampled point, we imagine it was a particle, approximate where that
             * particle would have been in the previous time step and interpolate what value
             * the field being advected had at that point.
             *
             * For approximating where the particle was, we use Forward Euler for now but it could
             * possibly be a good idea to use something more advanced here (c.f. Modified Euler).
             *
             * For interpolating we use simple trilinear interpolation. Again, investigating more
             * advanced options could be useful.
             */

            // TODO: Boundary cells (solid, empty) need special handling
            // TODO: How/where do we switch from sim to empty and back?

            for (var x = 0; x < grid.GridSize.x; x++) {
                for (var y = 0; y < grid.GridSize.y; y++) {
                    for (var z = 0; z < grid.GridSize.z; z++) {
                        Vector3 xG = new Vector3(x, y, z);
                        Vector3 uG = grid.VelocityAtCenter(x, y, z);
                        Vector3 xP = xG - dt * uG;

                        next[grid.Idx(x, y, z)] = Interpolater.LerpCenters(grid, current, xP);
                    }
                }
            }
        }

        // The other advection routines are essentially the same, but accounting for interpolation differences
        // (in both velocity and the field being advected) due to the staggered nature of the grid.
        private void AdvectStaggeredX(float dt, float[] current, float[] next) {
            for (var x = 0; x < grid.GridSize.x + 1; x++) {
                for (var y = 0; y < grid.GridSize.y; y++) {
                    for (var z = 0; z < grid.GridSize.z; z++) {
                        Vector3 xG = new Vector3(x, y, z);
                        Vector3 uG = grid.VelocityAtStaggeredX(x, y, z);
                        Vector3 xP = xG - dt * uG;

                        next[grid.Idx(x, y, z)] = Interpolater.Lerp(grid, current, xP, staggerX: true);
                    }
                }
            }
        }
        private void AdvectStaggeredY(float dt, float[] current, float[] next) {
            for (var x = 0; x < grid.GridSize.x; x++) {
                for (var y = 0; y < grid.GridSize.y + 1; y++) {
                    for (var z = 0; z < grid.GridSize.z; z++) {
                        Vector3 xG = new Vector3(x, y, z);
                        Vector3 uG = grid.VelocityAtStaggeredY(x, y, z);
                        Vector3 xP = xG - dt * uG;

                        next[grid.Idx(x, y, z)] = Interpolater.Lerp(grid, current, xP, staggerY: true);
                    }
                }
            }
        }
        private void AdvectStaggeredZ(float dt, float[] current, float[] next) {
            for (var x = 0; x < grid.GridSize.x; x++) {
                for (var y = 0; y < grid.GridSize.y; y++) {
                    for (var z = 0; z < grid.GridSize.z + 1; z++) {
                        Vector3 xG = new Vector3(x, y, z);
                        Vector3 uG = grid.VelocityAtStaggeredZ(x, y, z);
                        Vector3 xP = xG - dt * uG;

                        next[grid.Idx(x, y, z)] = Interpolater.Lerp(grid, current, xP, staggerZ: true);
                    }
                }
            }
        }

        private void AddBodyForces(float dt) {
            var gravityStep = dt * fluid.GravityGrid;
            for (var x = 0; x < grid.GridSize.x; x++) {
                for (var y = 0; y < grid.GridSize.y + 1; y++) {
                    for (var z = 0; z < grid.GridSize.z; z++) {
                        int i = grid.Idx(x, y, z);
                        grid.VelocityNextY[i] = grid.VelocityY[i] + gravityStep;
                    }
                }
            }
        }

        private void ProjectVelocity(float dt) {

        }
    }
}