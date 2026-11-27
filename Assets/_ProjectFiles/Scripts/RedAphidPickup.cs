using System.Collections;
using UnityEngine;

public class RedAphidPickup : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private int aphidAmount = 1;
    [SerializeField] private string playerTag = "Player";

    [Header("Dissolve Effect")]
    [Tooltip("Set to 0 for instant disappear.")]
    [SerializeField] private float dissolveDuration = 0.35f;

    private bool playerInRange;
    private bool isCollected;
    private PlayerAphidInventory nearbyPlayerInventory;

    private void Update()
    {
        if (isCollected || !playerInRange) return;

        if (Input.GetKeyDown(interactKey))
        {
            Interact();
        }
    }

    public void Interact()
    {
        if (isCollected) return;
        isCollected = true;

        // Increment red aphid count on the player
        if (nearbyPlayerInventory != null)
        {
            nearbyPlayerInventory.AddRedAphid(aphidAmount);
        }
        else
        {
            Debug.LogWarning("RedAphidPickup: Player is missing a PlayerAphidInventory component!");
        }

        // Disable all colliders right away so it cannot be interacted with twice
        foreach (Collider col in GetComponents<Collider>())
        {
            col.enabled = false;
        }

        if (dissolveDuration <= 0f)
        {
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(DissolveAndDestroy());
        }
    }

    private IEnumerator DissolveAndDestroy()
    {
        Vector3 startScale = transform.localScale;
        Renderer rend = GetComponentInChildren<Renderer>();
        Material mat = rend != null ? rend.material : null;
        Color startColor = mat != null && mat.HasProperty("_BaseColor")
            ? mat.GetColor("_BaseColor")
            : (mat != null && mat.HasProperty("_Color") ? mat.color : Color.red);

        float elapsed = 0f;
        while (elapsed < dissolveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dissolveDuration);

            // Shrink smoothly to zero
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            // Also fade alpha if the material uses a Transparent surface type
            if (mat != null)
            {
                Color faded = startColor;
                faded.a = Mathf.Lerp(startColor.a, 0f, t);

                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", faded);
                else if (mat.HasProperty("_Color"))
                    mat.color = faded;
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected || !other.CompareTag(playerTag)) return;

        playerInRange = true;
        nearbyPlayerInventory = other.GetComponentInParent<PlayerAphidInventory>();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInRange = false;
        nearbyPlayerInventory = null;
    }
}