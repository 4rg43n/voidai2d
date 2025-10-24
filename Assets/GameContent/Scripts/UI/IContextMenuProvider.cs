using System.Collections.Generic;
using UnityEngine;

public interface IContextMenuProvider
{
    // Add items into 'items'. You can inspect 'clicked' or your own component state.
    void BuildMenuItems(GameObject clicked, List<ContextMenuManager.MenuItem> items);
}
