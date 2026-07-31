using UnityEngine;

public class WeaponInstance : MonoBehaviour
{
    public Transform firePoint;
    public Transform muzzleVFXPoint;
    public ParticleSystem shootParticles;

    [Header("Auto Find Names")]
    [SerializeField] private string defaultFirePointName = "FirePoint";
    [SerializeField] private string defaultMuzzleVFXPointName = "MuzzleVFXPoint";

    public void ConfigureFromWeaponData(WeaponData weaponData)
    {
        string fireName = !string.IsNullOrWhiteSpace(weaponData?.firePointName)
            ? weaponData.firePointName
            : defaultFirePointName;

        string muzzleName = !string.IsNullOrWhiteSpace(weaponData?.muzzleVFXPointName)
            ? weaponData.muzzleVFXPointName
            : defaultMuzzleVFXPointName;

        if (firePoint == null)
            firePoint = FindDeepChild(transform, fireName);

        if (muzzleVFXPoint == null)
            muzzleVFXPoint = FindDeepChild(transform, muzzleName);
    }

    public void PlayShootParticles(Vector2 shootDir)
    {
        if (shootParticles == null)
            return;

        Transform spawnPoint = muzzleVFXPoint != null ? muzzleVFXPoint : firePoint;
        if (spawnPoint == null)
            return;

        shootParticles.transform.position = spawnPoint.position;

        float angle = Mathf.Atan2(shootDir.y, shootDir.x) * Mathf.Rad2Deg;
        shootParticles.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        var main = shootParticles.main;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        shootParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        shootParticles.Play(true);
    }

    private Transform FindDeepChild(Transform parent, string childName)
    {
        if (parent == null || string.IsNullOrEmpty(childName))
            return null;

        if (parent.name == childName)
            return parent;

        foreach (Transform child in parent)
        {
            Transform result = FindDeepChild(child, childName);
            if (result != null)
                return result;
        }

        return null;
    }
}