using UnityEngine;
using UnityEngine.AI;

public class UnitMovement : MonoBehaviour
{
    private NavMeshAgent _agent;

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
}