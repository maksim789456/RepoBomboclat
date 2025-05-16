using System;
using System.IO;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

namespace RepoBomboclat.Audio;

public static class AudioClipLoader
{
    [CanBeNull]
    public static AudioClip Load(string path)
    {
        var filename = Path.GetFileName(path);
        var extension = Path.GetExtension(path);
        if (string.IsNullOrEmpty(extension))
        {
            RepoBomboclat.Logger.LogError($"The path {path} is invalid.");
            return null;
        }

        var audioType = extension switch
        {
            ".wav" => AudioType.WAV,
            ".ogg" => AudioType.OGGVORBIS,
            ".mp3" => AudioType.MPEG,
            _ => AudioType.UNKNOWN
        };

        if (audioType == AudioType.UNKNOWN)
        {
            RepoBomboclat.Logger.LogError($"{filename} not loaded, because it is an unknown audio type.");
            return null;
        }

        return Load(path, audioType);
    }

    [CanBeNull]
    public static AudioClip Load(string path, AudioType type)
    {
        using var uwr = UnityWebRequestMultimedia.GetAudioClip(path, type);
        uwr.SendWebRequest();

        try
        {
            while (!uwr.isDone)
            {
            }

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                RepoBomboclat.Logger.LogError($"Failed to load AudioClip from path: {path} Full error: {uwr.error}");
                return null;
            }

            return DownloadHandlerAudioClip.GetContent(uwr);
        }
        catch (Exception err)
        {
            RepoBomboclat.Logger.LogError($"{err.Message}, {err.StackTrace}");
            return null;
        }
    }
}