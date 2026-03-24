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
    [SerializeField] private RectTransform _selectBox;
    [SerializeField] private float _dragThreshold = 10f;

    private Camera _cam;
    private Vector3 _mousePosition;
    private Vector2 _boxStartPos;
    private Vector2 _boxDimensions;
    private Vector3 _dragStartPos;
    private bool _isLeftDragging;
    private bool _isRightDragging;

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
        HandleRightClickMovementRequest();
    }

    private void HandleLeftClickSelection()
    {
        if (Mouse.current == null || (!Mouse.current.leftButton.wasPressedThisFrame &&
                                      !Mouse.current.leftButton.isPressed &&
                                      !Mouse.current.leftButton.wasReleasedThisFrame))
        {
            _isLeftDragging = false;
            return;
        }

        _mousePosition = GetCurrentMouseWorldPos();

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
        _isLeftDragging = false;
        CleanBox();
    }

    private void HandleLeftClickHeld()
    {
        if (!_isLeftDragging && Vector2.Distance(_boxStartPos, _mousePosition) > _dragThreshold)
        {
            _isLeftDragging = true;

            if (_selectBox != null)
            {
                _selectBox.gameObject.SetActive(true);
            }
        }

        if (_isLeftDragging)
        {
            UpdateSelectionBoxVisual();
            UpdateBoxSelection();
        }
    }

    private void HandleLeftClickReleased()
    {
        if (!_isLeftDragging)
        {
            HandleBasicSelect();
        }

        _isLeftDragging = false;
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

    private void HandleRightClickMovementRequest()
    {
        if (Mouse.current == null || (!Mouse.current.rightButton.wasPressedThisFrame &&
                                     !Mouse.current.rightButton.isPressed &&
                                     !Mouse.current.rightButton.wasReleasedThisFrame))
        {
            _isRightDragging = false;
            return;
        }
        _mousePosition = Mouse.current.position.ReadValue();

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            HandleRightClickPressed();
        }

        if (Mouse.current.rightButton.isPressed)
        {
            HandleRightClickHeld();
        }

        if (Mouse.current.rightButton.wasReleasedThisFrame)
        {
            HandleRightClickReleased();
        }
    }

    private void HandleRightClickPressed()
    {
        _dragStartPos = Mouse.current.position.ReadValue();
    }

    private void VisualiseFormation(int amountOfLines, Transform target)
    {
        
    }
    private void SendInFormation(int amountOfLines, Vector3 target)
    {
        
    }

    private List<Vector3> CreateFormation(int amountOfLines, Vector3 target)
    {
        //TODO: acknowledge borders
        int unitCount = unitsSelected.Count;
        int amountOfRows = unitCount / amountOfLines;
        int leftOverUnits = unitCount % amountOfLines;
        List<Vector3> positions = new List<Vector3>();
        int startOfRowOffset = -amountOfRows / 2;
        int endOfRowOffset = amountOfRows / 2 + amountOfRows % 2;



        Vector3 currentMouseWorldPos = GetCurrentMouseWorldPos();
        Vector3 direction = currentMouseWorldPos - _dragStartPos;
        direction.y = 0f;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        for (int line = 0; line < amountOfLines; line++)
        {
            for (int row = startOfRowOffset; row < endOfRowOffset; row++)
            {
                Vector3 currentOffset =
                    new Vector3(target.x + row, target.y,
                        target.z - line); //TODO: navigate higher/lower y levels (also in the next loop)
                positions.Add(currentOffset);
            }
        }

        for (int i = 0; i < leftOverUnits; i++)
        {
            if (i % 2 == 0)
            {
                Vector3 currentOffset =
                    new Vector3(target.x + startOfRowOffset + i, target.y, target.z - amountOfLines);
                positions.Add(currentOffset);
            }
            else
            {
                Vector3 currentOffset = new Vector3(target.x + endOfRowOffset - i, target.y, target.z - amountOfLines);
                positions.Add(currentOffset);
            }
        }
        
        
        return positions;
    }

    private Vector3 GetCurrentMouseWorldPos()
    {
        Vector2 mousePos2D = Mouse.current.position.ReadValue();
        
        Ray ray = _cam.ScreenPointToRay(mousePos2D);
        
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _ground))
        {
            return hit.point;
        }
        return Vector3.zero; //shouldn't reach
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
