using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SingletonPrefabAttribute : Attribute
{
	public string Path { get; set; }

	public SingletonPrefabAttribute(string path)
    {
		Path = path;
    }
}

public class Singleton<T> : MonoBehaviour where T : Singleton<T> {
    private static GameObject sSingletonRoot = null;
    private static T sInstance = null;
	

	protected virtual void Init() {
	}
	
	protected virtual void OnDestroyInstance() {
	}
	
	public static T Instance {
		get {
			if (!Application.isPlaying)
				return null;
#if UNITY_EDITOR
			if (!EditorApplication.isPlayingOrWillChangePlaymode)
				return null; // 準備關閉play mode
#endif

			if (sInstance != null)
				return sInstance;

            sInstance = FindObjectOfType(typeof(T)) as T;
			if (sInstance == null) {
				var type = typeof(T);
				var attrs = type.GetCustomAttributes(typeof(SingletonPrefabAttribute), false) as SingletonPrefabAttribute[];
				if(attrs != null && attrs.Length > 0)
                {
					var attr = attrs[0];
					var prefab = Resources.Load(attr.Path) as GameObject;
					var obj = Instantiate(prefab);
                    obj.name = typeof(T).ToString();
                    sInstance = obj.GetComponent<T>();
				}
                else
				{
					GameObject obj = new GameObject();
					obj.name = typeof(T).ToString();
					sInstance = obj.AddComponent<T>();
				}
			}

            if(sSingletonRoot == null)
            {
                sSingletonRoot = GameObject.Find("All Singleton");
                if (sSingletonRoot == null)
                {
                    sSingletonRoot = new GameObject("All Singleton");
                    DontDestroyOnLoad(sSingletonRoot);
                }
            }
            sInstance.transform.SetParent(sSingletonRoot.transform);

            //DontDestroyOnLoad(sInstance);
			sInstance.Init();
			return sInstance;
		}
	}

	public static void DestroyInstance() {
		if (sInstance == null)
			return;

		sInstance.OnDestroyInstance();
		Destroy(sInstance.gameObject);
		sInstance = null;
	}

	public static bool Visible {
		get { return (sInstance && sInstance.gameObject.activeInHierarchy); }
        set {
			if (Instance)
				Instance.gameObject.SetActive(value);
		}
	}

	public static bool IsExist {
		get { return (sInstance != null); }
	}
}