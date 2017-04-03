using UnityEngine;
using System.Collections;
using EasyWiFi.Core;
using System;

namespace EasyWiFi.ServerControls
{

    [AddComponentMenu("EasyWiFiController/Server/UserControls/Standard Joystick")]
    public class StandardJoystickServerController : MonoBehaviour, IServerController
    {

        public string control = "Joystick1";
        public EasyWiFiConstants.PLAYER_NUMBER player = EasyWiFiConstants.PLAYER_NUMBER.Player1;
        public EasyWiFiConstants.AXIS joystickHorizontal = EasyWiFiConstants.AXIS.XAxis;
        public EasyWiFiConstants.AXIS joystickVertical = EasyWiFiConstants.AXIS.YAxis;
        public EasyWiFiConstants.ACTION_TYPE action = EasyWiFiConstants.ACTION_TYPE.Position;
        public float sensitivity = .01f;

        //runtime variables
        JoystickControllerType[] joystick = new JoystickControllerType[EasyWiFiConstants.MAX_CONTROLLERS];
        int currentNumberControllers = 0;
        Vector3 actionVector3;
        float horizontal;
        float vertical;


        void OnEnable()
        {
            EasyWiFiController.On_ConnectionsChanged += checkForNewConnections;

            //do one check at the beginning just in case we're being spawned after startup and after the callbacks
            //have already been called
            if (joystick[0] == null && EasyWiFiController.lastConnectedPlayerNumber >= 0)
            {
                EasyWiFiUtilities.checkForClient(control, (int)player, ref joystick, ref currentNumberControllers);
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
                if (joystick[i] != null && joystick[i].serverKey != null && joystick[i].logicalPlayerNumber != EasyWiFiConstants.PLAYERNUMBER_DISCONNECTED)
                {
                    mapDataStructureToAction(i);
                }
            }
        }



        public void mapDataStructureToAction(int index)
        {
            horizontal = joystick[index].JOYSTICK_HORIZONTAL * sensitivity;
            vertical = joystick[index].JOYSTICK_VERTICAL * sensitivity;
            actionVector3 = EasyWiFiUtilities.getControllerVector3(horizontal, vertical, joystickHorizontal, joystickVertical);

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
            EasyWiFiUtilities.checkForClient(control, (int)player, ref joystick, ref currentNumberControllers);
        }
    }

}

