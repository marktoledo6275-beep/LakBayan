using TMPro;
using UnityEngine;

public class HUDController : MonoBehaviour
{
    [Header("HUD References")]
    [SerializeField] private TMP_Text currentQuestText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text instructionText;

    [Header("Defaults")]
    [TextArea(2, 4)]
    [SerializeField] private string defaultInstruction = "Move with the joystick or WASD. Stand near a villager, shrine, or mini-game marker, then tap ACT.";

    private void OnEnable()
    {
        RegisterEvents();
        RefreshHUD();
    }

    private void OnDisable()
    {
        if (QuestSystem.Instance != null)
        {
            QuestSystem.Instance.OnQuestUpdated -= RefreshHUD;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameDataChanged -= RefreshHUD;
        }
    }

    private void Start()
    {
        RegisterEvents();
        RefreshHUD();
    }

    public void SetInstruction(string message)
    {
        if (instructionText != null)
        {
            instructionText.text = string.IsNullOrWhiteSpace(message) ? defaultInstruction : message;
        }
    }

    public void RefreshHUD()
    {
        if (currentQuestText != null)
        {
            string summary = QuestSystem.Instance != null
                ? QuestSystem.Instance.GetCurrentQuestSummary()
                : "Talk to the Village Elder to begin your first lesson.";

            currentQuestText.text = FormatQuestSummary(summary);
        }

        if (scoreText != null)
        {
            int totalScore = GameManager.Instance != null ? GameManager.Instance.TotalScore : 0;
            scoreText.text = totalScore.ToString();
        }

        if (instructionText != null && string.IsNullOrWhiteSpace(instructionText.text))
        {
            instructionText.text = defaultInstruction;
        }
    }

    private void RegisterEvents()
    {
        if (QuestSystem.Instance != null)
        {
            QuestSystem.Instance.OnQuestUpdated -= RefreshHUD;
            QuestSystem.Instance.OnQuestUpdated += RefreshHUD;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameDataChanged -= RefreshHUD;
            GameManager.Instance.OnGameDataChanged += RefreshHUD;
        }
    }

    private string FormatQuestSummary(string summary)
    {
        if (string.IsNullOrWhiteSpace(summary))
        {
            return "Talk to the Village Elder to begin your first lesson.";
        }

        return summary.Replace("Current Quest: ", string.Empty).Trim();
    }
}
