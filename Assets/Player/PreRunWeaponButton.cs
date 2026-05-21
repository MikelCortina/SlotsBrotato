using UnityEngine;
using UnityEngine.UI;

public class PreRunWeaponButton : MonoBehaviour
{
    [Header("Weapon")]
    public WeaponData weaponData;

    [Header("UI")]
    public Image iconImage;
    public GameObject selectedFrame;

    private bool _selected;

    void OnEnable()
    {
        RefreshVisual();
    }

    void Start()
    {
        RefreshVisual();
    }

    public void SelectWeapon()
    {
        if (weaponData == null) return;
        if (RunConfig.Instance == null) return;

        RunConfig.Instance.SelectWeapon(weaponData);

        PreRunWeaponButton[] allButtons = FindObjectsOfType<PreRunWeaponButton>();

        foreach (var button in allButtons)
        {
            button._selected = false;
            button.UpdateVisual();
        }

        _selected = true;
        UpdateVisual();
    }

    private void RefreshVisual()
    {
        if (iconImage != null && weaponData != null)
            iconImage.sprite = weaponData.icon;

        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (selectedFrame)
            selectedFrame.SetActive(_selected);
    }
}