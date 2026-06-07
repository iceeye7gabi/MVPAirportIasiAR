using UnityEngine;
using UnityEngine.UI;

namespace AirportAR.AR
{
    /// <summary>
    /// Debug bar showing microphone input level while listening.
    /// </summary>
    public class VoiceLevelMeter : MonoBehaviour
    {
        [SerializeField] GameObject meterRoot;
        [SerializeField] Image fillBar;
        [SerializeField] Text levelLabel;

        float targetLevel;
        float displayLevel;

        public void SetVisible(bool visible)
        {
            if (meterRoot != null)
            {
                meterRoot.SetActive(visible);
            }

            if (!visible)
            {
                targetLevel = 0f;
                displayLevel = 0f;
                ApplyFill();
            }
        }

        public void SetLevel(float normalized)
        {
            targetLevel = Mathf.Clamp01(normalized);
        }

        void Update()
        {
            displayLevel = Mathf.Lerp(displayLevel, targetLevel, Time.deltaTime * 10f);
            ApplyFill();
        }

        void ApplyFill()
        {
            if (fillBar != null)
            {
                fillBar.fillAmount = displayLevel;
            }

            if (levelLabel != null)
            {
                levelLabel.text = $"Microfon: {Mathf.RoundToInt(displayLevel * 100f)}%";
            }
        }
    }
}
