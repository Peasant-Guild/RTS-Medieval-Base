using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

public class UnitSpawner : MonoBehaviour
{
    
    [SerializeField] private UnitDefinition _unitDef; //for debug (A temporary solution until a menu is configured - see OnMouseDown) 
    [SerializeField] private int _shiftClickAmount = 5;
    [SerializeField] private int _spawnTeamId = 1;
    [System.Serializable] private class SpawnInfo //does this survive the conventions test? :0
    {
        public UnitDefinition unitDefinition;
        public int amount;
        public SpawnInfo(UnitDefinition unitDef, int amount = 1)
        {
            this.unitDefinition = unitDef;
            this.amount = amount;
        }
    }
    

    [SerializeField] private float _spawnCountdownTimer; //is a serialized field for debugging

    //offsets are relative to the *center* of the building
    [SerializeField] private Vector3 _entranceSpawnOffset;
    [SerializeField] private Vector3 _outsideOffset; //walk out position (creation animation)
    
    
    private UnitDefinition _currentUnit = null;
    
    [SerializeField] private List<SpawnInfo> _unitsToSpawn =  new List<SpawnInfo>();
    

    private void Start()
    {
        _spawnCountdownTimer = 0;
    }

    private void OnMouseDown() //A temporary solution until a menu is configured - DEBUG SOLUTION
    {
        if (Keyboard.current.shiftKey.isPressed)
        {
            CallSpawn(_unitDef, _shiftClickAmount);
        }
        else
        {
            CallSpawn(_unitDef);
        }
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
        _currentUnit = _unitsToSpawn[0].unitDefinition;
        _spawnCountdownTimer = _unitsToSpawn[0].unitDefinition.SpawnTime + _spawnCountdownTimer; //conserve overall time frame (carry over negatives)
        if (--_unitsToSpawn[0].amount == 0)
        {
            _unitsToSpawn.RemoveAt(0);
        }
    }
    
    public void CallSpawn(UnitDefinition unitDef, int amount = 1)
    {
        if (unitDef == null)
        {
            Debug.LogError("Spawn Aborted: we are missing the required Unit Definition component");
            return;
        }
        if (amount <= 0)
        {
            return;
        }

        if (_unitsToSpawn.Count > 0 && _unitsToSpawn.Last().unitDefinition == unitDef)
        {
            _unitsToSpawn.Last().amount+= amount;
        }
        else
        {
            _unitsToSpawn.Add(new SpawnInfo(unitDef, amount));
        }
    }

    public void UndoLatestUnitCreationReq(UnitDefinition unitDef)
    {
        if (unitDef == null)
        {
            Debug.LogError("Spawn Aborted: we are missing the required Unit Definition component");
            return;
        }

        for (int i = _unitsToSpawn.Count - 1; i >= 0; i--)
        {
            if (_unitsToSpawn[i].unitDefinition == unitDef)
            {
                if (--_unitsToSpawn[i].amount == 0)
                {
                    _unitsToSpawn.RemoveAt(i);
                }

                return;
            }
        }

        if (_currentUnit == unitDef)
        {
            _currentUnit = null;
            _spawnCountdownTimer = 0f;
        }
    }
    
    private void SpawnUnit(UnitDefinition unitToSpawnDef)
    {
        GameObject unitToSpawn = unitToSpawnDef.Prefab;
        if (unitToSpawn == null)
        {
            Debug.LogError("Spawn Aborted: we are missing the required UnitPrefab");
            return; 
        }
        if (unitToSpawn.GetComponent<UnitMovement>() == null)
        {
            Debug.LogError("Spawn Aborted: we are missing the required UnitMovement component");
            return; 
        }
        Vector3 newPos = transform.position + _entranceSpawnOffset;
        newPos.y = 0;
        GameObject unit_obj = Instantiate(unitToSpawn, newPos, transform.rotation);
        if (unit_obj == null)
        {
            return;
        }
        //A temporary solution until a menu is configured - DEBUG SOLUTION:
        TeamMember spawnedUnit = unit_obj.GetComponent<TeamMember>();
        if (spawnedUnit != null)
        {
            spawnedUnit.SetTeamId(_spawnTeamId);
        }
        // end of DEBUG SOLUTION
        UnitMovement walker = unit_obj.GetComponent<UnitMovement>();
        walker.MoveTo(transform.position + _outsideOffset);
        
        //TODO: needs waypoints in order to send to a final destination
    }
}