using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    
    #region Input Actions Instances
    private InputControls playerInputActions;
    public InputAction movement;
    public InputAction interact;
    #endregion

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        playerInputActions = new InputControls();
        movement = playerInputActions.Player.Move;
        interact = playerInputActions.Player.Select;
        
        playerInputActions.Player.Enable();
    }
}
