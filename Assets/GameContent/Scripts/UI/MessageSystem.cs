using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum MsgType
{
    Error,
    System,
    Game
}

public delegate void MsgCallbackDel();

public class MessageSystem : MonoBehaviour
{
    public static MessageSystem Singleton { get; private set; }

    [Header("UI References")]
    public GameObject messagePanelPrefab; // A prefab with a background Image and a child TMP_Text
    public Transform panelParent; // Usually a canvas

    private Queue<MessageData> messageQueue = new();
    private GameObject currentMessageGO;

    public bool IsDisplaying { get; private set; } = false;

    private void Awake()
    {
        Singleton = this;
    }

    public void AddMessage(string msg, MsgType msgType, MsgCallbackDel callback, float onScreenTime = 3f)
    {
        messageQueue.Enqueue(new MessageData(msg, msgType, callback, onScreenTime));
        if (!IsDisplaying)
            StartCoroutine(DisplayNextMessage());
    }

    private IEnumerator DisplayNextMessage()
    {
        while (messageQueue.Count > 0)
        {
            var data = messageQueue.Dequeue();
            IsDisplaying = true;

            // Instantiate message panel
            currentMessageGO = Instantiate(messagePanelPrefab, panelParent);
            var panelImage = currentMessageGO.GetComponent<Image>();
            var textComponent = currentMessageGO.GetComponentInChildren<TMP_Text>();

            // Set color based on message type
            panelImage.color = data.MsgType switch
            {
                MsgType.Error => new Color(0.8f, 0.1f, 0.1f, 0.9f),
                MsgType.System => new Color(0.1f, 0.4f, 0.9f, 0.9f),
                MsgType.Game => new Color(0.1f, 0.7f, 0.2f, 0.9f),
                _ => Color.white
            };

            textComponent.text = data.Text;

            // Set up close on click
            var btn = currentMessageGO.AddComponent<Button>();
            btn.onClick.AddListener(() => {
                StopAllCoroutines();
                CloseMessage(data.Callback);
            });

            // Wait for timeout then close
            yield return new WaitForSeconds(data.Duration);
            CloseMessage(data.Callback);
        }

        IsDisplaying = false;
    }

    private void CloseMessage(MsgCallbackDel callback)
    {
        if (currentMessageGO != null)
            Destroy(currentMessageGO);

        callback?.Invoke();
        currentMessageGO = null;

        if (messageQueue.Count > 0)
            StartCoroutine(DisplayNextMessage());
        else
            IsDisplaying = false;
    }

    private class MessageData
    {
        public string Text;
        public MsgType MsgType;
        public MsgCallbackDel Callback;
        public float Duration;

        public MessageData(string text, MsgType type, MsgCallbackDel callback, float duration)
        {
            Text = text;
            MsgType = type;
            Callback = callback;
            Duration = duration;
        }
    }
}
