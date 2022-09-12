using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SineMovement : MonoBehaviour {
    public Vector3 distance = new Vector3(0f, 0.5f, 0f);
    public Vector3 frequency = new Vector3(1f, 1f, 1f);

    [HideInInspector]
    public Vector3 initialPosition;

    public Vector3 GetPosition(Vector3 offset, float time) {
        offset.x += Mathf.Sin(time * frequency.x) * distance.x;
        offset.y += Mathf.Sin(time * frequency.y) * distance.y;
        offset.z += Mathf.Sin(time * frequency.z) * distance.z;
        return offset;
    }

    void OnCollisionEnter(Collision collision) {
        if(collision.gameObject.TryGetComponent(out Player player)) {
            collision.transform.SetParent(transform);
        }
    }

    void OnCollisionExit(Collision collision) {
        if(collision.gameObject.TryGetComponent(out Player player)) {
            collision.transform.SetParent(null);
        }
    }

    void Start() { initialPosition = transform.localPosition; }
    private void Update() { transform.localPosition = GetPosition(initialPosition, Time.time); }

#if UNITY_EDITOR
    void OnDrawGizmosSelected() {
        Vector3 center = transform.position;

        if(Application.isPlaying) { center = initialPosition; };

        if(distance.x != 0f && frequency.x != 0f) {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(center - new Vector3(distance.x, 0f, 0f), center + new Vector3(distance.x, 0f, 0f));
        }

        if(distance.y != 0f && frequency.y != 0f) {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(center - new Vector3(0f, distance.y, 0f), center + new Vector3(0f, distance.y, 0f));
        }

        if(distance.z != 0f && frequency.z != 0f) {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(center - new Vector3(0f, 0f, distance.z), center + new Vector3(0f, 0f, distance.z));
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(GetPosition(center, Time.time), transform.localScale * 0.0125f);
    }
#endif
}
