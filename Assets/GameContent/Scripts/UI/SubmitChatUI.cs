using TMPro;
using UnityEngine;
using VoidAI.GenAI.Story;

public class SubmitChatUI : MonoBehaviour
{
    public TMP_InputField inputField;

    public void OnSubmitPressed()
    {
        string msg = inputField.text;
        inputField.text = "";

        msg = msg.Trim();
        if (msg.Length == 0)
            return;

        StoryManager.Singleton.SubmitPlayerInput(msg);
    }
}
