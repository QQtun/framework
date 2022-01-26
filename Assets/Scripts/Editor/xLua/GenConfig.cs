/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using System.Collections.Generic;
using System;
using XLua;
using System.Reflection;
//using System.Reflection;
//using System.Linq;

//配置的详细介绍请看Doc下《XLua的配置.doc》
public static class GenConfig
{
    //lua中要使用到C#库的配置，比如C#标准库，或者Unity API，第三方库等。

    [LuaCallCSharp]
    public static List<Type> SystemLuaCallCSharp = new List<Type>()
    {
        typeof(System.Object),
        typeof(System.Collections.Generic.List<int>),
        typeof(System.Collections.Generic.List<float>),
        typeof(System.Collections.Generic.List<string>),
        typeof(System.Collections.Generic.Dictionary<string, object>)
    };

    [LuaCallCSharp]
    public static List<Type> UnityEngineLuaCallCSharp = new List<Type>()
    {
        typeof(UnityEngine.Object),
        typeof(UnityEngine.Vector2),
        typeof(UnityEngine.Vector3),
        typeof(UnityEngine.Vector4),
        typeof(UnityEngine.Quaternion),
        typeof(UnityEngine.Color),
        typeof(UnityEngine.Ray),
        typeof(UnityEngine.Bounds),
        typeof(UnityEngine.Ray2D),
        typeof(UnityEngine.Time),
        typeof(UnityEngine.GameObject),
        typeof(UnityEngine.Component),
        typeof(UnityEngine.Behaviour),
        typeof(UnityEngine.Transform),
        typeof(UnityEngine.Resources),
        typeof(UnityEngine.TextAsset),
        typeof(UnityEngine.Keyframe),
        typeof(UnityEngine.AnimationCurve),
        typeof(UnityEngine.AnimationClip),
        typeof(UnityEngine.MonoBehaviour),
        typeof(UnityEngine.ParticleSystem),
        typeof(UnityEngine.SkinnedMeshRenderer),
        typeof(UnityEngine.Renderer),
        typeof(UnityEngine.Light),
        typeof(UnityEngine.Mathf),
        //typeof(UnityEngine.Debug),
        typeof(UnityEngine.Canvas),
        typeof(UnityEngine.Sprite),
        typeof(UnityEngine.Events.UnityEvent),
        typeof(UnityEngine.Events.UnityEvent<float>),
        typeof(UnityEngine.Events.UnityEvent<bool>),
        typeof(UnityEngine.Events.UnityEvent<string>),
        typeof(UnityEngine.Events.UnityEvent<UnityEngine.Vector2>),
        typeof(UnityEngine.WaitForEndOfFrame),
    };

    [LuaCallCSharp]
    public static List<Type> UnityEngineUILuaCallCSharp = new List<Type>()
    {
        typeof(UnityEngine.UI.Button),
        typeof(UnityEngine.UI.Image),
        typeof(UnityEngine.UI.InputField),
        typeof(UnityEngine.UI.Mask),
        typeof(UnityEngine.UI.RawImage),
        typeof(UnityEngine.UI.Slider),
        typeof(UnityEngine.UI.Text),
        typeof(UnityEngine.UI.Toggle),
        typeof(UnityEngine.UI.ToggleGroup),
        typeof(UnityEngine.UI.HorizontalLayoutGroup),
        typeof(UnityEngine.UI.VerticalLayoutGroup),
        typeof(UnityEngine.UI.Button.ButtonClickedEvent),
        typeof(UnityEngine.UI.InputField.OnChangeEvent),
        typeof(UnityEngine.UI.InputField.SubmitEvent),
        typeof(UnityEngine.UI.Slider.SliderEvent),
        typeof(UnityEngine.UI.Toggle.ToggleEvent),
    };

    [LuaCallCSharp]
    public static List<Type> CoreLuaCallCSharp = new List<Type>()
    {
        typeof(Core.Framework.Lua.LuaManager),
        typeof(Core.Framework.Lua.SimpleLuaBehaviour),
        typeof(Core.Framework.UI.UIManager),
        typeof(Core.Framework.UI.UIRoot),
        typeof(Core.Framework.Scene.SceneManager),
        typeof(Core.Framework.Res.LoadParam),
        typeof(Core.Framework.Res.ResourceManager),
        typeof(Core.Framework.Schedule.ScheduleManager),
        typeof(Core.Framework.Network.Data.Message),
        typeof(Core.Framework.Network.Data.MessageFactory),
        typeof(Core.Framework.Network.Data.StringMessage),
        typeof(Core.Framework.Network.Data.RawDataMessage),
        typeof(Core.Framework.Module.ModuleManager),
        typeof(Core.Framework.Utility.ArraySeg<byte>),
        typeof(Core.Framework.Utility.ArraySeg<string>),
        typeof(Core.Framework.Event.EventSystem),
        typeof(Core.Framework.Event.IEventListener),
    };

