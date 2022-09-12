using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointManager : MonoBehaviour {
    public static Vector3[] points;

    public static void Respawn(Transform it) {
        if(points != null && points.Length > 0) {
            it.position = points[Random.Range(0, points.Length)];
        } else {
            it.position = Vector3.zero;
        }
    }
}
