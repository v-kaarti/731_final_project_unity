using System;
using System.Collections;
using System.Collections.Generic;
using MVPRTModality.Core;
using MVPRTModality.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MVPRTModality.Core
{

    public class ReactionTimeController : MonoBehaviour
    {
        [Header("Wire these in the Inspector for each scene")]
        public MonoBehaviour StimulusBehaviour;
        public MonoBehaviour ResponseInputBehaviour;
        public Modality ModalityForThisScene = Modality.M1_Mouse2D;

        [Header("UI (optional, for between-trial guidance)")]
        public Text StatusText;
        public Text ProgressText;

        [Header("Next scene to load when this block ends")]
        public string NextSceneNameIfMoreBlocks = "BlockTransition";
        public string FinishedSceneName = "SessionDone";

        private IStimulus _stim;
        private IResponseInput _input;
        private TrialLogger _logger;
        private System.Random _rng;

        private bool _responsePressedSinceStim;
        private float _stimRenderedTimeMs;
        private float _responseTimeMs;
        private long _blockStartUnixMs;
        private float _blockStartGameTime;

        private void Start()
        {
            _stim = StimulusBehaviour as IStimulus;
            _input = ResponseInputBehaviour as IResponseInput;
            if (_stim == null)
                Debug.LogError("[RT] StimulusBehaviour does not implement IStimulus.");
            if (_input == null)
                Debug.LogError("[RT] ResponseInputBehaviour does not implement IResponseInput.");

            var cfg = SessionConfig.Instance;
            if (cfg == null)
            {
                Debug.LogError("[RT] No SessionConfig in scene. Start the session from the Main Menu.");
                return;
            }

            _rng = new System.Random(cfg.Seed + cfg.CurrentBlockIndex * 1009);

            if (cfg.CurrentBlock != ModalityForThisScene)
            {
                Debug.LogWarning(
                    $"[RT] Scene modality {ModalityForThisScene} doesn't match expected " +
                    $"{cfg.CurrentBlock}. Continuing, but check the scene routing.");
            }

            _logger = new TrialLogger(cfg.SubjectId, ModalityForThisScene.ShortCode(), cfg.SessionUtcIso);

            StartCoroutine(RunBlock());
        }

        private IEnumerator RunBlock()
        {
            var cfg = SessionConfig.Instance;

            yield return StartCoroutine(ShowMessage(
                $"Block {cfg.CurrentBlockIndex + 1} of {cfg.BlockOrder.Count}\n" +
                $"Modality: {ModalityForThisScene}\n" +
                $"Press response to begin.",
                waitForResponse: true));

            List<TrialKind> schedule = BuildTrialSchedule(cfg.GoTrialsPerBlock, cfg.CatchTrialsPerBlock);
            _blockStartUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _blockStartGameTime = Time.realtimeSinceStartup;

            for (int i = 0; i < schedule.Count; i++)
            {
                var kind = schedule[i];
                yield return StartCoroutine(RunOneTrial(i, kind));
                UpdateProgress(i + 1, schedule.Count);
                yield return new WaitForSeconds(0.4f);
            }

            _logger.Dispose();

            bool more = cfg.AdvanceBlock();
            yield return StartCoroutine(ShowMessage(
                more ? "Block complete. Stand up, stretch.\nPress response when ready for the next block."
                     : "Session complete. Thank you.\nPress response to exit.",
                waitForResponse: true));

            SceneManager.LoadScene(more ? NextSceneNameIfMoreBlocks : FinishedSceneName);
        }

        private IEnumerator RunOneTrial(int trialIndex, TrialKind kind)
        {
            var cfg = SessionConfig.Instance;

            float isiSec = (float)_rng.NextDouble() * (cfg.IsiMaxSeconds - cfg.IsiMinSeconds) + cfg.IsiMinSeconds;
            _stim.ShowOff();

            float isiStartGame = Time.realtimeSinceStartup;
            _responsePressedSinceStim = false;
            _responseTimeMs = -1f;
            _stimRenderedTimeMs = -1f;

            while ((Time.realtimeSinceStartup - isiStartGame) < isiSec)
            {
                if (_input != null && _input.ResponsePressedThisFrame())
                {
                    _responseTimeMs = NowBlockMs();
                    _responsePressedSinceStim = true;
                    yield return new WaitForSeconds(Mathf.Max(0f, isiSec - (Time.realtimeSinceStartup - isiStartGame)));
                    break;
                }
                yield return null;
            }

            float tScheduledMs = NowBlockMs();

            if (kind == TrialKind.Go)
            {
                _stim.ShowOn();
                yield return new WaitForEndOfFrame();
                _stimRenderedTimeMs = NowBlockMs();

                float windowStart = Time.realtimeSinceStartup;
                if (!_responsePressedSinceStim)
                {
                    while ((Time.realtimeSinceStartup - windowStart) < cfg.ResponseWindowSeconds)
                    {
                        if (_input.ResponsePressedThisFrame())
                        {
                            _responseTimeMs = NowBlockMs();
                            _responsePressedSinceStim = true;
                            break;
                        }
                        yield return null;
                    }
                }

                _stim.ShowOff();
                LogGoTrial(trialIndex, isiSec * 1000f, tScheduledMs);
            }
            else
            {
                _stimRenderedTimeMs = -1f;
                float windowStart = Time.realtimeSinceStartup;
                if (!_responsePressedSinceStim)
                {
                    while ((Time.realtimeSinceStartup - windowStart) < cfg.CatchWindowSeconds)
                    {
                        if (_input.ResponsePressedThisFrame())
                        {
                            _responseTimeMs = NowBlockMs();
                            _responsePressedSinceStim = true;
                            break;
                        }
                        yield return null;
                    }
                }
                LogCatchTrial(trialIndex, isiSec * 1000f, tScheduledMs);
            }
        }

        private void LogGoTrial(int trialIndex, float isiMs, float tScheduledMs)
        {
            var cfg = SessionConfig.Instance;
            TrialOutcome outcome;
            float rtMs = -1f;

            if (_responseTimeMs < 0f)
            {
                outcome = TrialOutcome.Miss;
            }
            else if (_responseTimeMs < _stimRenderedTimeMs)
            {
                outcome = TrialOutcome.Anticipation;
            }
            else
            {
                rtMs = _responseTimeMs - _stimRenderedTimeMs;
                outcome = (rtMs < cfg.AnticipationCutoffMs) ? TrialOutcome.Anticipation : TrialOutcome.Hit;
            }

            _logger.Write(new TrialRecord
            {
                SubjectId = cfg.SubjectId,
                SessionUtcIso = cfg.SessionUtcIso,
                BlockIndex = cfg.CurrentBlockIndex,
                Modality = ModalityForThisScene.ShortCode(),
                TrialIndexInBlock = trialIndex,
                TrialKindString = "go",
                IsiMs = isiMs,
                TBlockStartUnixMs = _blockStartUnixMs,
                TStimScheduledMs = tScheduledMs,
                TStimRenderedMs = _stimRenderedTimeMs,
                TResponseMs = _responseTimeMs,
                RtMs = rtMs,
                Outcome = outcome.ToString().ToLowerInvariant(),
                UnityFrameIndex = Time.frameCount,
                UnityDisplayHz = DisplayHzSafe(),
            });
        }

        private void LogCatchTrial(int trialIndex, float isiMs, float tScheduledMs)
        {
            var cfg = SessionConfig.Instance;
            TrialOutcome outcome = (_responseTimeMs < 0f) ? TrialOutcome.CorrectRejection : TrialOutcome.FalseAlarm;

            _logger.Write(new TrialRecord
            {
                SubjectId = cfg.SubjectId,
                SessionUtcIso = cfg.SessionUtcIso,
                BlockIndex = cfg.CurrentBlockIndex,
                Modality = ModalityForThisScene.ShortCode(),
                TrialIndexInBlock = trialIndex,
                TrialKindString = "catch",
                IsiMs = isiMs,
                TBlockStartUnixMs = _blockStartUnixMs,
                TStimScheduledMs = tScheduledMs,
                TStimRenderedMs = -1f,
                TResponseMs = _responseTimeMs,
                RtMs = -1f,
                Outcome = outcome.ToString().ToLowerInvariant(),
                UnityFrameIndex = Time.frameCount,
                UnityDisplayHz = DisplayHzSafe(),
            });
        }

        private float NowBlockMs() => (Time.realtimeSinceStartup - _blockStartGameTime) * 1000f;

        private static float DisplayHzSafe()
        {
            var r = Screen.currentResolution;
            double rr = r.refreshRateRatio.value;
            return (float)rr;
        }

        private List<TrialKind> BuildTrialSchedule(int nGo, int nCatch)
        {
            var list = new List<TrialKind>(nGo + nCatch);
            for (int i = 0; i < nGo; i++) list.Add(TrialKind.Go);
            for (int i = 0; i < nCatch; i++) list.Add(TrialKind.Catch);
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
            return list;
        }

        private void UpdateProgress(int done, int total)
        {
            if (ProgressText != null) ProgressText.text = $"{done} / {total}";
        }

        private IEnumerator ShowMessage(string msg, bool waitForResponse)
        {
            _stim.HideEverything();
            if (StatusText != null) StatusText.text = msg;

            if (waitForResponse)
            {
                yield return null;
                while (_input == null || !_input.ResponsePressedThisFrame()) yield return null;
                yield return null;
            }

            if (StatusText != null) StatusText.text = "";
        }
    }
}
