using UnityEngine;
using System;
using System.Collections;

//Here we define the structure of the controller types "over the wire" or in another words the network representation of all
namespace EasyWiFi.Core
{
    public class JoystickControllerType : BaseControllerType
    {
        //A Joystick is a simple control with a float for two axis going between -1 and 1 (normalized
        //just a simple string representation on both ends

        public float JOYSTICK_HORIZONTAL;
        public float JOYSTICK_VERTICAL;

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
                    if (splitMessage.Length > 1)
                    {
                        JOYSTICK_HORIZONTAL = (float) Convert.ToDecimal(splitMessage[0]);
                        JOYSTICK_VERTICAL = (float) Convert.ToDecimal(splitMessage[1]);
                    }
                }
            }

        }

        public override string mapStructureToNetworkData()
        {
            string message = "";

            message += Convert.ToDecimal(JOYSTICK_HORIZONTAL).ToString() + EasyWiFiConstants.SPLITMESSAGE_COMMA;
            message += Convert.ToDecimal(JOYSTICK_VERTICAL).ToString() + EasyWiFiConstants.SPLITMESSAGE_NEWLINE;

            return message;

        }


    }

}