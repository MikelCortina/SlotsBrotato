using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PreRunSymbolButton : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (SymbolTooltipUI.Instance == null)
            return;

        SymbolTooltipUI.Instance.Show(
            symbolData,
            Input.mousePosition + new Vector3(20f, -20f, 0f)
        );
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (SymbolTooltipUI.Instance == null)
            return;

        SymbolTooltipUI.Instance.Hide();
    }
}