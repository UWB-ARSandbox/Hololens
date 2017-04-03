using UnityEngine;
using System;
using System.Collections;

//Here we define the structure of the controller types "over the wire" or in another words the network representation of all
namespace EasyWiFi.Core
{
    public class DpadControllerType : BaseControllerType
    {
        //A dpad is a simple control with the directions and really just an off or on in a bool
        //just a simple string representation on both ends so I'll just be using "0" for off and "1" for on

        public bool DPAD_LEFT_PRESSED;
        public bool DPAD_UP_PRESSED;
        public bool DPAD_RIGHT_PRESSED;
        public bool DPAD_DOWN_PRESSED;

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
                    if (splitMessage.Length > 3)
                    {
                        //left
                        if (splitMessage[0].Equals("1"))
                            DPAD_LEFT_PRESSED = true;
                        else
                            DPAD_LEFT_PRESSED = false;

                        //up
                        if (splitMessage[1].Equals("1"))
                            DPAD_UP_PRESSED = true;
                        else
                            DPAD_UP_PRESSED = false;

                        //right
                        if (splitMessage[2].Equals("1"))
                            DPAD_RIGHT_PRESSED = true;
                        else
                            DPAD_RIGHT_PRESSED = false;

                        //down
                        if (splitMessage[3].Equals("1"))
                            DPAD_DOWN_PRESSED = true;
                        else
                            DPAD_DOWN_PRESSED = false;
                    }
                }
            }

        }

        public override string mapStructureToNetworkData()
        {
            string message = "";

            //left
            if (DPAD_LEFT_PRESSED == true)
            {
                message += "1" + EasyWiFiConstants.SPLITMESSAGE_COMMA;
            }
            else
            {
                message += "0" + EasyWiFiConstants.SPLITMESSAGE_COMMA;
            }

            //up
            if (DPAD_UP_PRESSED == true)
            {
                message += "1" + EasyWiFiConstants.SPLITMESSAGE_COMMA;
            }
            else
            {
                message += "0" + EasyWiFiConstants.SPLITMESSAGE_COMMA;
            }

            //right
            if (DPAD_RIGHT_PRESSED == true)
            {
                message += "1" + EasyWiFiConstants.SPLITMESSAGE_COMMA;
            }
            else
            {
                message += "0" + EasyWiFiConstants.SPLITMESSAGE_COMMA;
            }

            //down
            if (DPAD_DOWN_PRESSED == true)
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