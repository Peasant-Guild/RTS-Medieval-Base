using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using System.Collections.Generic;

public class UnitSpawner : MonoBehaviour
{
    
    [SerializeField] private GameObject _unitType; //for debug 
    private class spawnInfo
    
    {
        public GameObject gameObject;
        public int amount;
        public float spawnTime;
        public spawnInfo(GameObject unit, int amount, float spawnTime)
        {
            this.gameObject = unit;
            this.amount = amount;
            this.spawnTime = spawnTime;
        }
    }
    
    // [SerializeField] private float _spawnTime = 2f;
    // [SerializeField] private bool _spawnIndefinitely;
    // [SerializeField] private int _currentlySpawning = 0;

    [SerializeField] private float _spawnCountdownTimer;

    //offsets are relative to the *center* of the building
    [SerializeField] private Vector3 _entranceSpawnOffset;
    [SerializeField] private Vector3 _outsideOffset; //walk out position
    
    
    private GameObject _currentUnit = null;
    
    [SerializeField] private Queue<spawnInfo> _unitsToSpawn =  new Queue<spawnInfo>();
    

    private void Start()
    {
        _spawnCountdownTimer = 0;
        // CallSpawn(_unitType, 4, 3f);
        // CallSpawn(_unitType, 2, 10f);
    }
    private void Update()
    {
        if (_spawnCountdownTimer > 0f)
        {
            UpdateTimer();
        }
        else
        {
            if (_currentUnit != null)
            {
                SpawnUnit(_currentUnit);
            }
            
            // if (!_spawnIndefinitely)
            // {
            //     _currentlySpawning -= 1;
            // }
            if (_unitsToSpawn.Count <= 0)
            {
                _currentUnit = null;
                return;
            }

            LoadNextUnit();
            
            //whether or not we are currently working is determined by the _currentUnit (if its null, we're idle)
        }
    }

    private void UpdateTimer()
    {
        _spawnCountdownTimer -= Time.deltaTime;
    }

    private void LoadNextUnit()
    {
        _currentUnit = _unitsToSpawn.Peek().gameObject;
        _spawnCountdownTimer = _unitsToSpawn.Peek().spawnTime;
        if (_unitsToSpawn.Peek().amount-- == 1)
        {
            _unitsToSpawn.Dequeue();
        }
    }
    
    // private void OnMouseDown() //debug purposes only
    // {
    //     _spawnCountdownTimer = -1f;
    //     CallSpawn(_unitType, 4, 3f);
    //     CallSpawn(_unitType, 2, 10f);
    // }
    private void CallSpawn(GameObject unit, int amount = 1, float spawnTime = 2f)
    {
        if (unit == null || amount <= 0 || spawnTime <= 0f)
        {
            return;
        }
        
        _unitsToSpawn.Enqueue(new spawnInfo(unit, amount, spawnTime));
        
        
        // _currentlySpawning += amount;
    }

    // public void StartIndefiniteSpawning()
    // {
    //     _spawnIndefinitely = true;
    //     _currentlySpawning = 0;
    // }
    // public void StopIndefiniteSpawning()
    // {
    //     _spawnIndefinitely = false;
    //     _currentlySpawning = 0;
    // }
    private void SpawnUnit(GameObject unitToSpawn)
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