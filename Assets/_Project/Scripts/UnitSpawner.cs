using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using System.Collections.Generic;
using System.Linq;

public class UnitSpawner : MonoBehaviour
{
    
    [SerializeField] private GameObject _unitType; //for debug 
    
    private class SpawnInfo //does this survive the conventions test? :0
    {
        public GameObject unitPrefab;
        public int amount;
        public float spawnTime;
        public SpawnInfo(GameObject unit, int amount, float spawnTime)
        {
            this.unitPrefab = unit;
            this.amount = amount;
            this.spawnTime = spawnTime;
        }
    }
    

    [SerializeField] private float _spawnCountdownTimer; //is a serialized field for debugging

    //offsets are relative to the *center* of the building
    [SerializeField] private Vector3 _entranceSpawnOffset;
    [SerializeField] private Vector3 _outsideOffset; //walk out position (creation animation)
    
    
    private GameObject _currentUnit = null;
    
    [SerializeField] private List<SpawnInfo> _unitsToSpawn =  new List<SpawnInfo>();
    

    private void Start()
    {
        _spawnCountdownTimer = 0;
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
        _currentUnit = _unitsToSpawn[0].unitPrefab;
        _spawnCountdownTimer = _unitsToSpawn[0].spawnTime + _spawnCountdownTimer; //conserve overall time frame (carry over negatives)
        if (--_unitsToSpawn[0].amount == 0)
        {
            _unitsToSpawn.RemoveAt(0);
        }
    }
    
    public void CallSpawn(GameObject unit, int amount = 1, float spawnTime = 2f)
    {
        if (unit == null || amount <= 0 || spawnTime <= 0f)
        {
            return;
        }

        if (_unitsToSpawn.Count > 0 && _unitsToSpawn.Last().unitPrefab == unit)
        {
            _unitsToSpawn.Last().amount+= amount;
        }
        else
        {
            _unitsToSpawn.Add(new SpawnInfo(unit, amount, spawnTime));
        }
    }

    public void CancelLatestCreation(GameObject unit)
    {
        if (unit == null)
        {
            return;
        }

        for (int i = _unitsToSpawn.Count - 1; i >= 0; i--)
        {
            if (_unitsToSpawn[i].unitPrefab == unit)
            {
                if (--_unitsToSpawn[i].amount == 0)
                {
                    _unitsToSpawn.RemoveAt(i);
                }

                return;
            }
        }

        if (_currentUnit == unit)
        {
            _currentUnit = null;
            _spawnCountdownTimer = 0f;
        }
    }
    
    private void SpawnUnit(GameObject unitToSpawn)
    {
        GameObject unit_obj = Instantiate(unitToSpawn, transform.position + _entranceSpawnOffset, transform.rotation);
        if (unit_obj == null)
        {
            return;
        }
        UnitMovement walker = unit_obj.GetComponent<UnitMovement>();
        if (walker == null)
        {
            return;
        }
        walker.MoveTo(transform.position + _outsideOffset);
        
        //TODO: needs waypoints in order to send to a final destination
    }
}