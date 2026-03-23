using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class UnitSelectionManager : MonoBehaviour
{
    public static UnitSelectionManager Instance { get; set; }
    
    public List<GameObject> allUnitsList = new List<GameObject>();
    
    public IndexedSet<GameObject> unitsSelected = new IndexedSet<GameObject>(); //an optimized version of list implemented at the bottom
    
    [SerializeField] private LayerMask _clickable;
    [SerializeField] private LayerMask _ground;
    [SerializeField] private GameObject _groundMarker;
    [SerializeField] private RectTransform _selectBox;
    [SerializeField] private float _dragThreshold = 10f;

    private Camera _cam;
    private Vector2 _mousePosition;
    private Vector2 _boxStartPos;
    private Vector2 _boxDimensions;
    private bool _isDragging;

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

        if (_cam == null)
        {
            Debug.LogError("UnitSelectionManager could not find a main camera.", this);
            enabled = false;
            return;
        }

        if (_groundMarker == null)
        {
            Debug.LogWarning("UnitSelectionManager has no ground marker assigned.", this);
        }
    }

    private void Update()
    {
        HandleLeftClickSelection();
        HandleRightClickMovementMarker();
    }

    private void HandleLeftClickSelection()
    {
        if (Mouse.current == null || (!Mouse.current.leftButton.wasPressedThisFrame &&
                                      !Mouse.current.leftButton.isPressed &&
                                      !Mouse.current.leftButton.wasReleasedThisFrame))
        {
            _isDragging = false;
            return;
        }

        _mousePosition = Mouse.current.position.ReadValue();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleLeftClickPressed();
        }

        if (Mouse.current.leftButton.isPressed)
        {
            HandleLeftClickHeld();
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            HandleLeftClickReleased();
        }
    }

    private void HandleLeftClickPressed()
    {
        _boxStartPos = _mousePosition;
        _isDragging = false;
        CleanBox();
    }

    private void HandleLeftClickHeld()
    {
        if (!_isDragging && Vector2.Distance(_boxStartPos, _mousePosition) > _dragThreshold)
        {
            _isDragging = true;

            if (_selectBox != null)
            {
                _selectBox.gameObject.SetActive(true);
            }
        }

        if (_isDragging)
        {
            UpdateSelectionBoxVisual();
            UpdateBoxSelection();
        }
    }

    private void HandleLeftClickReleased()
    {
        if (!_isDragging)
        {
            HandleBasicSelect();
        }

        _isDragging = false;
        _boxStartPos = Vector2.zero;
        CleanBox();
    }

    private void UpdateSelectionBoxVisual()
    {
        _boxDimensions = new Vector2(_mousePosition.x - _boxStartPos.x, _mousePosition.y - _boxStartPos.y);

        if (_selectBox == null)
        {
            return;
        }

        if (!_selectBox.gameObject.activeInHierarchy)
        {
            _selectBox.gameObject.SetActive(true);
        }

        _selectBox.sizeDelta = new Vector2(Mathf.Abs(_boxDimensions.x), Mathf.Abs(_boxDimensions.y));
        _selectBox.anchoredPosition = _boxStartPos + _boxDimensions / 2f;
    }

    private void UpdateBoxSelection()
    {
        if (Keyboard.current == null || !Keyboard.current.leftShiftKey.isPressed)
        {
            ClearSelection();
        }

        SelectMultiUnit();
    }

    private void HandleBasicSelect()
    {
        Ray ray = _cam.ScreenPointToRay(_mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _clickable))
        {
            Unit clickedUnit = hit.collider.GetComponentInParent<Unit>();

            if (clickedUnit == null)
            {
                return;
            }

            GameObject clickedObject = clickedUnit.gameObject;

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

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _clickable))
        {
            TeamMember clickedTeamMember = hit.collider.GetComponentInParent<TeamMember>();

            if (clickedTeamMember != null && CommandSelectedUnitsToFollow(clickedTeamMember.transform))
            {
                return;
            }
        }

        if (Physics.Raycast(ray, out RaycastHit groundHit, Mathf.Infinity, _ground))
        {
            CommandSelectedUnitsToMove(groundHit.point);
            ShowGroundMarker(groundHit.point);
        }
    }
    
    private bool CommandSelectedUnitsToFollow(Transform target)
    {
        bool commandedAnyUnits = false;

        foreach (GameObject unit in unitsSelected)
        {
            if (unit == null)
            {
                continue;
            }

            UnitStateController stateController = unit.GetComponent<UnitStateController>();
            TargetDetector targetDetector = unit.GetComponent<TargetDetector>();

            if (stateController != null && targetDetector != null && targetDetector.CanTarget(target))
            {
                stateController.FollowTarget(target);
                commandedAnyUnits = true;
            }
        }

        return commandedAnyUnits;
    }

    private void CommandSelectedUnitsToMove(Vector3 destination)
    {
        foreach (GameObject unit in unitsSelected)
        {
            if (unit == null)
            {
                continue;
            }

            UnitStateController stateController = unit.GetComponent<UnitStateController>();

            if (stateController != null)
            {
                stateController.MoveTo(destination);
            }
        }
    }

    private void SelectSingleUnit(GameObject unit)
    {
        ClearSelection();
        
        unitsSelected.Add(unit);
        SetUnitSelected(unit, true);
    }

    private void ToggleUnitSelection(GameObject unit)
    {
        if (unitsSelected.Contains(unit))
        {
            SetUnitSelected(unit, false);
            unitsSelected.Remove(unit);
        }
        else
        {
            unitsSelected.Add(unit);
            SetUnitSelected(unit, true);
        }
    }

    private void SelectMultiUnit()
    {
        foreach (GameObject unit in allUnitsList)
        {
            if (unit == null || unitsSelected.Contains(unit) || !IsUnitInBox(unit))
            {
                continue;
            }

            unitsSelected.Add(unit);
            SetUnitSelected(unit, true);
        }
    }

    private bool IsUnitInBox(GameObject unit)
    {
        if (unit == null || _selectBox == null)
        {
            return false;
        }

        Vector3 screenPos = _cam.WorldToScreenPoint(unit.transform.position);
        return screenPos.z > 0 && RectTransformUtility.RectangleContainsScreenPoint(_selectBox, screenPos, null);
    }

    private void CleanBox()
    {
        if (_selectBox != null)
        {
            _selectBox.gameObject.SetActive(false);
            _selectBox.sizeDelta = Vector2.zero;
            _selectBox.anchoredPosition = Vector2.zero;
        }
    }

    private void ClearSelection()
    {
        foreach (GameObject unit in unitsSelected)
        {
            if (unit == null)
            {
                continue;
            }

            SetUnitSelected(unit, false);
        }
        unitsSelected.Clear();

        if (_groundMarker == null)
        {
            return;
        }

        _groundMarker.SetActive(false);
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
        if (_groundMarker == null)
        {
            return;
        }

        _groundMarker.transform.position = position;
        _groundMarker.SetActive(false);
        // TODO: Marker animation here
        _groundMarker.SetActive(true);
    }
}

