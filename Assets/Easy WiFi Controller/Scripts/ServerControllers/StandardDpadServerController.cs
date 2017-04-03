using UnityEngine;
using System.Collections;
using EasyWiFi.Core;
using System;

namespace EasyWiFi.ServerControls
{

    [AddComponentMenu("EasyWiFiController/Server/UserControls/Standard Dpad")]
    public class StandardDpadServerController : MonoBehaviour, IServerController
    {

        public string control = "DPad1";
        public EasyWiFiConstants.PLAYER_NUMBER player = EasyWiFiConstants.PLAYER_NUMBER.Player1;
        public EasyWiFiConstants.AXIS dPadHorizontal = EasyWiFiConstants.AXIS.XAxis;
        public EasyWiFiConstants.AXIS dPadVertical = EasyWiFiConstants.AXIS.YAxis;
        public EasyWiFiConstants.ACTION_TYPE action = EasyWiFiConstants.ACTION_TYPE.Position;
        public float sensitivity = .01f;

        //runtime variables
        DpadControllerType[] dPad = new DpadControllerType[EasyWiFiConstants.MAX_CONTROLLERS];
        int currentNumberControllers = 0;
        Vector3 actionVector3;
        float horizontal;
        float vertical;

        void OnEnable()
        {
            EasyWiFiController.On_ConnectionsChanged += checkForNewConnections;

            //do one check at the beginning just in case we're being spawned after startup and after the callbacks
            //have already been called
            if (dPad[0] == null && EasyWiFiController.lastConnectedPlayerNumber >= 0)
            {
                EasyWiFiUtilities.checkForClient(control, (int)player, ref dPad, ref currentNumberControllers);
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
                if (dPad[i] != null && dPad[i].serverKey != null && dPad[i].logicalPlayerNumber != EasyWiFiConstants.PLAYERNUMBER_DISCONNECTED)
                {
                    mapDataStructureToAction(i);
                }
            }
        }


        public void mapDataStructureToAction(int index)
        {
            //reset values
            horizontal = 0f;
            vertical = 0f;

            //left/right
            if (dPad[index].DPAD_LEFT_PRESSED)
                horizontal = -1f;
            if (dPad[index].DPAD_RIGHT_PRESSED)
                horizontal = 1f;

            //up/down
            if (dPad[index].DPAD_DOWN_PRESSED)
                vertical = -1f;
            if (dPad[index].DPAD_UP_PRESSED)
                vertical = 1f;

            horizontal *= sensitivity;
            vertical *= sensitivity;
            actionVector3 = EasyWiFiUtilities.getControllerVector3(horizontal, vertical, dPadHorizontal, dPadVertical);

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

        public void checkForNewConnections(bool isConnect, int playerNumber)
        {
            EasyWiFiUtilities.checkForClient(control, (int)player, ref dPad, ref currentNumberControllers);
        }
    }

}
