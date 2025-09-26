using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RX.Utils
{
    public static class ListUtils
    {
        public delegate bool SelectListDel<T>(T item);
        public delegate bool FilterListDel<T>(T item);

        public static void Clean<T>(this List<T> l)
        {
            List<int> toRemove = null;
            for (int i = 0; i < l.Count; i++)
            {
                T obj = l[i];
                if (obj == null)
                {
                    if (toRemove == null)
                        toRemove = new List<int>();
                    toRemove.Add(i);
                }
            }

            if (toRemove != null && toRemove.Count > 0)
            {
                foreach (int index in toRemove)
                {
                    l.RemoveAt(index);
                }
            }
        }

        public static List<T> GetList<T>(this Queue<T> q)
        {
            List<T> list = new List<T>();
            if (q.Count == 0)
                return list;

            foreach (T qe in q)
            {
                list.Add(qe);
            }

            return list;
        }

        public static void Add<T>(this Queue<T> q, List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
                q.Enqueue(list[i]);
        }

        public static void Randomize<T>(this List<T> l)
        {
            List<T> newList = new List<T>();
            while (l.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, l.Count);
                newList.Add(l[index]);
                l.RemoveAt(index);
            }
            l.AddRange(newList);
        }

        public static List<T> ClipToSize<T>(this List<T> list, int size)
        {
            if (size > list.Count)
                return new List<T>(list);

            List<T> newList = new List<T>();
            for (int i = 0; i < size; i++)
                newList.Add(list[i]);

            return newList;
        }

        public static List<T> FilterTo<T>(this List<T> list, FilterListDel<T> filterMeth)
        {
            List<T> temp = new List<T>();
            for (int i = 0; i < list.Count; i++)
            {
                if (filterMeth(list[i]))
                    temp.Add(list[i]);
            }

            return temp;
        }

        public static T Select<T>(this List<T> list, SelectListDel<T> selectMeth)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (selectMeth(list[i]))
                    return list[i];
            }

            return default(T);
        }

        public static T Last<T>(this List<T> list)
        {
            if (list.Count == 0)
                return default(T);

            T obj = list[list.Count - 1];
            return obj;
        }

        public static void Filter<T>(this List<T> list, FilterListDel<T> filterMeth)
        {
            List<T> temp = new List<T>(list);
            list.Clear();
            for (int i = 0; i < temp.Count; i++)
            {
                T to = temp[i];
                if (filterMeth(to))
                    list.Add(to);
            }
        }
    }
}


