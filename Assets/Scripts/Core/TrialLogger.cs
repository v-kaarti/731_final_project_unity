using System;
using System.IO;
using UnityEngine;

namespace MVPRTModality.Core
{
    public class TrialLogger : IDisposable
    {
        private StreamWriter _writer;
        public string FilePath { get; private set; }

        private static readonly string[] Header = new[]
        {
            "subject_id",
            "session_utc",
            "block_index",
            "modality",
            "trial_index_in_block",
            "trial_kind",
            "isi_ms",
            "t_block_start_unix_ms",
            "t_stim_scheduled_ms",
            "t_stim_rendered_ms",
            "t_response_ms",
            "rt_ms",
            "outcome",
            "unity_frame_index",
            "unity_display_hz",
        };

        public TrialLogger(string subjectId, string modalityCode, string sessionUtcIso)
        {
            string root = Path.Combine(Application.persistentDataPath, "MVPRTModality");
            Directory.CreateDirectory(root);

            string safeUtc = sessionUtcIso.Replace(":", "-");
            string filename = $"{subjectId}_{modalityCode}_{safeUtc}.csv";
            FilePath = Path.Combine(root, filename);

            _writer = new StreamWriter(FilePath, append: false);
            _writer.WriteLine(string.Join(",", Header));
            _writer.Flush();
            Debug.Log($"[TrialLogger] Logging to: {FilePath}");
        }

        public void Write(TrialRecord r)
        {
            if (_writer == null) return;
            _writer.WriteLine(r.ToCsvLine());
            _writer.Flush();
        }

        public void Dispose()
        {
            if (_writer != null)
            {
                _writer.Flush();
                _writer.Dispose();
                _writer = null;
            }
        }
    }
}
