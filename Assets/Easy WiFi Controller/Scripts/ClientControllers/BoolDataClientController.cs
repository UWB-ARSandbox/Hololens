using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EasyWiFi.Core;

namespace EasyWiFi.ClientControls
{
    [AddComponentMenu("EasyWiFiController/Client/DataControls/Bool")]
    public class BoolDataClientController : MonoBehaviour, IClientController
    {

        public string controlName = "BoolData1";

        //we reuse the backchannel data types even though this is a forward channel
        BoolBackchannelType boolData;
        string boolKey;

        //variable other script will modify via setValue to be sent across the network
        bool value;

        // Use this for initialization
        void Awake()
        {
            boolKey = EasyWiFiController.registerControl(EasyWiFiConstants.CONTROLLERTYPE_BOOL, controlName);
            boolData = (BoolBackchannelType)EasyWiFiController.controllerDataDictionary[boolKey];
        }

        //here we grab the input and map it to the data list
        void Update()
        {
            mapInputToDataStream();
        }

        public void setValue(bool newValue)
        {
            value = newValue;
        }

        public void mapInputToDataStream()
        {
            //for properties DO NOT reset to default values becasue there isn't a default
            boolData.BOOL_VALUE = value;
        }

    }

}