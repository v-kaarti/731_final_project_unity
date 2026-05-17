using UnityEngine;

namespace MVPRTModality.Core
{
    public interface IResponseInput
    {
        bool ResponsePressedThisFrame();

        string DebugLabel { get; }
    }
}
