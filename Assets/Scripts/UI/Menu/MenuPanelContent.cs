using UnityEngine;
using UnityEngine.UI;

public class MenuPanelContent : MonoBehaviour
{
    [SerializeField] private Text titleText;
    [SerializeField] private Text bodyText;

    public void SetContent(string title, string body)
    {
        if (titleText != null)
        {
            titleText.text = title;
        }

        if (bodyText != null)
        {
            bodyText.text = body;
        }
    }
}
