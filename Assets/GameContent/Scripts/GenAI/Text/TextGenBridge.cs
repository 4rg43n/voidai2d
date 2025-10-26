using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace VoidAI.GenAI.Text
{
    public class TextGenBridge : MonoBehaviour
    {
        public static TextGenBridge Singleton { get; private set; }

        public string ollamaURL = "http://localhost:11434/api/generate";
        public string modelName = LLM_Model_Defs.INSTRUCT_LLM;

        private List<string> defaultStop = new List<string>(new string[] { "<END>", "</END>" });

        static int numSending = 0;
        public static bool IsSending { get { return numSending > 0; } }

        private void Awake()
        {
            Singleton = this;
        }

        public void SendToLLM(string prompt, string speakerName, string modelName, System.Action<MessageLLM> callback)
        {
            StartCoroutine(_SendToLLM(prompt, speakerName, modelName, callback));
        }

        IEnumerator _SendToLLM(
            string prompt, 
            string speakerName, 
            string modelName, 
            System.Action<MessageLLM> callback)
        {
            string json = JsonUtility.ToJson(new RequestPayload
            {
                model = modelName,
                prompt = prompt,
                stream = false,
                stop = defaultStop.ToArray(),
                options = new Options()
            });

            using (UnityWebRequest request = UnityWebRequest.PostWwwForm(ollamaURL, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                numSending++;

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    numSending--;
                    ResponsePayload response = JsonUtility.FromJson<ResponsePayload>(request.downloadHandler.text);
                    string originalReply = response.response.Trim();
                    string reply = originalReply;

                    Debug.Log($"LLM Response: {reply}");
                    // TODO: This is not generic enough, fix later
                    for (int i = defaultStop.Count - 1; i >= 0; i--)
                    {
                        string stop = defaultStop[i];
                        if (!reply.Contains(stop, StringComparison.OrdinalIgnoreCase))
                            continue;
                        int idx = reply.IndexOf(stop, StringComparison.OrdinalIgnoreCase);
                        if (idx >= 0)
                        {
                            reply = reply.Substring(0, idx);// + stop.Length);
                            break;
                        }
                    }

                    MessageLLM msg = new MessageLLM(reply)
                    {
                        speakerName = speakerName,
                        modelName = modelName,
                        prompt = prompt,
                    };

                    callback(msg);
                }
                else
                {
                    numSending--;
                    Debug.LogError("LLM request failed: " + request.error);
                    callback(new MessageLLM("..."));
                }
            }
        }

        [System.Serializable]
        public class RequestPayload
        {
            public string model;
            public string prompt;
            public bool stream;
            public string[] stop;
            public Options options;
        }

        [System.Serializable]
        public class Options
        {
            public int num_predict = 512;   // how many tokens to predict (default 128 if omitted)
            public int num_ctx = 4096;      // total context size
            public float temperature = 0.8f;
            public float top_p = 0.9f;
            public float repeat_penalty = 1.15f;
        }

        [System.Serializable]
        public class ResponsePayload
        {
            public string response;
        }
    }

}


