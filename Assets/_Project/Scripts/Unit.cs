using System;
using UnityEngine;

public class Unit : MonoBehaviour
{
    
    void Start()
    {
        UnitSelectionManager.Instance.allUnitsList.Add(gameObject);
    }

    private void OnDestroy()
    {
        UnitSelectionManager.Instance.allUnitsList.Remove(gameObject);
    }
}
