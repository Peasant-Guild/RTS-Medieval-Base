using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class UnitSpawner : MonoBehaviour
{
    public GameObject unitToSpawn;
    [SerializeField] private float _spawnTime = 2f;
    [SerializeField] private float _spawnCountdownTimer;
    [SerializeField] private bool _spawnIndefinitely; //should the spawner keep spawning indefinitely
    [FormerlySerializedAs("_active")] [SerializeField] private int _currentlySpawning = 0; // amount of units to be spawned

    [SerializeField] private Vector3 _offsetVector;
    private void Start()
    {
        _spawnCountdownTimer = _spawnTime;
    }
    private void Update()
    {
        if (_currentlySpawning > 0 || _spawnIndefinitely)
        {
            _spawnCountdownTimer -= Time.deltaTime; // countdown mechanic
        }
        if (_spawnCountdownTimer <= 0) // <= is for subtle minus differences
        {
            SpawnUnit();
            _spawnCountdownTimer = _spawnTime;
            if (!_spawnIndefinitely)
            {
                _currentlySpawning -= 1;
            }
        }
    }

    // private void OnMouseDown() //debug purposes only
    // {
    //     StartIndefiniteSpawning();
    // }
    private void CallSpawn(int amount = 1)
    {
        _currentlySpawning += amount;
    }

    private void StartIndefiniteSpawning()
    {
        _spawnIndefinitely = true;
        _currentlySpawning = 0;
    }
    private void StopIndefiniteSpawning()
    {
        _spawnIndefinitely = false;
        _currentlySpawning = 0;
    }
    private void SpawnUnit()
    {
        Instantiate(unitToSpawn, transform.position + _offsetVector, transform.rotation);
    }
}