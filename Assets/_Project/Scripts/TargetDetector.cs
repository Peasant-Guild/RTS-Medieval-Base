using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(TeamMember))]
[RequireComponent(typeof(SphereCollider))]
public class TargetDetector : MonoBehaviour
{
    private readonly List<Transform> _targetsInDetectionRange = new();

    private SphereCollider _detectionTrigger;
    private TeamMember _teamMember;

    public float DetectionRange
    {
        get
        {
            if (_detectionTrigger == null)
            {
                return 0f;
            }

            float maxScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
            return _detectionTrigger.radius * maxScale;
        }
    }

    private void Awake()
    {
        _detectionTrigger = GetComponent<SphereCollider>();
        _teamMember = GetComponent<TeamMember>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!TryGetValidTarget(other, out Transform target))
        {
            return;
        }

        AddTarget(target);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!TryResolveTargetRoot(other.transform, out Transform target))
        {
            return;
        }

        _targetsInDetectionRange.Remove(target);
    }

    public Transform GetClosestDetectedTarget()
    {
        RefreshDetectedTargets();
        CleanupTargets();

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
        return TryGetValidTarget(target, out _);
    }

    public bool IsTargetDetected(Transform target)
    {
        if (target == null)
        {
            return false;
        }

        RefreshDetectedTargets();
        CleanupTargets();
        return _targetsInDetectionRange.Contains(target);
    }

    private void RefreshDetectedTargets()
    {
        if (_detectionTrigger == null || !_detectionTrigger.isTrigger)
        {
            return;
        }

        Vector3 worldCenter = transform.TransformPoint(_detectionTrigger.center);
        float radius = DetectionRange;
        Collider[] overlaps = Physics.OverlapSphere(worldCenter, radius, Physics.AllLayers, QueryTriggerInteraction.Collide);

        foreach (Collider overlap in overlaps)
        {
            if (!TryGetValidTarget(overlap, out Transform target))
            {
                continue;
            }

            AddTarget(target);
        }
    }

    private void CleanupTargets()
    {
        for (int i = _targetsInDetectionRange.Count - 1; i >= 0; i--)
        {
            Transform target = _targetsInDetectionRange[i];

            if (!CanTarget(target) || !IsWithinDetectionRange(target))
            {
                _targetsInDetectionRange.RemoveAt(i);
            }
        }
    }

    private bool IsWithinDetectionRange(Transform target)
    {
        if (target == null || _detectionTrigger == null || !_detectionTrigger.isTrigger)
        {
            return false;
        }

        Vector3 worldCenter = transform.TransformPoint(_detectionTrigger.center);
        float radius = DetectionRange;
        float distanceSqr = (target.position - worldCenter).sqrMagnitude;

        return distanceSqr <= radius * radius;
    }

    private void AddTarget(Transform target)
    {
        if (target == null || _targetsInDetectionRange.Contains(target))
        {
            return;
        }

        _targetsInDetectionRange.Add(target);
    }

    private bool TryGetValidTarget(Component source, out Transform target)
    {
        target = null;

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

        target = otherTeamMember.transform;
        return true;
    }

    private bool TryResolveTargetRoot(Transform candidate, out Transform targetRoot)
    {
        targetRoot = candidate == null
            ? null
            : candidate.GetComponentInParent<TeamMember>()?.transform ?? candidate.GetComponentInParent<Health>()?.transform;

        return targetRoot != null;
    }
}