    [LuaCallCSharp]
    public static List<Type> GameLuaCallCSharp = new List<Type>()
    {
        typeof(Core.Game.UI.UIName),
        typeof(Core.Game.Log.LogTagEx),
        //typeof(Core.Game.Network.GameConnection),
        //typeof(Core.Game.Network.LoginParam),
        typeof(Core.Game.Network.Session),
        typeof(Core.Game.Scene.GameScene),
    };

    // [TableLuaCallCSharp Start]
    [LuaCallCSharp]
    public static List<Type> TableLuaCallCSharp
    {
        get
        {
            List<Type> list = new List<Type>();
            var assembly = Assembly.Load("Assembly-CSharp");
            var allTypes = assembly.GetTypes();
            foreach(var t in allTypes)
            {
                if (t.FullName.StartsWith("Core.Game.Table"))
                {
                    list.Add(t);
                }
            }
            return list;
        }
    }
    // [TableLuaCallCSharp End]

    [LuaCallCSharp]
    public static List<Type> OtherLuaCallCSharp = new List<Type>()
    {
        typeof(XLuaTest.Coroutine_Runner),
        typeof(LogUtil.Debug),
        typeof(LogUtil.LogTag),
        typeof(UnityEngine.AddressableAssets.Addressables),
        typeof(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle),
        typeof(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.GameObject>),
        typeof(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<long>),
    };

    //C#静态调用Lua的配置（包括事件的原型），仅可以配delegate，interface
    [CSharpCallLua]
    public static List<Type> CSharpCallLua = new List<Type>() {
                typeof(Action),
                typeof(Action<object>),
                typeof(Action<string>),
                typeof(Action<double>),
                typeof(Action<float>),
                typeof(Action<bool>),
                typeof(Action<byte[]>),
                typeof(Action<UnityEngine.TextAsset>),
                typeof(Func<double, double, double>),
                typeof(Func<int, float, object, bool>),
                typeof(UnityEngine.Events.UnityAction),
                typeof(Core.Framework.Scene.ISceneEntry),
                typeof(Action<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>),
                typeof(Action<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.GameObject>>),
                typeof(Action<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<long>>),
                typeof(Action<Core.Framework.Network.Data.Message>),
                typeof(Action<Core.Framework.Network.Data.StringMessage>),
                typeof(Action<Core.Framework.Network.Data.RawDataMessage>),
                typeof(Google.Protobuf.IMessage),
                typeof(Core.Framework.Module.IModule),
                typeof(Core.Framework.Module.INetworkModule),
                typeof(Action<UnityEngine.Vector2>),
                //typeof(Core.Game.Network.GameConnection.OnConnect),
                //typeof(Core.Game.Network.GameConnection.OnDisconnect),
                //typeof(Core.Game.Network.GameConnection.OnReceiveMessage),
                typeof(Core.Game.Logic.ISceneObject),
            };

    //黑名单
    [BlackList]
    public static List<List<string>> BlackList = new List<List<string>>()  {
                new List<string>(){"System.Xml.XmlNodeList", "ItemOf"},
                new List<string>(){"UnityEngine.WWW", "movie"},
    #if UNITY_WEBGL
                new List<string>(){"UnityEngine.WWW", "threadPriority"},
    #endif
                new List<string>(){"UnityEngine.Texture2D", "alphaIsTransparency"},
                new List<string>(){"UnityEngine.Security", "GetChainOfTrustValue"},
                new List<string>(){"UnityEngine.CanvasRenderer", "onRequestRebuild"},
                new List<string>(){"UnityEngine.Light", "areaSize"},
                new List<string>(){"UnityEngine.Light", "lightmapBakeType"},
                new List<string>(){"UnityEngine.Light", "shadowRadius"},
                new List<string>(){"UnityEngine.Light", "shadowAngle"},
                new List<string>(){"UnityEngine.Light", "SetLightDirty"},
                new List<string>(){"UnityEngine.WWW", "MovieTexture"},
                new List<string>(){"UnityEngine.WWW", "GetMovieTexture"},
                new List<string>(){"UnityEngine.AnimatorOverrideController", "PerformOverrideClipListCleanup"},
    #if !UNITY_WEBPLAYER
                new List<string>(){"UnityEngine.Application", "ExternalEval"},
    #endif
                new List<string>(){"UnityEngine.GameObject", "networkView"}, //4.6.2 not support
                new List<string>(){"UnityEngine.Component", "networkView"},  //4.6.2 not support
                new List<string>(){"System.IO.FileInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
                new List<string>(){"System.IO.FileInfo", "SetAccessControl", "System.Security.AccessControl.FileSecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
                new List<string>(){"System.IO.DirectoryInfo", "SetAccessControl", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "CreateSubdirectory", "System.String", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "Create", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"UnityEngine.MonoBehaviour", "runInEditMode"},
                new List<string>(){ "UnityEngine.UI.Text", "OnRebuildRequested" },
    };
}
