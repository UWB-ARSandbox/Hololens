using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EasyWiFi.Core;

namespace EasyWiFi.ClientControls
{
    [AddComponentMenu("EasyWiFiController/Client/Connection/ConnectionWidget")]
    public class ConnectionClientController : MonoBehaviour
    {
        public Image connectImage;
        public Image disconnectImage;
        public Image wifiImage;                
        Image backgroundImage;

        Rect screenPixelsRect;

        Color clear = new Color(1f, 1f, 1f, 0f);
        Color white = new Color(1f, 1f, 1f, 1f);
        Color grey = new Color(.5f, .5f, .5f, 1f);
        Color temp = new Color(1f, 1f, 1f, 1f);

        bool isDescending;
        bool justStateChange;


        int touchCount;

        // Use this for initialization
        void Awake()
        {
            backgroundImage = gameObject.GetComponent<Image>();
        }

        void Start()
        {
            screenPixelsRect = EasyWiFiUtilities.GetScreenRect(backgroundImage.rectTransform);
        }

        //here we grab the input and map it to the data list
        void Update()
        {

            //show/Hide the correct connect images based on state
            if (EasyWiFiController.clientState == EasyWiFiConstants.CURRENT_CLIENT_STATE.NotConnected)
            {
                //hide wifi, grey out disconnect, and enable (white color) connect
                connectImage.color = white;
                disconnectImage.color = grey;
                wifiImage.color = clear;
            }
            else if (EasyWiFiController.clientState == EasyWiFiConstants.CURRENT_CLIENT_STATE.SendingControllerData)
            {
                //we're connected show wifi solid, grey out connect, and enable (white color) disconnect
                connectImage.color = grey;
                disconnectImage.color = white;
                wifiImage.color = white;
            }
            else
            {
                //we're in process animate wifi (alpha), grey out both connect and disconnect
                connectImage.color = grey;
                disconnectImage.color = grey;
                blink(wifiImage);
            }

            //check for mouse connect or disconnect

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (Input.mousePosition.x >= screenPixelsRect.x &&
                            Input.mousePosition.x <= (screenPixelsRect.x + screenPixelsRect.width) &&
                            Input.mousePosition.y >= screenPixelsRect.y &&
                            Input.mousePosition.y <= (screenPixelsRect.y + screenPixelsRect.height))
                    {
                        connectOrDisconnect();
                    }
                }
            }


            //check for touch of connect or disconnect
            touchCount = Input.touchCount;

            if (touchCount > 0)
            {
                for (int i = 0; i < touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);

                    //touch somewhere on connect on widget
                    if (touch.position.x >= screenPixelsRect.x &&
                            touch.position.x <= (screenPixelsRect.x + screenPixelsRect.width) &&
                            touch.position.y >= screenPixelsRect.y &&
                            touch.position.y <= (screenPixelsRect.y + screenPixelsRect.height))
                    {
                        connectOrDisconnect();
                    }

                }
            }
        }

        public void connectOrDisconnect()
        {
            if (!justStateChange && EasyWiFiController.clientState == EasyWiFiConstants.CURRENT_CLIENT_STATE.NotConnected)
            {
                //not connected currently so try to connect
                justStateChange = true;
                EasyWiFiController.checkForServer();
                Invoke("resetDisconnection", 2f);
                
            }
            if (!justStateChange && EasyWiFiController.clientState == EasyWiFiConstants.CURRENT_CLIENT_STATE.SendingControllerData)
            {
                //connected currently so disconnect
                justStateChange = true;
                EasyWiFiController.sendDisconnect(EasyWiFiController.createDisconnectMessage());
                Invoke("resetDisconnection", 2f);
            }

        }

        void resetDisconnection()
        {
            justStateChange = false;
        }

        void blink(Image image)
        {
            //figure out if should increase or decrease alpha
            if (image.color.a <= .05f)
                isDescending = false;

            if (image.color.a >= .95f)
                isDescending = true;
            
            //adjust temp's alpha
            if (isDescending)
                temp.a -= .01f;
            else
                temp.a += .01f;

            //change images color
            image.color = temp;
           

        }

    }

}