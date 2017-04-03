using UnityEngine;
using System.Collections;
using EasyWiFi.Core;
using System;

namespace EasyWiFi.ServerControls
{

    [AddComponentMenu("EasyWiFiController/Server/UserControls/PinchZoom Touchpad")]
    public class CameraPinchZoomTouchpadServerController : MonoBehaviour, IServerController
    {

        public string control = "PinchZoom1";
        public EasyWiFiConstants.PLAYER_NUMBER player = EasyWiFiConstants.PLAYER_NUMBER.Player1;        
        [Tooltip("Three finger touch will do the following action")]
        public EasyWiFiConstants.SECOND_TOUCH_TYPE secondTouchType = EasyWiFiConstants.SECOND_TOUCH_TYPE.Tilt;
        public float panSensitivity = 2f;
        public float tiltSensitivity = 50f;
        public float zoomSensitivity = 4f;


        //runtime variables
        PinchZoomTouchpadControllerType[] touchpad = new PinchZoomTouchpadControllerType[EasyWiFiConstants.MAX_CONTROLLERS];
        int currentNumberControllers = 0;
        Vector3 actionVector3;
        float horizontal;
        float vertical;
        float zoomFactor;
        int numTouches;
        float lastFrameHorizontal;
        float lastFrameVertical;
        int lastFrameNumTouches;
        float lastFrameZoomFactor;

        void OnEnable()
        {
            EasyWiFiController.On_ConnectionsChanged += checkForNewConnections;

            //do one check at the beginning just in case we're being spawned after startup and after the callbacks
            //have already been called
            if (touchpad[0] == null && EasyWiFiController.lastConnectedPlayerNumber >= 0)
            {
                EasyWiFiUtilities.checkForClient(control, (int)player, ref touchpad, ref currentNumberControllers);
            }
        }

        void OnDestroy()
        {
            EasyWiFiController.On_ConnectionsChanged -= checkForNewConnections;
        }

        // Update is called once per frame
        void Update()
        {
            //iterate over the current number of connected controllers
            for (int i = 0; i < currentNumberControllers; i++)
            {
                if (touchpad[i] != null && touchpad[i].serverKey != null && touchpad[i].logicalPlayerNumber != EasyWiFiConstants.PLAYERNUMBER_DISCONNECTED)
                {
                    mapDataStructureToAction(i);
                }
            }
        }



        public void mapDataStructureToAction(int index)
        {
            lastFrameNumTouches = numTouches;
            lastFrameZoomFactor = zoomFactor;
            lastFrameHorizontal = horizontal;
            lastFrameVertical = vertical;

            numTouches = touchpad[index].TOUCH_COUNT;
            zoomFactor = touchpad[index].ZOOM_FACTOR * zoomSensitivity;
            horizontal = touchpad[index].TOUCH1_POSITION_HORIZONTAL;
            vertical = touchpad[index].TOUCH1_POSITION_VERTICAL;

            //only if we were touching both last frame and this
            if (numTouches > 0 && lastFrameNumTouches > 0)
            {

                if (numTouches == 1)
                {
                    //panning
                    if (secondTouchType == EasyWiFiConstants.SECOND_TOUCH_TYPE.Tilt)
                    {
                        //panning in the camera context should be local position moves (x and y axis)
                        actionVector3 = EasyWiFiUtilities.getControllerVector3((horizontal - lastFrameHorizontal) * panSensitivity, (vertical - lastFrameVertical) * panSensitivity, EasyWiFiConstants.AXIS.XAxis, EasyWiFiConstants.AXIS.YAxis);
                        transform.Translate(actionVector3);
                    }
                    //tilting
                    else
                    {
                        //tilting in the camera context should be local rotation moves (y and x axis)
                        actionVector3 = EasyWiFiUtilities.getControllerVector3((horizontal - lastFrameHorizontal) * tiltSensitivity, (vertical - lastFrameVertical) * tiltSensitivity, EasyWiFiConstants.AXIS.YAxis, EasyWiFiConstants.AXIS.XAxis);
                        transform.Rotate(actionVector3);
                    }
                }
                else if (numTouches == 2)
                {
                    //zooming in the camera
                    actionVector3 = new Vector3(0f, 0f, zoomFactor - lastFrameZoomFactor);
                    transform.Translate(actionVector3);
                }                
                else if (numTouches == 3)
                {
                    //panning
                    if (secondTouchType == EasyWiFiConstants.SECOND_TOUCH_TYPE.Pan)
                    {
                        //panning in the camera context should be local position moves (x and y axis)
                        actionVector3 = EasyWiFiUtilities.getControllerVector3((horizontal - lastFrameHorizontal) * panSensitivity, (vertical - lastFrameVertical) * panSensitivity, EasyWiFiConstants.AXIS.XAxis, EasyWiFiConstants.AXIS.YAxis);
                        transform.Translate(actionVector3);
                    }
                    //tilting
                    else
                    {
                        //tilting in the camera context should be local rotation moves (y and x axis)
                        actionVector3 = EasyWiFiUtilities.getControllerVector3((horizontal - lastFrameHorizontal) * tiltSensitivity, (vertical - lastFrameVertical) * tiltSensitivity, EasyWiFiConstants.AXIS.YAxis, EasyWiFiConstants.AXIS.XAxis);
                        transform.Rotate(actionVector3);
                    }
                    
                }
            }
        }

        public void checkForNewConnections(bool isConnect, int playerNumber)
        {
            EasyWiFiUtilities.checkForClient(control, (int)player, ref touchpad, ref currentNumberControllers);
        }
    }

}

