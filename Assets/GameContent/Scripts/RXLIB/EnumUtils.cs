using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RX.Utils
{
    public static class EnumUtils
    {
        public static int GetRandomEnum(Type et)
        {
            Array values = Enum.GetValues(et);
            return UnityEngine.Random.Range(0, values.Length);
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

    }
}


