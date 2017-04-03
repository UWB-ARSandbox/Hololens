using UnityEngine;
using System;
using System.Collections;

//Here we define the structure of the controller types "over the wire" or in another words the network representation of all
namespace EasyWiFi.Core
{
    public class BoolBackchannelType : BaseControllerType
    {
        public bool BOOL_VALUE;

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
                    if (line.Length > 0)
                    {
                        if (line.Equals("1"))
                            BOOL_VALUE = true;
                        else
                            BOOL_VALUE = false;
                    }
                }
            }

        }

        public override string mapStructureToNetworkData()
        {
            string message = "";

            if (BOOL_VALUE == true)
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