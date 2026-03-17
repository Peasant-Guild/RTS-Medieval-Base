using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private GameObject _selectionIndicator;

    private void Start()
    {
        UnitSelectionManager.Instance.allUnitsList.Add(gameObject);
    }

    private void OnDestroy()
    {
        if (UnitSelectionManager.Instance != null)
        {
            UnitSelectionManager.Instance.allUnitsList.Remove(gameObject);
        }
    }

    public void SetSelected(bool isSelected)
    {
        if (_selectionIndicator != null)
        {
            _selectionIndicator.SetActive(isSelected);
        }
    }
}