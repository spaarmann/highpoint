﻿using System;
using System.Linq;
using Highpoint.Misc;
using UnityEngine;

namespace Highpoint {
    public class Fluid : MonoBehaviour {
        public Vector3 ContainerSize = Vector3.one;
        public float GridStepSize = 0.1f;

        public Range3D[] SolidRegions; 

        private MacGrid grid;
        private Simulator simulator;

        private void Start() {
            grid = new MacGrid(ContainerSize, GridStepSize);
            Debug.Log(grid);

            grid.MarkSolidCells(t => SolidRegions.Any(r => r.Contains(t)));

            simulator = new Simulator(this);
        }

        private void Update() { }

        private void OnDrawGizmos() {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Colors.VolumeOutline;
            Gizmos.DrawWireCube(0.5f * ContainerSize, ContainerSize);

            Gizmos.color = Colors.VolumeSolid;
            foreach (var solid in SolidRegions) {
                Gizmos.DrawCube(0.5f * (solid.Max + solid.Min), solid.Max - solid.Min);
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
        }
    }
}
