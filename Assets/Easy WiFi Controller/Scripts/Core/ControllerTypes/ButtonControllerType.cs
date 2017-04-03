using UnityEngine;
using System;
using System.Collections;

//Here we define the structure of the controller types "over the wire" or in another words the network representation of all
namespace EasyWiFi.Core
{
    public class ButtonControllerType : BaseControllerType
    {
        //A button is a simple control and is really just pressed/not pressed in a bool
        //just a simple string representation on both ends so I'll just be using "0" for off and "1" for on

        public bool BUTTON_STATE_IS_PRESSED;

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
                    if (line.Length > 0 && line.Length < 2)
                    {
                        if (line.Equals("1"))
                            BUTTON_STATE_IS_PRESSED = true;
                        else
                            BUTTON_STATE_IS_PRESSED = false;
                    }
                }
            }

        }

        public override string mapStructureToNetworkData()
        {
            string message = "";

            if (BUTTON_STATE_IS_PRESSED == true)
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