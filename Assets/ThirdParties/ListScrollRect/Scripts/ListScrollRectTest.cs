using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListScrollRectTest : MonoBehaviour, IContentFiller
{
    public int count;
    public GameObject template;
    public ListScrollRect list;

    private int mCount = -1;

    int IContentFiller.GetItemCount()
    {
        return count;
    }

    int IContentFiller.GetItemType(int index)
    {
        return 0;
    }

    GameObject IContentFiller.GetListItem(int index, int itemType, GameObject obj)
    {
        if(obj == null)
        {
            obj = Instantiate(template);
            obj.SetActive(true);
        }
        return obj;
    }

    // Start is called before the first frame update
    private void Start()
    {
        template.SetActive(false);
        list = list ?? GetComponent<ListScrollRect>();
        list.RefreshContent();
        mCount = count;
    }

    private void Update()
    {
        if(mCount != count)
        {
            list.RefreshContent();
            mCount = count;
        }
    }
}
