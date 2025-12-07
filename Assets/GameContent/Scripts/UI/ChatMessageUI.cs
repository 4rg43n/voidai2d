using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoidAI.GenAI.Story;

public class ChatMessageUI : MonoBehaviour, IContextMenuProvider
{
    public TextMeshProUGUI text;
    public Image image;
    public bool isPlayer = false;

    public string frameSrcId = string.Empty;

    public void BuildMenuItems(GameObject clicked, List<ContextMenuManager.MenuItem> items)
    {
        ChatPanelUI chatPanel = transform.GetTop<ChatPanelUI>();

        // A default option, always available
        items.Add(new ContextMenuManager.MenuItem
        {
            Label = "Copy",
            Action = () => { Debug.Log($"Copy {clicked.name}"); GenGameUtils.CopyToClipboard(text.text); }
        });

        bool isInLastFrame = StoryManager.Singleton.storyContext.IsInLastFrame(frameSrcId);

        if (isInLastFrame)
        {
            if (isPlayer)
            {
                items.Add(new ContextMenuManager.MenuItem
                {
                    Label = "Delete",
                    Action = () => { Debug.Log($"Delete {clicked.name}"); StoryManager.Singleton.DeleteFrame(frameSrcId); /*CharacterGameManager.Singleton.DeleteFrame();*/ }
                });
            }
            else
            {
                items.Add(new ContextMenuManager.MenuItem
                {
                    Label = "Reroll",
                    Action = () => { Debug.Log($"Reroll {clicked.name}"); StoryManager.Singleton.RerollResponse(); /*CharacterGameManager.Singleton.RerollFrame();*/ }
                });
            }
        }


        //items.Add(new ContextMenuManager.MenuItem
        //{
        //    Label = "Paste",
        //    Action = () => { Debug.Log($"Paste {clicked.name}"); text.text += GenGameUtils.CopyFromClipboard(); }
        //});
    }

    //public void SetColor(Color col)
    //{
    //    image.color = col;
    //}

    //public void SetText(string msg)
    //{
    //    text.text = msg;
    //    ScrollToBottom();
    //}

    //void ScrollToBottom()
    //{
    //    Canvas.ForceUpdateCanvases();
    //}
}


