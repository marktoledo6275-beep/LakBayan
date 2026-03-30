using UnityEngine;
using UnityEngine.UI;

public class TumbangPresoManager : MonoBehaviour
{
    [Header("Throwing Setup")]
    [SerializeField] private Rigidbody2D canRigidbody;
    [SerializeField] private Rigidbody2D slipperPrefab;
    [SerializeField] private Transform throwOrigin;
    [SerializeField] private float throwForce = 12f;
    [SerializeField] private int maximumThrows = 5;

    [Header("Win Rules")]
    [SerializeField] private float toppleDistance = 1.25f;
    [SerializeField] private float toppleRotation = 30f;

    [Header("Rewards")]
    [SerializeField] private string objectiveTargetId = "tumbang_preso";
    [SerializeField] private string rewardItemId = "shell_medal";
    [SerializeField] private int rewardAmount = 1;
    [SerializeField] private int scoreReward = 35;

    [Header("UI")]
    [SerializeField] private Text throwsLeftText;
    [SerializeField] private Text resultText;
    [SerializeField] private Text instructionText;

    private Vector2 currentAimDirection = Vector2.right;
    private int throwsUsed;
    private bool roundEnded;
    private Vector3 initialCanPosition;

    private void Start()
    {
        if (canRigidbody != null)
        {
            initialCanPosition = canRigidbody.transform.position;
        }

        UpdateUI();

        if (instructionText != null)
        {
            instructionText.text = "Choose a direction and throw the tsinelas to knock down the can.";
        }
    }

    private void Update()
    {
        if (roundEnded || canRigidbody == null)
        {
            return;
        }

        float movedDistance = Vector2.Distance(initialCanPosition, canRigidbody.transform.position);
        float absoluteRotation = Mathf.Abs(canRigidbody.transform.eulerAngles.z);
        float normalizedRotation = absoluteRotation > 180f ? 360f - absoluteRotation : absoluteRotation;

        if (movedDistance >= toppleDistance || normalizedRotation >= toppleRotation)
        {
            EndRound(true);
        }
    }

    public void SetAimDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.01f)
        {
            return;
        }

        currentAimDirection = direction.normalized;
    }

    public void AimUp()
    {
        SetAimDirection(Vector2.up);
    }

    public void AimDown()
    {
        SetAimDirection(Vector2.down);
    }

    public void AimLeft()
    {
        SetAimDirection(Vector2.left);
    }

    public void AimRight()
    {
        SetAimDirection(Vector2.right);
    }

    public void Throw()
    {
        if (roundEnded || slipperPrefab == null || throwOrigin == null)
        {
            return;
        }

        if (throwsUsed >= maximumThrows)
        {
            EndRound(false);
            return;
        }

        Rigidbody2D slipper = Instantiate(slipperPrefab, throwOrigin.position, Quaternion.identity);
        slipper.AddForce(currentAimDirection * throwForce, ForceMode2D.Impulse);
        throwsUsed++;
        UpdateUI();

        if (throwsUsed >= maximumThrows)
        {
            Invoke(nameof(CheckLoseConditionAfterLastThrow), 1f);
        }
    }

    [ContextMenu("Debug Force Win")]
    public void DebugForceWin()
    {
        EndRound(true);
    }

    [ContextMenu("Debug Force Lose")]
    public void DebugForceLose()
    {
        EndRound(false);
    }

    private void CheckLoseConditionAfterLastThrow()
    {
        if (!roundEnded)
        {
            EndRound(false);
        }
    }

    private void EndRound(bool playerWon)
    {
        if (roundEnded)
        {
            return;
        }

        roundEnded = true;

        if (resultText != null)
        {
            resultText.text = playerWon
                ? "You knocked down the can!"
                : "Out of throws. Try adjusting your aim and power.";
        }

        if (!playerWon)
        {
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMiniGameScore(scoreReward);
            GameManager.Instance.AddReward(rewardItemId, rewardAmount);
        }

        if (QuestSystem.Instance != null)
        {
            QuestSystem.Instance.TryAdvanceObjectivesByTarget(objectiveTargetId, QuestObjectiveType.FinishMiniGame, 1);
        }
    }

    private void UpdateUI()
    {
        if (throwsLeftText != null)
        {
            throwsLeftText.text = $"Throws Left: {Mathf.Max(0, maximumThrows - throwsUsed)}";
        }

        if (resultText != null && !roundEnded)
        {
            resultText.text = "Knock the can down before you run out of throws.";
        }
    }
}
