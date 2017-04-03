using UnityEngine;
using System;
using System.Collections;

//Here we define the structure of the controller types "over the wire" or in another words the network representation of all
namespace EasyWiFi.Core
{
    public class SliderControllerType : BaseControllerType
    {
        //A Slider is a simple control with a float going between -1 and 1 (normalized)
        //just a simple string representation on both ends

        public float SLIDER_VALUE;

        public override void mapNetworkDataToStructure(int packetNumber, string line)
        {

            //if we've received a packet on connection we thought was disconnected take action
            if (logicalPlayerNumber == EasyWiFiConstants.PLAYERNUMBER_DISCONNECTED)
            {
                reuseOrGetAnotherConnection(previousConnectionPlayerNumber);
            }

            //remember when receiving the data the serverkey has already been parsed out to get the right
            //server instance in the data structure so we're left with datapoint,datapoint,datapoint...etc.
            if (isNewPacket(packetNumber))
            {
                lastPacketNumber = packetNumber;

                if (line != null && !line.Equals(string.Empty))
                {
                    SLIDER_VALUE = (float)Convert.ToDecimal(line);
                }

            }

        }

        public override string mapStructureToNetworkData()
        {
            string message = "";

            message += Convert.ToDecimal(SLIDER_VALUE).ToString() + EasyWiFiConstants.SPLITMESSAGE_NEWLINE;

            return message;

        }


    }

}