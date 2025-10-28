using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using VoidAI.GenAI.Text;

public class ChatPanelUI : MonoBehaviour
{
    public static ChatPanelUI Singleton { get; private set; }

    public ChatMessageUI playerMsgPrefab;
    public ChatMessageUI speakerMsgPrefab;

    public TMP_InputField inputField;
    public ScrollRect scrollRect;
    public Transform content;

    public Transform root;
    public bool startVis = true;

    bool isVis = true;

    public string lastInput = "";

    public Action<string> OnSubmit;

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        Clear();
        isVis = startVis;
        UpdateVis();
    }

    private void Update()
    {
        inputField.enabled = !TextGenBridge.IsSending;
    }

    public void ToggleVis()
    {
        isVis = !isVis;
        UpdateVis();
    }

    void UpdateVis()
    {
        root.gameObject.SetActive(isVis);
    }

    public void Clear()
    {
        ChatMessageUI[] chats = content.GetComponentsInChildren<ChatMessageUI>();
        foreach (ChatMessageUI ch in chats)
        {
            Destroy(ch.gameObject);
        }
    }

    public void SubmitInput()
    {
        string msg = inputField.text;
        inputField.text = "";

        msg = msg.Trim();
        if (msg.Length == 0)
            return;

        AddPlayerMessage(msg);
        OnSubmit?.Invoke(msg);
    }

    public void AddCharacterMessage(string msg)
    {
        AddMessage($"{msg}", false);
    }

    public void AddPlayerMessage(string input)
    {
        AddMessage(input, true);
    }

    public void AddMessage(string message, bool isPlayer)
    {
        ChatMessageUI msgUI = isPlayer ? playerMsgPrefab : speakerMsgPrefab;
        msgUI.isPlayer = isPlayer;

        msgUI = Instantiate(msgUI);
        msgUI.text.text = message;
        msgUI.transform.parent = content;
        msgUI.transform.localScale = Vector3.one;

        ScrollToBottom();
    }

    public void RemoveMessages(int num)
    {
        List<ChatMessageUI> chats = new List<ChatMessageUI>(content.GetComponentsInChildren<ChatMessageUI>());

        for (int i=0;i<num;i++)
        {
            Destroy(chats[chats.Count - 1].gameObject);
            chats.RemoveAt(chats.Count - 1);
        }

        ScrollToBottom();
    }

    public bool IsInLastFrame(ChatMessageUI chatMsg)
    {
        List<ChatMessageUI> chats = new List<ChatMessageUI>(content.GetComponentsInChildren<ChatMessageUI>());

        int lastInputIndex = -1;
        for (int i = chats.Count-1; i >=0; i--)
        {
            ChatMessageUI msgToCheck = chats[i];
            if (msgToCheck.isPlayer)
            {
                lastInputIndex = i;
                break;
            }
        }

        if (lastInputIndex == -1)
            return false;

        for (int i = chats.Count - 1; i >= 0; i--)
        {
            ChatMessageUI msgToCheck = chats[i];
            if (msgToCheck == chatMsg && i >= lastInputIndex)
                return true;
        }

        return false;
    }

    void SetInputFocus()
    {
        inputField.ActivateInputField();
        inputField.Select();
    }

    void ScrollToBottom()
    {
        StartCoroutine(_ScrollToBottom());
    }

    IEnumerator _ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        yield return new WaitForEndOfFrame();

        scrollRect.content.GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
        scrollRect.content.GetComponent<ContentSizeFitter>().SetLayoutVertical();
        scrollRect.verticalNormalizedPosition = 0f;
    }

}



