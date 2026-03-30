using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class HideSpotData
{
    public string spotId;
    public GameObject hiddenMarker;
    public GameObject foundMarker;
}

public class TaguTaguanManager : MonoBehaviour
{
    [Header("Round Setup")]
    [SerializeField] private HideSpotData[] hideSpots;
    [SerializeField] private int hidersToFind = 3;
    [SerializeField] private float roundTimeSeconds = 45f;

    [Header("Rewards")]
    [SerializeField] private string objectiveTargetId = "tagu_taguan";
    [SerializeField] private string rewardItemId = "woven_token";
    [SerializeField] private int rewardAmount = 1;
    [SerializeField] private int scoreReward = 25;

    [Header("UI")]
    [SerializeField] private Text timerText;
    [SerializeField] private Text resultText;
    [SerializeField] private Text instructionText;

    private readonly HashSet<int> occupiedSpots = new HashSet<int>();
    private readonly HashSet<int> revealedSpots = new HashSet<int>();

    private float timeRemaining;
    private int foundCount;
    private bool roundEnded;

    private void Start()
    {
        StartRound();
    }

    private void Update()
    {
        if (roundEnded)
        {
            return;
        }

        timeRemaining -= Time.deltaTime;

        if (timerText != null)
        {
            timerText.text = $"Time: {Mathf.CeilToInt(Mathf.Max(0f, timeRemaining))}";
        }

        if (timeRemaining <= 0f)
        {
            EndRound(false);
        }
    }

    public void StartRound()
    {
        occupiedSpots.Clear();
        revealedSpots.Clear();
        foundCount = 0;
        roundEnded = false;
        timeRemaining = roundTimeSeconds;

        for (int i = 0; i < hideSpots.Length; i++)
        {
            if (hideSpots[i].hiddenMarker != null)
            {
                hideSpots[i].hiddenMarker.SetActive(false);
            }

            if (hideSpots[i].foundMarker != null)
            {
                hideSpots[i].foundMarker.SetActive(false);
            }
        }

        int targetCount = Mathf.Clamp(hidersToFind, 1, hideSpots.Length);

        while (occupiedSpots.Count < targetCount)
        {
            occupiedSpots.Add(Random.Range(0, hideSpots.Length));
        }

        if (instructionText != null)
        {
            instructionText.text = "Tap the hiding spots to find the hidden children before time runs out.";
        }

        if (resultText != null)
        {
            resultText.text = "Find all the hidden players.";
        }
    }

    public void RevealSpot(int spotIndex)
    {
        if (roundEnded || spotIndex < 0 || spotIndex >= hideSpots.Length || revealedSpots.Contains(spotIndex))
        {
            return;
        }

        revealedSpots.Add(spotIndex);
        HideSpotData spot = hideSpots[spotIndex];
        bool foundHider = occupiedSpots.Contains(spotIndex);

        if (spot.foundMarker != null)
        {
            spot.foundMarker.SetActive(true);
        }

        if (spot.hiddenMarker != null)
        {
            spot.hiddenMarker.SetActive(foundHider);
        }

        if (foundHider)
        {
            foundCount++;
            Debug.Log($"Tagu-taguan: found a hider at spot {spotIndex}.", this);
        }

        if (resultText != null)
        {
            resultText.text = foundHider
                ? $"Nice! You found {foundCount}/{occupiedSpots.Count}."
                : "No one is hiding there. Keep looking.";
        }

        if (foundCount >= occupiedSpots.Count)
        {
            EndRound(true);
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
                ? "You won Tagu-taguan! The village children trust you more now."
                : "Time is up. Try again and search more carefully.";
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
