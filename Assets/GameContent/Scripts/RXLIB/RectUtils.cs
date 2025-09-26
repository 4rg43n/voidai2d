using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RX.Utils
{
    public static class RectUtils
    {
        #region
        public static Rect ToRect(this RectInt rect)
        {
            return new Rect(rect.x,rect.y,rect.width,rect.height);
        }

        public static RectInt Offset(this RectInt rect, Transform transform)
        {
            Vector2Int bl = new Vector2Int(
                Mathf.RoundToInt(transform.position.x - rect.width / 2f),
                Mathf.RoundToInt(transform.position.y - rect.height / 2f));
            return new RectInt(bl.x, bl.y, rect.width, rect.height);
        }

        public static void DrawGizmos(this RectInt rect, Color color)
        {
            Vector2Int bl = rect.BL();
            Vector2Int br = rect.BR();
            Vector2Int tr = rect.TR();
            Vector2Int tl = rect.TL();

            Gizmos.color = color;
            Gizmos.DrawLine(bl.ToV2(), br.ToV2());
            Gizmos.DrawLine(br.ToV2(), tr.ToV2());
            Gizmos.DrawLine(tr.ToV2(), tl.ToV2());
            Gizmos.DrawLine(tl.ToV2(), bl.ToV2());
        }

        public static Vector2 ToV2(this Vector2Int v)
        {
            return new Vector2(v.x, v.y);
        }

        public static Vector2Int BL(this RectInt rect)
        {
            Vector2Int bl = new Vector2Int(rect.x, rect.y);
            return bl;
        }

        public static Vector2Int BR(this RectInt rect)
        {
            Vector2Int bl = rect.BL();
            bl.x += rect.width;
            return bl;
        }

        public static Vector2Int TR(this RectInt rect)
        {
            Vector2Int br = rect.BR();
            br.y += rect.height;
            return br;
        }

        public static Vector2Int TL(this RectInt rect)
        {
            Vector2Int bl = rect.BL();
            bl.y += rect.height;
            return bl;
        }
        #endregion


        #region RECT utils
        public static Rect Offset(this Rect rect, Transform transform)
        {
            Vector2 bl = new Vector2(transform.position.x - rect.width / 2f, transform.position.y - rect.height / 2f);
            return new Rect(bl.x, bl.y, rect.width, rect.height);
        }

        public static void DrawGizmos(this Rect rect, Color color)
        {
            Vector2 bl = rect.BL();
            Vector2 br = rect.BR();
            Vector2 tr = rect.TR();
            Vector2 tl = rect.TL();

            Gizmos.color = color;
            Gizmos.DrawLine(bl, br);
            Gizmos.DrawLine(br, tr);
            Gizmos.DrawLine(tr, tl);
            Gizmos.DrawLine(tl, bl);
        }

        public static void DrawGizmos(this Rect rect, Vector2 pos, Color color)
        {
            Vector2 bl = rect.BL();
            Vector2 br = rect.BR();
            Vector2 tr = rect.TR();
            Vector2 tl = rect.TL();

            Gizmos.color = color;
            Gizmos.DrawLine(pos + bl, pos + br);
            Gizmos.DrawLine(pos + br, pos + tr);
            Gizmos.DrawLine(pos + tr, pos + tl);
            Gizmos.DrawLine(pos + tl, pos + bl);
        }

        public static Vector2 BL(this Rect rect)
        {
            Vector2 bl = new Vector2(rect.x, rect.y);
            return bl;
        }

        public static Vector2 BR(this Rect rect)
        {
            Vector2 bl = rect.BL();
            bl.x += rect.width;
            return bl;
        }

        public static Vector2 TR(this Rect rect)
        {
            Vector2 br = rect.BR();
            br.y += rect.height;
            return br;
        }

        public static Vector2 TL(this Rect rect)
        {
            Vector2 bl = rect.BL();
            bl.y += rect.height;
            return bl;
        }
        #endregion
    }
}



