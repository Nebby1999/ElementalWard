using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ElementalWard
{
    public class LoadingScreenController : MonoBehaviour
    {
        public TextMeshProUGUI elipsis;
        public float intervalBetweenDots;
        public RectTransform circleSpinner;
        public float circleSpinnerVelocity;

        private float elipsisStopwatch;
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            if(circleSpinner)
            {
                var z = circleSpinner.rotation.eulerAngles.z;
                Quaternion newRotation = Quaternion.Euler(0, 0, z + circleSpinnerVelocity * Time.deltaTime);
                circleSpinner.rotation = newRotation;
            }
            if(elipsis)
            {
                var txt = elipsis.text;
                elipsisStopwatch += Time.deltaTime;
                if(elipsisStopwatch > intervalBetweenDots)
                {
                    elipsisStopwatch = 0;
                    switch(txt)
                    {
                        case "":
                            txt = ".";
                            break;
                        case ".":
                            txt = "..";
                            break;
                        case "..":
                            txt = "...";
                            break;
                        case "...":
                            txt = "";
                            break;
                    }
                }
                elipsis.text = txt;
            }
        }
    }
}
