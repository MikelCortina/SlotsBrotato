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

        if (SymbolUnlockManager.Instance != null &&
            !SymbolUnlockManager.Instance.IsUnlocked(symbolData))
        {
            _isSelected = false;

            if (iconImage != null)
                iconImage.color = Color.gray;
        }

        UpdateVisual();
    }

    public void ToggleSelection()
    {
        if (symbolData == null)
            return;

        if (SymbolUnlockManager.Instance != null &&
            !SymbolUnlockManager.Instance.IsUnlocked(symbolData))
        {
            Debug.Log("Símbolo bloqueado.");
            return;
        }

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