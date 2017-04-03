using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EasyWiFi.Core;

namespace EasyWiFi.ClientControls
{
    [AddComponentMenu("EasyWiFiController/Client/UserControls/PinchZoom Touchpad")]
    public class PinchZoomTouchpadClientController : MonoBehaviour, IClientController
    {

        public string controlName = "PinchZoom1";

        PinchZoomTouchpadControllerType pinchZoom;
        Image pinchZoomImage;
        string pinchZoomKey;
        Rect screenPixelsRect;
        int touchCount;
        int touchMode = 0; //0 is no touch, 1 is one touch, 2 is two touch (zoom), 3 is tilt/pan
        int lastFrameTouchMode = 0;
        float currentZoomFactor = 1f;
        float currentZoomDistance = 0f;
        float lastFrameZoomDistance = 0f;
        float normalizeFactorX, normalizeFactorY;
        float touchZoomMultiplier = 1f;

        // Use this for initialization
        void Awake()
        {
            pinchZoomKey = EasyWiFiController.registerControl(EasyWiFiConstants.CONTROLLERTYPE_PINCHZOOMTOUCHPAD, controlName);
            pinchZoom = (PinchZoomTouchpadControllerType)EasyWiFiController.controllerDataDictionary[pinchZoomKey];
            pinchZoomImage = gameObject.GetComponent<Image>();
        }

        void Start()
        {
            screenPixelsRect = EasyWiFiUtilities.GetScreenRect(pinchZoomImage.rectTransform);
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
            int numTouches;
            float x1, y1, x2, y2;

            Vector2 position1, position2;

            lastFrameZoomDistance = currentZoomDistance;
            lastFrameTouchMode = touchMode;

            //reset to default values;
            //touch count is 0
            pinchZoom.TOUCH_COUNT = 0;
            pinchZoom.TOUCH1_POSITION_HORIZONTAL = 0f;
            pinchZoom.TOUCH1_POSITION_VERTICAL = 0f;
            pinchZoom.TOUCH2_POSITION_HORIZONTAL = 0f;
            pinchZoom.TOUCH2_POSITION_VERTICAL = 0f;
            pinchZoom.ZOOM_FACTOR = currentZoomFactor;

            numTouches = 0; position1.x = 0f; position1.y = 0f; position2.x = 0; position2.y = 0; currentZoomDistance = 0f;

            //mouse/keyboard

            if (Input.GetAxis("Mouse ScrollWheel") != 0f)
            {
                //mouse is zooming so ignore clicks this time around
                //on touch this would be 2 finger gesture (pinch) so we simulate the touches
                numTouches = 2;
                position1.x = EasyWiFiConstants.SIMULATED_TOUCH;
                position1.y = EasyWiFiConstants.SIMULATED_TOUCH;
                position2.x = EasyWiFiConstants.SIMULATED_TOUCH;
                position2.y = EasyWiFiConstants.SIMULATED_TOUCH;
                currentZoomFactor += Input.GetAxis("Mouse ScrollWheel");
            }
            else if (Input.GetKey(KeyCode.Mouse2))
            {
                //middle mouse button
                //on touch this would be 2 finger gesture (pan/tilt) so we simulate the touches
                if (Input.mousePosition.x >= screenPixelsRect.x &&
                        Input.mousePosition.x <= (screenPixelsRect.x + screenPixelsRect.width) &&
                        Input.mousePosition.y >= screenPixelsRect.y &&
                        Input.mousePosition.y <= (screenPixelsRect.y + screenPixelsRect.height))
                {
                    numTouches = 3;
                    position2.x = EasyWiFiConstants.SIMULATED_TOUCH;
                    position2.y = EasyWiFiConstants.SIMULATED_TOUCH;
                    position1.x = Input.mousePosition.x - screenPixelsRect.x;
                    position1.y = Input.mousePosition.y - screenPixelsRect.y;
                }
            }
            else if (Input.GetKey(KeyCode.Mouse0))
            {
                if (Input.mousePosition.x >= screenPixelsRect.x &&
                        Input.mousePosition.x <= (screenPixelsRect.x + screenPixelsRect.width) &&
                        Input.mousePosition.y >= screenPixelsRect.y &&
                        Input.mousePosition.y <= (screenPixelsRect.y + screenPixelsRect.height))
                {
                    //since we have a touch also check control key and middle mouse button since this simulates 2nd touch for panning
                    if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                    {
                        numTouches = 3;
                        position2.x = EasyWiFiConstants.SIMULATED_TOUCH;
                        position2.y = EasyWiFiConstants.SIMULATED_TOUCH;
                    }
                    else
                    {
                        numTouches = 1;
                    }

                    position1.x = Input.mousePosition.x - screenPixelsRect.x;
                    position1.y = Input.mousePosition.y - screenPixelsRect.y;
                }

            }

            //touch
            //there are a couple of things to note about touch
            //1. fingers when get too close together will get treated as one touch (screens don't seem good enough to reliable report 2 touches until a good distance apart)
            //2. For this reason once we enter a mode we will stay with it until there is a frame with no touches
            //3. exceptions are one touch -> two touch and two touch -> three touch but not other way around
            
            touchCount = Input.touchCount;

            if (touchCount > 0)
            {
                //apparently the first touch can also be gotten through Unity's mouse API even though it's touch
                //Reset variables here since for this control we do care about multitouch
                numTouches = 0; position1.x = 0f; position1.y = 0f; position2.x = 0; position2.y = 0;

                for (int i = 0; i < touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);

                    //touch somewhere on control
                    if (touch.position.x >= screenPixelsRect.x &&
                            touch.position.x <= (screenPixelsRect.x + screenPixelsRect.width) &&
                            touch.position.y >= screenPixelsRect.y &&
                            touch.position.y <= (screenPixelsRect.y + screenPixelsRect.height))
                    {

                        if (numTouches == 0)
                        {
                            //first touch
                            position1.x = touch.position.x - screenPixelsRect.x;
                            position1.y = touch.position.y - screenPixelsRect.y;
                            numTouches = 1;

                            if (lastFrameTouchMode < 2)
                            {
                                //assign only if we weren't already second touch or zooming last frame
                                //otherwise we're locked in to the higher mode
                                touchMode = 1;
                            }
                        }
                        else if (numTouches == 1)
                        {
                            //second touch (zooming)
                            position2.x = touch.position.x - screenPixelsRect.x;
                            position2.y = touch.position.y - screenPixelsRect.y;
                            numTouches = 2;


                            //distance between touches (normalized)
                            currentZoomDistance = Mathf.Sqrt(
                                                             Mathf.Pow(((position1.x - position2.x) / normalizeFactorX),2) + 
                                                             Mathf.Pow(((position1.y - position2.y) / normalizeFactorY),2)
                                                             );

                            if (currentZoomDistance != 0f && lastFrameZoomDistance != 0f && lastFrameTouchMode < 3)
                            {
                                //only adjust the zoom factor if we've had data at least two frames
                                //and we're not in tilt mode
                                currentZoomFactor += ((currentZoomDistance - lastFrameZoomDistance) * touchZoomMultiplier);
                            }

                            if (lastFrameTouchMode < 3)
                            {
                                //only if we weren't already pan/tilt last frame
                                touchMode = 2;
                            }

                        }
                        else if (numTouches == 2)
                        {
                            //third touch
                            numTouches = 3;
                            touchMode = 3;

                            //there is a tricky problem with fingers getting too close together and this happens a lot
                            //with a 3 finger gesture as you drag

                            //this is third touch and we don't care about 4 or more so break out of touch input loop
                            break;
                        }
                        
                    }
                }
            }

