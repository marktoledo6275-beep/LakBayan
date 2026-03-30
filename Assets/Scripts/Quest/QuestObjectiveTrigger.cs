using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class QuestObjectiveTrigger : MonoBehaviour
{
    [SerializeField] private string targetId = "village_shrine";
    [SerializeField] private QuestObjectiveType objectiveType = QuestObjectiveType.ReachLocation;
    [SerializeField] private int progressAmount = 1;
    [SerializeField] private bool triggerOnlyOnce = true;
    [SerializeField] private bool disableAfterTrigger = true;

    private bool hasTriggered;

    /// <summary>
    /// Sets up a trigger collider automatically for simple quest objects.
    /// </summary>
    private void Reset()
    {
        Collider2D triggerCollider = GetComponent<Collider2D>();
        triggerCollider.isTrigger = true;

        if (string.IsNullOrWhiteSpace(targetId))
        {
            targetId = gameObject.name.ToLowerInvariant();
        }
    }

    /// <summary>
    /// Adds quest progress when the player enters the objective trigger.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered && triggerOnlyOnce)
        {
            return;
        }

        PlayerController player = other.GetComponent<PlayerController>();

        if (player == null || QuestSystem.Instance == null)
        {
            return;
        }

        bool progressAdded = QuestSystem.Instance.TryAdvanceObjectivesByTarget(targetId, objectiveType, progressAmount);

        if (!progressAdded)
        {
            return;
        }

        hasTriggered = true;

        if (disableAfterTrigger)
        {
            gameObject.SetActive(false);
        }
    }
}
