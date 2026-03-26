using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal.Internal;

public class CameraController : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private Camera _cam;

    [Header("Zoom")]
    [SerializeField] private float _zoomSpeed = 100f;
    [SerializeField] private float _minZoom = 10f;
    [SerializeField] private float _maxZoom = 50f;

    [Header("Movement")]
    [SerializeField] private float _panSpeedKeyboard = 20f;
    [SerializeField] private float _panSpeedMouse = 30f;

    [SerializeField] private float _panBorderThickness = 20f;

    private bool _cameraActive = true;
    private float _lastActivtionTime = 0f;
    private float _activationCooldownTime = 0.5f;
    private float _scrollValue;
    private Vector3 _direction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _cam = Camera.main;

        if (_cam == null)
        {
            Debug.LogError("CameraController could not find a main camera.", this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current == null || Mouse.current == null || _cam == null)
        {
            return;
        }

        HandleCameraActivation();
        if (!_cameraActive)
        {
            return;
        }
        HandleCameraMovement();
        HandleCameraZoom();
    }

    private void HandleCameraActivation()
    {
        if (Time.time - _lastActivtionTime >= _activationCooldownTime &&
            Keyboard.current.leftShiftKey.isPressed && Keyboard.current.cKey.isPressed)
        {
            _cameraActive = !_cameraActive;
            _lastActivtionTime = Time.time;
        }
    }
    private void HandleCameraMovement()
    {
        float yaw = _cam.transform.eulerAngles.y;

        Vector3 forward = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
        Vector3 right   = Quaternion.Euler(0f, yaw, 0f) * Vector3.right;

        _direction = GetKeyboardDirection(forward, right) + GetMouseDirection(forward, right);

        transform.position += _direction * Time.deltaTime;
    }

    private Vector3 GetMouseDirection(Vector3 forward, Vector3 right)
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        Vector3 mouseDirection = Vector3.zero;

        int height = Screen.height;
        int width = Screen.width;

        if (mousePos.y >= height - _panBorderThickness)
        {
            mouseDirection += forward;
        }
        if (mousePos.y <= _panBorderThickness)
        {
            mouseDirection -= forward;
        }
        if (mousePos.x >= width - _panBorderThickness)
        {
            mouseDirection += right;
        }
        if (mousePos.x <= _panBorderThickness)
        {
            mouseDirection -= right;
        }

        if (mouseDirection.sqrMagnitude > 1f)
        {
            mouseDirection.Normalize();
        }

        return mouseDirection * _panSpeedMouse;
    }

    private Vector3 GetKeyboardDirection(Vector3 forward, Vector3 right)
    {
        Vector3 keyboardDirection = Vector3.zero;

        if (Keyboard.current.upArrowKey.isPressed)
        {
            keyboardDirection += forward;
        }
        if (Keyboard.current.downArrowKey.isPressed)
        {
            keyboardDirection -= forward;
        }
        if (Keyboard.current.rightArrowKey.isPressed)
        {
            keyboardDirection += right;
        }
        if (Keyboard.current.leftArrowKey.isPressed)
        {
            keyboardDirection -= right;
        }

        if (keyboardDirection.sqrMagnitude > 1f)
        {
            keyboardDirection.Normalize();
        }

        return keyboardDirection * _panSpeedKeyboard;
    }

    private void HandleCameraZoom()
    {
        _scrollValue = Mouse.current.scroll.ReadValue().y;

        if (Mathf.Approximately(_scrollValue, 0f))
        {
            return;
        }

        Vector3 newPos = transform.position + _cam.transform.forward * (_scrollValue * _zoomSpeed * Time.deltaTime);
        newPos.y = Mathf.Clamp(newPos.y, _minZoom, _maxZoom);

        if (newPos.y != _minZoom && newPos.y != _maxZoom)
        {
            transform.position = newPos;
        }
    }
}
