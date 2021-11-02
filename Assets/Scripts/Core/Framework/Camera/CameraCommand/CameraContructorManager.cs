using System.Collections.Generic;
using UnityEngine;

namespace Core.Framework.Camera.CameraCommand
{

    /// <summary>
    /// 命令狀態
    /// </summary>
    public enum CommandState
    {
        Waitting = 0,
        Playing = 1,
        Finished = 2,
    }

    /// <summary>
    /// 命令的基類
    /// 
    /// 運作流程是
    /// 初始化 => SetCommandStruct => Init
    /// 更新資料  控制器的Update => Update => UpdateDate
    /// 執行命令  控制器的LateUpdate => Execute
    /// 後製     控制器的OnRenderImage => RenderEffect
    /// </summary>
    public class CommandStruct
    {
        //命令的控制器
        protected CameraContructorManager cameraCommandManager;

        protected CommandState state = CommandState.Waitting;
        public CommandState State
        {
            get { return state; }
        }

        public string originalData = "";//原始數據

        public string type = "";//命令類別

        public string tip = "";//類型 magic.....

        protected float[] data;//數據

        public float time = 0f; //現在秒數
        public float begintime = 0f; //開始時間

        public float durationtime = 0f;//持續時間

        protected bool isInit = false;


        /// <summary>
        /// 命令是否完成
        /// </summary>
        public bool IsFinished()
        {
            return state == CommandState.Finished;
        }

        /// <summary>
        /// 命令是否衝突
        /// </summary>
        /// <param name="s">命令</param>
        public bool IsClash(CommandStruct s)
        {

            if (this.begintime - this.time > s.begintime + s.durationtime
             || this.begintime - this.time + this.durationtime < s.begintime)
            {
                return false;
            }

            //命令相衝
            Debug.LogError("CommandStruct :" + this.type + " : 命令衝突");
            return true;
        }

        /// <summary>
        /// 輸入資料(!!一定要呼叫!!)
        /// </summary>
        /// <param name="command">控制器</param>
        /// <param name="data">資料</param>
        /// <param name="tip">類型</param>
        public void SetCommandStruct(CameraContructorManager command, string data, string tip)
        {
            cameraCommandManager = command;

            //設定數據 數據=> 類型 , 開始時間 ,持續時間 , Data.........
            this.originalData = data;
            string[] strlist = data.Split(',');
            type = strlist[0];//設定類型
            this.tip = tip;//設定啟動類別 例:技能......

            float.TryParse(strlist[1], out begintime);
            float.TryParse(strlist[2], out durationtime);

            begintime /= 1000.0f; // 開始時間
            durationtime /= 1000.0f;//持續時間

            //將資料放入陣列中
            this.data = new float[strlist.Length - 3];
            for (int i = 0; i < this.data.Length; i++)
            {
                float.TryParse(strlist[3 + i], out this.data[i]);
            }

            time = 0;

            state = CommandState.Waitting;//命令狀態

            isInit = false;
        }

        /// <summary>
        /// 命令初始化
        /// </summary>
        protected virtual void Init() { Debug.LogWarning("未覆寫命令初始化 Init"); }

        /// <summary>
        /// 外部呼叫的Update
        /// </summary>
        public bool Update()
        {
            time += Time.deltaTime;

            //時間沒到彈出
            if (time < begintime || state == CommandState.Finished)
            { return false; }

            if (time >= begintime && isInit == false)
            {
                Init();
                isInit = true;
                state = CommandState.Playing;
            }

            //執行更新資料
            if (time >= begintime && state == CommandState.Playing)
            {
                UpdateDate();
            }

            if (time > begintime + durationtime)
            {
                state = CommandState.Finished;
            }

            return true;
        }

        /// <summary>
        /// 更新命令資料(!!注意不是執行命令!!)
        /// </summary>
        protected virtual void UpdateDate() { Debug.LogWarning("未覆寫命令更新 UpdateDate"); }

        /// <summary>
        /// 執行命令
        /// </summary>
        public virtual void Execute() { Debug.LogWarning("未覆寫命令執行 Execute"); }

        /// <summary>
        /// 後製用，用來Render貼圖
        /// 以一個個命令傳接下，將渲染用材質球設定好
        /// A 渲染 => B 渲染 => 最後輸出
        /// </summary>
        /// <param name="source">貼圖</param>
        /// <param name="destination">貼圖</param>
        /// <param name="material">渲染後的材質球</param>
        public virtual void RenderEffect(RenderTexture source, RenderTexture destination, ref Material material) { }

    }

