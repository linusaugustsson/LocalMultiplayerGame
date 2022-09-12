using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour {
    void Start() {
        if(SpawnPointManager.points != null) {
            System.Array.Resize(ref SpawnPointManager.points, SpawnPointManager.points.Length + 1);
        } else {
            SpawnPointManager.points = new Vector3[1];
        }
        SpawnPointManager.points[SpawnPointManager.points.Length - 1] = transform.position;
        Destroy(gameObject);
    }
}
