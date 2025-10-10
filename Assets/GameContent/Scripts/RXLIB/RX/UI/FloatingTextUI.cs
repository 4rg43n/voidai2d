using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RX.UI
{
    public class FloatingTextUI : MonoBehaviour
    {
        public TextMeshProUGUI[] textMeshPros;
        public float speed = 5;
        public float lifetime = 2;

        Camera cam;
        Color c = Color.white;
        float startTime;
        float startA;

        bool inited = false;

        public void SetText(Color col, string txt)
        {
            c = col;

            foreach (TextMeshProUGUI textMeshPro in textMeshPros)
                if (textMeshPro != null)
                    textMeshPro.text = txt;
            cam = Camera.main;

            textMeshPros[0].color = c;

            Destroy(gameObject, lifetime);
            startTime = 0;
            startA = c.a;
            inited = true;
        }

        private void Update()
        {
            if (!inited)
            {
                string txt = "";
                foreach (TextMeshProUGUI textMeshPro in textMeshPros)
                    if (textMeshPro != null)
                        txt = textMeshPro.text;

                SetText(c, txt);
            }

            transform.position += Vector3.up * speed * Time.deltaTime;
            transform.rotation = cam.transform.rotation;
            float perc = Mathf.Clamp01(startTime / lifetime);
            perc = 1 - perc;
            c.a = perc * startA;

            if (textMeshPros != null && textMeshPros.Length > 0)
                textMeshPros[0].color = c;

            startTime += Time.deltaTime;
        }
    }
}



