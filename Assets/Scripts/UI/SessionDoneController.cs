using UnityEngine;
using UnityEngine.UI;

namespace MVPRTModality.UI
{
    public class SessionDoneController : MonoBehaviour
    {
        public Text MessageText;

        private void Start()
        {
            if (MessageText != null)
                MessageText.text =
                    "Session complete. Data saved to Application.persistentDataPath/MVPRTModality.\n" +
                    "Quest path: /sdcard/Android/data/<package>/files/MVPRTModality/\n\n" +
                    "Close the app and adb-pull the files.";
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }
    }
}
