using System.Collections.Generic;
using UnityEngine;

public class UnitCombat : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] private string _enemyTag = "Enemy";
    [SerializeField] private float _attackRange = 2f;

    [Header("Attack")]
    [SerializeField] private float _attackInterval = 1f;

    private readonly List<Transform> _targetsInDetectionRange = new();

    private bool _isCommandedTarget;
    private float _lastAttackTime = -999f;
    private Animator _animator;

    public Transform CurrentTarget { get; private set; }
    public float AttackRange => _attackRange;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!TryGetEnemyTransform(other, out Transform target))
        {
            return;
        }

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
        if (!TryGetEnemyTransform(other, out Transform target))
        {
            return;
        }

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

        float distanceSqr = (CurrentTarget.position - transform.position).sqrMagnitude;
        return distanceSqr <= _attackRange * _attackRange;
    }

    public void TryAttack()
    {
        if (CurrentTarget == null)
        {
            return;
        }

        if (Time.time < _lastAttackTime + _attackInterval)
        {
            return;
        }

        _lastAttackTime = Time.time;
        PerformAttack();
    }

    private void PerformAttack()
    {
        Debug.Log($"{name} attacks {CurrentTarget.name}");
        // TODO: deal damage to enemy here.
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

    private bool TryGetEnemyTransform(Collider other, out Transform target)
    {
        target = null;

        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy != null)
        {
            target = enemy.transform;
            return true;
        }

        if (!other.CompareTag(_enemyTag))
        {
            return false;
        }

        target = other.transform;
        return true;
    }
}