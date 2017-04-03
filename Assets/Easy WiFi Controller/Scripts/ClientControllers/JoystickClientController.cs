using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EasyWiFi.Core;

namespace EasyWiFi.ClientControls
{
    [AddComponentMenu("EasyWiFiController/Client/UserControls/Joystick")]
    public class JoystickClientController : MonoBehaviour, IClientController
    {

        public string controlName = "Joystick1";
        public float highThreshold = .6f;
        public float lowThreshold = .1f;
        public Image joystickImageForeground;

        JoystickControllerType joystick;
        Image joystickImageBackground;
        string joystickKey;
        Rect screenPixelsRect;
        int touchCount;
        Vector3 defaultNubPosition;
        Vector2 tmpVector;
        float normalizeFactorX, normalizeFactorY;

        // Use this for initialization
        void Awake()
        {
            joystickKey = EasyWiFiController.registerControl(EasyWiFiConstants.CONTROLLERTYPE_JOYSTICK, controlName);
            joystick = (JoystickControllerType)EasyWiFiController.controllerDataDictionary[joystickKey];
            joystickImageBackground = gameObject.GetComponent<Image>();


        }

        void Start()
        {
            screenPixelsRect = EasyWiFiUtilities.GetScreenRect(joystickImageBackground.rectTransform);
            defaultNubPosition = joystickImageForeground.transform.position;
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
            float x, y;
            Vector3 nubMovement;

            //reset to default values;
            joystick.JOYSTICK_HORIZONTAL = 0f;
            joystick.JOYSTICK_VERTICAL = 0f;
            nubMovement.x = 0f; nubMovement.y = 0f; nubMovement.z = 0f;

            //mouse
            if (Input.GetKey(KeyCode.Mouse0))
            {
                if (Input.mousePosition.x >= screenPixelsRect.x &&
                        Input.mousePosition.x <= (screenPixelsRect.x + screenPixelsRect.width) &&
                        Input.mousePosition.y >= screenPixelsRect.y &&
                        Input.mousePosition.y <= (screenPixelsRect.y + screenPixelsRect.height))
                {
                    nubMovement.x = Input.mousePosition.x - defaultNubPosition.x;
                    nubMovement.y = Input.mousePosition.y - defaultNubPosition.y;

                    
                    if (Mathf.Sqrt(Mathf.Pow(nubMovement.x,2) + Mathf.Pow(nubMovement.y,2)) > normalizeFactorX)
                    {
                        //a joystick is a circular control and a sprite is a square graphic
                        //we have touched in the sprite but we need to make sure we actually touched in the circle
                        //if not reset so no touch
                        nubMovement.x = 0;
                        nubMovement.y = 0;
                    }
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
                        nubMovement.x = touch.position.x - screenPixelsRect.center.x;
                        nubMovement.y = touch.position.y - screenPixelsRect.center.y;

                        if (Mathf.Sqrt(Mathf.Pow(nubMovement.x, 2) + Mathf.Pow(nubMovement.y, 2)) > normalizeFactorX)
                        {
                            //a joystick is a circular control and a sprite is a square graphic
                            //we have touched in the sprite but we need to make sure we actually touched in the circle
                            //if not reset so no touch
                            nubMovement.x = 0;
                            nubMovement.y = 0;
                        }


                        break;
                    }
                }
            }

            //if both are zero don't bother we're already correct
            if (!(nubMovement.x == 0f && nubMovement.y == 0f))
            {

                //for transmission over the network (also introduces dead area and threshold for where values are 1
                x = nubMovement.x / normalizeFactorX;
                y = nubMovement.y / normalizeFactorY;

                //thresholds (should also be circle shaped not square)

                if (Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(y, 2)) < lowThreshold)
                {
                    //the joystick hasn't met the low threshold reset to zero
                    x = 0;
                    y = 0;
                }

                if (Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(y, 2)) > highThreshold)
                {
                    //the joystick has exceeded the high threshold so we need to make the distance 1 (normalize) in the same direction
                    tmpVector.x = x;
                    tmpVector.y = y;
                    tmpVector.Normalize();
                    x = tmpVector.x;
                    y = tmpVector.y;
                }

                joystick.JOYSTICK_HORIZONTAL = x;
                joystick.JOYSTICK_VERTICAL = y;
            } 

            joystickImageForeground.rectTransform.position = defaultNubPosition + nubMovement;
        }

    }

}