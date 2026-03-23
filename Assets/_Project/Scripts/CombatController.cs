using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(TeamMember))]
public class CombatController : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private float _attackRange = 2f;
    [SerializeField] private float _attackDamage = 1f;
    [SerializeField] private float _attackInterval = 1f;

    private float _lastAttackTime = float.NegativeInfinity;
    private Health _health;
    private TeamMember _teamMember;

    public float AttackRange => _attackRange;

    private void Awake()
    {
        _health = GetComponent<Health>();
        _teamMember = GetComponent<TeamMember>();
    }

    public bool IsTargetInAttackRange(Transform target)
    {
        if (!CanAttackTarget(target))
        {
            return false;
        }

        float distanceSqr = (target.position - transform.position).sqrMagnitude;
        return distanceSqr <= _attackRange * _attackRange;
    }

    public bool TryAttack(Transform target)
    {
        if (!_health.IsAlive || !CanAttackTarget(target))
        {
            return false;
        }

        if (Time.time < _lastAttackTime + _attackInterval)
        {
            return false;
        }

        _lastAttackTime = Time.time;
        target.GetComponent<Health>().TakeDamage(_attackDamage);
        return true;
    }

    private bool CanAttackTarget(Transform target)
    {
        if (target == null || _teamMember == null)
        {
            return false;
        }

        TeamMember otherTeamMember = target.GetComponent<TeamMember>();
        Health otherHealth = target.GetComponent<Health>();

        if (otherTeamMember == null || otherHealth == null)
        {
            return false;
        }

        return _teamMember.IsHostileTo(otherTeamMember) && otherHealth.IsAlive;
    }
}
