using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EasyWiFi.Core;

namespace EasyWiFi.ClientControls
{
    [AddComponentMenu("EasyWiFiController/Client/UserControls/Switch")]
    public class SwitchButtonClientController : MonoBehaviour, IClientController
    {

        public string controlName = "Button1";
        public Sprite buttonPressedSprite;
        [Tooltip("To prevent rapid switching on/off this float controls the fastest you want to be able to press the button twice. After the first press it is locked for pressing again for this amount of time")]
        [Range(0.1f, 10f)]
        public float fastestPressRate = 1.0f;

        ButtonControllerType button;
        Image currentImage;
        Sprite buttonRegularSprite;
        string buttonKey;
        Rect screenPixelsRect;
        int touchCount;
        bool pressed;
        bool momentartyLock;

        // Use this for initialization
        void Awake()
        {
            buttonKey = EasyWiFiController.registerControl(EasyWiFiConstants.CONTROLLERTYPE_BUTTON, controlName);
            button = (ButtonControllerType)EasyWiFiController.controllerDataDictionary[buttonKey];
            currentImage = gameObject.GetComponent<Image>();
            buttonRegularSprite = currentImage.sprite;

        }

        void Start()
        {
            screenPixelsRect = EasyWiFiUtilities.GetScreenRect(currentImage.rectTransform);
        }

        //here we grab the input and map it to the data list
        void Update()
        {
            mapInputToDataStream();
        }

        void resetLock()
        {
            momentartyLock = false;
        }

        public void mapInputToDataStream()
        {

            //this is a switch so don't reset to default values

            //mouse
            if (Input.GetKey(KeyCode.Mouse0))
            {
                if (Input.mousePosition.x >= screenPixelsRect.x &&
                        Input.mousePosition.x <= (screenPixelsRect.x + screenPixelsRect.width) &&
                        Input.mousePosition.y >= screenPixelsRect.y &&
                        Input.mousePosition.y <= (screenPixelsRect.y + screenPixelsRect.height))
                {
                    if (!momentartyLock)
                    {
                        if (pressed)
                            pressed = false;
                        else
                            pressed = true;

                        //don't allow rapid switching
                        momentartyLock = true;
                        Invoke("resetLock", fastestPressRate);
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
                        if (!momentartyLock)
                        {

                            if (pressed)
                                pressed = false;
                            else
                                pressed = true;

                            //don't allow rapid switching
                            momentartyLock = true;
                            Invoke("resetLock", fastestPressRate);
                            break;
                        }

                        
                    }
                }
            }

            //show the correct image
            if (pressed)
            {
                button.BUTTON_STATE_IS_PRESSED = true;
                currentImage.sprite = buttonPressedSprite;
            }
            else
            {
                button.BUTTON_STATE_IS_PRESSED = false;
                currentImage.sprite = buttonRegularSprite;

            }

        }

    }

}