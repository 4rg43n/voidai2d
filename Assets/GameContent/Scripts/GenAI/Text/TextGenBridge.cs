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

        public static int numSending = 0;
        public static bool IsSending { get { return numSending > 0; } }

        private void Awake()
        {
            Singleton = this;
        }

        public IEnumerator SendToLLM(
            string prompt, 
            string speakerName, 
            string modelName, 
            System.Action<string> callback)
        {
            string json = JsonUtility.ToJson(new RequestPayload
            {
                model = modelName,
                prompt = prompt,
                stream = false
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
                    callback(reply);
                }
                else
                {
                    numSending--;
                    Debug.LogError("LLM request failed: " + request.error);
                    callback("...");
                }
            }
        }

        [System.Serializable]
        public class RequestPayload
        {
            public string model;
            public string prompt;
            public bool stream;
        }

        [System.Serializable]
        public class ResponsePayload
        {
            public string response;
        }
    }
}