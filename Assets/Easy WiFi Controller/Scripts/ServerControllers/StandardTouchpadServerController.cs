using UnityEngine;
using System.Collections;
using EasyWiFi.Core;
using System;

namespace EasyWiFi.ServerControls
{

    [AddComponentMenu("EasyWiFiController/Server/UserControls/Standard Touchpad")]
    public class StandardTouchpadServerController : MonoBehaviour, IServerController
    {

        public string control = "Touchpad1";
        public EasyWiFiConstants.PLAYER_NUMBER player = EasyWiFiConstants.PLAYER_NUMBER.Player1;
        public EasyWiFiConstants.AXIS touchpadHorizontal = EasyWiFiConstants.AXIS.XAxis;
        public EasyWiFiConstants.AXIS touchpadVertical = EasyWiFiConstants.AXIS.YAxis;
        public EasyWiFiConstants.ACTION_TYPE action = EasyWiFiConstants.ACTION_TYPE.Position;
        public float sensitivity = .01f;

        //runtime variables
        TouchpadControllerType[] touchpad = new TouchpadControllerType[EasyWiFiConstants.MAX_CONTROLLERS];
        int currentNumberControllers = 0;
        Vector3 actionVector3;
        float horizontal;
        float vertical;
        bool isTouching;
        float lastFrameHorizontal;
        float lastFrameVertical;
        bool lastFrameIsTouching;


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
            lastFrameHorizontal = horizontal;
            lastFrameVertical = vertical;
            lastFrameIsTouching = isTouching;

            horizontal = touchpad[index].POSITION_HORIZONTAL * sensitivity;
            vertical = touchpad[index].POSITION_VERTICAL * sensitivity;
            isTouching = touchpad[index].IS_TOUCHING;

            //only if we were touching both last frame and this
            if (isTouching && lastFrameIsTouching)
            {
                actionVector3 = EasyWiFiUtilities.getControllerVector3(horizontal - lastFrameHorizontal, vertical - lastFrameVertical, touchpadHorizontal, touchpadVertical);

                switch (action)
                {
                    case EasyWiFiConstants.ACTION_TYPE.Position:
                        transform.position += actionVector3;
                        break;
                    case EasyWiFiConstants.ACTION_TYPE.Rotation:
                        transform.Rotate(actionVector3, Space.World);
                        break;
                    case EasyWiFiConstants.ACTION_TYPE.LocalPosition:
                        transform.Translate(actionVector3);
                        break;
                    case EasyWiFiConstants.ACTION_TYPE.LocalRotation:
                        transform.Rotate(actionVector3);
                        break;
                    case EasyWiFiConstants.ACTION_TYPE.LocalScale:
                        transform.localScale += actionVector3;
                        break;
                    default:
                        Debug.Log("Invalid Action");
                        break;

                }
            }
        }

        public void checkForNewConnections(bool isConnect, int playerNumber)
        {
            EasyWiFiUtilities.checkForClient(control, (int)player, ref touchpad, ref currentNumberControllers);
        }
    }

}

