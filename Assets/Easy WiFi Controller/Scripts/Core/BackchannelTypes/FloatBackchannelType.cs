using UnityEngine;
using System;
using System.Collections;

//Here we define the structure of the controller types "over the wire" or in another words the network representation of all
namespace EasyWiFi.Core
{
    public class FloatBackchannelType : BaseControllerType
    {
        public float FLOAT_VALUE;

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
                        FLOAT_VALUE = (float) Convert.ToDecimal(line);
                    }
                }
            }

        }

        public override string mapStructureToNetworkData()
        {
            string message = "";

            //double and float if converted straight to string can have e notation in it which causes issues on when trying
            //to convert on the the other end so decimal is used "over the wire"
            message += Convert.ToDecimal(FLOAT_VALUE).ToString() + EasyWiFiConstants.SPLITMESSAGE_NEWLINE;

            return message;

        }


    }

}