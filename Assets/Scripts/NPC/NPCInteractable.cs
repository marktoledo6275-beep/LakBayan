using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    [TextArea(2, 4)]
    public string text;
}

public class NPCInteractable : Interactable
{
    [Header("NPC Info")]
    [SerializeField] private string displayName = "Villager";

    [Header("Default Dialogue")]
    [SerializeField] private DialogueLine[] dialogueLines;

    [Header("Optional Quest")]
    [SerializeField] private bool givesQuest;
    [SerializeField] private QuestDefinition quest;
    [SerializeField] private DialogueLine[] questOfferDialogue;
    [SerializeField] private DialogueLine[] questInProgressDialogue;
    [SerializeField] private DialogueLine[] questReadyToTurnInDialogue;
    [SerializeField] private DialogueLine[] questCompletedDialogue;

    [Header("References")]
    [SerializeField] private DialogueUI dialogueUI;

    /// <summary>
    /// Exposes the NPC name to the dialogue UI.
    /// </summary>
    public string DisplayName => displayName;

    /// <summary>
    /// Starts dialogue with this NPC when the player interacts.
    /// </summary>
    public override void Interact(PlayerController player)
    {
        if (dialogueUI == null)
        {
            dialogueUI = FindFirstObjectByType<DialogueUI>();
        }

        if (dialogueUI == null)
        {
            Debug.LogWarning($"No DialogueUI found in the scene for NPC '{displayName}'.", this);
            return;
        }

        if (dialogueUI.IsCurrentSpeaker(this))
        {
            dialogueUI.AdvanceDialogue();
            return;
        }

        if (givesQuest && quest != null && quest.IsValid)
        {
            HandleQuestInteraction(player);
            return;
        }

        StartDialogue(player, GetDialogueText(dialogueLines));
    }

    /// <summary>
    /// Converts the inspector dialogue data into a simple string array.
    /// </summary>
    private string[] GetDialogueText(DialogueLine[] sourceLines)
    {
        if (sourceLines == null || sourceLines.Length == 0)
        {
            return new string[0];
        }

        string[] lines = new string[sourceLines.Length];

        for (int i = 0; i < sourceLines.Length; i++)
        {
            lines[i] = sourceLines[i] != null ? sourceLines[i].text : string.Empty;
        }

        return lines;
    }

    /// <summary>
    /// Chooses quest dialogue based on the current quest state and runs the matching quest action.
    /// </summary>
    private void HandleQuestInteraction(PlayerController player)
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogWarning($"No QuestManager found in the scene for NPC '{displayName}'.", this);
            StartDialogue(player, GetDialogueText(dialogueLines));
            return;
        }

        QuestStatus questStatus = QuestManager.Instance.GetQuestStatus(quest.QuestId);

        switch (questStatus)
        {
            case QuestStatus.NotStarted:
                StartDialogue(
                    player,
                    GetDialogueTextOrFallback(questOfferDialogue),
                    () => QuestManager.Instance.StartQuest(quest));
                break;

            case QuestStatus.InProgress:
                StartDialogue(player, GetDialogueTextOrFallback(questInProgressDialogue));
                break;

            case QuestStatus.ReadyToTurnIn:
                StartDialogue(
                    player,
                    GetDialogueTextOrFallback(questReadyToTurnInDialogue),
                    () => QuestManager.Instance.CompleteQuest(quest.QuestId));
                break;

            case QuestStatus.Completed:
                StartDialogue(player, GetDialogueTextOrFallback(questCompletedDialogue));
                break;
        }
    }

    /// <summary>
    /// Falls back to the default NPC dialogue when a quest-specific set is empty.
    /// </summary>
    private string[] GetDialogueTextOrFallback(DialogueLine[] sourceLines)
    {
        string[] lines = GetDialogueText(sourceLines);

        if (lines.Length > 0)
        {
            return lines;
        }

        return GetDialogueText(dialogueLines);
    }

    /// <summary>
    /// Starts the dialogue UI and logs a warning if this NPC has no lines configured.
    /// </summary>
    private void StartDialogue(PlayerController player, string[] lines, System.Action onComplete = null)
    {
        if (lines.Length == 0)
        {
            Debug.LogWarning($"NPC '{displayName}' has no dialogue lines assigned.", this);
            return;
        }

        dialogueUI.SetPlayer(player);
        dialogueUI.StartDialogue(this, lines, onComplete);
    }
}
