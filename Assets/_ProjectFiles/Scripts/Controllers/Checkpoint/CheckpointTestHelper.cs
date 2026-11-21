using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// TESTING ONLY: lets you test the checkpoint menu before enemies / pickups exist.
/// Put it on any object in a test scene. Remove it from real levels.
///
///   1 = +200 scraps        2 = +1 red aphid
///   3 = report a BlackAnt kill
///   4 = player takes 30 damage (to see checkpoint healing)
///   5 = report BullAnt elite defeated
/// </summary>
public class CheckpointTestHelper : MonoBehaviour
{
    [SerializeField] private int scrapsPerPress = 200;
    [SerializeField] private float damagePerPress = 30f;

    private PlayerScrapesHandler scrapes;
    private HealthHandler health;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            scrapes = player.GetComponentInChildren<PlayerScrapesHandler>();
            health = player.GetComponentInChildren<HealthHandler>();
        }
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.digit1Key.wasPressedThisFrame && scrapes != null)
        {
            scrapes.AddScrapesToPlayer(scrapsPerPress);
            Debug.Log($"[Test] +{scrapsPerPress} scraps -> {PlayerDataModel.LoadScrapesAmount()}");
            RefreshUI();
        }

        if (keyboard.digit2Key.wasPressedThisFrame)
        {
            ProgressionEvents.ReportRedAphidsCollected(1);
            Debug.Log($"[Test] +1 red aphid -> {GameProgressModel.Instance.CurrentData.redAphids}");
        }

        if (keyboard.digit3Key.wasPressedThisFrame)
        {
            ProgressionEvents.ReportEnemyDefeated("BlackAnt");
            Debug.Log("[Test] Reported BlackAnt kill");
        }

        if (keyboard.digit4Key.wasPressedThisFrame && health != null)
        {
            health.TakeDmg(damagePerPress);
            Debug.Log($"[Test] Player health -> {health.CurrHealth} / {health.MaxHealth}");
        }

        if (keyboard.digit5Key.wasPressedThisFrame)
        {
            ProgressionEvents.ReportEliteDefeated("BullAnt");
            Debug.Log("[Test] Reported BullAnt elite defeated");
        }
    }

    // Scraps don't go through the progression model, so tell the open menu to redraw
    private void RefreshUI()
    {
        if (ProgressionModel.Instance != null) ProgressionModel.Instance.NotifyChanged();
    }
}
