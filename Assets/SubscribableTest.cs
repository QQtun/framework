using Core.Framework.Event.Property;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubscribableTest : MonoBehaviour
{
    [Serializable]
    public class Data
    {
        public int intValue;
        public string strValue;
    }

    public enum Test
    {
        A,
        B,
        C
    }

    [Serializable]
    public class InnerClass
    {
        public SubscribableField<int> innerIntField = new SubscribableField<int>(1);
    }

    public SubscribableField<int> intField = new SubscribableField<int>(1);
    public SubscribableField<float> floatField = new SubscribableField<float>(1);
    public SubscribableField<string> stringField = new SubscribableField<string>("BB");
    public SubscribableField<bool> boolField = new SubscribableField<bool>(true);
    public SubscribableField<Test> enumField = new SubscribableField<Test>(Test.B);
    public SubscribableField<GameObject> objectField = new SubscribableField<GameObject>(null);

    public InnerClass innerClass = new InnerClass();

    public SubscribableList<int> intListField = new SubscribableList<int>();
    public SubscribableList<Data> objListField = new SubscribableList<Data>();
    public SubscribableList<SubscribableField<Data>> obj2ListField = new SubscribableList<SubscribableField<Data>>();

    void Start()
    {
        intField.AddValueChangeListener(OnIntFieldChanged);
        floatField.AddValueChangeListener(OnFloatFieldChanged);
        stringField.AddValueChangeListener(OnStringFieldChanged);
        boolField.AddValueChangeListener(OnBoolFieldChanged);
        enumField.AddValueChangeListener(OnEnumFieldChanged);
        objectField.AddValueChangeListener(OnObjectFieldChanged);
        innerClass.innerIntField.AddValueChangeListener(OnIntFieldChanged2);

        intListField.SubscribeItemAddedEvent(OnListAdd);
        intListField.SubscribeItemClearedEvent(OnListClear);
        intListField.SubscribeItemRemovedEvent(OnListRemoveAt);
        intListField.SubscribeItemInsertedEvent(OnListInsert);
        intListField.SubscribeItemReplacedEvent(OnListItemReplace);

        objListField.SubscribeItemAddedEvent(OnListAdd2);
        objListField.SubscribeItemClearedEvent(OnListClear2);
        objListField.SubscribeItemRemovedEvent(OnListRemoveAt2);
        objListField.SubscribeItemInsertedEvent(OnListInsert2);
        objListField.SubscribeItemReplacedEvent(OnListItemReplace2);

        obj2ListField.SubscribeItemAddedEvent(OnListAdd3);
        obj2ListField.SubscribeItemClearedEvent(OnListClear3);
        obj2ListField.SubscribeItemRemovedEvent(OnListRemoveAt3);
        obj2ListField.SubscribeItemInsertedEvent(OnListInsert3);
        obj2ListField.SubscribeItemReplacedEvent(OnListItemReplace3);
    }

    private void OnObjectFieldChanged(GameObject last, GameObject current)
    {
        Debug.Log($"OnObjectFieldChanged last={last} current={current}");
    }

    private void OnEnumFieldChanged(Test last, Test current)
    {
        Debug.Log($"OnEnumFieldChanged last={last} current={current}");
    }

    private void OnBoolFieldChanged(bool last, bool current)
    {
        Debug.Log($"OnBoolFieldChanged last={last} current={current}");
    }

    private void OnStringFieldChanged(string last, string current)
    {
        Debug.Log($"OnStringFieldChanged last={last} current={current}");
    }

    private void OnFloatFieldChanged(float last, float current)
    {
        Debug.Log($"OnFloatFieldChanged last={last} current={current}");
    }
    private void OnIntFieldChanged2(int last, int current)
    {
        Debug.Log($"OnIntFieldChanged2 last={last} current={current}");
    }

    private void OnIntFieldChanged(int last, int current)
    {
        Debug.Log($"OnIntFieldChanged last={last} current={current}");
    }
    private void OnListItemReplace3(SubscribableListItemRepalceEvent<SubscribableField<Data>> evt)
    {
        Debug.Log($"OnListItemReplace3 count={evt.List.Count} index={evt.Index}");
    }

    private void OnListInsert3(SubscribableListItemInsertEvent<SubscribableField<Data>> evt)
    {
        Debug.Log($"OnListInsert3 count={evt.List.Count} index={evt.Index}");
    }

    private void OnListRemoveAt3(SubscribableListItemRemoveEvent<SubscribableField<Data>> evt)
    {
        Debug.Log($"OnListRemoveAt3 count={evt.List.Count} index={evt.Index} item={evt.RemovedItem}");
    }

    private void OnListClear3(SubscribableListClearEvent<SubscribableField<Data>> evt)
    {
        Debug.Log($"OnListClear3 count={evt.List.Count} lastCount={evt.LastCount}");
    }

    private void OnListAdd3(SubscribableListItemAddEvent<SubscribableField<Data>> evt)
    {
        Debug.Log($"OnListAdd3 count={evt.List.Count}");
    }

    private void OnListItemReplace2(SubscribableListItemRepalceEvent<Data> evt)
    {
        Debug.Log($"OnListItemReplace2 count={evt.List.Count} index={evt.Index}");
    }

    private void OnListInsert2(SubscribableListItemInsertEvent<Data> evt)
    {
        Debug.Log($"OnListInsert2 count={evt.List.Count} index={evt.Index}");
    }

    private void OnListRemoveAt2(SubscribableListItemRemoveEvent<Data> evt)
    {
        Debug.Log($"OnListRemoveAt2 count={evt.List.Count} index={evt.Index} item={evt.RemovedItem}");
    }

    private void OnListClear2(SubscribableListClearEvent<Data> evt)
    {
        Debug.Log($"OnListClear2 count={evt.List.Count} lastCount={evt.LastCount}");
    }

    private void OnListAdd2(SubscribableListItemAddEvent<Data> evt)
    {
        Debug.Log($"OnListAdd2 count={evt.List.Count}");
    }

    private void OnListItemReplace(SubscribableListItemRepalceEvent<int> evt)
    {
        Debug.Log($"OnListItemReplace count={evt.List.Count} index={evt.Index}");
    }

    private void OnListInsert(SubscribableListItemInsertEvent<int> evt)
    {
        Debug.Log($"OnListInsert count={evt.List.Count} index={evt.Index}");
    }

    private void OnListRemoveAt(SubscribableListItemRemoveEvent<int> evt)
    {
        Debug.Log($"OnListRemoveAt count={evt.List.Count} index={evt.Index} item={evt.RemovedItem}");
    }

    private void OnListClear(SubscribableListClearEvent<int> evt)
    {
        Debug.Log($"OnListClear count={evt.List.Count} lastCount={evt.LastCount}");
    }

    private void OnListAdd(SubscribableListItemAddEvent<int> evt)
    {
        Debug.Log($"OnListAdd count={evt.List.Count}");
    }
}
