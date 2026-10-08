using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputHandler : MonoBehaviour
{
    private PlayerInput _input;
    public event Action OnPlacement;

    private void Awake()
    {
        _input = GetComponent<PlayerInput>();
    }
    
    public void OnPlaceUnit()
    {
        OnPlacement?.Invoke();
    }
}