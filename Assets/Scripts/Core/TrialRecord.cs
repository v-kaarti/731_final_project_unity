using System.Globalization;

namespace MVPRTModality.Core
{
    public enum TrialKind { Go, Catch }

    public enum TrialOutcome
    {
        Hit,
        Miss,
        Anticipation,
        CorrectRejection,
        FalseAlarm,
    }

    public class TrialRecord
    {
        public string SubjectId;
        public string SessionUtcIso;
        public int BlockIndex;
        public string Modality;
        public int TrialIndexInBlock;
        public string TrialKindString;
        public float IsiMs;
        public long TBlockStartUnixMs;
        public float TStimScheduledMs;
        public float TStimRenderedMs;
        public float TResponseMs;
        public float RtMs;
        public string Outcome;
        public int UnityFrameIndex;
        public float UnityDisplayHz;

        public string ToCsvLine()
        {
            var ci = CultureInfo.InvariantCulture;
            return string.Join(",", new string[]
            {
                Escape(SubjectId),
                Escape(SessionUtcIso),
                BlockIndex.ToString(ci),
                Escape(Modality),
                TrialIndexInBlock.ToString(ci),
                Escape(TrialKindString),
                IsiMs.ToString("0.###", ci),
                TBlockStartUnixMs.ToString(ci),
                TStimScheduledMs.ToString("0.###", ci),
                TStimRenderedMs.ToString("0.###", ci),
                TResponseMs.ToString("0.###", ci),
                RtMs.ToString("0.###", ci),
                Escape(Outcome),
                UnityFrameIndex.ToString(ci),
                UnityDisplayHz.ToString("0.###", ci),
            });
        }

        private static string Escape(string s)
        {
            if (s == null) return "";
            if (s.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0)
            {
                return "\"" + s.Replace("\"", "\"\"") + "\"";
            }
            return s;
        }
    }
}
