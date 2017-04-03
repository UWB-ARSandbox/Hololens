using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EasyWiFi.Core;

namespace EasyWiFi.ClientControls
{
    [AddComponentMenu("EasyWiFiController/Client/UserControls/Dpad")]
    public class DpadClientController : MonoBehaviour, IClientController
    {

        public string controlName = "DPad1";
        public float lowThreshold = .2f;

        DpadControllerType dPad;
        Image dPadImage;
        string dPadKey;
        Rect screenPixelsRect;
        int touchCount;
        float normalizeFactorX, normalizeFactorY;

        // Use this for initialization
        void Awake()
        {
            dPadKey = EasyWiFiController.registerControl(EasyWiFiConstants.CONTROLLERTYPE_DPAD, controlName);
            dPad = (DpadControllerType)EasyWiFiController.controllerDataDictionary[dPadKey];
            dPadImage = gameObject.GetComponent<Image>();
        }

        void Start()
        {
            screenPixelsRect = EasyWiFiUtilities.GetScreenRect(dPadImage.rectTransform);
            normalizeFactorX = screenPixelsRect.width / 2;
            normalizeFactorY = screenPixelsRect.height / 2;
        }

        //here we grab the input and map it to the data list
        void Update()
        {
            mapInputToDataStream();
        }

        public void mapInputToDataStream()
        {
            float x,y;
            Vector2 movement;

            //reset to default values;
            //touch count is 0
            dPad.DPAD_DOWN_PRESSED = false;
            dPad.DPAD_LEFT_PRESSED = false;
            dPad.DPAD_RIGHT_PRESSED = false;
            dPad.DPAD_UP_PRESSED = false;
            movement.x = 0f; movement.y = 0f;

            //mouse
            if (Input.GetKey(KeyCode.Mouse0))
            {
                if (Input.mousePosition.x >= screenPixelsRect.x &&
                        Input.mousePosition.x <= (screenPixelsRect.x + screenPixelsRect.width) &&                         
                        Input.mousePosition.y >= screenPixelsRect.y &&
                        Input.mousePosition.y <= (screenPixelsRect.y + screenPixelsRect.height))
                {
                    movement.x = Input.mousePosition.x - screenPixelsRect.center.x;
                    movement.y = Input.mousePosition.y - screenPixelsRect.center.y;
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

                        movement.x = touch.position.x - screenPixelsRect.center.x;
                        movement.y = touch.position.y - screenPixelsRect.center.y;
                        break;
                    }
                }
            }

            //if both are zero don't bother we're already correct
            if (!(movement.x == 0f && movement.y == 0f))
            {
                //for transmission over the network (also introduces dead area and threshold for where values are 1
                x = movement.x / normalizeFactorX;
                y = movement.y / normalizeFactorY;

                //thresholds 
                x = (x > lowThreshold) ? 1 : x;
                x = (x < -lowThreshold) ? -1 : x;
                y = (y > lowThreshold) ? 1 : y;
                y = (y < -lowThreshold) ? -1 : y;

                if (x == 1)
                    dPad.DPAD_RIGHT_PRESSED = true;
                if (x == -1)
                    dPad.DPAD_LEFT_PRESSED = true;
                if (y == 1)
                    dPad.DPAD_UP_PRESSED = true;
                if (y == -1)
                    dPad.DPAD_DOWN_PRESSED = true;
            }
                
            
        }

    }

}