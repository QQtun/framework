using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Core.Framework.Camera.CameraCommand;

[CustomEditor(typeof(CameraContructorManager))]
public class CameraContructorManagerEditor : Editor
{
    private CameraContructorManager mCameraContructor;

    private bool[] mCommandFoldout = null; // 声明折叠菜单的状态

    override public void OnInspectorGUI()
    {
        mCameraContructor = (CameraContructorManager)target;

        //命令堆疊
        GUILayout.BeginVertical("執行命令堆疊", "window");
        CommandListGUI();
        GUILayout.EndVertical();

        GUILayout.Space(10);

        //物件池
        GUILayout.BeginVertical("命令物件池", "window");
        CommandPoolGUI();
        GUILayout.EndVertical();


        base.OnInspectorGUI();
    }

    /// <summary>
    /// 執行堆疊 GUI
    /// </summary>
    private void CommandListGUI()
    {
        //確認Unity 是否在Play
        if (EditorApplication.isPlaying)
        {
            if (mCameraContructor.commandList != null)
            {
                //摺疊的 bool
                if (mCommandFoldout == null)
                {
                    mCommandFoldout = new bool[mCameraContructor.commandList.Count];
                }

                //運行秒數
                GUI.skin.label.fontSize = 15;
                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("目前時間 : " + mCameraContructor.commandTime.ToString("#0.00") + "秒");
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
                GUI.skin.label.fontSize = 12;

                //抓commandList
                for (int i = 0; i < mCameraContructor.commandList.Count; i++)
                {
                    //摺疊按鈕
                    mCommandFoldout[i] = GUILayout.Toggle(mCommandFoldout[i],
                    (i + 1).ToString() + " . " + mCameraContructor.commandList[i].type);

                    //摺疊窗顯示內容
                    if (mCommandFoldout[i] == true)
                    {

                        string type = "flow node ";

                        //暫停的方塊
                        if (mCameraContructor.commandList[i].State == CommandState.Waitting)
                        { type += 6; }//紅色

                        //運行中的方塊
                        if (mCameraContructor.commandList[i].State == CommandState.Playing)
                        { type += 3; }//綠色

                        //完成的方塊
                        if (mCameraContructor.commandList[i].State == CommandState.Finished)
                        { type += 0; }//灰色


                        GUILayout.BeginVertical("", type);
                        {
                            GUI.skin.label.fontSize = 15;
                            GUILayout.Label(mCameraContructor.commandList[i].type);
                            GUI.skin.label.fontSize = 12;

                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label("開始時間 : " + mCameraContructor.commandList[i].begintime.ToString("#0.00") + "秒");
                                GUILayout.Label("持續時間 : " + mCameraContructor.commandList[i].durationtime.ToString("#0.00") + "秒");
                                GUILayout.Label("目前時間 : " + mCameraContructor.commandList[i].time.ToString("#0.00") + "秒");
                            }
                            GUILayout.EndHorizontal();

                            ShowCommadeData(mCameraContructor.commandList[i]);
                        }
                        GUILayout.EndVertical();
                    }

                }
            }
            else
            {
                GUILayout.BeginHorizontal("無執行命令", "CN EntryInfoIcon");
                string log = "無執行命令";
                GUILayout.Label(log);
                GUILayout.EndHorizontal();

                mCommandFoldout = null;
            }
        }
        else
        {
            GUILayout.BeginHorizontal("", "Wizard Error");
            string log = "需要Unity Play";
            GUILayout.Label(log);
            GUILayout.EndHorizontal();
        }
    }

    /// <summary>
    /// 因每個命令數據都不同所以用這個生成
    /// </summary>
    private void ShowCommadeData(CommandStruct s)
    {
        switch (s.type)
        {
            case "randommove":
                CameraCommandRandommove outLabelRandommove = (CameraCommandRandommove)s;
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("X震動幅度 : " + outLabelRandommove.XShakeOffect.ToString("#0.0000"));
                    GUILayout.Label("Y震動幅度 : " + outLabelRandommove.YShakeOffect.ToString("#0.0000"));
                }
                GUILayout.EndHorizontal();
                GUILayout.Label("攝影機搖動位移 : " + outLabelRandommove.ShakeOffset);
                break;

            case "scale":
                CameraCommandScale outLabelScale = (CameraCommandScale)s;
                GUILayout.Label("攝影機縮放目標物 : " + outLabelScale.ScaleTarget);
                GUILayout.Label("攝影機與目標物的距離 : " + outLabelScale.Dis);
                GUILayout.Label("拉近距離 百分比 : " + (outLabelScale.Shorten * 100.0f).ToString("#0.00") + "%");
                break;

            case "motion":
                //CameraCommandMotion outLabelMotion = (CameraCommandMotion)s;
                break;

            case "reduce":
                CameraCommandReduce outLabelReduce = (CameraCommandReduce)s;
                GUILayout.Label("灰度值 : " + outLabelReduce.Screenreduce);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("淡入的時間點 : " +
                     (outLabelReduce.begintime + (outLabelReduce.durationtime * outLabelReduce.ScreenreduceDown)).ToString("#0.00") + "秒");

                    GUILayout.Label("淡出的時間點 : " +
                    (outLabelReduce.begintime + (outLabelReduce.durationtime * (outLabelReduce.ScreenreduceDuration + outLabelReduce.ScreenreduceDown))).ToString("#0.00") + "秒");
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("現在顏色 : " + outLabelReduce.ScreenreduceNowColor,
                     GUILayout.MaxWidth(300), GUILayout.MaxHeight(30));

                    GUI.backgroundColor = outLabelReduce.ScreenreduceNowColor;
                    GUILayout.BeginHorizontal("", "window");
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("設定顏色 : " + outLabelReduce.ScreenreduceTargetColor,
                    GUILayout.MaxWidth(300), GUILayout.MaxHeight(30));

                    GUI.backgroundColor = outLabelReduce.ScreenreduceTargetColor;
                    GUILayout.BeginHorizontal("", "window");
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndHorizontal();

                GUI.backgroundColor = Color.white;

                break;
        }
    }


    /// <summary>
    /// 堆疊裡的命令
    /// </summary>
    private void CommandPoolGUI()
    {
        //確認Unity 是否在Play
        if (EditorApplication.isPlaying)
        {
            if (mCameraContructor.commandPool != null)
            {
                //跑Dictionary
                foreach (var item in mCameraContructor.commandPool)
                {
                    int index = 0;

                    //計算List裡有幾個
                    for (int i = 0; i < item.Value.Count; i++)
                    {
                        if (item.Value[i] != null)
                        {
                            index++;
                        }
                    }

                    //顯示
                    GUILayout.BeginVertical("", "GroupBox");
                    {
                        GUILayout.Label("Keys : " + item.Key + " , " + "Value : " + index);
                    }
                    GUILayout.EndVertical();
                }
            }
        }
        else
        {
            GUILayout.BeginHorizontal("", "Wizard Error");
            string log = "需要Unity Play";
            GUILayout.Label(log);
            GUILayout.EndHorizontal();
        }
    }



}
