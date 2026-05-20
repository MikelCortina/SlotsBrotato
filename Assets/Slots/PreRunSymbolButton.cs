using UnityEngine;
using UnityEngine.UI;

public class PreRunSymbolButton : MonoBehaviour
{
    [Header("Symbol")]
    public SlotSymbolData symbolData;

    [Header("UI")]
    public Image iconImage;
    public GameObject selectedFrame;

    private bool _isSelected;

    void Start()
    {
        if (iconImage && symbolData)
            iconImage.sprite = symbolData.icon;

        UpdateVisual();
    }

    public void ToggleSelection()
    {
        if (symbolData == null) return;

        _isSelected = !_isSelected;

        if (_isSelected)
            RunConfig.Instance.AddSymbol(symbolData);
        else
            RunConfig.Instance.RemoveSymbol(symbolData);

        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (selectedFrame)
            selectedFrame.SetActive(_isSelected);
    }
}