public class IndexedSet<T>: IEnumerable<T>
{
    public int Count;
    private List<T> _unitsSelectedList;
    private Dictionary<T, int> _unitsSelectedIndexDict;
    public IndexedSet()
    {
        _unitsSelectedList = new List<T>();
        _unitsSelectedIndexDict = new Dictionary<T, int>();
        Count = 0;
    }

    public IndexedSet(IEnumerable<T> collection): this()
    {
        foreach (T item in collection)
        {
            Add(item);
        }
    }
        
        
    public void Add(T objToAdd)
    {
        if (Contains(objToAdd))
        {
            return;
        }
        
        _unitsSelectedIndexDict.Add(objToAdd, Count);
        _unitsSelectedList.Add(objToAdd);
        Count++;
    }

    public void Remove(T objToRemove)
    {
        if (!_unitsSelectedIndexDict.ContainsKey(objToRemove))
        {
            return;
        }
            
        int index = _unitsSelectedIndexDict[objToRemove];
        _unitsSelectedList[index] = _unitsSelectedList[Count - 1];
        _unitsSelectedIndexDict[_unitsSelectedList[index]] = index;

        _unitsSelectedIndexDict.Remove(objToRemove);
        _unitsSelectedList.RemoveAt(Count - 1);
            
        Count--;
    }

    public T this[int index] //READ ONLY!
    {
        get
        {
            return _unitsSelectedList[index];
        }
    }
    
    public void Clear()
    {
        _unitsSelectedList.Clear();
        _unitsSelectedIndexDict.Clear();
        Count = 0;
    }
    public bool Contains(T objToCheck)
    {
        return _unitsSelectedIndexDict.ContainsKey(objToCheck);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _unitsSelectedList.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
        
        
}

public static class IndexedSetExtensions
{
    public static IndexedSet<T> ToIndexedSet<T>(this IEnumerable<T> source)
    {
        return new IndexedSet<T>(source);
    }
}
