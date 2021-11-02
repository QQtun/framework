namespace Core.Game.Network
{
    public class Session
    {
        public int userID;
        public string userName = "";
        public string userToken = "";
        public bool userIsAdult;
        public int roleRandToken = -1;
        //public int RoleID = -1;
        //public int RoleSex = 0;
        //public string RoleName = "";
        //public bool PlayGame = false;
        //public RoleData roleData = null;
        //public string RolePathString = "";
        public int gameZoneID = 1;
        //public int GameLineID = 1;
        //public string GameLineIP = "";
        //public int GameLinePort = 0;
        //public string GamePingTaiID = "local";
        //public MarriageData MarriageData;
        //public MarriageData_EX OtherMarriageData;
    }
}