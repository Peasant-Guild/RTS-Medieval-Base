using UnityEngine;
public class UnitSpawner : MonoBehaviour
{
    public GameObject _unitToSpawn;
    [SerializeField] private float _spawnTime = 2f;
    [SerializeField] private float _spawnCountdownTimer;
    [SerializeField] private bool _active = false; //answers the question: "Should I start spawning a unit?"

    [SerializeField] private Vector3 _offsetVector;
    private void Start()
    {
        _spawnCountdownTimer = _spawnTime;
    }
    private void Update()
    {
        if (_active)
        {
            _spawnCountdownTimer -= Time.deltaTime; //countdown mechanic
        }
        if (_spawnCountdownTimer <= 0) // <= is for subtle minus differences
        {
            SpawnUnit();
            _spawnCountdownTimer = _spawnTime;
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