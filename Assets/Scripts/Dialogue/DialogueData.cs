using UnityEngine;

[System.Serializable]
public class DialogueEntryData
{
    [TextArea(2, 5)]
    public string text;
    public bool showAsHistoricalFact;
}

[CreateAssetMenu(fileName = "DialogueData", menuName = "LAKBAYAN/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [SerializeField] private string speakerName = "Villager";
    [SerializeField] private DialogueEntryData[] lines;

    public string SpeakerName => speakerName;
    public DialogueEntryData[] Lines => lines;
    public bool HasLines => lines != null && lines.Length > 0;
}
