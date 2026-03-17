using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{

    public float panSpeed = 20f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.position;

        if (Keyboard.current.upArrowKey.isPressed)
        {
            pos.z += panSpeed * Time.deltaTime;
        }
        if (Keyboard.current.downArrowKey.isPressed)
        {
            pos.z -= panSpeed * Time.deltaTime;
        }
        if (Keyboard.current.rightArrowKey.isPressed)
        {
            pos.x += panSpeed * Time.deltaTime;
        }
        if (Keyboard.current.leftArrowKey.isPressed)
        {
            pos.x -= panSpeed * Time.deltaTime;
        }

        transform.position = pos;
    }
}
