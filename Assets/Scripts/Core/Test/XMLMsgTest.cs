using Core.Framework.Event;
using Core.Framework.Module;
using Core.Framework.Network;
using Core.Framework.Network.Data;
using Core.Framework.Res;
using Core.Framework.Schedule;
using Core.Framework.Utility;
using Core.Game.Network;
using System;
using System.Collections;
using UnityEngine;
//#if UNITY_EDITOR
//using UnityEditor;
//#endif

public class XMLMsgTest : MonoBehaviour
{

    private IEnumerator Start()
    {
        var ins = ModuleManager.Instance;
        EventSystem.Instance.RegisterStringKeyListenerWithObject("AAA", (evt) =>
        {
            int bp = 1;
        }, this);


//        Core.Game.Table.TableGroup.Loader += (name, byteCB, jsonCB) =>
//        {
//#if UNITY_EDITOR
//            //var txtAsset = AssetDatabase.LoadAssetAtPath<TextAsset>($"Assets/PublicAssets/Table/{name}JsonData.bytes");
//            //jsonCB.Invoke(txtAsset.text);

        //#endif
        //            ResourceManager.Instance.LoadAssetAsync<TextAsset>($"Assets/PublicAssets/Table/{name}ByteData.bytes",
        //            (asset) =>
        //            {
        //                byteCB.Invoke(asset.bytes);
        //            });
        //        };
        //        Core.Game.Table.TableGroup.ReloadAll(() =>
        //        {
        //            int bp = 1;
        //        });

        GameConnection.Instance.OnConnected += OnConnected;
        GameConnection.Instance.OnReceivedMessage += OnReceivedMessage;
        Core.Framework.Lua.LuaManager.Instance.DownloadAllLua();

        while (!Core.Framework.Lua.LuaManager.Instance.IsLuaDownloaded)
            yield return null;
        Core.Framework.Lua.LuaManager.Instance.StartLuaMain();

        ResourceManager.Instance.LoadAssetAsync<ServerConfigs>(GameConnection.ConfigPath, (config) =>
        {
            EventSystem.Instance.SendStringKeyEvent("AAA", this);
            GameConnection.Instance.Login(
                config.GetServerSet("Programmer Login"),
                config.GetServerSet("Programmer Game"),
                new LoginParam()
                {
                    userName = "test341",
                    password = "1",
                    protoVer = "20190831"
                });
        });

        ResourceManager.Instance.LoadAssetAsync<I2.Loc.LanguageSourceAsset>("Assets/PublicAssets/Language/Language-zh-TW.asset",
            (source) =>
            {
                I2.Loc.LocalizationManager.AddSource(source.SourceData);
                I2.Loc.LocalizationManager.CurrentLanguageCode = "zh-TW";

                var currentLanguage = I2.Loc.LocalizationManager.CurrentLanguage;
                var currentLanguageCode = I2.Loc.LocalizationManager.CurrentLanguageCode;
                Debug.Log($"2 currentLanguage={currentLanguage} currentLanguageCode={currentLanguageCode}");

                var txt2 = I2.Loc.LocalizationManager.GetTranslation("ACHVPoint");
                Debug.Log($"txt2={txt2}");
            });

        //ResourceManager.Instance.LoadAssetAsync<I2.Loc.LanguageSourceAsset>("Assets/PublicAssets/Language/Language-zh-CN.asset",
        //    (source) =>
        //    {
        //        I2.Loc.LocalizationManager.AddSource(source.SourceData);
        //        I2.Loc.LocalizationManager.CurrentLanguageCode = "zh-CN";

        //        var currentLanguage = I2.Loc.LocalizationManager.CurrentLanguage;
        //        var currentLanguageCode = I2.Loc.LocalizationManager.CurrentLanguageCode;
        //        Debug.Log($"3 currentLanguage={currentLanguage} currentLanguageCode={currentLanguageCode}");

        //        var txt2 = I2.Loc.LocalizationManager.GetTranslation("ACHVPoint");
        //        Debug.Log($"txt2={txt2}");
        //    });
    }

    private void OnDestroy()
    {
        if(EventSystem.Instance != null)
            EventSystem.Instance.UnregisterAll(this);
    }

    private void OnConnected()
    {
        //var session = GameConnection.Instance.CurrentSession;
        //var req = XMLStringMessage.Allocate((int)ServerCmd.CMD_ROLE_LIST);
        //req.Append(session.userID, session.gameZoneID.ToString()); // zoneId
        //GameConnection.Instance.Send(req);
    }

    private void OnReceivedMessage(Message msg)
    {
        switch ((ServerCmd)msg.MessageId)
        {
            //case ServerCmd.CMD_ROLE_LIST:
            //{
            //    var fields = msg.GetData<ArraySeg<string>>();
            //    var ret = fields[0];
            //    var roles = fields[1].Split('|');
            //    var temps = roles[0].Split('$');
            //    var roleID = temps[0];
            //    var roleSex = temps[1];
            //    var occupation = temps[2];
            //    var roleName = temps[3];
            //    var level = temps[4];
            //    var roleCreateTime = temps[6];

            //    var req = XMLStringMessage.Allocate((int)ServerCmd.CMD_SPR_GETROLEUSINGGOODSDATALIST);
            //    req.Append(roleID);
            //    GameConnection.Instance.Send(req);
            //    break;
            //}
            //case ServerCmd.CMD_SPR_GETROLEUSINGGOODSDATALIST:
            //{
            //    //var data = msg.GetData<RoleData4Selector>();
            //    //var data = msg.GetData<ArraySeg<byte>>();

            //    var relativePath = Core.Framework.Lua.LuaManager.Instance.GetRelativePath("Assets/Scripts/LuaScripts/Test/TestMain.lua.txt");
            //    var bytes = Core.Framework.Lua.LuaManager.Instance.GetFileBytesByRelativePath(relativePath);
            //    Core.Framework.Lua.LuaManager.Instance.LuaEnv.DoString(bytes, relativePath);

            //    ScheduleManager.Instance.Schedule(5, (key, dt, obj) =>
            //    {
            //        var func = Core.Framework.Lua.LuaManager.Instance.LuaEnv.Global.Get<Action<Message>>("TestDecode");
            //        func.Invoke(msg);
            //        return false;
            //    });
            //    break;
            //}
        }
    }
}
