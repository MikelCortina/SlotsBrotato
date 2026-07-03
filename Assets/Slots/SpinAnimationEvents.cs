using UnityEngine;

public class SpinAnimationEvents : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonClickSfx;

    public void OnSpinAnimationReady()
    {
        SlotMachine.Instance?.OnSpinAnimationReady();
    }

    public void OnButtonPressed()
    {
        Debug.Log("Botón pulsado");

        if (audioSource == null)
        {
            Debug.LogError("AudioSource no asignado");
            return;
        }

        if (buttonClickSfx == null)
        {
            Debug.LogError("AudioClip no asignado");
            return;
        }

        audioSource.PlayOneShot(buttonClickSfx, 1f);
        Debug.Log("Intentando reproducir sonido");
    }
}