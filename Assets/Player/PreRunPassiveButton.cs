using UnityEngine;
using UnityEngine.UI;

public class PreRunPassiveButton : MonoBehaviour
{
    [Header("Passive")]
    public PassiveData passiveData;

    [Header("UI")]
    public Image iconImage;
    public GameObject selectedFrame;

    private bool _isSelected;

    void OnEnable()
    {
        RefreshVisual();
    }

    void Start()
    {
        RefreshVisual();
    }

    public void ToggleSelection()
    {
        if (passiveData == null) return;
        if (RunConfig.Instance == null) return;

        _isSelected = !_isSelected;

        if (_isSelected)
            RunConfig.Instance.AddPassive(passiveData);
        else
            RunConfig.Instance.RemovePassive(passiveData);

        UpdateVisual();
    }

    private void RefreshVisual()
    {
        if (iconImage != null && passiveData != null)
            iconImage.sprite = passiveData.icon;

        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (selectedFrame)
            selectedFrame.SetActive(_isSelected);
    }


}