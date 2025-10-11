using RX.Utils;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace RX.Utils
{
    public class TimeMgr : MonoBehaviour
    {
        public static TimeMgr Singleton { get; private set; }

        [SerializeField]
        private TimeOfDayType timeOfDay = TimeOfDayType.MORNING;
        [SerializeField]
        private TextMeshProUGUI todTxt;

        [SerializeField]
        private Light2D sunLight;
        [SerializeField]
        private List<LightInfo> lightInfos = new List<LightInfo>();

        public TimeOfDayType TimeOfDay { get { return timeOfDay; } }


        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            UpdateTimeOfDay();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.N))
            {
                NextTOD();
            }
        }

        void UpdateTimeOfDay()
        {
            string str = timeOfDay.ToString().Capitalize();
            todTxt.text = str;

            LightInfo info = lightInfos[(int)timeOfDay];
            sunLight.color = info.color;
            sunLight.intensity = info.intensity;
        }

        public void NextTOD()
        {
            TimeOfDayType tod = GetNextTOD(timeOfDay);
            SetTOD(tod);
        }

        public void SetTOD(TimeOfDayType tod)
        {
            timeOfDay = tod;
            UpdateTimeOfDay();
        }

        TimeOfDayType GetNextTOD(TimeOfDayType tod)
        {
            int i = (int)tod;
            i = (i + 1) % System.Enum.GetValues(typeof(TimeOfDayType)).Length;
            return (TimeOfDayType)i;
        }

        [System.Serializable]
        public class LightInfo
        {
            public string name;
            public Color color;
            public float intensity;
        }
    }

    public enum TimeOfDayType
    {
        MORNING = 0,
        AFTERNOON,
        EVENING,
        NIGHT,
    }
}


