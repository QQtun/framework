using System.Collections;
using Core.Framework.Lua;
using Core.Framework.Scene;
using UnityEngine;

public class InitScene : MonoBehaviour
{
    private void Start()
    {
        // TODO check network state

        GameApp.Instance.StartInit();

        StartCoroutine(init());
    }

    private IEnumerator init()
    {
        LuaManager.Instance.DownloadAllLua();
        while (!LuaManager.Instance.IsLuaDownloaded)
            yield return null;
        LuaManager.Instance.StartLuaMain();

        SceneManager.Instance.CurSceneName = "InitScene";
        SceneManager.Instance.LoadScene("LoginScene");
    }
}
