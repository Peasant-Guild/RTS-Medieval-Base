using UnityEngine;
using UnityEngine.AI;

public class UnitMovement : MonoBehaviour
{
    private const float AccelerationMultiplier = 4f;
    private const float MinMoveSpeed = 0.1f;

    private NavMeshAgent _agent;

    public float MoveSpeed => _agent != null ? _agent.speed : 0f;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();

        if (_agent == null)
        {
            Debug.LogError("UnitMovement could not find a NavMeshAgent.", this);
            enabled = false;
        }
    }

    public void MoveTo(Vector3 destination, float stoppingDistance = 0f)
    {
        if (_agent == null)
        {
            return;
        }

        _agent.stoppingDistance = stoppingDistance;
        _agent.isStopped = false;
        _agent.SetDestination(destination);
    }

    public void Stop()
    {
        if (_agent == null)
        {
            return;
        }

        _agent.isStopped = true;
    }
    
    public void RotateTowards(Vector3 worldPosition)
    {
        if (_agent == null)
        {
            return;
        }

        Vector3 direction = worldPosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        float maxDegreesDelta = _agent.angularSpeed * Time.deltaTime;

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            maxDegreesDelta
        );
    }

    public bool HasReachedDestination()
    {
        if (_agent == null)
        {
            return true;
        }

        if (_agent.pathPending)
        {
            return false;
        }

        return _agent.remainingDistance <= _agent.stoppingDistance;
    }

    public void ConfigureMoveSpeed(float moveSpeed)
    {
        if (_agent == null)
        {
            return;
        }

        _agent.speed = Mathf.Max(MinMoveSpeed, moveSpeed);
        _agent.acceleration = Mathf.Max(_agent.speed * AccelerationMultiplier, _agent.speed + MinMoveSpeed);
    }
}