            if (numTouches == 0 || numTouches == 1)
            {
                //if one or no touches reset the distance between touches
                currentZoomDistance = 0f;
                lastFrameZoomDistance = 0f;
            }

            //the real number of touches this frame is numtouches
            //however we want to lock into touchmode since sometimes finger's get too close together
            //for mouse touchMode stays 0
            if (numTouches < touchMode)
            {
                if (numTouches == 0)
                    touchMode = 0;
                else
                    numTouches = touchMode;
            }


            //if not touching we're already correct to send data
            if (numTouches > 0)
            {

                //for transmission over the network
                x1 = (position1.x != EasyWiFiConstants.SIMULATED_TOUCH) ? (position1.x / normalizeFactorX) : position1.x;
                y1 = (position1.y != EasyWiFiConstants.SIMULATED_TOUCH) ? (position1.y / normalizeFactorY) : position1.y;
                x2 = (position2.x != EasyWiFiConstants.SIMULATED_TOUCH) ? (position2.x / normalizeFactorX) : position2.x;
                y2 = (position2.y != EasyWiFiConstants.SIMULATED_TOUCH) ? (position2.y / normalizeFactorY) : position2.y;

                pinchZoom.TOUCH_COUNT = numTouches;
                pinchZoom.TOUCH1_POSITION_HORIZONTAL = x1;
                pinchZoom.TOUCH1_POSITION_VERTICAL = y1;
                pinchZoom.TOUCH2_POSITION_HORIZONTAL = x2;
                pinchZoom.TOUCH2_POSITION_VERTICAL = y2;
                pinchZoom.ZOOM_FACTOR = currentZoomFactor;
            }


        }

    }

}