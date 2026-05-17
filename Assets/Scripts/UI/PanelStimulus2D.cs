using UnityEngine;
using UnityEngine.UI;

namespace MVPRTModality.UI
{
    [RequireComponent(typeof(Image))]
    public class PanelStimulus2D : MonoBehaviour, IStimulus
    {
        public Color OffColor = new Color(120f / 255f, 120f / 255f, 120f / 255f, 1f);
        public Color OnColor = new Color(60f / 255f, 220f / 255f, 90f / 255f, 1f);

        private Image _image;

        private void Awake()
        {
            _image = GetComponent<Image>();
            ShowOff();
        }

        public void ShowOff()
        {
            _image.enabled = true;
            _image.color = OffColor;
        }

        public void ShowOn()
        {
            _image.enabled = true;
            _image.color = OnColor;
        }

        public void HideEverything()
        {
            _image.enabled = false;
        }
    }
}
