using UnityEngine;
using UnityEngine.UI;

namespace Core.Framework.Localization
{
    [RequireComponent(typeof(Text))]
    public class LocalizedText : MonoBehaviour
    {
        public Text text;

        [SerializeField]
        private string mKey;
        private object[] mArgs;

        private void Start()
        {
            I2.Loc.LocalizationManager.OnLocalizeEvent += UpdateText;

            if (!string.IsNullOrEmpty(mKey))
                UpdateText();
        }

        private void OnDestroy()
        {
            I2.Loc.LocalizationManager.OnLocalizeEvent -= UpdateText;
        }

        public void SetText(string key, params object[] args)
        {
            mKey = key;
            mArgs = args;
            UpdateText();
        }

        private void UpdateText()
        {
            if (text == null)
                text = GetComponent<Text>();

            if(text != null)
                text.text = LocalizationUtil.GetTranslationFormat(mKey, mArgs);
        }
    }
}