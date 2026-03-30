using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class PatinteroLaneData
{
    public Transform blocker;
    public float minX = -4f;
    public float maxX = 4f;
    public float speed = 2f;
    [HideInInspector] public int direction = 1;
}

public class PatinteroManager : MonoBehaviour
{
    [Header("Grid Setup")]
    [SerializeField] private Transform gridOrigin;
    [SerializeField] private Transform playerToken;
    [SerializeField] private int gridColumns = 5;
    [SerializeField] private int gridRows = 5;
    [SerializeField] private float cellSize = 1.5f;

    [Header("Blockers")]
    [SerializeField] private PatinteroLaneData[] lanes;
    [SerializeField] private float collisionDistance = 0.45f;

    [Header("Rewards")]
    [SerializeField] private string objectiveTargetId = "patintero";
    [SerializeField] private string rewardItemId = "bamboo_badge";
    [SerializeField] private int rewardAmount = 1;
    [SerializeField] private int scoreReward = 30;

    [Header("UI")]
    [SerializeField] private Text resultText;
    [SerializeField] private Text instructionText;

    private Vector2Int playerGridPosition;
    private bool roundEnded;

    private void Start()
    {
        RestartRound();
    }

    private void Update()
    {
        MoveBlockers();
        CheckForCollision();
    }

    public void MoveUp()
    {
        TryMovePlayer(Vector2Int.up);
    }

    public void MoveDown()
    {
        TryMovePlayer(Vector2Int.down);
    }

    public void MoveLeft()
    {
        TryMovePlayer(Vector2Int.left);
    }

    public void MoveRight()
    {
        TryMovePlayer(Vector2Int.right);
    }

    public void RestartRound()
    {
        roundEnded = false;
        playerGridPosition = new Vector2Int(Mathf.Max(0, gridColumns / 2), 0);
        UpdatePlayerTokenPosition();

        for (int i = 0; i < lanes.Length; i++)
        {
            lanes[i].direction = i % 2 == 0 ? 1 : -1;
        }

        if (instructionText != null)
        {
            instructionText.text = "Cross the grid without touching the blockers.";
        }

        if (resultText != null)
        {
            resultText.text = "Reach the top row to win.";
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

    private void TryMovePlayer(Vector2Int direction)
    {
        if (roundEnded)
        {
            return;
        }

        playerGridPosition += direction;
        playerGridPosition.x = Mathf.Clamp(playerGridPosition.x, 0, gridColumns - 1);
        playerGridPosition.y = Mathf.Clamp(playerGridPosition.y, 0, gridRows - 1);

        UpdatePlayerTokenPosition();
        CheckForCollision();

        if (!roundEnded && playerGridPosition.y >= gridRows - 1)
        {
            EndRound(true);
        }
    }

    private void MoveBlockers()
    {
        if (roundEnded)
        {
            return;
        }

        foreach (PatinteroLaneData lane in lanes)
        {
            if (lane == null || lane.blocker == null)
            {
                continue;
            }

            Vector3 position = lane.blocker.position;
            position.x += lane.direction * lane.speed * Time.deltaTime;

            if (position.x > lane.maxX)
            {
                position.x = lane.maxX;
                lane.direction = -1;
            }
            else if (position.x < lane.minX)
            {
                position.x = lane.minX;
                lane.direction = 1;
            }

            lane.blocker.position = position;
        }
    }

    private void CheckForCollision()
    {
        if (roundEnded || playerToken == null)
        {
            return;
        }

        foreach (PatinteroLaneData lane in lanes)
        {
            if (lane == null || lane.blocker == null)
            {
                continue;
            }

            if (Vector2.Distance(playerToken.position, lane.blocker.position) <= collisionDistance)
            {
                EndRound(false);
                return;
            }
        }
    }

    private void UpdatePlayerTokenPosition()
    {
        if (gridOrigin == null || playerToken == null)
        {
            return;
        }

        Vector3 targetPosition = gridOrigin.position + new Vector3(playerGridPosition.x * cellSize, playerGridPosition.y * cellSize, 0f);
        playerToken.position = targetPosition;
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
                ? "You cleared the Patintero grid!"
                : "Caught by the blocker. Try a different route.";
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
}
