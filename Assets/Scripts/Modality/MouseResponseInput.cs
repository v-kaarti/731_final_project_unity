using MVPRTModality.Core;
using UnityEngine;

namespace MVPRTModality.Inputs
{
    public class MouseResponseInput : MonoBehaviour, IResponseInput
    {
        public string DebugLabel => "MouseLeftClick";

        public bool ResponsePressedThisFrame()
        {
            return Input.GetMouseButtonDown(0);
        }
    }
}
