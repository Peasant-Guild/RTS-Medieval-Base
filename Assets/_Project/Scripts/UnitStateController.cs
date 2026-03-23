using UnityEngine;

public class UnitStateController : MonoBehaviour
{
    public enum UnitState
    {
        Idle,
        Moving,
        Following,
        Attacking
    }

    [SerializeField] private Animator _animator;

    private UnitMovement _movement;
    private CombatController _combatController;
    private TargetDetector _targetDetector;
    private bool _isCommandedTarget;

    public Transform CurrentTarget { get; private set; }

    public UnitState CurrentState { get; private set; } = UnitState.Idle;

    private void Awake()
    {
        _movement = GetComponent<UnitMovement>();
        _combatController = GetComponent<CombatController>();
        _targetDetector = GetComponent<TargetDetector>();

        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }
    }

    private void Update()
    {
        switch (CurrentState)
        {
            case UnitState.Idle:
                UpdateIdleState();
                break;

            case UnitState.Moving:
                UpdateMovingState();
                break;

            case UnitState.Following:
                UpdateFollowingState();
                break;

            case UnitState.Attacking:
                UpdateAttackingState();
                break;
        }

        UpdateAnimatorParameters();
    }

    public void MoveTo(Vector3 destination)
    {
        ClearTarget();

        if (_movement != null)
        {
            _movement.MoveTo(destination, 0f);
        }

        ChangeState(UnitState.Moving);
    }

    public void FollowTarget(Transform target)
    {
        if (target == null || _targetDetector == null || !_targetDetector.CanTarget(target))
        {
            return;
        }

        CurrentTarget = target;
        _isCommandedTarget = true;
        ChangeState(UnitState.Following);
    }

    public void ChangeState(UnitState newState)
    {
        CurrentState = newState;
    }

    private void UpdateIdleState()
    {
        if (ResolveCurrentTarget())
        {
            ChangeState(UnitState.Following);
        }
    }

    private void UpdateMovingState()
    {
        if (_movement != null && _movement.HasReachedDestination())
        {
            ChangeState(UnitState.Idle);
        }
    }

    private void UpdateFollowingState()
    {
        if (_combatController == null || !ResolveCurrentTarget())
        {
            ChangeState(UnitState.Idle);
            return;
        }

        if (_movement != null)
        {
            _movement.MoveTo(CurrentTarget.position, _combatController.AttackRange);
        }

        if (_combatController != null && _combatController.IsTargetInAttackRange(CurrentTarget))
        {
            ChangeState(UnitState.Attacking);
        }
    }

    private void UpdateAttackingState()
    {
        if (_combatController == null || !ResolveCurrentTarget())
        {
            ChangeState(UnitState.Idle);
            return;
        }

        if (_combatController == null || !_combatController.IsTargetInAttackRange(CurrentTarget))
        {
            ChangeState(UnitState.Following);
            return;
        }

        if (_movement != null)
        {
            _movement.Stop();
            _movement.RotateTowards(CurrentTarget.position);
        }
        
        _combatController.TryAttack(CurrentTarget);
    }

    private void UpdateAnimatorParameters()
    {
        if (_animator == null)
        {
            return;
        }

        _animator.SetBool("IsMoving", CurrentState == UnitState.Moving || CurrentState == UnitState.Following);
        _animator.SetBool("IsFollowing", CurrentState == UnitState.Following);
        _animator.SetBool("IsAttacking", CurrentState == UnitState.Attacking);
    }

    private bool ResolveCurrentTarget()
    {
        if (_targetDetector == null)
        {
            ClearTarget();
            return false;
        }

        if (_isCommandedTarget)
        {
            if (_targetDetector.CanTarget(CurrentTarget))
            {
                return true;
            }

            _isCommandedTarget = false;
            CurrentTarget = null;
        }

        CurrentTarget = _targetDetector.GetClosestDetectedTarget();
        return CurrentTarget != null;
    }

    private void ClearTarget()
    {
        CurrentTarget = null;
        _isCommandedTarget = false;
    }
}
