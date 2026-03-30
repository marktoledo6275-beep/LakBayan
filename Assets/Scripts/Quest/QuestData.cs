using System;
using UnityEngine;

public enum QuestObjectiveType
{
    TalkToNpc,
    ReachLocation,
    CollectItem,
    FinishMiniGame,
    AnswerQuiz
}

public enum QuestState
{
    NotStarted,
    InProgress,
    ReadyToComplete,
    Completed
}

[Serializable]
public class QuestObjectiveData
{
    [SerializeField] private string objectiveId = "objective_001";
    [TextArea(2, 4)]
    [SerializeField] private string description = "Talk to the village elder.";
    [SerializeField] private QuestObjectiveType objectiveType = QuestObjectiveType.TalkToNpc;
    [SerializeField] private string targetId = "elder";
    [SerializeField] private int requiredAmount = 1;

    public string ObjectiveId => objectiveId;
    public string Description => description;
    public QuestObjectiveType ObjectiveType => objectiveType;
    public string TargetId => targetId;
    public int RequiredAmount => Mathf.Max(1, requiredAmount);
}

[Serializable]
public class QuestRewardData
{
    [SerializeField] private string rewardItemId = "golden_shell";
    [SerializeField] private int rewardAmount = 1;
    [SerializeField] private int scoreBonus = 20;
    [SerializeField] private string unlockContentId;

    public string RewardItemId => rewardItemId;
    public int RewardAmount => rewardAmount;
    public int ScoreBonus => scoreBonus;
    public string UnlockContentId => unlockContentId;
}

[CreateAssetMenu(fileName = "QuestData", menuName = "LAKBAYAN/Quest Data")]
public class QuestData : ScriptableObject
{
    [SerializeField] private string questId = "quest_001";
    [SerializeField] private string title = "Meet the Datu";
    [TextArea(2, 5)]
    [SerializeField] private string description = "Learn about how a barangay is led by the datu.";
    [TextArea(2, 5)]
    [SerializeField] private string historicalFact = "In many pre-colonial communities, the datu served as a community leader, judge, and protector.";
    [SerializeField] private QuestObjectiveData[] objectives;
    [SerializeField] private QuestRewardData[] rewards;

    public string QuestId => questId;
    public string Title => title;
    public string Description => description;
    public string HistoricalFact => historicalFact;
    public QuestObjectiveData[] Objectives => objectives;
    public QuestRewardData[] Rewards => rewards;
    public bool IsValid => !string.IsNullOrWhiteSpace(questId) && objectives != null && objectives.Length > 0;
}
