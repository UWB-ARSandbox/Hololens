using UnityEngine;
using System;
using System.Collections;

//Here we define the structure of the controller types "over the wire" or in another words the network representation of all
namespace EasyWiFi.Core
{
    public class TouchpadControllerType : BaseControllerType
    {
        //A Touchpad is a simple control with a float for two axis going between 0 and 1 (normalized) and a bool for if we are touching
        //just a simple string representation on both ends
        //(0,0) is lower left corner of touchpad

        public float POSITION_HORIZONTAL;
        public float POSITION_VERTICAL;
        public bool IS_TOUCHING;

        public override void mapNetworkDataToStructure(int packetNumber, string line)
        {
            string[] comma = new String[] { EasyWiFiConstants.SPLITMESSAGE_COMMA };

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
                string[] splitMessage;

                if (line != null && !line.Equals(string.Empty))
                {
                    splitMessage = line.Split(comma, StringSplitOptions.RemoveEmptyEntries);
                    if (splitMessage.Length > 2)
                    {
                        POSITION_HORIZONTAL = (float) Convert.ToDecimal(splitMessage[0]);
                        POSITION_VERTICAL = (float) Convert.ToDecimal(splitMessage[1]);

                        if (splitMessage[2].Equals("1"))
                            IS_TOUCHING = true;
                        else
                            IS_TOUCHING = false;

                    }
                }
            }

        }

        public override string mapStructureToNetworkData()
        {
            string message = "";

            message += Convert.ToDecimal(POSITION_HORIZONTAL).ToString() + EasyWiFiConstants.SPLITMESSAGE_COMMA;
            message += Convert.ToDecimal(POSITION_VERTICAL).ToString() + EasyWiFiConstants.SPLITMESSAGE_COMMA;

            if (IS_TOUCHING == true)
            {
                message += "1" + EasyWiFiConstants.SPLITMESSAGE_NEWLINE;
            }
            else
            {
                message += "0" + EasyWiFiConstants.SPLITMESSAGE_NEWLINE;
            }

            return message;

        }


    }

}