//using RX.AI.Actor;
using System.Collections.Generic;

namespace RX.AI.KBB
{
    public class KBBList<T>
    {
        public List<KnowledgeBasedBehavior<T>> kbList = new List<KnowledgeBasedBehavior<T>>();

        public KnowledgeBasedBehavior<T> Selected { get; set; }

        //string desc = "";

        public void UpdateList(T ai)
        {
            if (kbList.Count < 1)
                return;

            // let the current executing rule finish
            if (Selected != null && Selected.executing)
                return;

            // update the utilities
            UpdateKBB(ai);

            // select a new one
            SelectKBB(ai);

            if (Selected != null)
                Selected.Execute(ai);
        }

        void UpdateKBB(T ai)
        {
            foreach (KnowledgeBasedBehavior<T> kb in kbList)
                kb.UpdateUtility(ai);

            kbList.Sort((a, b) => { return -a.utility.CompareTo(b.utility); });
        }

        void SelectKBB(T ai)
        {
            //desc = "MTP: ";
            //for (int i = 0; i < kbList.Count; i++)
            //{
            //    KnowledgeBasedBehavior kbb = kbList[i];
            //    desc += "(" + kbb.name + ") " + kbb.utility + " ";
            //}
            //Debug.Log(desc);

            if (kbList[0].utility <= 0)
                Selected = null;
            else
                Selected = kbList[0];
        }
    }
}
