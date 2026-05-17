using UnityEngine;

namespace MVPRTModality.UI
{
    [RequireComponent(typeof(Renderer))]
    public class CubeStimulusVR : MonoBehaviour, IStimulus
    {
        public Color OffColor = new Color(120f / 255f, 120f / 255f, 120f / 255f, 1f);
        public Color OnColor = new Color(60f / 255f, 220f / 255f, 90f / 255f, 1f);

        private Renderer _renderer;
        private MaterialPropertyBlock _mpb;
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int LegacyColorId = Shader.PropertyToID("_Color");

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _mpb = new MaterialPropertyBlock();
            ShowOff();
        }

        private void SetColor(Color c)
        {
            _renderer.enabled = true;
            _renderer.GetPropertyBlock(_mpb);
            _mpb.SetColor(BaseColorId, c);
            _mpb.SetColor(LegacyColorId, c);
            _renderer.SetPropertyBlock(_mpb);
        }

        public void ShowOff() => SetColor(OffColor);
        public void ShowOn() => SetColor(OnColor);

        public void HideEverything()
        {
            _renderer.enabled = false;
        }
    }
}
