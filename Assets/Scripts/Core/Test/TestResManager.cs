using System.Collections;
using Core.Framework.Res;
using UnityEngine;

public class TestResManager : MonoBehaviour
{
    // Start is called before the first frame update
    IEnumerator Start()
    {
        var rm = ResourceManager.Instance;
        while (!rm.IsInitialized)
            yield return null;
    }

    [ContextMenu("LoadTest")]
    public void LoadTest()
    {
        //ResourceManager.Get.LoadAssetAsync<GameObject>("Assets/Arts/model/Equip/Prefab/1_back_25.prefab", (prefab) =>
        //{
        //    Instantiate(prefab);
        //});
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
