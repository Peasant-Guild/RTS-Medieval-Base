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
    private UnitCombat _combat;

    public UnitState CurrentState { get; private set; } = UnitState.Idle;

    private void Awake()
    {
        _movement = GetComponent<UnitMovement>();
        _combat = GetComponent<UnitCombat>();

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
        if (_combat != null)
        {
            _combat.ClearTarget();
        }

        if (_movement != null)
        {
            _movement.MoveTo(destination, 0f);
        }

        ChangeState(UnitState.Moving);
    }

    public void FollowTarget(Transform target)
    {
        if (target == null || _combat == null)
        {
            return;
        }

        _combat.SetTarget(target, true);
        ChangeState(UnitState.Following);
    }

    public void ChangeState(UnitState newState)
    {
        CurrentState = newState;
    }

    private void UpdateIdleState()
    {
        if (_combat != null && _combat.HasTarget())
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
        if (_combat == null || !_combat.HasTarget())
        {
            ChangeState(UnitState.Idle);
            return;
        }

        Transform target = _combat.CurrentTarget;

        if (_movement != null)
        {
            _movement.MoveTo(target.position, _combat.AttackRange);
        }

        if (_combat.IsTargetInAttackRange())
        {
            ChangeState(UnitState.Attacking);
        }
    }

    private void UpdateAttackingState()
    {
        if (_combat == null || !_combat.HasTarget())
        {
            ChangeState(UnitState.Idle);
            return;
        }

        if (!_combat.IsTargetInAttackRange())
        {
            ChangeState(UnitState.Following);
            return;
        }

        if (_movement != null)
        {
            _movement.Stop();
        }

        // TODO: Real attack logic
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
}