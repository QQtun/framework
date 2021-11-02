using System.Collections;
using System.Collections.Generic;
using Core.Framework.Scene;
using UnityEngine;

public class TestSceneManager : MonoBehaviour
{
    public string sceneName;

    public void ChangeScene()
    {
        SceneManager.Instance.LoadScene(sceneName);
    }
}
