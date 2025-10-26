using System;
using System.Collections;
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
                stop = new string[] { "</CHARACTER>" } // new string[] { "</CHARACTER>", "<END>" }
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
                    string reply = response.response.Trim();

                    // TODO: This is not generic enough, fix later
                    int idx = reply.IndexOf("</CHARACTER>", StringComparison.OrdinalIgnoreCase);
                    if (idx >= 0)
                        reply = reply.Substring(0, idx + "</CHARACTER>".Length);

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
        }

        [System.Serializable]
        public class ResponsePayload
        {
            public string response;
        }
    }

    public class MessageLLM
    {
        public string speakerName;
        public string modelName;
        public string prompt;

        public string response;

        public MessageLLM(string resp)
        {
            response = resp;
        }
    }
}