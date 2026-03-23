using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(TeamMember))]
public class UnitCombat : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] private float _attackRange = 2f;

    [Header("Attack")]
    [SerializeField] private float _attackDamage = 1f;
    [SerializeField] private float _attackInterval = 1f;

    private readonly List<Transform> _targetsInDetectionRange = new();

    private bool _isCommandedTarget;
    private float _lastAttackTime = -999f;
    private SphereCollider _detectionTrigger;
    private Health _health;
    private TeamMember _teamMember;

    public Transform CurrentTarget { get; private set; }
    public float AttackRange => _attackRange;

    private void Awake()
    {
        _detectionTrigger = GetComponent<SphereCollider>();
        _health = GetComponent<Health>();
        _teamMember = GetComponent<TeamMember>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!TryGetTargetTransform(other, out Transform target))
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
        if (!TryResolveTargetRoot(other.transform, out Transform target))
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
        RefreshDetectedTargets();
        CleanupTargets();

        if (!_isCommandedTarget && CurrentTarget == null)
        {
            CurrentTarget = GetClosestTargetInDetectionRange();
        }

        return CurrentTarget != null;
    }

    public void SetTarget(Transform target, bool commanded = false)
    {
        if (!CanTarget(target))
        {
            ClearTarget();
            return;
        }

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
        if (!CanTarget(CurrentTarget))
        {
            return false;
        }

        float distanceSqr = (CurrentTarget.position - transform.position).sqrMagnitude;
        return distanceSqr <= _attackRange * _attackRange;
    }

    public void TryAttack()
    {
        if (!_health.IsAlive || !CanTarget(CurrentTarget))
        {
            ClearTarget();
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
        if (!TryGetTargetContext(CurrentTarget, out TargetContext targetContext))
        {
            ClearTarget();
            return;
        }

        targetContext.Health.TakeDamage(_attackDamage);
    }

    private void CleanupTargets()
    {
        for (int i = _targetsInDetectionRange.Count - 1; i >= 0; i--)
        {
            if (!CanTarget(_targetsInDetectionRange[i]) || !IsWithinDetectionRange(_targetsInDetectionRange[i]))
            {
                _targetsInDetectionRange.RemoveAt(i);
            }
        }

        if (_isCommandedTarget)
        {
            if (!CanTarget(CurrentTarget))
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

    private void RefreshDetectedTargets()
    {
        if (_detectionTrigger == null || !_detectionTrigger.isTrigger)
        {
            return;
        }

        Vector3 worldCenter = transform.TransformPoint(_detectionTrigger.center);
        float maxScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
        float radius = _detectionTrigger.radius * maxScale;
        Collider[] overlaps = Physics.OverlapSphere(worldCenter, radius, Physics.AllLayers, QueryTriggerInteraction.Collide);

        foreach (Collider overlap in overlaps)
        {
            if (!TryGetTargetTransform(overlap, out Transform target) || _targetsInDetectionRange.Contains(target))
            {
                continue;
            }

            _targetsInDetectionRange.Add(target);
        }
    }

    private bool IsWithinDetectionRange(Transform target)
    {
        if (target == null || _detectionTrigger == null || !_detectionTrigger.isTrigger)
        {
            return false;
        }

        Vector3 worldCenter = transform.TransformPoint(_detectionTrigger.center);
        float maxScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
        float radius = _detectionTrigger.radius * maxScale;
        float distanceSqr = (target.position - worldCenter).sqrMagnitude;

        return distanceSqr <= radius * radius;
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

    public bool CanTarget(Transform target)
    {
        return TryGetTargetContext(target, out _);
    }

    private bool TryGetTargetTransform(Collider other, out Transform target)
    {
        if (!TryGetTargetContext(other, out TargetContext targetContext))
        {
            target = null;
            return false;
        }

        target = targetContext.Transform;
        return true;
    }

    private bool TryResolveTargetRoot(Transform candidate, out Transform targetRoot)
    {
        targetRoot = candidate == null
            ? null
            : candidate.GetComponentInParent<TeamMember>()?.transform ?? candidate.GetComponentInParent<Health>()?.transform;

        return targetRoot != null;
    }

    private bool TryGetTargetContext(Component source, out TargetContext targetContext)
    {
        targetContext = default;

        if (source == null || _teamMember == null)
        {
            return false;
        }

        TeamMember otherTeamMember = source.GetComponentInParent<TeamMember>();
        Health otherHealth = source.GetComponentInParent<Health>();

        if (otherTeamMember == null || otherHealth == null)
        {
            return false;
        }

        if (!_teamMember.IsHostileTo(otherTeamMember) || !otherHealth.IsAlive)
        {
            return false;
        }

        targetContext = new TargetContext(otherTeamMember.transform, otherHealth);
        return true;
    }

    private readonly struct TargetContext
    {
        public TargetContext(Transform transform, Health health)
        {
            Transform = transform;
            Health = health;
        }

        public Transform Transform { get; }
        public Health Health { get; }
    }
}
