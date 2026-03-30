using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventoryEntry
{
    public string itemId;
    public int amount;
}

[Serializable]
public class GameSaveData
{
    public int historyScore;
    public int quizScore;
    public int miniGameScore;
    public string lastWorldSceneName;
    public List<string> unlockedContentIds = new List<string>();
    public List<InventoryEntry> inventoryEntries = new List<InventoryEntry>();
}

public class GameManager : MonoBehaviour
{
    private const string SaveKey = "lakbayan_save_data";

    [Header("Bootstrap")]
    [SerializeField] private bool dontDestroyOnLoad = true;
    [SerializeField] private string startingWorldScene = "MainWorld";

    private readonly Dictionary<string, int> inventory = new Dictionary<string, int>();
    private readonly HashSet<string> unlockedContentIds = new HashSet<string>();

    private int historyScore;
    private int quizScore;
    private int miniGameScore;
    private string lastWorldSceneName;

    public static GameManager Instance { get; private set; }

    public event Action OnGameDataChanged;

    public int HistoryScore => historyScore;
    public int QuizScore => quizScore;
    public int MiniGameScore => miniGameScore;
    public int TotalScore => historyScore + quizScore + miniGameScore;
    public string LastWorldSceneName => string.IsNullOrWhiteSpace(lastWorldSceneName) ? startingWorldScene : lastWorldSceneName;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }

        LoadProgress();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            SaveProgress();
        }
    }

    private void OnApplicationQuit()
    {
        SaveProgress();
    }

    public void AddHistoryScore(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        historyScore += amount;
        Debug.Log($"History score increased by {amount}. Total history score: {historyScore}", this);
        NotifyDataChanged();
    }

    public void AddQuizScore(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        quizScore += amount;
        Debug.Log($"Quiz score increased by {amount}. Total quiz score: {quizScore}", this);
        NotifyDataChanged();
    }

    public void AddMiniGameScore(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        miniGameScore += amount;
        Debug.Log($"Mini-game score increased by {amount}. Total mini-game score: {miniGameScore}", this);
        NotifyDataChanged();
    }

    public void AddReward(string itemId, int amount)
    {
        if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
        {
            return;
        }

        if (!inventory.ContainsKey(itemId))
        {
            inventory[itemId] = 0;
        }

        inventory[itemId] += amount;
        Debug.Log($"Reward added: {itemId} x{amount}. Current total: {inventory[itemId]}", this);
        NotifyDataChanged();
    }

    public int GetItemCount(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return 0;
        }

        return inventory.TryGetValue(itemId, out int amount) ? amount : 0;
    }

    public void UnlockContent(string contentId)
    {
        if (string.IsNullOrWhiteSpace(contentId))
        {
            return;
        }

        if (unlockedContentIds.Add(contentId))
        {
            Debug.Log($"Unlocked content: {contentId}", this);
            NotifyDataChanged();
        }
    }

    public bool IsContentUnlocked(string contentId)
    {
        if (string.IsNullOrWhiteSpace(contentId))
        {
            return false;
        }

        return unlockedContentIds.Contains(contentId);
    }

    public void SetLastWorldScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return;
        }

        lastWorldSceneName = sceneName;
        NotifyDataChanged();
    }

    public void ResetProgress()
    {
        historyScore = 0;
        quizScore = 0;
        miniGameScore = 0;
        lastWorldSceneName = startingWorldScene;
        inventory.Clear();
        unlockedContentIds.Clear();
        PlayerPrefs.DeleteKey(SaveKey);
        NotifyDataChanged();
    }

    public void SaveProgress()
    {
        GameSaveData saveData = new GameSaveData
        {
            historyScore = historyScore,
            quizScore = quizScore,
            miniGameScore = miniGameScore,
            lastWorldSceneName = LastWorldSceneName
        };

        foreach (string unlockedContentId in unlockedContentIds)
        {
            saveData.unlockedContentIds.Add(unlockedContentId);
        }

        foreach (KeyValuePair<string, int> item in inventory)
        {
            saveData.inventoryEntries.Add(new InventoryEntry
            {
                itemId = item.Key,
                amount = item.Value
            });
        }

        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(saveData));
        PlayerPrefs.Save();
    }

    public void LoadProgress()
    {
        inventory.Clear();
        unlockedContentIds.Clear();
        historyScore = 0;
        quizScore = 0;
        miniGameScore = 0;
        lastWorldSceneName = startingWorldScene;

        if (!PlayerPrefs.HasKey(SaveKey))
        {
            NotifyDataChanged();
            return;
        }

        GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(PlayerPrefs.GetString(SaveKey));

        if (saveData == null)
        {
            NotifyDataChanged();
            return;
        }

        historyScore = saveData.historyScore;
        quizScore = saveData.quizScore;
        miniGameScore = saveData.miniGameScore;
        lastWorldSceneName = string.IsNullOrWhiteSpace(saveData.lastWorldSceneName)
            ? startingWorldScene
            : saveData.lastWorldSceneName;

        if (saveData.unlockedContentIds != null)
        {
            foreach (string unlockedId in saveData.unlockedContentIds)
            {
                if (!string.IsNullOrWhiteSpace(unlockedId))
                {
                    unlockedContentIds.Add(unlockedId);
                }
            }
        }

        if (saveData.inventoryEntries != null)
        {
            foreach (InventoryEntry entry in saveData.inventoryEntries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.itemId))
                {
                    continue;
                }

                inventory[entry.itemId] = Mathf.Max(0, entry.amount);
            }
        }

        NotifyDataChanged();
    }

    private void NotifyDataChanged()
    {
        OnGameDataChanged?.Invoke();
    }
}
