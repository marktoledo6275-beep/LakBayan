using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum QuestStatus
{
    NotStarted,
    InProgress,
    ReadyToTurnIn,
    Completed
}

[Serializable]
public class QuestDefinition
{
    [SerializeField] private string questId = "quest_001";
    [SerializeField] private string title = "New Quest";
    [TextArea(2, 4)]
    [SerializeField] private string description = "Talk to the village elder.";
    [SerializeField] private string objectiveDescription = "Complete the objective.";
    [SerializeField] private int requiredProgress = 1;

    /// <summary>
    /// Unique ID used to look up this quest at runtime.
    /// </summary>
    public string QuestId => questId;

    /// <summary>
    /// Short title displayed in the quest UI.
    /// </summary>
    public string Title => title;

    /// <summary>
    /// Longer description that explains the quest.
    /// </summary>
    public string Description => description;

    /// <summary>
    /// Text that explains the current objective to the player.
    /// </summary>
    public string ObjectiveDescription => objectiveDescription;

    /// <summary>
    /// Total amount of progress needed before the quest can be turned in.
    /// </summary>
    public int RequiredProgress => Mathf.Max(1, requiredProgress);

    /// <summary>
    /// Validates that the quest has the minimum data it needs to function.
    /// </summary>
    public bool IsValid => !string.IsNullOrWhiteSpace(questId);
}

public class QuestManager : MonoBehaviour
{
    private class QuestRuntimeData
    {
        public QuestDefinition Definition;
        public QuestStatus Status;
        public int CurrentProgress;
    }

    private readonly Dictionary<string, QuestRuntimeData> questsById = new Dictionary<string, QuestRuntimeData>();
    private readonly List<string> questOrder = new List<string>();

    /// <summary>
    /// Provides easy global access to the current quest manager.
    /// </summary>
    public static QuestManager Instance { get; private set; }

    /// <summary>
    /// Raised whenever a quest changes so UI can refresh.
    /// </summary>
    public event Action OnQuestUpdated;

    /// <summary>
    /// Registers the singleton instance for the scene.
    /// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// Clears the singleton when this manager is removed.
    /// </summary>
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Returns the current state of a quest, or NotStarted if it is unknown.
    /// </summary>
    public QuestStatus GetQuestStatus(string questId)
    {
        if (string.IsNullOrWhiteSpace(questId))
        {
            return QuestStatus.NotStarted;
        }

        return questsById.TryGetValue(questId, out QuestRuntimeData questData)
            ? questData.Status
            : QuestStatus.NotStarted;
    }

    /// <summary>
    /// Starts a quest and makes it visible in the quest log.
    /// </summary>
    public void StartQuest(QuestDefinition quest)
    {
        if (quest == null || !quest.IsValid)
        {
            Debug.LogWarning("Cannot start a quest with missing quest data.", this);
            return;
        }

        if (!questsById.TryGetValue(quest.QuestId, out QuestRuntimeData questData))
        {
            questData = new QuestRuntimeData
            {
                Definition = quest,
                Status = QuestStatus.InProgress,
                CurrentProgress = 0
            };

            questsById.Add(quest.QuestId, questData);
            questOrder.Add(quest.QuestId);
        }
        else if (questData.Status == QuestStatus.NotStarted)
        {
            questData.Status = QuestStatus.InProgress;
        }
        else
        {
            return;
        }

        NotifyQuestUpdated();
    }

    /// <summary>
    /// Adds progress to an active quest and marks it ready when the objective is complete.
    /// </summary>
    public bool AddProgress(string questId, int amount = 1)
    {
        if (amount <= 0 || !questsById.TryGetValue(questId, out QuestRuntimeData questData))
        {
            return false;
        }

        if (questData.Status != QuestStatus.InProgress)
        {
            return false;
        }

        questData.CurrentProgress = Mathf.Clamp(
            questData.CurrentProgress + amount,
            0,
            questData.Definition.RequiredProgress);

        if (questData.CurrentProgress >= questData.Definition.RequiredProgress)
        {
            questData.Status = QuestStatus.ReadyToTurnIn;
        }

        NotifyQuestUpdated();
        return true;
    }

    /// <summary>
    /// Completes a ready quest after the player returns to the quest NPC.
    /// </summary>
    public void CompleteQuest(string questId)
    {
        if (!questsById.TryGetValue(questId, out QuestRuntimeData questData))
        {
            return;
        }

        if (questData.Status != QuestStatus.ReadyToTurnIn)
        {
            return;
        }

        questData.Status = QuestStatus.Completed;
        NotifyQuestUpdated();
    }

    /// <summary>
    /// Returns the current progress amount for a quest.
    /// </summary>
    public int GetCurrentProgress(string questId)
    {
        return questsById.TryGetValue(questId, out QuestRuntimeData questData)
            ? questData.CurrentProgress
            : 0;
    }

    /// <summary>
    /// Builds a player-facing quest log string for a simple UI text field.
    /// </summary>
    public string GetQuestLogText()
    {
        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < questOrder.Count; i++)
        {
            string questId = questOrder[i];

            if (!questsById.TryGetValue(questId, out QuestRuntimeData questData))
            {
                continue;
            }

            if (questData.Status == QuestStatus.Completed)
            {
                continue;
            }

            if (builder.Length > 0)
            {
                builder.AppendLine();
                builder.AppendLine();
            }

            builder.Append(questData.Definition.Title);
            builder.AppendLine();
            builder.Append(questData.Definition.ObjectiveDescription);
            builder.Append(" (");
            builder.Append(questData.CurrentProgress);
            builder.Append("/");
            builder.Append(questData.Definition.RequiredProgress);
            builder.Append(")");

            if (questData.Status == QuestStatus.ReadyToTurnIn)
            {
                builder.AppendLine();
                builder.Append("Return to the quest giver.");
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Notifies all listeners that quest data changed.
    /// </summary>
    private void NotifyQuestUpdated()
    {
        OnQuestUpdated?.Invoke();
    }
}
