using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelectionManager : MonoBehaviour
{
    public static UnitSelectionManager Instance { get; set; }

    public List<GameObject> allUnitsList = new List<GameObject>();
    public List<GameObject> unitsSelected = new List<GameObject>();

    [SerializeField] private LayerMask _clickable;
    [SerializeField] private LayerMask _ground;
    [SerializeField] private GameObject _groundMarker;

    private Camera _cam;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        _cam = Camera.main;
    }

    private void Update()
    {
        HandleLeftClickSelection();
        HandleRightClickMovementMarker();
    }

    private void HandleLeftClickSelection()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }

        Ray ray = _cam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _clickable))
        {
            GameObject clickedObject = hit.collider.gameObject;

            if (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed)
            {
                ToggleUnitSelection(clickedObject);
            }
            else
            {
                SelectSingleUnit(clickedObject);
            }
        }
        else if (Keyboard.current == null || !Keyboard.current.leftShiftKey.isPressed)
        {
            ClearSelection();
        }
    }

    private void HandleRightClickMovementMarker()
    {
        if (Mouse.current == null || !Mouse.current.rightButton.wasPressedThisFrame || unitsSelected.Count == 0)
        {
            return;
        }

        Ray ray = _cam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _ground))
        {
            ShowGroundMarker(hit.point);
        }
    }

    private void SelectSingleUnit(GameObject unit)
    {
        ClearSelection();

        unitsSelected.Add(unit);
        SetUnitSelected(unit, true);
        SetUnitMovementEnabled(unit, true);
    }

    private void ToggleUnitSelection(GameObject unit)
    {
        if (unitsSelected.Contains(unit))
        {
            SetUnitSelected(unit, false);
            SetUnitMovementEnabled(unit, false);
            unitsSelected.Remove(unit);
        }
        else
        {
            unitsSelected.Add(unit);
            SetUnitSelected(unit, true);
            SetUnitMovementEnabled(unit, true);
        }
    }

    private void ClearSelection()
    {
        foreach (GameObject unit in unitsSelected)
        {
            SetUnitSelected(unit, false);
            SetUnitMovementEnabled(unit, false);
        }

        unitsSelected.Clear();
        _groundMarker.SetActive(false);
    }

    private void SetUnitMovementEnabled(GameObject unit, bool shouldEnable)
    {
        UnitMovement unitMovement = unit.GetComponent<UnitMovement>();

        if (unitMovement != null)
        {
            unitMovement.enabled = shouldEnable;
        }
    }

    private void SetUnitSelected(GameObject unit, bool isSelected)
    {
        Unit unitComponent = unit.GetComponent<Unit>();

        if (unitComponent != null)
        {
            unitComponent.SetSelected(isSelected);
        }
    }

    private void ShowGroundMarker(Vector3 position)
    {
        _groundMarker.transform.position = position;
        _groundMarker.SetActive(false);
        // TODO: Marker animation here
        _groundMarker.SetActive(true);
    }
}