using Core.Framework.UI;
using Core.Game.UI;
using UnityEngine;

public class TestUIManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    [ContextMenu("Open1")]
    public void Open1()
    {
        //UIManager.Instance.Open(UIName.TestUI1);
    }

    [ContextMenu("Open2")]
    public void Open2()
    {
        //UIManager.Instance.Open(UIName.TestUI2);
    }

    [ContextMenu("Open3")]
    public void Open3()
    {
        //UIManager.Instance.Open(UIName.TestUI3);
    }

    [ContextMenu("Close1")]
    public void Close1()
    {
        //UIManager.Instance.Close(UIName.TestUI1);
    }

    [ContextMenu("Close2")]
    public void Close2()
    {
        //UIManager.Instance.Close(UIName.TestUI2);
    }

    [ContextMenu("Close3")]
    public void Close3()
    {
        //UIManager.Instance.Close(UIName.TestUI3);
    }

    [ContextMenu("CloseTop")]
    public void CloseTop()
    {
        UIManager.Instance.CloseTopWindow();
    }
}
