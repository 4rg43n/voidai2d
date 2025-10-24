using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerInputFieldUI : MonoBehaviour, IContextMenuProvider
{
    public TMP_InputField text;

    public void BuildMenuItems(GameObject clicked, List<ContextMenuManager.MenuItem> items)
    {
        // A default option, always available
        items.Add(new ContextMenuManager.MenuItem
        {
            Label = "Copy",
            Action = () => { Debug.Log($"Copy {clicked.name}"); GenGameUtils.CopyToClipboard(text.text); }
        });
        items.Add(new ContextMenuManager.MenuItem
        {
            Label = "Paste",
            Action = () => { Debug.Log($"Paste {clicked.name}"); text.text += GenGameUtils.CopyFromClipboard(); }
        });
    }
}
