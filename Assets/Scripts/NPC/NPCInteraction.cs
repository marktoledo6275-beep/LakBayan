using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class NPCInteraction : Interactable
{
    [Header("NPC Info")]
    [SerializeField] private string npcId = "elder";
    [SerializeField] private string displayName = "Village Elder";

    [Header("Dialogue Sets")]
    [SerializeField] private DialogueData defaultDialogue;
    [SerializeField] private DialogueData questOfferDialogue;
    [SerializeField] private DialogueData questInProgressDialogue;
    [SerializeField] private DialogueData questReadyToTurnInDialogue;
    [SerializeField] private DialogueData questCompletedDialogue;

    [Header("Quest Hooks")]
    [SerializeField] private QuestData questToGive;
    [SerializeField] private string questIdToAdvance;
    [SerializeField] private string objectiveIdToAdvance;

    [Header("Rewards")]
    [SerializeField] private int talkScoreReward = 3;
    [SerializeField] private bool rewardOnlyOnce = true;

    private bool hasGivenTalkReward;

    public string NpcId => npcId;
    public string DisplayName => displayName;

    public override void Interact(PlayerController player)
    {
        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning($"NPC '{displayName}' could not start dialogue because no DialogueManager exists in the scene.", this);
            return;
        }

        if (DialogueManager.Instance.IsCurrentSource(this))
        {
            DialogueManager.Instance.AdvanceDialogue();
            return;
        }

        DialogueData selectedDialogue = ResolveDialogue();

        if (selectedDialogue == null)
        {
            Debug.LogWarning($"NPC '{displayName}' does not have a dialogue asset assigned.", this);
            return;
        }

        Action completionAction = BuildCompletionAction();

        AwardTalkScoreIfNeeded();
        DialogueManager.Instance.StartDialogue(selectedDialogue, this, player, completionAction);
    }

    private DialogueData ResolveDialogue()
    {
        if (questToGive == null || QuestSystem.Instance == null)
        {
            return defaultDialogue;
        }

        QuestState state = QuestSystem.Instance.GetQuestState(questToGive.QuestId);

        switch (state)
        {
            case QuestState.NotStarted:
                return questOfferDialogue != null ? questOfferDialogue : defaultDialogue;
            case QuestState.InProgress:
                return questInProgressDialogue != null ? questInProgressDialogue : defaultDialogue;
            case QuestState.ReadyToComplete:
                return questReadyToTurnInDialogue != null ? questReadyToTurnInDialogue : defaultDialogue;
            case QuestState.Completed:
                return questCompletedDialogue != null ? questCompletedDialogue : defaultDialogue;
            default:
                return defaultDialogue;
        }
    }

    private Action BuildCompletionAction()
    {
        Action completionAction = null;

        if (QuestSystem.Instance != null && questToGive != null)
        {
            QuestState questState = QuestSystem.Instance.GetQuestState(questToGive.QuestId);

            if (questState == QuestState.NotStarted)
            {
                completionAction += () => QuestSystem.Instance.StartQuest(questToGive);
            }
            else if (questState == QuestState.ReadyToComplete)
            {
                completionAction += () => QuestSystem.Instance.CompleteQuest(questToGive.QuestId);
            }
        }

        if (QuestSystem.Instance != null &&
            !string.IsNullOrWhiteSpace(questIdToAdvance) &&
            !string.IsNullOrWhiteSpace(objectiveIdToAdvance))
        {
            completionAction += () => QuestSystem.Instance.AdvanceObjective(questIdToAdvance, objectiveIdToAdvance, 1);
        }

        return completionAction;
    }

    private void AwardTalkScoreIfNeeded()
    {
        if (talkScoreReward <= 0 || GameManager.Instance == null)
        {
            return;
        }

        if (rewardOnlyOnce && hasGivenTalkReward)
        {
            return;
        }

        GameManager.Instance.AddHistoryScore(talkScoreReward);
        hasGivenTalkReward = true;
    }
}
