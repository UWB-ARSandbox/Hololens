namespace EasyWiFi.ServerControls
{

    interface IServerController
    {
        void mapDataStructureToAction(int index);
        void checkForNewConnections(bool isConnect, int playerNumber);
    }

}
