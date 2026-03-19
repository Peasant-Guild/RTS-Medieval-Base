using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
public class UnitSpawner : MonoBehaviour
{
    public GameObject _unitToSpawn;
    [SerializeField] private float _spawnTime = 2f;
    [SerializeField] private float _spawnCountdownTimer;
    [SerializeField] private bool _active = false;

    [SerializeField] private Vector3 _offsetVector;
    private void Start()
    {
        _spawnCountdownTimer = _spawnTime;
    }
    private void Update()
    {
        if (_active)
        {
            _spawnCountdownTimer -= Time.deltaTime;
        }
        if (_spawnCountdownTimer <= 0) // <= is for subtle minus differences
        {
            SpawnUnit();
            _spawnCountdownTimer = 2f;
            _active = false;
        }
    }
    private void CallSpawn()
    {
        _active = true;
    }
    private void SpawnUnit()
    {
        Instantiate(_unitToSpawn, transform.position + _offsetVector, transform.rotation);
    }
}