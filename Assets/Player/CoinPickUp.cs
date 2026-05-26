using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [SerializeField] private int value = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (SlotMachine.Instance != null)
            SlotMachine.Instance.OnCoinCollected(value);

        Destroy(gameObject);
    }
}