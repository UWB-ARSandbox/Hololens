using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EasyWiFi.Core;

namespace EasyWiFi.ClientControls
{
    [AddComponentMenu("EasyWiFiController/Client/UserControls/Slider")]
    public class SliderClientController : MonoBehaviour, IClientController
    {

        public string controlName = "Slider1";
        public EasyWiFiConstants.SLIDER_TYPE orientation = EasyWiFiConstants.SLIDER_TYPE.Horizonal;
        public Image sliderImageForeground;

        SliderControllerType slider;
        Image sliderImageBackground;
        string sliderKey;
        Rect screenPixelsRect;
        int touchCount;
        Vector3 defaultNubPosition;
        float normalizeFactor;

        // Use this for initialization
        void Awake()
        {
            sliderKey = EasyWiFiController.registerControl(EasyWiFiConstants.CONTROLLERTYPE_SLIDER, controlName);
            slider = (SliderControllerType)EasyWiFiController.controllerDataDictionary[sliderKey];
            sliderImageBackground = gameObject.GetComponent<Image>();
        }

        void Start()
        {
            screenPixelsRect = EasyWiFiUtilities.GetScreenRect(sliderImageBackground.rectTransform);
            defaultNubPosition = sliderImageForeground.transform.position;
            
            if (orientation == EasyWiFiConstants.SLIDER_TYPE.Horizonal)
                normalizeFactor = screenPixelsRect.width / 2;
            else
                normalizeFactor = screenPixelsRect.height / 2;
        }

        //here we grab the input and map it to the data list
        void Update()
        {
            mapInputToDataStream();
        }

        public void mapInputToDataStream()
        {
            float x, y;
            Vector3 nubMovement;

            //reset to default values;
            slider.SLIDER_VALUE = 0f;
            nubMovement.x = 0f; nubMovement.y = 0f; nubMovement.z = 0f;

            //mouse
            if (Input.GetKey(KeyCode.Mouse0))
            {
                if (Input.mousePosition.x >= screenPixelsRect.x &&
                        Input.mousePosition.x <= (screenPixelsRect.x + screenPixelsRect.width) &&
                        Input.mousePosition.y >= screenPixelsRect.y &&
                        Input.mousePosition.y <= (screenPixelsRect.y + screenPixelsRect.height))
                {
                    if (orientation == EasyWiFiConstants.SLIDER_TYPE.Horizonal)
                        nubMovement.x = Input.mousePosition.x - defaultNubPosition.x;
                    else
                        nubMovement.y = Input.mousePosition.y - defaultNubPosition.y;                                      
                }

            }

            //touch
            touchCount = Input.touchCount;

            if (touchCount > 0)
            {
                for (int i = 0; i < touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);

                    //touch somewhere on control
                    if (touch.position.x >= screenPixelsRect.x &&
                            touch.position.x <= (screenPixelsRect.x + screenPixelsRect.width) &&
                            touch.position.y >= screenPixelsRect.y &&
                            touch.position.y <= (screenPixelsRect.y + screenPixelsRect.height))
                    {
                        //moving the nub
                        if (orientation == EasyWiFiConstants.SLIDER_TYPE.Horizonal)
                            nubMovement.x = touch.position.x - screenPixelsRect.center.x;
                        else
                            nubMovement.y = touch.position.y - screenPixelsRect.center.y; 
                        nubMovement.z = 0;
                        break;
                    }
                }
            }

            //if both are zero don't bother we're already correct
            if (!(nubMovement.x == 0f && nubMovement.y == 0f))
            {

                //for transmission over the network (also introduces dead area and threshold for where values are 1
                x = nubMovement.x / normalizeFactor;
                y = nubMovement.y / normalizeFactor;

                if (orientation == EasyWiFiConstants.SLIDER_TYPE.Horizonal)
                    slider.SLIDER_VALUE = x;
                else
                    slider.SLIDER_VALUE = y; 
            }

            sliderImageForeground.rectTransform.position = defaultNubPosition + nubMovement;
        }

    }

}