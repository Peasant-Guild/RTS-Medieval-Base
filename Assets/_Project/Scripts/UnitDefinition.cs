using UnityEngine;
using System;

[CreateAssetMenu(menuName = "RTS/Unit Definition", fileName = "UnitDefinition")]
public class UnitDefinition : ScriptableObject
{
    [SerializeField] private GameObject _prefab;
    [SerializeField] private RuntimeAnimatorController _animatorController;
    [SerializeField] private float _maxHealth = 10f;
    [SerializeField] private float _moveSpeed = 3.5f;
    [SerializeField] private float _detectionRange = 15f;
    [SerializeField] private float _attackRange = 5f;
    [SerializeField] private float _attackDamage = 4f;
    [SerializeField] private float _attackInterval = 1f;
    [SerializeField] private float _spawnTime = 1f;
    

    public GameObject Prefab => _prefab;
    public RuntimeAnimatorController AnimatorController => _animatorController;
    public float MaxHealth => _maxHealth;
    public float MoveSpeed => _moveSpeed;
    public float DetectionRange => _detectionRange;
    public float AttackRange => _attackRange;
    public float AttackDamage => _attackDamage;
    public float AttackInterval => _attackInterval;
    public float SpawnTime => _spawnTime;
    
    public static event Action<UnitDefinition> Changed;

    private void OnValidate()
    {
        Changed?.Invoke(this);
    }
}
