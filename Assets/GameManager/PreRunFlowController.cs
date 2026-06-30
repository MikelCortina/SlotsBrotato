using UnityEngine;

public class PreRunFlowController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject initialPanel;
    public GameObject boosterPanel;
    public GameObject symbolPanel;
    public GameObject weaponPanel;
    public GameObject gameplayUI;

    void Start()
    {
        ShowInitialPanel();
    }

    void HideAllPanels()
    {
        if (initialPanel) initialPanel.SetActive(false);
        if (boosterPanel) boosterPanel.SetActive(false);
        if (symbolPanel) symbolPanel.SetActive(false);
        if (weaponPanel) weaponPanel.SetActive(false);
        if (gameplayUI) gameplayUI.SetActive(false);
    }

    public void ShowInitialPanel()
    {
        HideAllPanels();
        if (initialPanel) initialPanel.SetActive(true);
    }

    public void ShowBoosterPanel()
    {
        HideAllPanels();
        if (boosterPanel) boosterPanel.SetActive(true);
    }

    public void ShowSymbolPanel()
    {
        HideAllPanels();
        if (symbolPanel) symbolPanel.SetActive(true);
    }

    public void ShowWeaponPanel()
    {
        HideAllPanels();
        if (weaponPanel) weaponPanel.SetActive(true);
    }

    public void ShowGameplay()
    {
        HideAllPanels();
        if (gameplayUI) gameplayUI.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Salir del juego");
    }
}