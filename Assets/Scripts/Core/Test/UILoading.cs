using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILoading : UISingleton<UILoading>
{
    // Start is called before the first frame update
    void Start()
    {
        UILoading.Visible = true;
        if (UILoading.IsExist)
            UILoading.DestroyInstance();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
