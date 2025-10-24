using UnityEngine;

public static class GenGameUtils
{
    public static void CopyToClipboard(string text)
    {
        GUIUtility.systemCopyBuffer = text;
        Debug.Log("Copied to clipboard: " + text);
    }

    public static string CopyFromClipboard()
    {
        string paste = GUIUtility.systemCopyBuffer;
        Debug.Log("Copied from clipboard: " + paste);
        return paste;
    }

    public static T GetTop<T>(this Transform trans)
    {
        Transform currentTrans = trans;

        while (currentTrans != null)
        {
            T t = currentTrans.gameObject.GetComponent<T>();
            if (t != null && !t.Equals(null) && t is T)
                return t;
            currentTrans = currentTrans.parent;
        }

        return default(T);
    }

    public static Transform GetTop(this Transform trans)
    {
        Transform currentTrans = trans;

        while (currentTrans.parent != null)
            currentTrans = currentTrans.parent;

        return currentTrans;
    }

}
