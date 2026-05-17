using MVPRTModality.Core;
using UnityEngine;

namespace MVPRTModality.Inputs
{
    public class VRTriggerResponseInput : MonoBehaviour, IResponseInput
    {
        public string DebugLabel => "OVRTrigger";

        public bool ResponsePressedThisFrame()
        {
            return OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger)
                || OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger);
        }
    }
}
