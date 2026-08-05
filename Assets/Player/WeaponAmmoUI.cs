using TMPro;
using UnityEngine;

public class WeaponAmmoUI : MonoBehaviour
{
    public PlayerShooter shooter;

    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI reloadText;

    void Update()
    {
        if (shooter == null)
            return;

        if (!shooter.usesAmmo)
        {
            ammoText.gameObject.SetActive(false);
            reloadText.gameObject.SetActive(false);
            return;
        }

        ammoText.gameObject.SetActive(true);

        ammoText.text =
            $"{shooter.currentAmmo}/{shooter.maxAmmo}";

        if (shooter.IsReloading)
        {
            reloadText.gameObject.SetActive(true);
            reloadText.text = "RECARGANDO...";
        }
        else
        {
            reloadText.gameObject.SetActive(false);
        }
    }
}