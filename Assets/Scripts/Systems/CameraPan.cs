using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraPan : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private float _panSpeed = 0.2f;
    [SerializeField] private InputActionReference _panInput;

    bool isPanning = false;
    Vector2 startingMousePosition;

    private void OnEnable()
    {
        _panInput.action.started += StartPanning;
        _panInput.action.canceled += StopPanning;
        _panInput.action.Enable();
    }

    private void StartPanning(InputAction.CallbackContext context)
    {
        startingMousePosition = Mouse.current.position.ReadValue();
        isPanning = true;
    }

    private void StopPanning(InputAction.CallbackContext context)
    {
        isPanning = false;
    }

    private void OnDisable()
    {
        _panInput.action.Disable();
        _panInput.action.started -= StartPanning;
        _panInput.action.canceled -= StopPanning;
    }

    private void Update()
    {
        if(!isPanning) return;
        Vector2 currentMousePosition = Mouse.current.position.ReadValue();
        Vector2 delta = currentMousePosition - startingMousePosition;
        Vector3 move = new Vector3(-delta.x * _panSpeed * Time.deltaTime, 0, -delta.y * _panSpeed * Time.deltaTime);
        _camera.transform.Translate(move, Space.World);
    }

}
