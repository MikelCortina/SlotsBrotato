using TMPro;
using UnityEngine;

public class WaveDebugUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private TMP_InputField waveInput;
    [SerializeField] private TextMeshProUGUI currentWaveText;

    private void Update()
    {
        if (gameManager == null || currentWaveText == null)
            return;

        currentWaveText.text =
            $"Oleada actual: {gameManager.CurrentWave}";
    }

    public void StartSelectedWave()
    {
        if (gameManager == null || waveInput == null)
            return;

        if (!int.TryParse(waveInput.text, out int selectedWave))
        {
            Debug.LogWarning("Introduce un número de oleada válido.");
            return;
        }

        gameManager.DebugJumpToWave(selectedWave);
    }

    public void PreviousWave()
    {
        if (gameManager != null)
            gameManager.DebugPreviousWave();
    }

    public void RestartWave()
    {
        if (gameManager != null)
            gameManager.DebugRestartWave();
    }

    public void NextWave()
    {
        if (gameManager != null)
            gameManager.DebugNextWave();
    }

    public void FinishWave()
    {
        if (gameManager != null)
            gameManager.DebugFinishCurrentWave();
    }
}