using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EasyWiFi.Core;

namespace EasyWiFi.ClientControls
{
    [AddComponentMenu("EasyWiFiController/Client/DataControls/Int")]
    public class IntDataClientController : MonoBehaviour, IClientController
    {

        public string controlName = "IntData1";

        //we reuse the backchannel data types even though this is a forward channel
        IntBackchannelType intData;
        string intKey;

        //variable other script will modify via setValue to be sent across the network
        int value;

        // Use this for initialization
        void Awake()
        {
            intKey = EasyWiFiController.registerControl(EasyWiFiConstants.CONTROLLERTYPE_INT, controlName);
            intData = (IntBackchannelType)EasyWiFiController.controllerDataDictionary[intKey];
        }

        //here we grab the input and map it to the data list
        void Update()
        {
            mapInputToDataStream();
        }

        public void setValue(int newValue)
        {
            value = newValue;
        }

        public void mapInputToDataStream()
        {
            //for properties DO NOT reset to default values becasue there isn't a default
            intData.INT_VALUE = value;
        }

    }

}