    public class CameraContructorManager : MonoBehaviour
    {
        //如果有單一個命令，超出數字，視為無用數據
        private const int mCommandMax = 5;
        
        //Editor要用的，監測現在命令總共執行多少時間
        public float commandTime = 0;

        //命令的List
        public List<CommandStruct> commandList = null;

        //命令池
        public Dictionary<string, List<CommandStruct>> commandPool;

        //後製Shader
        private string mShaderName = "Hidden/PostEffects/Uber";

        //後製Shader
        private Shader mShader;

        //後製的Material
        private Material mMotionMaterial;


        #region  命令能調用的變數

        public UnityEngine.Camera mainCamera; //Camera組件

        public Transform mainTransform; //Camera所在的Transform

        public Transform cameraTarget; //Camera鎖定目標的Transform

        #endregion


        private void Start()
        {
            mainCamera = this.GetComponent<UnityEngine.Camera>();
            mainTransform = this.transform;
            cameraTarget = GameObject.Find("Leader").transform; //TODO:這行要改，要設定攝影機目標物

            //找尋Shader
            mShader = Shader.Find(mShaderName);
            if (mShader == null)
            {
                Debug.LogError(string.Format("Shader not found ({0})", mShaderName));
            }

            //設定動態模糊材質球
            mMotionMaterial = new Material(mShader)
            {
                name = string.Format("PostFX - {0}", mShaderName.Substring(mShaderName.LastIndexOf("/") + 1)),
                hideFlags = HideFlags.DontSave
            };
        }

        /// <summary>
        /// 更新命令
        /// </summary>
        private void Update()
        {
            if (commandList != null)
            {
                Debug.Log("當前命令列表 : " + commandList.Count);

                commandTime += Time.deltaTime;

                int finishedCount = 0;

                //更新命令
                for (int i = 0; i < commandList.Count; i++)
                {
                    CommandStruct cmd = commandList[i];

                    if (cmd.IsFinished())
                    {
                        finishedCount++;
                    }
                    else
                    {
                        cmd.Update();
                    }
                }

                //當命令執行完，要將命令放回物件池
                if (finishedCount == commandList.Count)
                {
                    for (int i = 0; i < commandList.Count; i++)
                    {
                        CommandStruct cmd = commandList[i];

                        if (commandPool.ContainsKey(cmd.type) == false)
                        {
                            List<CommandStruct> newList = new List<CommandStruct>();
                            newList.Add(commandList[i]);
                            commandPool.Add(cmd.type, newList);
                        }
                        else
                        {
                            for (int j = 0; j <= commandPool[cmd.type].Count; j++)
                            {
                                //如果沒找到空位就在List新增欄位
                                if (j == commandPool[cmd.type].Count)
                                {
                                    commandPool[cmd.type].Add(cmd);
                                    break;
                                }

                                //找到空位就塞進去
                                if (commandPool[cmd.type][j] == null)
                                {
                                    commandPool[cmd.type][j] = cmd;
                                    break;
                                }
                            }
                        }
                    }

                    //清掉命令堆
                    commandList = null;

                    //重製材質球
                    mMotionMaterial = new Material(mShader)
                    {
                        name = string.Format("PostFX - {0}", mShaderName.Substring(mShaderName.LastIndexOf("/") + 1)),
                        hideFlags = HideFlags.DontSave
                    };

                    //清掉時間
                    commandTime = 0;
                }

            }
        }


        /// <summary>
        /// 執行命令
        /// </summary>
        private void LateUpdate()
        {
            if (commandList != null)
            {
                for (int i = 0; i < commandList.Count; i++)
                {
                    CommandStruct cmd = commandList[i];

                    if (cmd.IsFinished() == false)
                    {
                        cmd.Execute();
                    }
                }
            }
        }


        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (commandList != null)
            {
                for (int i = 0; i < commandList.Count; i++)
                {
                    CommandStruct cmd = commandList[i];

                    cmd.RenderEffect(source, destination, ref mMotionMaterial);
                }

                Graphics.Blit(source, destination, mMotionMaterial, 0);
            }
            else
            {
                Graphics.Blit(source, destination);
            }
        }


