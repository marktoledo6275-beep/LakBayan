using UnityEngine;

[System.Serializable]
public class QuizQuestion
{
    [TextArea(2, 5)]
    public string question;

    [TextArea(2, 4)]
    public string historicalContext;

    public string[] choices = new string[4];
    public int correctAnswerIndex;

    [TextArea(2, 4)]
    public string explanation;

    public int scoreReward = 10;

    public bool IsCorrect(int selectedIndex)
    {
        return selectedIndex == correctAnswerIndex;
    }
}
