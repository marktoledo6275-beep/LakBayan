using UnityEngine;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    [Header("Quiz Content")]
    [SerializeField] private string quizTitle = "Pre-Colonial Philippines Quiz";
    [SerializeField] private QuizQuestion[] questions;
    [SerializeField] private string completionTargetId = "quiz_precolonial_basics";
    [SerializeField] private bool requirePassToAdvanceQuest = true;
    [SerializeField] private string unlockContentIdOnPass = "quiz_mastery";

    [Header("Passing Rules")]
    [SerializeField] private int passingScore = 20;

    [Header("UI References")]
    [SerializeField] private Text titleText;
    [SerializeField] private Text questionText;
    [SerializeField] private Text contextText;
    [SerializeField] private Text feedbackText;
    [SerializeField] private Text progressText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Button[] answerButtons;

    private int currentQuestionIndex;
    private int currentScore;
    private bool answerLocked;

    public bool HasQuestions => questions != null && questions.Length > 0;

    private void Start()
    {
        StartQuiz();
    }

    public void StartQuiz()
    {
        currentQuestionIndex = 0;
        currentScore = 0;
        answerLocked = false;

        if (titleText != null)
        {
            titleText.text = quizTitle;
        }

        if (feedbackText != null)
        {
            feedbackText.text = "Choose the best answer.";
        }

        DisplayCurrentQuestion();
        RefreshScoreUI();
    }

    public void SubmitAnswerFromButton(int answerIndex)
    {
        if (!HasQuestions || answerLocked || currentQuestionIndex >= questions.Length)
        {
            return;
        }

        answerLocked = true;
        QuizQuestion question = questions[currentQuestionIndex];
        bool isCorrect = question.IsCorrect(answerIndex);

        if (isCorrect)
        {
            currentScore += Mathf.Max(0, question.scoreReward);
        }

        if (feedbackText != null)
        {
            feedbackText.text = isCorrect
                ? $"Correct! {question.explanation}"
                : $"Not quite. {question.explanation}";
        }

        RefreshScoreUI();
        SetAnswerButtonsInteractable(false);
    }

    public void NextQuestion()
    {
        if (!HasQuestions)
        {
            return;
        }

        currentQuestionIndex++;
        answerLocked = false;

        if (currentQuestionIndex >= questions.Length)
        {
            FinishQuiz();
            return;
        }

        if (feedbackText != null)
        {
            feedbackText.text = "Choose the best answer.";
        }

        SetAnswerButtonsInteractable(true);
        DisplayCurrentQuestion();
    }

    public void RestartQuiz()
    {
        StartQuiz();
    }

    public bool ValidateAnswerForDebug(int questionIndex, int answerIndex)
    {
        if (questions == null || questionIndex < 0 || questionIndex >= questions.Length)
        {
            return false;
        }

        return questions[questionIndex].IsCorrect(answerIndex);
    }

    [ContextMenu("Debug Finish With Pass")]
    public void DebugFinishWithPass()
    {
        currentScore = Mathf.Max(currentScore, passingScore);
        FinishQuiz();
    }

    [ContextMenu("Debug Finish With Fail")]
    public void DebugFinishWithFail()
    {
        currentScore = 0;
        FinishQuiz();
    }

    private void DisplayCurrentQuestion()
    {
        if (!HasQuestions)
        {
            if (questionText != null)
            {
                questionText.text = "No questions have been added yet.";
            }

            return;
        }

        QuizQuestion question = questions[currentQuestionIndex];

        if (questionText != null)
        {
            questionText.text = question.question;
        }

        if (contextText != null)
        {
            contextText.text = question.historicalContext;
        }

        if (progressText != null)
        {
            progressText.text = $"Question {currentQuestionIndex + 1}/{questions.Length}";
        }

        for (int i = 0; i < answerButtons.Length; i++)
        {
            Button button = answerButtons[i];

            if (button == null)
            {
                continue;
            }

            bool hasChoice = question.choices != null && i < question.choices.Length;
            button.gameObject.SetActive(hasChoice);
            button.interactable = hasChoice;

            if (!hasChoice)
            {
                continue;
            }

            Text buttonText = button.GetComponentInChildren<Text>();

            if (buttonText != null)
            {
                buttonText.text = question.choices[i];
            }
        }
    }

    private void FinishQuiz()
    {
        bool passed = currentScore >= passingScore;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddQuizScore(currentScore);

            if (passed && !string.IsNullOrWhiteSpace(unlockContentIdOnPass))
            {
                GameManager.Instance.UnlockContent(unlockContentIdOnPass);
            }
        }

        if (QuestSystem.Instance != null && (!requirePassToAdvanceQuest || passed))
        {
            QuestSystem.Instance.TryAdvanceObjectivesByTarget(completionTargetId, QuestObjectiveType.AnswerQuiz, 1);
        }

        if (questionText != null)
        {
            questionText.text = passed ? "Quiz Complete!" : "Quiz Finished";
        }

        if (contextText != null)
        {
            contextText.text = passed
                ? "Great work! You remembered important facts about pre-colonial Philippine life."
                : "Review the facts and try again to improve your score.";
        }

        if (feedbackText != null)
        {
            feedbackText.text = passed
                ? $"You passed with {currentScore} points."
                : $"You scored {currentScore} points. Passing score: {passingScore}.";
        }

        SetAnswerButtonsInteractable(false);
        RefreshScoreUI();
    }

    private void RefreshScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore}";
        }
    }

    private void SetAnswerButtonsInteractable(bool isEnabled)
    {
        foreach (Button button in answerButtons)
        {
            if (button != null)
            {
                button.interactable = isEnabled;
            }
        }
    }
}
