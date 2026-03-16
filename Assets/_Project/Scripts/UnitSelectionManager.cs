using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class UnitSelectionManager : MonoBehaviour
{
    public static UnitSelectionManager Instance { get; set; }

    public List<GameObject> allUnitsList = new List<GameObject>();
    public List<GameObject> unitsSelected = new List<GameObject>();
    
    [SerializeField] private LayerMask clickable;
    [SerializeField] private LayerMask ground;
    [SerializeField] private GameObject groundMarker;
    
    private Camera _cam;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        _cam = Camera.main;
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = _cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            // If hitting a clickable object
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, clickable))
            {
                if (Keyboard.current.leftShiftKey.isPressed)
                {
                    MultiSelect(hit.collider.gameObject);
                }
                else
                {
                    SelectByClicking(hit.collider.gameObject);
                }
            }
            // If not hitting a clickable object
            else if (!Keyboard.current.leftShiftKey.isPressed)
            {
                DeselectAll();
            }
        }

        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame && unitsSelected.Count > 0)
        {
            Ray ray = _cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            // If hitting a clickable object
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ground))
            {
                groundMarker.transform.position = hit.point;
                
                groundMarker.SetActive(false);
                // TODO: Animation
                groundMarker.SetActive(true);
            }
        }
    }

    private void MultiSelect(GameObject unit)
    {
        if (!unitsSelected.Contains(unit))
        {
            unitsSelected.Add(unit);
            TriggerSelectionIndicator(unit, true);
            EnableUnitMovement(unit, true);
        }
        else
        {
            EnableUnitMovement(unit, false);
            TriggerSelectionIndicator(unit, false);
            unitsSelected.Remove(unit);
        }
    }

    private void DeselectAll()
    {
        foreach (var unit in unitsSelected)
        {
            EnableUnitMovement(unit, false);
            TriggerSelectionIndicator(unit, false);
        }
        
        groundMarker.SetActive(false);
        
        unitsSelected.Clear();
    }
    
    private void SelectByClicking(GameObject unit)
    {
        DeselectAll();
        
        unitsSelected.Add(unit);

        TriggerSelectionIndicator(unit, true);
        EnableUnitMovement(unit, true);
    }

    private void EnableUnitMovement(GameObject unit, bool shouldMove)
    {
        unit.GetComponent<UnitMovement>().enabled = shouldMove;
    }

    private void TriggerSelectionIndicator(GameObject unit, bool isSelected)
    {
        unit.transform.GetChild(0).gameObject.SetActive(isSelected);
    }
}
