using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Core.Framework.Audio;

[CustomEditor(typeof(AudioManager))]
public class AudioEditor : Editor
{
    private AudioManager mAudioManager;

    private bool[] mCommandFoldout = null; // 声明折叠菜单的状态
    private int mCommandFoldoutNum;//現在菜單數目

    private bool mAudioSetting = false;//開啟檢視群組設定

    override public void OnInspectorGUI()
    {
        mAudioManager = (AudioManager)target;

        //當前在場景上的Audio控制器
        GUILayout.BeginVertical("當前Audio List", "window");
        AudioGUI();
        GUILayout.EndVertical();

        GUILayout.Space(10);

        //物件池
        GUILayout.BeginVertical("Audio物件池", "window");

        GUILayout.Label("是否全體靜音 : " + mAudioManager.IsAllMute);

        GUILayout.Label("是否全體暫停 : " + mAudioManager.IsAllPause);

        List<string> muteString = new List<string>();
        List<string> pauseString = new List<string>();
        foreach (var item in mAudioManager.MAudioSetting)
        {
            if (item.Value.isMute == true)
            {
                muteString.Add(item.Key);
            }
            if (item.Value.isPause == true)
            {
                pauseString.Add(item.Key);
            }
        }
        GUILayout.Label("靜音中的群組 : \n");
        GUILayout.SelectionGrid(0, muteString.ToArray(), 2);

        GUILayout.Label("暫停撥放的群組 : \n");
        GUILayout.SelectionGrid(0, pauseString.ToArray(), 2);

        GUILayout.Space(10);

        GUILayout.Label("待命中Audio物件 : " + mAudioManager.AudioContructorPool.Count);

        GUILayout.Space(10);

        AudioShowSetting();

        GUILayout.EndVertical();

        base.OnInspectorGUI();
    }

    //生成GUI
    private void AudioGUI()
    {
        //新增 Toggle按鈕
        if (mCommandFoldoutNum != mAudioManager.AudioNowList.Keys.Count)
        {
            mCommandFoldout = new bool[mAudioManager.AudioNowList.Keys.Count];
            mCommandFoldoutNum = mAudioManager.AudioNowList.Keys.Count;
        }
        else
        {
            int index = 0;
            foreach (var item in mAudioManager.AudioNowList)
            {
                mCommandFoldout[index] = GUILayout.Toggle(mCommandFoldout[index], item.Key);
                //toggle按下時，開啟資料
                if (mCommandFoldout[index] == true)
                {
                    AudioStatus(item.Value);
                }
                index++;
            }
        }
    }

    //生成Audio狀態
    private void AudioStatus(List<AudioContructor> data)
    {

        for (var i = 0; i < data.Count; i++)
        {
            string type = "flow node ";

            //設定狀態
            {
                if (data[i].IsPause == true)
                {
                    type += 4;//黃色
                }

                if (data[i].audioSource.isPlaying == true)
                {
                    type += 3;//綠色
                }

                if (data[i].audioSource.isPlaying == false)
                {
                    type += 0;//灰色
                }
            }


            GUILayout.BeginVertical("", type);
            {
                EditorGUILayout.ObjectField("音樂文件", data[i].audioClip, typeof(AudioClip));

                if (data[i].audioSource.loop == true)
                {
                    GUILayout.Label("一直重複");
                }
                else
                {
                    GUILayout.Label("重複次數 : " + data[i].loopNum);
                }

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("延遲時間 : " + data[i].delayTime.ToString("#0.00"));
                    GUILayout.Label("當前時間 : " + data[i].MTime.ToString("#0.00"));
                }
                GUILayout.EndHorizontal();

                GUILayout.Label("音樂時間 : " + data[i].ClipTime.ToString("#0.000"));

                if (GUILayout.Button("尋找物件"))
                {
                    //找到GameObject ID
                    GameObject go = EditorUtility.InstanceIDToObject(data[i].gameObject.GetInstanceID()) as GameObject;
                    //選取到物件
                    Selection.activeGameObject = go;
                }
            }
            GUILayout.EndVertical();
        }

    }

    private void AudioShowSetting()
    {
        mAudioSetting = GUILayout.Toggle(mAudioSetting, "顯示群組設定");

        if (mAudioSetting == true)
        {
            foreach (var item in mAudioManager.MAudioSetting)
            {
                string type = "flow node hex 0";

                GUILayout.BeginVertical("", type);
                {
                    GUILayout.Label("群組 : " + item.Key);
                    GUILayout.Label("靜音 : " + item.Value.isMute);
                    GUILayout.Label("暫停 : " + item.Value.isPause);
                    GUILayout.Label("音量 : " + item.Value.volume);
                }
                GUILayout.EndVertical();
            }
        }
    }


}
