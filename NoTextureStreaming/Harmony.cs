using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace NoTextureStreaming
{
    public class NoTextureStreaming : IModApi
    {
        public void InitMod(Mod mod)
        {
            new Harmony(GetType().ToString()).PatchAll(Assembly.GetExecutingAssembly());
            DisableTextureStreaming();
        }
        public static void DisableTextureStreaming()
        {
            if (QualitySettings.streamingMipmapsActive)
            {
                QualitySettings.streamingMipmapsActive = false;
                Log.Out("Texture Streaming Disabled");
            }
        }
    }

    [HarmonyPatch(typeof(GameOptionsManager))]
    internal class HarmonyPatches_GameOptionsManager
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameOptionsManager.ApplyTextureQuality))]
        static bool Prefix_ApplyTextureQuality(ref int _overrideValue)
        {
            int textureQuality = GameOptionsManager.GetTextureQuality(_overrideValue);
            QualitySettings.streamingMipmapsActive = false;
            QualitySettings.streamingMipmapsMaxLevelReduction = Math.Max(3, GameRenderManager.TextureMipmapLimit);
            GameRenderManager.TextureMipmapLimit = textureQuality;
            float num = 0.6776996f;
            if (textureQuality > 0)
            {
                switch (textureQuality)
                {
                    case 1:
                        num = 0.6f;
                        break;
                    case 2:
                        num = 0.5f;
                        break;
                    case 3:
                        num = 0.4f;
                        break;
                }
            }
            Shader.SetGlobalFloat("_MipSlope", num);
            var eventField = typeof(GameOptionsManager).GetField(
                "TextureQualityChanged",
                BindingFlags.Static | BindingFlags.NonPublic
            );
            if (eventField != null)
            {
                var handler = eventField.GetValue(null) as Action<int>;
                handler?.Invoke(textureQuality);
            }
            Log.Out("Texture quality is set to " + GameRenderManager.TextureMipmapLimit.ToString());
            return false;
        }
    }
}