        /// <summary>
        /// 設定命令
        /// </summary>
        /// <param name="cmd">命令碼</param>
        /// <param name="tip">使用類別 例:magic....</param>
        public void SetCameraByCommand(string cmd, string tip)
        {
            # region 檢查數據

            string[] strlists = cmd.Split(';');

            //如果有單一個命令，連續出現，認定為錯誤數據
            if (strlists.Length > mCommandMax)
            {
                Dictionary<string, int> commandNum = new Dictionary<string, int>();

                for (int i = 0; i < strlists.Length; i++)
                {
                    int typeNum = strlists[i].IndexOf(',');
                    string type = strlists[i].Substring(0, typeNum);

                    if (commandNum.ContainsKey(type) == false)
                    {
                        commandNum.Add(type, 1);
                    }
                    else
                    {
                        commandNum[type]++;
                    }
                }

                foreach (var item in commandNum)
                {
                    if (item.Value > mCommandMax)
                    {
                        Debug.LogError(string.Format("錯誤 : 有相同命令超過 {0} 個", mCommandMax));
                        return;
                    }
                }

            }

            #endregion

            //執行命令設定
            if (commandList == null)
            { commandList = new List<CommandStruct>(); }

            if (strlists != null)
            {
                //跑全部命令
                foreach (var str in strlists)
                {
                    //找尋命令
                    CommandStruct cmdStruct = CameraCommandPool(str, tip);

                    bool bAdd = true;

                    //如果命令為Null強制停止
                    if (cmdStruct == null)
                    {
                        Debug.LogError("有無法生成的命令，請檢查");
                        break;
                    }
                    else
                    {
                        cmdStruct.SetCommandStruct(this, str, tip);
                    }

                    //添加命令
                    for (int i = commandList.Count - 1; i >= 0; i--)
                    {

                        if (commandList[i].type == cmdStruct.type)
                        {
                            //檢查如果時間為0，認定為無效數據
                            if (commandList[i].durationtime == 0)
                            {
                                if (tip == "magic")
                                {
                                    commandList.RemoveAt(i);
                                }
                                else
                                {
                                    bAdd = false;
                                    break;
                                }
                            }
                            else
                            {
                                //檢查命令是否相撞
                                if (commandList[i].IsClash(cmdStruct))
                                {
                                    if (tip == "magic")
                                    {
                                        commandList.RemoveAt(i);
                                    }
                                    else
                                    {
                                        bAdd = false;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (bAdd)
                        commandList.Add(cmdStruct);
                }
            }
        }

        /// <summary>
        /// 命令的物件池
        /// </summary>
        /// <param name="data">資料</param>
        /// <param name="tip">類別</param>
        public CommandStruct CameraCommandPool(string data, string tip)
        {
            string[] strlists = data.Split(',');
            string type = strlists[0];

            if (commandPool == null)
            { commandPool = new Dictionary<string, List<CommandStruct>>(); }

            //查詢物件池
            CommandStruct cmdStruct;

            //找，如果List最後一位不是Null則代表List還有數據
            if (commandPool.ContainsKey(type) == false
            || ((commandPool[type][commandPool[type].Count - 1]) == null))
            {
                //如果沒有要生成 //TODO:以後新增命令在這
                CommandStruct command;
                switch (type)
                {
                    case "randommove":
                        command = new CameraCommandRandommove();
                        return command;
                    case "scale":
                        command = new CameraCommandScale();
                        return command;
                    case "motion":
                        command = new CameraCommandMotion();
                        return command;
                    case "reduce":
                        command = new CameraCommandReduce();
                        return command;
                }
            }
            else
            {
                //如果有則 return回去，且要改成null
                for (int i = 0; i < commandPool[type].Count; i++)
                {
                    if (commandPool[type][i] != null)
                    {
                        cmdStruct = commandPool[type][i];
                        commandPool[type][i] = null;
                        return cmdStruct;
                    }
                }
            }


            return null;
        }

        /// <summary>
        /// 刪除特定類型的全部命令
        /// 例:move randommove scale...
        /// </summary>
        /// <param name="type">類別名</param>
        public void CancelShakeByType(string type)
        {
            if (commandList != null)
            {
                for (int i = commandList.Count - 1; i >= 0; i--)
                {
                    if (commandList[i].type == type)
                    {
                        commandList.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// 刪除命令
        /// </summary>
        /// <param name="RemoveAveryOne">是否刪除所有命令</param>
        public void CancelCommand(bool RemoveAveryOne = false)
        {
            if (commandList == null)
            {
                Debug.LogError("刪除命令失敗，命令列表為null");
            }
            else
            {
                for (int i = commandList.Count - 1; i >= 0; i--)
                {
                    string type = commandList[i].type;
                    //刪除的排除範圍
                    if ((type != "reduce" && type != "slow" && type != "motion")
                    || RemoveAveryOne)
                    {
                        commandList.RemoveAt(i);
                    }
                }
            }
        }


    }



}