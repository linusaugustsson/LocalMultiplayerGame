using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartPad : MonoBehaviour {

    public delegate void PlayerEnterDelegate();
    public event PlayerEnterDelegate playerEnterEvent;

    public delegate void PlayerLeaveDelegate();
    public event PlayerEnterDelegate playerLeaveEvent;

    private MeshRenderer meshRenderer;

    public int playerCount;
    private Color defaultColor;

    private void Start() {
        meshRenderer = GetComponent<MeshRenderer>();
        defaultColor = meshRenderer.material.color;
    }

    void OnTriggerEnter(Collider other) {
        if(other.gameObject.TryGetComponent(out Player player)) {
            playerCount += 1;
            playerEnterEvent();
            meshRenderer.material.color = Color.green;
        }
    }

    void OnTriggerExit(Collider other) {
        if(other.gameObject.TryGetComponent(out Player player)) {
            playerCount -= 1;
            playerLeaveEvent();

            if(playerCount == 0) {
                meshRenderer.material.color = defaultColor;
            }
        }
    }
}
