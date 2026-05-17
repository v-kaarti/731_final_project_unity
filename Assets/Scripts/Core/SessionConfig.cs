using System;
using System.Collections.Generic;
using UnityEngine;

namespace MVPRTModality.Core
{

    public class SessionConfig : MonoBehaviour
    {
        public static SessionConfig Instance { get; private set; }

        // TOGGLE THIS!
        public const bool IncludeVRBlock = true;

        // TOGGLE THIS!
        public const bool VROnlySession = true;

        public string SubjectId = "S00";
        public string SessionUtcIso = "";
        public int Seed = 12345;

        public List<Modality> BlockOrder = new List<Modality>();
        public int CurrentBlockIndex = 0;

        public int GoTrialsPerBlock = 100;
        public int CatchTrialsPerBlock = 10;
        public float IsiMinSeconds = 1.5f;
        public float IsiMaxSeconds = 3.5f;
        public float ResponseWindowSeconds = 2.0f;
        public float CatchWindowSeconds = 3.0f;
        public float AnticipationCutoffMs = 100f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void InitializeForSubject(string subjectId)
        {
            SubjectId = subjectId.Trim().ToUpperInvariant();
            SessionUtcIso = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            Seed = StableSeedFromSubject(SubjectId);
            BlockOrder = LatinSquareOrderFor(SubjectId);
            CurrentBlockIndex = 0;
        }

        public Modality CurrentBlock => BlockOrder[CurrentBlockIndex];

        public bool AdvanceBlock()
        {
            CurrentBlockIndex++;
            return CurrentBlockIndex < BlockOrder.Count;
        }

        private static List<Modality> LatinSquareOrderFor(string subjectId)
        {
            if (VROnlySession)
            {
                return new List<Modality> { Modality.M3_TriggerVR };
            }

            if (!IncludeVRBlock)
            {
                switch (subjectId)
                {
                    case "S01":
                        return new List<Modality> { Modality.M1_Mouse2D, Modality.M2_Key2D };
                    case "S02":
                        return new List<Modality> { Modality.M2_Key2D, Modality.M1_Mouse2D };
                    case "S03":
                        return new List<Modality> { Modality.M1_Mouse2D, Modality.M2_Key2D };
                    default:
                        Debug.LogWarning($"[SessionConfig] Unknown subjectId '{subjectId}', using S01 (2D-only) block order for piloting.");
                        return new List<Modality> { Modality.M1_Mouse2D, Modality.M2_Key2D };
                }
            }

            switch (subjectId)
            {
                case "S01":
                    return new List<Modality> { Modality.M1_Mouse2D, Modality.M2_Key2D, Modality.M3_TriggerVR };
                case "S02":
                    return new List<Modality> { Modality.M2_Key2D, Modality.M3_TriggerVR, Modality.M1_Mouse2D };
                case "S03":
                    return new List<Modality> { Modality.M3_TriggerVR, Modality.M1_Mouse2D, Modality.M2_Key2D };
                default:
                    Debug.LogWarning($"[SessionConfig] Unknown subjectId '{subjectId}', using S01 block order for piloting.");
                    return new List<Modality> { Modality.M1_Mouse2D, Modality.M2_Key2D, Modality.M3_TriggerVR };
            }
        }

        private static int StableSeedFromSubject(string subjectId)
        {
            unchecked
            {
                int h = 17;
                foreach (char c in subjectId) h = h * 31 + c;
                return h;
            }
        }
    }

    public enum Modality
    {
        M1_Mouse2D,
        M2_Key2D,
        M3_TriggerVR,
    }

    public static class ModalityExtensions
    {
        public static string ShortCode(this Modality m)
        {
            switch (m)
            {
                case Modality.M1_Mouse2D: return "M1";
                case Modality.M2_Key2D: return "M2";
                case Modality.M3_TriggerVR: return "M3";
                default: return "??";
            }
        }

        public static bool IsVR(this Modality m) => m == Modality.M3_TriggerVR;
    }
}
