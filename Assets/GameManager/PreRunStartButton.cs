using UnityEngine;

public class PreRunStartButton : MonoBehaviour
{
    [Header("Panels")]
    public GameObject preRunPanel;
    public GameObject gameplayUI;

    [Header("Gameplay")]
    public EnemySpawner enemySpawner;
    public PlayerShooter playerShooter;

    public void StartRun()
    {
        if (RunConfig.Instance == null) return;

        if (!RunConfig.Instance.HasAtLeastOneSymbol())
        {
            Debug.Log("Debes seleccionar al menos un símbolo.");
            return;
        }

        if (preRunPanel)
            preRunPanel.SetActive(false);

        if (gameplayUI)
            gameplayUI.SetActive(true);

        if (GameManager.Instance != null)
            GameManager.Instance.BeginRun();

        if (enemySpawner)
        {
            enemySpawner.enabled = true;
            enemySpawner.BeginSpawning();
        }

        if (playerShooter)
        {
            playerShooter.ApplyWeaponData(RunConfig.Instance.selectedWeapon);
            playerShooter.enabled = true;
        }
    }
}