using UnityEngine;
using System.Collections;
using EasyWiFi.Core;
using System;

namespace EasyWiFi.ServerControls
{

    [AddComponentMenu("EasyWiFiController/Server/UserControls/Standard Slider")]
    public class StandardSliderServerController : MonoBehaviour, IServerController
    {

        public string control = "Slider1";
        public EasyWiFiConstants.PLAYER_NUMBER player = EasyWiFiConstants.PLAYER_NUMBER.Player1;
        public EasyWiFiConstants.AXIS sliderEffect = EasyWiFiConstants.AXIS.XAxis;
        public EasyWiFiConstants.ACTION_TYPE action = EasyWiFiConstants.ACTION_TYPE.Position;
        public float sensitivity = .01f;

        //runtime variables
        SliderControllerType[] slider = new SliderControllerType[EasyWiFiConstants.MAX_CONTROLLERS];
        int currentNumberControllers = 0;
        Vector3 actionVector3;
        float sliderValue;

        void OnEnable()
        {
            EasyWiFiController.On_ConnectionsChanged += checkForNewConnections;

            //do one check at the beginning just in case we're being spawned after startup and after the callbacks
            //have already been called
            if (slider[0] == null && EasyWiFiController.lastConnectedPlayerNumber >= 0)
            {
                EasyWiFiUtilities.checkForClient(control, (int)player, ref slider, ref currentNumberControllers);
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
                if (slider[i] != null && slider[i].serverKey != null && slider[i].logicalPlayerNumber != EasyWiFiConstants.PLAYERNUMBER_DISCONNECTED)
                {
                    mapDataStructureToAction(i);
                }
            }
        }



        public void mapDataStructureToAction(int index)
        {
            sliderValue = slider[index].SLIDER_VALUE * sensitivity;
            actionVector3 = EasyWiFiUtilities.getControllerVector3(sliderValue, 0f, sliderEffect, EasyWiFiConstants.AXIS.None);

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
            EasyWiFiUtilities.checkForClient(control, (int)player, ref slider, ref currentNumberControllers);
        }
    }

}

