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

    void Start()
    {
        if (weaponData && iconImage)
            iconImage.sprite = weaponData.icon;

        UpdateVisual();
    }

    public void SelectWeapon()
    {
        if (weaponData == null) return;

        RunConfig.Instance.SelectWeapon(weaponData);

        PreRunWeaponButton[] all =
            FindObjectsOfType<PreRunWeaponButton>();

        foreach (var b in all)
        {
            b._selected = false;
            b.UpdateVisual();
        }

        _selected = true;
        UpdateVisual();
    }

    void UpdateVisual()
    {
        if (selectedFrame)
            selectedFrame.SetActive(_selected);
    }
}