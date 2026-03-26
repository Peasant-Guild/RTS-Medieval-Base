using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UnitDebugVisualizer : MonoBehaviour
{
    [Header("State Colors")]
    [SerializeField] private Color _idleColor = Color.gray;
    [SerializeField] private Color _movingColor = Color.cyan;
    [FormerlySerializedAs("_followingColor")]
    [SerializeField] private Color _pursuingColor = Color.yellow;
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
    [SerializeField] private Vector3 _labelOffset = new Vector3(0f, 2.6f, 0f);
    [SerializeField] private Color _labelColor = Color.white;

    private UnitStateController _stateController;
    private CombatController _combatController;
    private SphereCollider _detectionTrigger;
    private Health _health;
    private TeamMember _teamMember;

    private void Awake()
    {
        _stateController = GetComponent<UnitStateController>();
        _combatController = GetComponent<CombatController>();
        _detectionTrigger = GetComponent<SphereCollider>();
        _health = GetComponent<Health>();
        _teamMember = GetComponent<TeamMember>();
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
        DrawDebugLabel();
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
            UnitStateController.UnitState.Pursuing => _pursuingColor,
            UnitStateController.UnitState.Attacking => _attackingColor,
            _ => _idleColor
        };
    }

    private void DrawAttackRange()
    {
        if (_combatController == null)
        {
            _combatController = GetComponent<CombatController>();
        }

        if (_combatController == null)
        {
            return;
        }

        Gizmos.color = _attackRangeColor;
        Gizmos.DrawWireSphere(transform.position, _combatController.AttackRange);
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
        if (_stateController == null)
        {
            _stateController = GetComponent<UnitStateController>();
        }

        if (_stateController == null || _stateController.CurrentTarget == null)
        {
            return;
        }

        Gizmos.color = _targetLineColor;
        Gizmos.DrawLine(transform.position, _stateController.CurrentTarget.position);
    }

    private void DrawDebugLabel()
    {
#if UNITY_EDITOR
        if (_health == null)
        {
            _health = GetComponent<Health>();
        }

        if (_teamMember == null)
        {
            _teamMember = GetComponent<TeamMember>();
        }

        if (_health == null && _teamMember == null)
        {
            return;
        }

        GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = _labelColor }
        };

        string labelText = BuildDebugLabel();
        Handles.Label(transform.position + _labelOffset, labelText, style);
#endif
    }

    private string BuildDebugLabel()
    {
        string teamText = _teamMember != null ? $"T{_teamMember.TeamId}" : "T?";

        if (_health == null)
        {
            return teamText;
        }

        return $"{teamText} | HP {_health.CurrentHealth:0.#}/{_health.MaxHealth:0.#}";
    }
}
