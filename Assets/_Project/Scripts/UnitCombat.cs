using System.Collections.Generic;
using UnityEngine;

public class UnitCombat : MonoBehaviour
{
    [SerializeField] private string _enemyTag = "Enemy";
    [SerializeField] private float _attackRange = 2f;

    private readonly List<Transform> _targetsInDetectionRange = new List<Transform>();

    private bool _isCommandedTarget;

    public Transform CurrentTarget { get; private set; }
    public float AttackRange => _attackRange;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(_enemyTag))
        {
            return;
        }

        Transform target = other.transform;

        if (!_targetsInDetectionRange.Contains(target))
        {
            _targetsInDetectionRange.Add(target);
        }

        if (!_isCommandedTarget)
        {
            CurrentTarget = GetClosestTargetInDetectionRange();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(_enemyTag))
        {
            return;
        }

        Transform target = other.transform;
        _targetsInDetectionRange.Remove(target);

        if (!_isCommandedTarget)
        {
            CurrentTarget = GetClosestTargetInDetectionRange();
        }
    }

    public bool HasTarget()
    {
        CleanupTargets();

        if (!_isCommandedTarget && CurrentTarget == null)
        {
            CurrentTarget = GetClosestTargetInDetectionRange();
        }

        return CurrentTarget != null;
    }

    public void SetTarget(Transform target, bool commanded = false)
    {
        CurrentTarget = target;
        _isCommandedTarget = commanded;
    }

    public void ClearTarget()
    {
        CurrentTarget = null;
        _isCommandedTarget = false;
    }

    public bool IsTargetInAttackRange()
    {
        if (CurrentTarget == null)
        {
            return false;
        }

        float distance = Vector3.Distance(transform.position, CurrentTarget.position);
        return distance <= _attackRange;
    }

    private void CleanupTargets()
    {
        for (int i = _targetsInDetectionRange.Count - 1; i >= 0; i--)
        {
            if (_targetsInDetectionRange[i] == null)
            {
                _targetsInDetectionRange.RemoveAt(i);
            }
        }

        if (_isCommandedTarget)
        {
            if (CurrentTarget == null)
            {
                _isCommandedTarget = false;
                CurrentTarget = GetClosestTargetInDetectionRange();
            }

            return;
        }

        if (CurrentTarget == null || !_targetsInDetectionRange.Contains(CurrentTarget))
        {
            CurrentTarget = GetClosestTargetInDetectionRange();
        }
    }

    private Transform GetClosestTargetInDetectionRange()
    {
        Transform closestTarget = null;
        float closestDistanceSqr = float.MaxValue;
        Vector3 currentPosition = transform.position;

        foreach (Transform target in _targetsInDetectionRange)
        {
            if (target == null)
            {
                continue;
            }

            float distanceSqr = (target.position - currentPosition).sqrMagnitude;

            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closestTarget = target;
            }
        }

        return closestTarget;
    }
}