using UnityEngine;

public class UISingleton<T> : MonoBehaviour where T : UISingleton<T> {
    private static T instance = null;

	protected virtual void Init() {
	}
	
	protected virtual void OnDestroyInstance() {
	}
	
	public static T Get {
		get {
			if (instance != null)
				return instance;

			string name = typeof(T).ToString();
			GameObject obj = Resources.Load("Prefabs/UI/" + name) as GameObject;
			if (obj) {
				obj = Instantiate(obj);

				instance = obj.GetComponent<T>();
				obj.name = name;

				if (instance == null) {
					instance = obj.AddComponent<T>();
				}

				instance.setParent();
				instance.Init();
				return instance;
			} else
				return null;
		}
	}

	private void setParent() {
		int layout = gameObject.layer;
		GameObject parent = null;
		switch (layout) {
			case 5:
				parent = GameObject.Find("UI2D/Canvas");
				break;
			case 8:
				parent = GameObject.Find("UI3D/Canvas");
				break;
			case 9:
				parent = GameObject.Find("ARManager/Canvas");
				break;
			case 10:
				parent = GameObject.Find("UITop/Canvas");
				break;
			case 11:
				parent = GameObject.Find("UIForeground/Canvas");
				break;
		}

		if (parent) {
			RectTransform rect = gameObject.GetComponent<RectTransform>();
			if (rect) {
				Vector2 offsetMin = rect.offsetMin;
				Vector2 offsetMax = rect.offsetMax;
				Vector3 localPosition = rect.localPosition;
				Vector2 anchoredPosition = rect.anchoredPosition;

				gameObject.transform.SetParent(parent.transform);

				rect.localScale = Vector3.one;
				rect.localPosition = localPosition;
				rect.anchoredPosition = anchoredPosition;
				rect.offsetMin = offsetMin;
				rect.offsetMax = offsetMax;
			}
		}
	}

	public static void DestroyInstance() {
		if (instance == null)
			return;

		instance.OnDestroyInstance();
		Destroy(instance.gameObject, 0.2f);
		instance = null;
	}

	public static bool Visible {
		get { return (instance && instance.gameObject.activeInHierarchy); }
        set {
			if (Get) {
				Get.gameObject.SetActive(value);

				if (value && Get.transform.parent == null) {
					Get.setParent();
				}
			}
		}
	}

	public static bool IsExist {
		get { return (instance != null); }
	}
}