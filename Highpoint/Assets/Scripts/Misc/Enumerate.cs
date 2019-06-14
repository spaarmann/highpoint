using System.Collections.Generic;
using UnityEngine;

namespace Highpoint.Misc {
    public static class Enumerate {
        public static IEnumerable<(int x, int y, int z)> FromTo3D(Vector3Int start, Vector3Int end) {
            for (var x = start.x; x < end.x; x++) {
                for (var y = start.y; y < end.y; y++) {
                    for (var z = start.z; z < end.z; z++) {
                        yield return (x, y, z);
                    }
                }
            }
        }
    }
}