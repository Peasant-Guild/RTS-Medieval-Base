using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class UnitMovement : MonoBehaviour
{
    [SerializeField] private LayerMask _ground;

    private Camera _cam;
    private NavMeshAgent _agent;

    private void Start()
    {
        _cam = Camera.main;
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            Ray ray = _cam.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _ground))
            {
                _agent.SetDestination(hit.point);
            }
        }
    }
}