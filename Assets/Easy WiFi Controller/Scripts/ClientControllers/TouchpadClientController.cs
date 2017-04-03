using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EasyWiFi.Core;

namespace EasyWiFi.ClientControls
{
    [AddComponentMenu("EasyWiFiController/Client/UserControls/Touchpad")]
    public class TouchpadClientController : MonoBehaviour, IClientController
    {

        public string controlName = "Touchpad1";

        TouchpadControllerType touchpad;
        Image touchpadImage;
        string touchpadKey;
        Rect screenPixelsRect;
        int touchCount;
        float normalizeFactorX, normalizeFactorY;

        // Use this for initialization
        void Awake()
        {
            touchpadKey = EasyWiFiController.registerControl(EasyWiFiConstants.CONTROLLERTYPE_TOUCHPAD, controlName);
            touchpad = (TouchpadControllerType)EasyWiFiController.controllerDataDictionary[touchpadKey];
            touchpadImage = gameObject.GetComponent<Image>();
        }

        void Start()
        {
            screenPixelsRect = EasyWiFiUtilities.GetScreenRect(touchpadImage.rectTransform);
            normalizeFactorX = screenPixelsRect.width;
            normalizeFactorY = screenPixelsRect.height;
        }

        //here we grab the input and map it to the data list
        void Update()
        {
            mapInputToDataStream();
        }

        public void mapInputToDataStream()
        {
            float x, y;
            bool isTouching;
            Vector2 position;

            //reset to default values;
            //touch count is 0
            touchpad.POSITION_HORIZONTAL = 0f;
            touchpad.POSITION_VERTICAL = 0f;
            touchpad.IS_TOUCHING = false;
            position.x = 0f; position.y = 0f; isTouching = false;

            //mouse
            if (Input.GetKey(KeyCode.Mouse0))
            {
                if (Input.mousePosition.x >= screenPixelsRect.x &&
                        Input.mousePosition.x <= (screenPixelsRect.x + screenPixelsRect.width) &&
                        Input.mousePosition.y >= screenPixelsRect.y &&
                        Input.mousePosition.y <= (screenPixelsRect.y + screenPixelsRect.height))
                {
                    position.x = Input.mousePosition.x - screenPixelsRect.x;
                    position.y = Input.mousePosition.y - screenPixelsRect.y;
                    isTouching = true;
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

                        position.x = touch.position.x - screenPixelsRect.x;
                        position.y = touch.position.y - screenPixelsRect.y;
                        isTouching = true;
                        break;
                    }
                }
            }

            //if not touching we're already correct
            if (isTouching)
            {
                //for transmission over the network (also introduces dead area and threshold for where values are 1
                x = position.x / normalizeFactorX;
                y = position.y / normalizeFactorY;

                touchpad.POSITION_HORIZONTAL = x;
                touchpad.POSITION_VERTICAL = y;
                touchpad.IS_TOUCHING = isTouching;
            }


        }

    }

}