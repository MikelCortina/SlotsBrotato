using UnityEngine;

public class WeaponInstance : MonoBehaviour
{
    public Transform firePoint;
    public Transform muzzleVFXPoint;
    public ParticleSystem shootParticles;

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
}