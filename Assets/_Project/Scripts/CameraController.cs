using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{

    [SerializeField] private float panSpeed = 10f;
    [SerializeField] private Camera _cam;
    [SerializeField] private float _zoomSpeed = 50f;
    [SerializeField] private float _maxZoom = 30f;
    [SerializeField] private float _minZoom = 5f;
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
        if (Keyboard.current == null || Mouse.current == null)
        {
            return;
        }

        HandleCameraMovement();
        HandleCameraZoom();
    }

    private void HandleCameraMovement()
    {
        float yaw = _cam.transform.eulerAngles.y;

        Vector3 forward = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
        Vector3 right   = Quaternion.Euler(0f, yaw, 0f) * Vector3.right;

        _direction = GetDirection(forward, right);

        if (_direction.sqrMagnitude > 1f)
        {
            _direction.Normalize();
        }

        transform.position += _direction * panSpeed * Time.deltaTime;
    }
    private Vector3 GetDirection(Vector3 forward, Vector3 right)
    {
        if (Keyboard.current.upArrowKey.isPressed)
        {
            return forward;  
        }
        if (Keyboard.current.downArrowKey.isPressed)
        {
            return -forward;
        }
        if (Keyboard.current.rightArrowKey.isPressed)
        {
            return right;
        }
        if (Keyboard.current.leftArrowKey.isPressed)
        {
            return -right;
        }
        return Vector3.zero;
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
