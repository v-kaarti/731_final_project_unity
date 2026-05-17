using MVPRTModality.Core;
using UnityEngine;

namespace MVPRTModality.Inputs
{
    public class KeyboardResponseInput : MonoBehaviour, IResponseInput
    {
        public string DebugLabel => "SpacebarDown";

        public bool ResponsePressedThisFrame()
        {
            return Input.GetKeyDown(KeyCode.Space);
        }
    }
}
