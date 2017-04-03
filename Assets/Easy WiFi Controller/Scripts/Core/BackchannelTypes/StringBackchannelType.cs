using UnityEngine;
using System;
using System.Collections;

//Here we define the structure of the controller types "over the wire" or in another words the network representation of all
namespace EasyWiFi.Core
{
    public class StringBackchannelType : BaseControllerType
    {
        public string STRING_VALUE;

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
                        STRING_VALUE = line;
                    }
                }
            }

        }

        public override string mapStructureToNetworkData()
        {
            string message = "";

            if (STRING_VALUE != null && !STRING_VALUE.Equals(String.Empty))
            {
                message += STRING_VALUE + EasyWiFiConstants.SPLITMESSAGE_NEWLINE;
            }

            return message;

        }


    }

}