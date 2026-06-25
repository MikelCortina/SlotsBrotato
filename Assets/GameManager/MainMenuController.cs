using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject modifiersPanel;

    void Awake()
    {
        mainMenuPanel.SetActive(true);
        modifiersPanel.SetActive(false);
    }

    // Botón "Jugar" del menú principal
    public void OnClickJugar()
    {
        mainMenuPanel.SetActive(false);
        modifiersPanel.SetActive(true);
    }

    // Botón "Volver" dentro del panel de modificadores
    public void OnClickVolver()
    {
        modifiersPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // Botón "Opciones"
    public void OnClickOpciones()
    {
        // conecta tu panel de opciones aquí
    }

    // Botón "Salir"
    public void OnClickSalir()
    {
        Application.Quit();
    }
}