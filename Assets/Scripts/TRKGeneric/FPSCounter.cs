using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TRKGeneric
{
    [RequireComponent(typeof(Text))]
    public class FPSCounter : MonoBehaviour
    {
        public float timeToUpdate = 0.5f;

        private Text displayText;
        private float timer;
        private List<float> frameTimes = new List<float>();
    
        private void Start()
        {
            displayText = GetComponent<Text>();
        }
        private void Update()
        {
            frameTimes.Add(1f / Time.unscaledDeltaTime);
            if (timer < Time.time)
            {
                displayText.text = GetAverageFPS().ToString();

                frameTimes.Clear();                
                timer = Time.time + timeToUpdate;
            }
        }       
        private int GetAverageFPS()
        {
            float total = 0;
            for(int i=0;i<frameTimes.Count;i++)
            {
                total += frameTimes[i];
            }
            return (int)total / frameTimes.Count;
        }
    }
}
