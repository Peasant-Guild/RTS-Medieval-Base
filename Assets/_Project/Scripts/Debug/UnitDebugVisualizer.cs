using UnityEngine;

public class UnitDebugVisualizer : MonoBehaviour
{
    [Header("State Colors")]
    [SerializeField] private Color _idleColor = Color.gray;
    [SerializeField] private Color _movingColor = Color.cyan;
    [SerializeField] private Color _followingColor = Color.yellow;
    [SerializeField] private Color _attackingColor = Color.red;

    [Header("Gizmo Colors")]
    [SerializeField] private Color _attackRangeColor = Color.red;
    [SerializeField] private Color _detectionRangeColor = Color.yellow;
    [SerializeField] private Color _targetLineColor = Color.white;

    [Header("Arrow")]
    [SerializeField] private float _arrowLength = 2f;
    [SerializeField] private float _arrowHeadLength = 0.4f;
    [SerializeField] private float _arrowHeadAngle = 30f;
    [SerializeField] private Vector3 _arrowOffset = new Vector3(0f, 1.5f, 0f);

    private UnitStateController _stateController;
    private UnitCombat _combat;
    private SphereCollider _detectionTrigger;

    private void Awake()
    {
        _stateController = GetComponent<UnitStateController>();
        _combat = GetComponent<UnitCombat>();
        _detectionTrigger = GetComponent<SphereCollider>();
    }

    private void OnDrawGizmos()
    {
        if (!ShouldDrawDebug())
        {
            return;
        }

        DrawFacingArrow();
        DrawAttackRange();
        DrawDetectionRange();
        DrawTargetLine();
    }

    private bool ShouldDrawDebug()
    {
        if (!Application.isPlaying)
        {
            return false;
        }

        switch (DebugSettings.CurrentMode)
        {
            case DebugSettings.UnitDebugMode.Off:
                return false;

            case DebugSettings.UnitDebugMode.AllUnits:
                return true;

            case DebugSettings.UnitDebugMode.SelectedOnly:
                return IsThisUnitSelected();

            case DebugSettings.UnitDebugMode.SingleUnit:
                return IsSingleDebugTarget();

            default:
                return false;
        }
    }

    private bool IsThisUnitSelected()
    {
        if (UnitSelectionManager.Instance == null)
        {
            return false;
        }

        return UnitSelectionManager.Instance.unitsSelected.Contains(gameObject);
    }

    private bool IsSingleDebugTarget()
    {
        if (DebugSettings.SingleDebugTarget != null)
        {
            return DebugSettings.SingleDebugTarget == gameObject;
        }

        if (UnitSelectionManager.Instance == null || UnitSelectionManager.Instance.unitsSelected.Count == 0)
        {
            return false;
        }

        return UnitSelectionManager.Instance.unitsSelected[0] == gameObject;
    }

    private void DrawFacingArrow()
    {
        if (_stateController == null)
        {
            _stateController = GetComponent<UnitStateController>();
        }

        Gizmos.color = GetStateColor();

        Vector3 start = transform.position + _arrowOffset;
        Vector3 forward = transform.forward.normalized;
        Vector3 end = start + forward * _arrowLength;

        Gizmos.DrawLine(start, end);

        Vector3 rightHeadDirection = Quaternion.LookRotation(forward) *
                                     Quaternion.Euler(0f, 180f + _arrowHeadAngle, 0f) *
                                     Vector3.forward;

        Vector3 leftHeadDirection = Quaternion.LookRotation(forward) *
                                    Quaternion.Euler(0f, 180f - _arrowHeadAngle, 0f) *
                                    Vector3.forward;

        Gizmos.DrawLine(end, end + rightHeadDirection * _arrowHeadLength);
        Gizmos.DrawLine(end, end + leftHeadDirection * _arrowHeadLength);
    }

    private Color GetStateColor()
    {
        if (_stateController == null)
        {
            return _idleColor;
        }

        return _stateController.CurrentState switch
        {
            UnitStateController.UnitState.Idle => _idleColor,
            UnitStateController.UnitState.Moving => _movingColor,
            UnitStateController.UnitState.Following => _followingColor,
            UnitStateController.UnitState.Attacking => _attackingColor,
            _ => _idleColor
        };
    }

    private void DrawAttackRange()
    {
        if (_combat == null)
        {
            _combat = GetComponent<UnitCombat>();
        }

        if (_combat == null)
        {
            return;
        }

        Gizmos.color = _attackRangeColor;
        Gizmos.DrawWireSphere(transform.position, _combat.AttackRange);
    }

    private void DrawDetectionRange()
    {
        if (_detectionTrigger == null)
        {
            _detectionTrigger = GetComponent<SphereCollider>();
        }

        if (_detectionTrigger == null || !_detectionTrigger.isTrigger)
        {
            return;
        }

        Gizmos.color = _detectionRangeColor;

        Vector3 worldCenter = transform.TransformPoint(_detectionTrigger.center);
        float maxScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
        float radius = _detectionTrigger.radius * maxScale;

        Gizmos.DrawWireSphere(worldCenter, radius);
    }

    private void DrawTargetLine()
    {
        if (_combat == null)
        {
            _combat = GetComponent<UnitCombat>();
        }

        if (_combat == null || _combat.CurrentTarget == null)
        {
            return;
        }

        Gizmos.color = _targetLineColor;
        Gizmos.DrawLine(transform.position, _combat.CurrentTarget.position);
    }
}