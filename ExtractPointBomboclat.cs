using UnityEngine;

namespace RepoBomboclat;

public class ExtractPointBomboclat : MonoBehaviour
{
    private ExtractionPoint _extractionPoint;
    private Sound _bomboclatSound;

    private bool _played;

    private void Start()
    {
        _extractionPoint = GetComponent<ExtractionPoint>();
        _bomboclatSound = new Sound
        {
            Sounds = [RepoBomboclat.BomboclatClip]
        };
    }

    private void Update()
    {
        if (_played && _extractionPoint.StateIs(ExtractionPoint.State.Cancel))
        {
            // Stop playing bomboclat if EP state changes to cancel (item destroy / leave EP)
            RepoBomboclat.Logger.LogInfo("Extraction point is cancel, stop bomboclat");
            _played = false;
            _bomboclatSound.Stop();
        }

        if (!_extractionPoint.StateIs(ExtractionPoint.State.Surplus) || _played) return;

        if (RepoBomboclat.SurplusQuota.Value > 0.0f
            && _extractionPoint.haulSurplus < RepoBomboclat.SurplusQuota.Value)
            return;

        RepoBomboclat.Logger.LogInfo("EP surplus is: " + _extractionPoint.haulSurplus);

        if (RepoBomboclat.OnlyOnLastExtract.Value)
        {
            if (RoundDirector.instance.extractionPointsCompleted != RoundDirector.instance.extractionPoints - 1)
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