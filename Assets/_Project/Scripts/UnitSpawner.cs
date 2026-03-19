using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class UnitSpawner : MonoBehaviour
{
    public GameObject unitToSpawn;
    [SerializeField] private float _spawnTime = 2f;
    [SerializeField] private float _spawnCountdownTimer;
    [SerializeField] private bool _spawnIndefinitely;
    [SerializeField] private int _currentlySpawning = 0;

    //offsets are relative to the *center* of the building
    [SerializeField] private Vector3 _entranceSpawnOffset;
    [SerializeField] private Vector3 _outsideOffset; //walk out position
    [SerializeField] private Vector3 _destination;
    

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
    //     CallSpawn();
    // }
    private void CallSpawn(int amount = 1)
    {
        _currentlySpawning += amount;
    }

    public void StartIndefiniteSpawning()
    {
        _spawnIndefinitely = true;
        _currentlySpawning = 0;
    }
    public void StopIndefiniteSpawning()
    {
        _spawnIndefinitely = false;
        _currentlySpawning = 0;
    }
    private void SpawnUnit()
    {
        GameObject unit_obj = Instantiate(unitToSpawn, transform.position + _entranceSpawnOffset, transform.rotation);
        UnitMovement walker = unit_obj.GetComponent<UnitMovement>();
        walker.MoveTo(transform.position + _outsideOffset);
        
        //needs waypoints!
        // if (_destination != Vector3.zero)
        // {
        //     walker.MoveTo(_destination);
        // }
    }
}