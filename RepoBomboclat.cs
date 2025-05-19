using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using RepoBomboclat.Audio;
using UnityEngine;

namespace RepoBomboclat;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class RepoBomboclat : BaseUnityPlugin
{
    public static RepoBomboclat Instance;
    public new static ManualLogSource Logger { get; set; }
    private static Harmony _harmony;

    internal static AudioClip BomboclatClip;
    internal static ConfigEntry<bool> Enabled;
    internal static ConfigEntry<bool> OnlyOnLastExtract;
    internal static ConfigEntry<float> SurplusQuota;

    private void Awake()
    {
        Logger = BepInEx.Logging.Logger.CreateLogSource(MyPluginInfo.PLUGIN_GUID);
        Instance = this;
        Logger.LogInfo("Plugin RepoBomboclat is loaded!");
        GenerateConfig();

        if (!Enabled.Value)
            return;

        _harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);
        _harmony.PatchAll();

        var path = ExtractEmbeddedAudio("RepoBomboclat.bomboclat.mp3");
        BomboclatClip = AudioClipLoader.Load(path);
    }

    private void GenerateConfig()
    {
        Enabled = Config.Bind("", "Enabled", true, "Enables trigger sound");
        OnlyOnLastExtract = Config.Bind("Trigger Options", "Only On Last Extract", false,
            new ConfigDescription("Trigger sound only at last extract point | Default False"));
        SurplusQuota = Config.Bind("Trigger Options", "Surplus Quota", 10000f,
            new ConfigDescription("The surplus value after which trigger sound | Default 10k"));
    }

    [CanBeNull]
    private string ExtractEmbeddedAudio(string resourceName)
    {
        if (string.IsNullOrEmpty(resourceName))
        {
            Logger.LogError("Resource name is null or empty.");
            return null;
        }

        var tempPath = Path.Combine(Application.temporaryCachePath, resourceName);

        using var manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        using var fileWriterStream = File.Open(tempPath, FileMode.OpenOrCreate, FileAccess.Write);
        if (manifestResourceStream == null)
        {
            Logger.LogError("Failed to find resource: " + resourceName);
            return null;
        }

        manifestResourceStream.CopyTo(fileWriterStream);

        return tempPath;
    }
}