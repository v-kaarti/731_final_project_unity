using MVPRTModality.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MVPRTModality.UI
{
    public class MainMenuController : MonoBehaviour
    {
        public InputField SubjectIdField;
        public Button StartButton;
        public Text WarningText;

        public string M1SceneName = "Block_M1_Mouse2D";
        public string M2SceneName = "Block_M2_Key2D";
        public string M3SceneName = "Block_M3_TriggerVR";

        private SessionConfig _config;

        private void Awake()
        {
            _config = SessionConfig.Instance;
            if (_config == null)
            {
                var go = new GameObject("SessionConfig");
                _config = go.AddComponent<SessionConfig>();
            }

            if (StartButton != null) StartButton.onClick.AddListener(OnStartClicked);
        }

        private void OnStartClicked()
        {
            string sid = (SubjectIdField != null) ? SubjectIdField.text : "";
            if (string.IsNullOrWhiteSpace(sid))
            {
                if (WarningText != null) WarningText.text = "Subject id required (e.g., S01).";
                return;
            }

            _config.InitializeForSubject(sid);
            LoadCurrentBlockScene();
        }

        private void LoadCurrentBlockScene()
        {
            switch (_config.CurrentBlock)
            {
                case Modality.M1_Mouse2D: SceneManager.LoadScene(M1SceneName); break;
                case Modality.M2_Key2D:   SceneManager.LoadScene(M2SceneName); break;
                case Modality.M3_TriggerVR: SceneManager.LoadScene(M3SceneName); break;
            }
        }
    }
}
