using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private GameObject _selectionIndicator;
    [SerializeField] private UnitDefinition _definition;

    private Animator _animator;
    private Health _health;
    private CombatController _combatController;
    private TargetDetector _targetDetector;
    private UnitMovement _movement;
    private SphereCollider _detectionTrigger;

    public UnitDefinition Definition => _definition;

    private void Awake()
    {
        CacheComponents();
    }

    private void OnEnable()
    {
        UnitDefinition.Changed += HandleDefinitionChanged;
    }

    private void Start()
    {
        ApplyDefinition(resetCurrentHealth: true);
        UnitSelectionManager.Instance.allUnitsList.Add(gameObject);
    }

    private void OnDisable()
    {
        UnitDefinition.Changed -= HandleDefinitionChanged;
    }

    private void OnDestroy()
    {
        if (UnitSelectionManager.Instance != null)
        {
            UnitSelectionManager.Instance.allUnitsList.Remove(gameObject);
        }
    }

    public void SetSelected(bool isSelected)
    {
        if (_selectionIndicator != null)
        {
            _selectionIndicator.SetActive(isSelected);
        }
    }

    public void SetDefinition(UnitDefinition definition, bool resetCurrentHealth = true)
    {
        _definition = definition;
        CacheComponents();
        ApplyDefinition(resetCurrentHealth);
    }

    [ContextMenu("Reapply Definition")]
    public void ReapplyDefinition()
    {
        CacheComponents();
        ApplyDefinition(resetCurrentHealth: false);
    }

    private void OnValidate()
    {
        CacheComponents();
        ApplyDefinition(resetCurrentHealth: false);
    }

    private void CacheComponents()
    {
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }

        if (_health == null)
        {
            _health = GetComponent<Health>();
        }

        if (_combatController == null)
        {
            _combatController = GetComponent<CombatController>();
        }

        if (_targetDetector == null)
        {
            _targetDetector = GetComponent<TargetDetector>();
        }

        if (_movement == null)
        {
            _movement = GetComponent<UnitMovement>();
        }

        if (_detectionTrigger == null)
        {
            _detectionTrigger = GetComponent<SphereCollider>();
        }
    }

    private void ApplyDefinition(bool resetCurrentHealth)
    {
        if (_definition == null)
        {
            return;
        }

        if (_health != null)
        {
            _health.ConfigureMaxHealth(_definition.MaxHealth, resetCurrentHealth);
        }

        if (_combatController != null)
        {
            _combatController.ConfigureAttack(
                _definition.AttackRange,
                _definition.AttackDamage,
                _definition.AttackInterval
            );
        }

        if (_movement != null)
        {
            _movement.ConfigureMoveSpeed(_definition.MoveSpeed);
        }

        if (_detectionTrigger != null)
        {
            _detectionTrigger.radius = Mathf.Max(0.1f, _definition.DetectionRange);
        }

        if (_animator != null && _definition.AnimatorController != null)
        {
            _animator.runtimeAnimatorController = _definition.AnimatorController;
        }
    }

    private void HandleDefinitionChanged(UnitDefinition changedDefinition)
    {
        if (changedDefinition == null || changedDefinition != _definition)
        {
            return;
        }

        ApplyDefinition(resetCurrentHealth: false);
    }
}
