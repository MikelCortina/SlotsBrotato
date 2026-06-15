using System.Collections;
using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [SerializeField] private int value = 1;
    [SerializeField] private float pullSpeed = 40f;
    [SerializeField] private float acceleration = 80f;
    [SerializeField] private float maxSpeed = 200f;
    [SerializeField] private float minCollectionDistance = 0.2f;

    [Header("Oscillation")]
    [SerializeField] private float oscillationDistance = 0.2f;
    [SerializeField] private float oscillationDuration = 0.2f;
    [SerializeField] private float oscillationSpeed = 8f;

    [Header("SFX")]
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private float collectVolume = 1f;

    private Transform playerTransform;
    private AudioSource playerAudioSource;
    private bool isCollected = false;
    private bool isPulled = false;
    private bool isOscillating = false;
    private float currentSpeed = 0f;
    private float oscillationTimer = 0f;
    private Vector3 startPosition = Vector3.zero;
    private Vector3 oscillationTarget = Vector3.zero;

    private void OnEnable()
    {
        isCollected = false;
        isPulled = false;
        isOscillating = false;
        currentSpeed = pullSpeed;
        oscillationTimer = 0f;
        startPosition = transform.position;
        playerTransform = null;
        playerAudioSource = null;
    }

    private void Update()
    {
        if (isCollected) return;

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            playerTransform = player.transform;

            if (!player.TryGetComponent<AudioSource>(out playerAudioSource))
                playerAudioSource = player.GetComponentInChildren<AudioSource>();

            Debug.Log("PlayerAudioSource found: " + (playerAudioSource != null));
        }

        PlayerStats playerStats = playerTransform.GetComponent<PlayerStats>();
        if (playerStats == null) return;

        float radius = playerStats.GetCoinPickupRadius();
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (!isPulled && !isOscillating && distance <= radius)
        {
            isPulled = true;
            isOscillating = true;
            oscillationTimer = 0f;
            startPosition = transform.position;

            Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
            oscillationTarget = transform.position - (Vector3)directionToPlayer * oscillationDistance;

            CoinDropController dropController = GetComponent<CoinDropController>();
            if (dropController != null)
                dropController.StartBeingPulled();
        }

        if (isOscillating)
        {
            oscillationTimer += Time.deltaTime;

            float progress = Mathf.Clamp01(oscillationTimer / oscillationDuration);
            float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);

            transform.position = Vector3.Lerp(startPosition, oscillationTarget, easedProgress);

            if (oscillationTimer >= oscillationDuration)
            {
                isOscillating = false;
                currentSpeed = pullSpeed;
            }
            return;
        }

        if (isPulled)
        {
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);

            float step = currentSpeed * Time.deltaTime;
            float dist = Vector2.Distance(transform.position, playerTransform.position);

            // Evitar overshooting
            if (step >= dist)
            {
                transform.position = playerTransform.position;
                CollectCoin();
                return;
            }

            transform.position += (Vector3)direction * step;

            if (dist <= minCollectionDistance)
                CollectCoin();
        }
    }

    // ? Activa el pull directo saltándose la oscilación, con timeScale = 1 todavía activo
    public void ForceCollect()
    {
        if (isCollected) return;

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                CollectCoin();
                return;
            }

            playerTransform = player.transform;
            if (!player.TryGetComponent<AudioSource>(out playerAudioSource))
                playerAudioSource = player.GetComponentInChildren<AudioSource>();
        }

        // Saltar oscilación y arrancar pull directo, un poco más rápido para que se vea snappy
        isOscillating = false;
        isPulled = true;
        currentSpeed = pullSpeed * 2f;
    }

    private void CollectCoin()
    {
        if (isCollected) return;
        isCollected = true;

        if (playerAudioSource != null && collectSound != null)
        {
            playerAudioSource.volume = 1f;
            playerAudioSource.mute = false;
            playerAudioSource.spatialBlend = 0f;
            playerAudioSource.PlayOneShot(collectSound, collectVolume);

            Debug.Log("Playing collect sound");
        }
        else
        {
            Debug.LogWarning("No audio source or collect sound assigned");
        }

        if (CoinDropManager.Instance != null)
            CoinDropManager.Instance.SpawnDropFromCoin(transform.position);

        if (SlotMachine.Instance != null)
            SlotMachine.Instance.OnCoinCollected(1);

        Destroy(gameObject);
    }
}