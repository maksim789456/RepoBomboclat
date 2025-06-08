using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace RepoBomboclat;

public class ExtractPointBomboclat : MonoBehaviour
{
    private ExtractionPoint _extractionPoint;
    private Sound _bomboclatSound;

    private bool _played;
    private FieldInfo _haulSurplusField;
    private FieldInfo _currentStateField;
    private FieldInfo _extractionPointsCompletedField;
    private FieldInfo _extractionPointsField;

    private void Start()
    {
        _extractionPoint = GetComponent<ExtractionPoint>();
        _bomboclatSound = new Sound
        {
            Sounds = [RepoBomboclat.BomboclatClip]
        };

        _haulSurplusField = AccessTools.Field(typeof(ExtractionPoint), "haulSurplus");
        _currentStateField = AccessTools.Field(typeof(ExtractionPoint), "currentState");
        _extractionPointsCompletedField = AccessTools.Field(typeof(RoundDirector), "extractionPointsCompleted");
        _extractionPointsField = AccessTools.Field(typeof(RoundDirector), "extractionPoints");
    }

    private void Update()
    {
        var currentState = (ExtractionPoint.State)_currentStateField.GetValue(_extractionPoint);
        if (_played && currentState == ExtractionPoint.State.Cancel)
        {
            // Stop playing bomboclat if EP state changes to cancel (item destroy / leave EP)
            RepoBomboclat.Logger.LogInfo("Extraction point is cancel, stop bomboclat");
            _played = false;
            _bomboclatSound.Stop();
        }

        if (currentState != ExtractionPoint.State.Surplus || _played) return;

        var haulSurplus = (int)_haulSurplusField.GetValue(_extractionPoint);
        if (RepoBomboclat.SurplusQuota.Value > 0.0f
            && haulSurplus < RepoBomboclat.SurplusQuota.Value)
            return;

        RepoBomboclat.Logger.LogInfo("EP surplus is: " + haulSurplus);

        if (RepoBomboclat.OnlyOnLastExtract.Value)
        {
            var extractionPointsCompleted = (int)_extractionPointsCompletedField.GetValue(_extractionPoint);
            var extractionPoints = (int)_extractionPointsField.GetValue(_extractionPoint);
            if (extractionPointsCompleted != extractionPoints - 1)
                return;

            RepoBomboclat.Logger.LogInfo("Last extraction point at surplus, playing bomboclat");
            PlaySound();
            return;
        }

        RepoBomboclat.Logger.LogInfo("Extraction point at surplus, playing bomboclat");
        PlaySound();
    }

    private void PlaySound()
    {
        _played = true;
        _bomboclatSound.Source = _bomboclatSound.Play(_extractionPoint.transform.position);
    }
}