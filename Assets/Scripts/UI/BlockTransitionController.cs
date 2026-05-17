using MVPRTModality.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MVPRTModality.UI
{
    public class BlockTransitionController : MonoBehaviour
    {
        public Text MessageText;

        public string M1SceneName = "Block_M1_Mouse2D";
        public string M2SceneName = "Block_M2_Key2D";
        public string M3SceneName = "Block_M3_TriggerVR";

        private bool _routed;

        private void Start()
        {
            var cfg = SessionConfig.Instance;
            if (cfg == null)
            {
                if (MessageText != null) MessageText.text = "No session config. Return to main menu.";
                return;
            }
            if (MessageText != null)
                MessageText.text = $"Next block: {cfg.CurrentBlock}\n(any key / click / trigger to continue)";
        }

        private void Update()
        {
            if (_routed) return;
            if (Input.anyKeyDown || Input.GetMouseButtonDown(0) || AnyTrigger())
            {
                _routed = true;
                var cfg = SessionConfig.Instance;
                switch (cfg.CurrentBlock)
                {
                    case Modality.M1_Mouse2D: SceneManager.LoadScene(M1SceneName); break;
                    case Modality.M2_Key2D: SceneManager.LoadScene(M2SceneName); break;
                    case Modality.M3_TriggerVR: SceneManager.LoadScene(M3SceneName); break;
                }
            }
        }

        private static bool AnyTrigger()
        {
            return OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger)
                || OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger);
        }
    }
}
