using UnityEngine;
using System.Collections;
using EasyWiFi.Core;
using System;

namespace EasyWiFi.ServerControls
{

    [AddComponentMenu("EasyWiFiController/Server/UserControls/Standard Accelerometer")]
    public class StandardAccelerometerServerController : MonoBehaviour, IServerController
    {

        public string control = "Accelerometer";
        public EasyWiFiConstants.PLAYER_NUMBER player = EasyWiFiConstants.PLAYER_NUMBER.Player1;
        public EasyWiFiConstants.AXIS tiltHorizontal = EasyWiFiConstants.AXIS.XAxis;
        public EasyWiFiConstants.AXIS tiltVertical = EasyWiFiConstants.AXIS.YAxis;
        public EasyWiFiConstants.ACTION_TYPE action = EasyWiFiConstants.ACTION_TYPE.Position;
        public float sensitivity = 1f;

        //runtime variables
        AccelerometerControllerType[] accelerometer = new AccelerometerControllerType[EasyWiFiConstants.MAX_CONTROLLERS];
        int currentNumberControllers = 0;
        Vector3 actionVector3;
        Vector3 accel;
        float horizontal;
        float vertical;
        float normalizeDegrees = 90f;

        void OnEnable()
        {
            EasyWiFiController.On_ConnectionsChanged += checkForNewConnections;

            //do one check at the beginning just in case we're being spawned after startup and after the callbacks
            //have already been called
            if (accelerometer[0] == null && EasyWiFiController.lastConnectedPlayerNumber >= 0)
            {
                EasyWiFiUtilities.checkForClient(control, (int)player, ref accelerometer, ref currentNumberControllers);
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
                if (accelerometer[i] != null && accelerometer[i].serverKey != null && accelerometer[i].logicalPlayerNumber != EasyWiFiConstants.PLAYERNUMBER_DISCONNECTED)
                {
                    mapDataStructureToAction(i);
                }
            }
        }



        public void mapDataStructureToAction(int index)
        {
            //data
            accel.x = accelerometer[index].ACCELERATION_X;
            accel.y = accelerometer[index].ACCELERATION_Y;
            accel.z = accelerometer[index].ACCELERATION_Z;

            //accelerometers due to gravity can really only sense 2 axis (can't filter out gravity)
            //here we convert those 2 axis into horizontal and vertical and normalize
            horizontal = EasyWiFiUtilities.relativeAngleInAxis(Vector3.up, -accel, Vector3.forward) / normalizeDegrees;
            vertical = EasyWiFiUtilities.relativeAngleInAxis(Vector3.up, -accel, Vector3.right) / normalizeDegrees;

            horizontal *= -sensitivity;
            vertical *= sensitivity;

            actionVector3 = EasyWiFiUtilities.getControllerVector3(horizontal, vertical, tiltHorizontal, tiltVertical);


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
            EasyWiFiUtilities.checkForClient(control, (int)player, ref accelerometer, ref currentNumberControllers);
        }
    }

}

