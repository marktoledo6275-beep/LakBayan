using UnityEngine;

public class LakbayanDebugTools : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private QuizManager quizManager;
    [SerializeField] private TaguTaguanManager taguTaguanManager;
    [SerializeField] private PatinteroManager patinteroManager;
    [SerializeField] private TumbangPresoManager tumbangPresoManager;

    [ContextMenu("Test Movement Setup")]
    public void TestMovementSetup()
    {
        if (playerController == null)
        {
            playerController = FindFirstObjectByType<PlayerController>();
        }

        if (playerController == null)
        {
            Debug.LogWarning("Movement test failed: no PlayerController found in the scene.", this);
            return;
        }

        Rigidbody2D body = playerController.GetComponent<Rigidbody2D>();
        Collider2D hitbox = playerController.GetComponent<Collider2D>();

        Debug.Log(body != null && hitbox != null
            ? "Movement test passed: PlayerController, Rigidbody2D, and Collider2D are present."
            : "Movement test failed: the player is missing a Rigidbody2D or Collider2D.", this);
    }

    [ContextMenu("Test Quiz Validation")]
    public void TestQuizValidation()
    {
        if (quizManager == null)
        {
            quizManager = FindFirstObjectByType<QuizManager>();
        }

        if (quizManager == null || !quizManager.HasQuestions)
        {
            Debug.LogWarning("Quiz test failed: no QuizManager with questions was found.", this);
            return;
        }

        bool firstCorrect = quizManager.ValidateAnswerForDebug(0, 0) || quizManager.ValidateAnswerForDebug(0, 1) ||
                            quizManager.ValidateAnswerForDebug(0, 2) || quizManager.ValidateAnswerForDebug(0, 3);

        Debug.Log(firstCorrect
            ? "Quiz test passed: at least one answer for the first question is correctly configured."
            : "Quiz test failed: the first quiz question may not have a valid correct answer index.", this);
    }

    [ContextMenu("Test Mini-Game Managers")]
    public void TestMiniGameManagers()
    {
        if (taguTaguanManager == null)
        {
            taguTaguanManager = FindFirstObjectByType<TaguTaguanManager>();
        }

        if (patinteroManager == null)
        {
            patinteroManager = FindFirstObjectByType<PatinteroManager>();
        }

        if (tumbangPresoManager == null)
        {
            tumbangPresoManager = FindFirstObjectByType<TumbangPresoManager>();
        }

        bool allPresent = taguTaguanManager != null && patinteroManager != null && tumbangPresoManager != null;

        Debug.Log(allPresent
            ? "Mini-game test passed: all three mini-game managers are available in the open scene."
            : "Mini-game test warning: one or more mini-game managers are missing from the scene.", this);
    }
}
