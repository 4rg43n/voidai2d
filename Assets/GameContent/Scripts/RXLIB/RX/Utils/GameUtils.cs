using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RX.Utils
{
    public static class GameUtils
    {
        public static void SetSortingOrder(GameObject go)
        {
            int z = Mathf.RoundToInt(-go.transform.position.y * 100);
            List<SpriteRenderer> renderers = new List<SpriteRenderer>();
            SpriteRenderer sprr = go.GetComponent<SpriteRenderer>();
            if (sprr != null)
                renderers.Add(sprr);
            SpriteRenderer[] sprrs = go.GetComponentsInChildren<SpriteRenderer>();
            if (sprrs != null && sprrs.Length > 0)
                renderers.AddRange(sprrs);

            foreach (SpriteRenderer sr in renderers)
                sr.sortingOrder = z;
        }

        public static Vector2 GetNormalizedRectTransformPosition(RectTransform rt, PointerEventData pointerEventData)
        {
            Vector2 localPosition = Vector2.zero;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, pointerEventData.position, pointerEventData.pressEventCamera, out localPosition);
            localPosition = Rect.PointToNormalized(rt.rect, localPosition);
            return localPosition;
        }

        public static int GetNearestInt(float i)
        {
            int v = (int)i;
            int v2 = Mathf.FloorToInt(i);
            int v3 = Mathf.CeilToInt(i);

            if (i - (float)v >= 0.5f)
                return v3;
            else
                return v2;
        }

        public static List<string> ReadLines(string text, bool debug)
        {
            TextAsset ta = Resources.Load<TextAsset>(text);
            List<string> lines = new List<string>(Regex.Split(ta.text, "\n|\r|\r\n"));
            lines.Filter(a => { return a.Length > 0 && !a.StartsWith('#') && !a.StartsWith("//"); });

            if (debug)
                Debug.Log("Read " + lines.Count + " lines from file=" + text + ".");

            return lines;
        }

        public static string Capitalize(this string s)
        {
            string str = "" + s.ToUpper()[0] + s.ToLower().Substring(1);
            return str;
        }

        public static bool IsOverUI()
        {
            GameObject[] gos = GameObject.FindGameObjectsWithTag("MainMenu");
            if (gos == null || gos.Length == 0)
                return false;

            List<Canvas> cvList = new List<Canvas>();

            if (gos != null && gos.Length > 0)
            {
                foreach (GameObject go in gos)
                {
                    Canvas cv = go.GetComponent<Canvas>();
                    if (cv != null)
                        cvList.Add(cv);
                }
            }

            GameObject eventSysGO = GameObject.FindGameObjectWithTag("EventSystem");
            if (eventSysGO == null)
                return false;

            EventSystem es = eventSysGO.GetComponent<EventSystem>();
            if (es == null)
                return false;

            return IsOverUI(cvList.ToArray(), es);
        }

        public static bool IsOverUI(Canvas[] cvs, EventSystem eventSys)
        {
            foreach (Canvas cv in cvs)
            {
                if (IsOverUI(cv, eventSys))
                    return true;
            }

            return false;
        }

        public static bool IsOverUI(Canvas cv, EventSystem eventSys)
        {
            return IsOverUI(Input.mousePosition, cv, eventSys);
        }

        public static bool IsOverUI(Vector2 pointerPos, Canvas cv, EventSystem eventSys)
        {
            GraphicRaycaster gr = cv.GetComponent<GraphicRaycaster>();
            PointerEventData ped;


            ped = new PointerEventData(eventSys);
            ped.position = pointerPos;
            List<RaycastResult> results = new List<RaycastResult>();

            gr.Raycast(ped, results);

            if (results.Count > 0)
                return true;

            return false;
        }


        public static void SetSortingOrder(GameObject go, int sortOrder)
        {
            List<ParticleSystem> pss = new List<ParticleSystem>();
            ParticleSystem ps = go.GetComponent<ParticleSystem>();
            if (ps!=null)
                pss.Add(ps);
            ParticleSystem[] psArr=go.GetComponentsInChildren<ParticleSystem>();
            if (psArr!=null&&psArr.Length>0)
                pss.AddRange(psArr);

            List<SpriteRenderer> sprrs = new List<SpriteRenderer>();
            SpriteRenderer tsprr = go.GetComponent<SpriteRenderer>();
            if (tsprr!=null)
                sprrs.Add(tsprr);
            SpriteRenderer[] sprrArr=go.GetComponentsInChildren<SpriteRenderer>();
            if (sprrArr!=null&&sprrArr.Length>0)
                sprrs.AddRange(sprrArr);

            foreach(ParticleSystem pso in pss)
            {
                ParticleSystemRenderer psr = pso.GetComponent<ParticleSystemRenderer>();
                psr.sortingOrder = sortOrder;
            }

            foreach(SpriteRenderer spriteRenderer in sprrs)
            {
                spriteRenderer.sortingOrder = sortOrder;
            }
        }

        public static int GetNumerOfTypes(Type et)
        {
            return Enum.GetValues(et).Length;
        }

        public static string[] GetTypeNames(Type et)
        {
            List<string> vals = new List<string>();
            foreach (object ob in Enum.GetValues(et))
            {
                vals.Add(ob.ToString());
            }

            return vals.ToArray();
        }

        public static T GetTop<T>(this Transform trans)
        {
            Transform currentTrans = trans;

            while (currentTrans != null)
            {
                T t = currentTrans.gameObject.GetComponent<T>();
                if (t is T)
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

        public static Transform GetChildByName(this Transform trans, string name)
        {
            if (trans.name == name)
                return trans;

            for (int i = 0; i < trans.childCount; i++)
            {
                Transform tc = GetChildByName(trans.GetChild(i), name);
                if (tc != null)
                    return tc;
            }

            return null;
        }

        public static GameObject[] GetChildrenGOs(this Transform trans)
        {
            Transform[] ts = trans.GetChildren();
            GameObject[] gos = null;

            if (ts != null && ts.Length > 0)
            {
                for (int i = 0; i < ts.Length; i++)
                    gos[i] = ts[i].gameObject;
            }

            return gos;
        }

        public static Transform[] GetChildren(this Transform trans)
        {
            if (trans.childCount < 1)
                return new Transform[] { };

            List<Transform> ts = new List<Transform>();
            for (int i = 0; i < trans.childCount; i++)
            {
                ts.Add(trans.GetChild(i));
            }

            return ts.ToArray();
        }
    }
}


