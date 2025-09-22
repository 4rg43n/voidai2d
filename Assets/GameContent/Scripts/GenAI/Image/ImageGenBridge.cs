using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using UnityEditor;

namespace VoidAI.GenAI.Image
{
    public class ImageGenBridge : MonoBehaviour
    {
        public delegate void OnImageGeneratedDel(Texture2D tex);

        public static ImageGenBridge Singleton { get; private set; }

        public static string[] allModels = { "dreamshaper_8.safetensors" };
        public static string selectedModel = "dreamshaper_8.safetensors";

        public string stableDiffusionURL = "http://127.0.0.1:7860/sdapi/v1/txt2img";

        OnImageGeneratedDel callback = null;

        private void Awake()
        {
            Singleton = this;
        }

        [System.Serializable]
        public class Txt2ImgRequest
        {
            public string prompt;
            public int steps = 20;
            public int width = 512;
            public int height = 512;

            public string sd_model_checkpoint;
        }

        [System.Serializable]
        public class Txt2ImgResponse
        {
            public string[] images;
        }

        [System.Serializable]
        public class ImageGenSettings
        {
            public int imageWidth = 768;
            public int imageHeight = 1024;
            public string imagePrompt;
            public string imageModel = "dreamshaper_8.safetensors";
        }

        public void GenerateNPCPortrait(ImageGenSettings imageSettings, OnImageGeneratedDel callback)
        {
            if (callback == null)
            {
                Debug.LogWarning("Callback cannot be null during image generation.");
                return;
            }

            this.callback = callback;

            StartCoroutine(SwitchModelAndGenerate(imageSettings));
        }

        IEnumerator SwitchModelAndGenerate(ImageGenSettings imageSettings)
        {
            yield return StartCoroutine(SwitchModel(imageSettings.imageModel));
            yield return new WaitForSeconds(3f); // give time for model to load
            yield return StartCoroutine(SendPrompt(imageSettings));
        }

        IEnumerator SwitchModel(string modelTitle)
        {
            string json = "{\"sd_model_checkpoint\": \"" + modelTitle + "\"}";
            using (UnityWebRequest req = UnityWebRequest.Put("http://127.0.0.1:7860/sdapi/v1/options", json))
            {
                req.method = UnityWebRequest.kHttpVerbPOST;
                req.SetRequestHeader("Content-Type", "application/json");
                yield return req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                    Debug.LogError("Model switch failed: " + req.error);
            }
        }

        private IEnumerator SendPrompt(ImageGenSettings imageSettings)
        {
            Txt2ImgRequest requestBody = new Txt2ImgRequest
            {
                prompt = imageSettings.imagePrompt,
                width = imageSettings.imageWidth,
                height = imageSettings.imageHeight,
                sd_model_checkpoint = imageSettings.imageModel
            };

            string jsonData = JsonUtility.ToJson(requestBody);

            using (UnityWebRequest www = UnityWebRequest.Put(stableDiffusionURL, jsonData))
            {
                www.method = UnityWebRequest.kHttpVerbPOST;
                www.SetRequestHeader("Content-Type", "application/json");

                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Image generation failed: " + www.error);
                    yield break;
                }

                Txt2ImgResponse result = JsonUtility.FromJson<Txt2ImgResponse>(www.downloadHandler.text);

                if (result.images.Length > 0)
                {
                    byte[] imageBytes = System.Convert.FromBase64String(result.images[0]);

                    Texture2D texture = new Texture2D(2, 2);
                    texture.LoadImage(imageBytes);

                    callback(texture);
                    callback = null;
                }
            }
        }
    }
}
