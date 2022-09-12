using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{

    public GlobalData globalData;

    public Vector2 moveVal;
    public Vector2 lookVal;

    [HideInInspector]
    public bool jumped = false;
    [HideInInspector]
    public bool ability = false;

    private UnityEngine.InputSystem.PlayerInput playerInput;

    public float lookSensitivityGamepad = 15.0f;
    public float lookSensitivityMouse = 1.0f;

    private void Awake() {
        playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
        
    }


    private void LateUpdate() {

        jumped = false;
        ability = false;
    }

    public void OnMovement(InputValue _value) {
        moveVal = _value.Get<Vector2>();
    }

    public void OnJump() {
        jumped = true;
    }

    public void OnActivateAbility() {
        ability = true;
    }

    public void OnCamera(InputValue _value) {
        lookVal = _value.Get<Vector2>();

        if (playerInput.currentControlScheme == "KeyboardMouse") {
            lookVal *= lookSensitivityMouse;
        } else {
            lookVal *= lookSensitivityGamepad;
        }
    }

    public void OnStart() {
        GlobalData.gameManager.StartGame();
    }

}
