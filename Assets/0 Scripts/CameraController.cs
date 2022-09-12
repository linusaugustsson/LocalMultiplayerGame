using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct PlayerCamera {
    public Camera camera;

    public float tilt;
    public float rotation;
    public float distance;

    public Vector2 sensitivity;

    public float shakeMagnitude;
    public float shakeTime;
    public float shakeTimeTotal;

    public static float PI_OVER_TWO = Mathf.PI / 2f;

    public void Init() {
        sensitivity.x = 0.25f;
        sensitivity.y = -0.125f;
    }

    public void SetShake(float magnitude, float duration) {
        shakeMagnitude = magnitude;
        shakeTime = duration;
        shakeTimeTotal = duration;
    }

    public void Update(Vector3 center, Vector2 lookVal, float deltaTime) {
        // Look
        rotation += lookVal.x * deltaTime * sensitivity.x;
        tilt += lookVal.y * deltaTime * sensitivity.y;

        // Bounds
        if(tilt < 0.001f) { tilt = 0.001f; } else if(tilt > 0.9f) { tilt = 0.9f; }

        if(rotation < -Mathf.PI) { rotation += Mathf.PI * 2f; } else if(rotation > Mathf.PI) { rotation -= Mathf.PI * 2f; }

        if(distance < 4f) { distance = 4f; } else if(distance > 2048f) { distance = 2048f; }

        // Screen shake
        if(shakeTime > 0f) {
            float t = shakeTime / shakeTimeTotal;

            center += camera.transform.right * (Mathf.Sin(Time.realtimeSinceStartup * shakeMagnitude) * t);

            shakeTime -= deltaTime;
        }

        // Move to target
        Vector3 desiredPosition = new Vector3();
        desiredPosition.x = center.x + ((Mathf.Sin(rotation) * distance) * Mathf.Cos(tilt));
        desiredPosition.z = center.z + ((Mathf.Cos(rotation) * distance) * Mathf.Cos(tilt));
        desiredPosition.y = center.y + (((tilt <= 0f) ? -1f : 1f) * ((Mathf.Sin(tilt) * distance) * Mathf.Sin(tilt)));

        camera.transform.position = Vector3.Lerp(camera.transform.position, desiredPosition, 0.5f);
        camera.transform.LookAt(center);
    }
};
