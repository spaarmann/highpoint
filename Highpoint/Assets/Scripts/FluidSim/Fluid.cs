using UnityEngine;

namespace Highpoint {
    public class Fluid : MonoBehaviour {
        public Vector3 ContainerSize = Vector3.one;
        public float GridStepSize = 0.1f;

        private MacGrid grid;

        private void Start() {
            grid = new MacGrid(ContainerSize, GridStepSize);
            Debug.Log(grid);
        }

        private void Update() { }

        private void OnDrawGizmos() {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(Vector3.zero, ContainerSize * 0.5f);
        }
    }
}
