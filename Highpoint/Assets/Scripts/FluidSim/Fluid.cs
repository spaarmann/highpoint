using System.Linq;
using Highpoint.Math;
using Highpoint.Misc;
using UnityEngine;

namespace Highpoint {
    public class Fluid : MonoBehaviour {
        [Header("Simulation Volume Setup")]
        public Vector3 ContainerSize = Vector3.one;
        public float GridStepSize = 0.1f;

        [Header("Simulation Parameters")]
        public float Gravity = Physics.gravity.y;
        public int SimStepsPerFrame = 5;

        [Header("Fluid Setup")]
        public Range3D[] SolidRegions;
        public Source[] Sources;
        public Vector3[] Sinks;
        public MacGrid Grid { get; private set; }

        public float GravityGrid;

        private Simulator simulator;
        private const int targetFPS = 60;

        private void Start() {
            Grid = new MacGrid(ContainerSize, GridStepSize);
            Debug.Log(Grid);

            Grid.MarkSolidCells(t => SolidRegions.Any(r => r.Contains(t)));

            foreach (var source in Sources) Grid.AddSource(source);
            foreach (var sink in Sinks) Grid.AddSink(sink);

            simulator = new Simulator(this);
            Application.targetFrameRate = targetFPS;

            GravityGrid = Grid.MetersToGrid(Gravity);
        }

        private void Update() {
            // TODO better time step handling
            float step = (1f / targetFPS) / SimStepsPerFrame;
            for (var i = 0; i < SimStepsPerFrame; i++) {
                simulator.SimulateStep(step);
            }
        }

        private void OnDrawGizmos() {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Colors.VolumeOutline;
            Gizmos.DrawWireCube(0.5f * ContainerSize, ContainerSize);

            Gizmos.color = Colors.VolumeSolid;
            if (SolidRegions != null) foreach (var solid in SolidRegions) {
                Gizmos.DrawCube(0.5f * (solid.Max + solid.Min), solid.Max - solid.Min);
            }

            Gizmos.color = Colors.CellSource;
            if (Sources != null) foreach (var source in Sources) {
                Gizmos.DrawCube(GridStepSize * ((Vector3) Vec3Int.FloorToInt(source.Position / GridStepSize) + Vector3.one * 0.5f),
                    Vector3.one * GridStepSize);
            }

            Gizmos.color = Colors.CellSink;
            if (Sinks != null) foreach (var sink in Sinks) {
                Gizmos.DrawCube(GridStepSize * ((Vector3) Vec3Int.FloorToInt(sink / GridStepSize) + Vector3.one * 0.5f),
                    Vector3.one * GridStepSize);
            }
        }

        public void OnValidate() {
            for (var i = 0; i < SolidRegions?.Length; i++) {
                SolidRegions[i].Min.x = Mathf.Max(0f, SolidRegions[i].Min.x);
                SolidRegions[i].Min.y = Mathf.Max(0f, SolidRegions[i].Min.y);
                SolidRegions[i].Min.z = Mathf.Max(0f, SolidRegions[i].Min.z);
                SolidRegions[i].Max.x = Mathf.Min(ContainerSize.x, SolidRegions[i].Max.x);
                SolidRegions[i].Max.y = Mathf.Min(ContainerSize.y, SolidRegions[i].Max.y);
                SolidRegions[i].Max.z = Mathf.Min(ContainerSize.z, SolidRegions[i].Max.z);
            }

            GravityGrid = Gravity / GridStepSize;
        }
    }
}
