using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class QuestSystem : MonoBehaviour
{
    private class QuestRuntimeData
    {
        public QuestData definition;
        public QuestState state;
        public int currentObjectiveIndex;
        public readonly Dictionary<string, int> progressByObjective = new Dictionary<string, int>();
    }

    [Header("Optional Starting Quests")]
    [SerializeField] private QuestData[] registeredQuests;

    private readonly Dictionary<string, QuestRuntimeData> questsById = new Dictionary<string, QuestRuntimeData>();
    private readonly List<string> questOrder = new List<string>();

    public static QuestSystem Instance { get; private set; }

    public event Action OnQuestUpdated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (registeredQuests != null)
        {
            foreach (QuestData quest in registeredQuests)
            {
                RegisterQuest(quest);
            }
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void RegisterQuest(QuestData quest)
    {
        if (quest == null || !quest.IsValid || questsById.ContainsKey(quest.QuestId))
        {
            return;
        }

        questsById.Add(quest.QuestId, new QuestRuntimeData
        {
            definition = quest,
            state = QuestState.NotStarted,
            currentObjectiveIndex = 0
        });

        questOrder.Add(quest.QuestId);
        NotifyQuestUpdated();
    }

    public bool StartQuest(QuestData quest)
    {
        if (quest == null || !quest.IsValid)
        {
            Debug.LogWarning("QuestSystem could not start a quest because its data is incomplete.", this);
            return false;
        }

        if (!questsById.ContainsKey(quest.QuestId))
        {
            RegisterQuest(quest);
        }

        QuestRuntimeData questData = questsById[quest.QuestId];

        if (questData.state == QuestState.Completed || questData.state == QuestState.InProgress || questData.state == QuestState.ReadyToComplete)
        {
            return false;
        }

        questData.state = QuestState.InProgress;
        questData.currentObjectiveIndex = 0;
        questData.progressByObjective.Clear();

        Debug.Log($"Quest started: {quest.Title}", this);
        NotifyQuestUpdated();
        return true;
    }

    public QuestState GetQuestState(string questId)
    {
        if (string.IsNullOrWhiteSpace(questId) || !questsById.TryGetValue(questId, out QuestRuntimeData questData))
        {
            return QuestState.NotStarted;
        }

        return questData.state;
    }

    public bool AdvanceObjective(string questId, string objectiveId, int amount)
    {
        if (amount <= 0 || !questsById.TryGetValue(questId, out QuestRuntimeData questData))
        {
            return false;
        }

        if (questData.state != QuestState.InProgress)
        {
            return false;
        }

        QuestObjectiveData currentObjective = GetCurrentObjective(questData);

        if (currentObjective == null || currentObjective.ObjectiveId != objectiveId)
        {
            return false;
        }

        if (!questData.progressByObjective.ContainsKey(objectiveId))
        {
            questData.progressByObjective[objectiveId] = 0;
        }

        questData.progressByObjective[objectiveId] = Mathf.Clamp(
            questData.progressByObjective[objectiveId] + amount,
            0,
            currentObjective.RequiredAmount);

        Debug.Log($"Quest progress: {questData.definition.Title} - {currentObjective.Description} ({questData.progressByObjective[objectiveId]}/{currentObjective.RequiredAmount})", this);

        if (questData.progressByObjective[objectiveId] >= currentObjective.RequiredAmount)
        {
            questData.currentObjectiveIndex++;

            if (questData.currentObjectiveIndex >= questData.definition.Objectives.Length)
            {
                questData.state = QuestState.ReadyToComplete;
            }
        }

        NotifyQuestUpdated();
        return true;
    }

    public bool TryAdvanceObjectivesByTarget(string targetId, QuestObjectiveType objectiveType, int amount = 1)
    {
        bool progressChanged = false;

        foreach (string questId in questOrder)
        {
            if (!questsById.TryGetValue(questId, out QuestRuntimeData questData))
            {
                continue;
            }

            if (questData.state != QuestState.InProgress)
            {
                continue;
            }

            QuestObjectiveData currentObjective = GetCurrentObjective(questData);

            if (currentObjective == null)
            {
                continue;
            }

            if (currentObjective.ObjectiveType != objectiveType || currentObjective.TargetId != targetId)
            {
                continue;
            }

            progressChanged |= AdvanceObjective(questId, currentObjective.ObjectiveId, amount);
        }

        return progressChanged;
    }

    public bool CompleteQuest(string questId)
    {
        if (!questsById.TryGetValue(questId, out QuestRuntimeData questData))
        {
            return false;
        }

        if (questData.state != QuestState.ReadyToComplete)
        {
            return false;
        }

        questData.state = QuestState.Completed;
        ApplyRewards(questData.definition);
        Debug.Log($"Quest completed: {questData.definition.Title}", this);
        NotifyQuestUpdated();
        return true;
    }

    public string GetCurrentQuestSummary()
    {
        QuestRuntimeData activeQuest = GetFirstActiveQuest();

        if (activeQuest == null)
        {
            return "Current Quest: Explore the barangay and talk to the townsfolk.";
        }

        if (activeQuest.state == QuestState.ReadyToComplete)
        {
            return $"Current Quest: {activeQuest.definition.Title}\nReturn to the quest giver.";
        }

        QuestObjectiveData currentObjective = GetCurrentObjective(activeQuest);

        if (currentObjective == null)
        {
            return $"Current Quest: {activeQuest.definition.Title}";
        }

        int progress = activeQuest.progressByObjective.TryGetValue(currentObjective.ObjectiveId, out int currentAmount)
            ? currentAmount
            : 0;

        return $"Current Quest: {activeQuest.definition.Title}\n{currentObjective.Description} ({progress}/{currentObjective.RequiredAmount})";
    }

    public string GetQuestLogText()
    {
        StringBuilder builder = new StringBuilder();

        foreach (string questId in questOrder)
        {
            if (!questsById.TryGetValue(questId, out QuestRuntimeData questData))
            {
                continue;
            }

            if (questData.state == QuestState.NotStarted || questData.state == QuestState.Completed)
            {
                continue;
            }

            if (builder.Length > 0)
            {
                builder.AppendLine();
                builder.AppendLine();
            }

            builder.Append(questData.definition.Title);
            builder.AppendLine();
            builder.AppendLine(questData.definition.Description);

            if (questData.state == QuestState.ReadyToComplete)
            {
                builder.AppendLine("Return to the quest giver.");
                continue;
            }

            QuestObjectiveData currentObjective = GetCurrentObjective(questData);

            if (currentObjective == null)
            {
                continue;
            }

            int progress = questData.progressByObjective.TryGetValue(currentObjective.ObjectiveId, out int amount)
                ? amount
                : 0;

            builder.Append(currentObjective.Description);
            builder.Append(" (");
            builder.Append(progress);
            builder.Append("/");
            builder.Append(currentObjective.RequiredAmount);
            builder.Append(")");
        }

        return builder.ToString();
    }

    private QuestRuntimeData GetFirstActiveQuest()
    {
        foreach (string questId in questOrder)
        {
            if (!questsById.TryGetValue(questId, out QuestRuntimeData questData))
            {
                continue;
            }

            if (questData.state == QuestState.InProgress || questData.state == QuestState.ReadyToComplete)
            {
                return questData;
            }
        }

        return null;
    }

    private QuestObjectiveData GetCurrentObjective(QuestRuntimeData questData)
    {
        if (questData == null ||
            questData.definition == null ||
            questData.definition.Objectives == null ||
            questData.currentObjectiveIndex < 0 ||
            questData.currentObjectiveIndex >= questData.definition.Objectives.Length)
        {
            return null;
        }

        return questData.definition.Objectives[questData.currentObjectiveIndex];
    }

    private void ApplyRewards(QuestData quest)
    {
        if (quest == null || quest.Rewards == null || GameManager.Instance == null)
        {
            return;
        }

        foreach (QuestRewardData reward in quest.Rewards)
        {
            if (reward == null)
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(reward.RewardItemId) && reward.RewardAmount > 0)
            {
                GameManager.Instance.AddReward(reward.RewardItemId, reward.RewardAmount);
            }

            if (reward.ScoreBonus > 0)
            {
                GameManager.Instance.AddHistoryScore(reward.ScoreBonus);
            }

            if (!string.IsNullOrWhiteSpace(reward.UnlockContentId))
            {
                GameManager.Instance.UnlockContent(reward.UnlockContentId);
            }
        }
    }

    private void NotifyQuestUpdated()
    {
        OnQuestUpdated?.Invoke();
    }
}
