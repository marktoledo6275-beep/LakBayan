using UnityEngine;
using UnityEngine.UI;

public class QuestUI : MonoBehaviour
{
    [SerializeField] private GameObject questPanel;
    [SerializeField] private Text questText;
    [SerializeField] private bool hideWhenEmpty = false;

    /// <summary>
    /// Subscribes to quest updates when this UI becomes active.
    /// </summary>
    private void OnEnable()
    {
        RegisterToQuestManager();
        RefreshQuestText();
    }

    /// <summary>
    /// Unsubscribes from quest updates when this UI is disabled.
    /// </summary>
    private void OnDisable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestUpdated -= RefreshQuestText;
        }
    }

    /// <summary>
    /// Refreshes the quest text after the scene finishes loading.
    /// </summary>
    private void Start()
    {
        RegisterToQuestManager();
        RefreshQuestText();
    }

    /// <summary>
    /// Connects the UI to the current quest manager instance.
    /// </summary>
    private void RegisterToQuestManager()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestUpdated -= RefreshQuestText;
            QuestManager.Instance.OnQuestUpdated += RefreshQuestText;
        }
    }

    /// <summary>
    /// Updates the visible quest log text from the quest manager.
    /// </summary>
    public void RefreshQuestText()
    {
        string questLog = QuestManager.Instance != null ? QuestManager.Instance.GetQuestLogText() : string.Empty;

        if (questText != null)
        {
            questText.text = questLog;
        }

        if (questPanel != null && hideWhenEmpty)
        {
            questPanel.SetActive(!string.IsNullOrEmpty(questLog));
        }
    }
}
