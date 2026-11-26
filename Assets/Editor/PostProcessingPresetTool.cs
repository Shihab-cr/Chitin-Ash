#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class PostProcessingPresetTool
{
    [MenuItem("Tools/Post-Processing/Apply Stylized Souls-Like Preset to Selected VolumeProfile")]
    public static void ApplyStylizedSoulsLikePreset()
    {
        VolumeProfile profile = Selection.activeObject as VolumeProfile;
        if (profile == null)
        {
            Debug.LogWarning("Select a VolumeProfile asset in the Project window first.");
            return;
        }

        Undo.RecordObject(profile, "Apply Stylized Souls-Like Post-Processing Preset");

        // 1. Tonemapping (Neutral keeps stylized colors rich and shadows readable)
        Tonemapping tonemapping = GetOrAdd<Tonemapping>(profile);
        tonemapping.mode.Override(TonemappingMode.Neutral);

        // 2. Color Adjustments (Warm daylight filter RGB: 255, 252, 245)
        ColorAdjustments colorAdjustments = GetOrAdd<ColorAdjustments>(profile);
        colorAdjustments.postExposure.Override(0.15f);
        colorAdjustments.contrast.Override(12f);
        colorAdjustments.colorFilter.Override(new Color(255f / 255f, 252f / 255f, 245f / 255f, 1f));
        colorAdjustments.saturation.Override(12f);

        // 3. Shadows, Midtones, Highlights (Readable cool shadows, warm sunlit highlights)
        ShadowsMidtonesHighlights smh = GetOrAdd<ShadowsMidtonesHighlights>(profile);
        smh.shadows.Override(new Vector4(0.96f, 0.99f, 1.05f, 0f));
        smh.midtones.Override(new Vector4(1.01f, 1.00f, 0.99f, 0f));
        smh.highlights.Override(new Vector4(1.05f, 1.02f, 0.95f, 0f));
        smh.shadowsStart.Override(0f);
        smh.shadowsEnd.Override(0.35f);
        smh.highlightsStart.Override(0.55f);
        smh.highlightsEnd.Override(1f);

        // 4. Bloom (RGB: 255, 246, 228)
        Bloom bloom = GetOrAdd<Bloom>(profile);
        bloom.threshold.Override(1.0f);
        bloom.intensity.Override(0.45f);
        bloom.scatter.Override(0.6f);
        bloom.tint.Override(new Color(255f / 255f, 246f / 255f, 228f / 255f, 1f));
        bloom.highQualityFiltering.Override(true);

        // 5. Vignette (Light framing, RGB: 18, 22, 30)
        Vignette vignette = GetOrAdd<Vignette>(profile);
        vignette.color.Override(new Color(18f / 255f, 22f / 255f, 30f / 255f, 1f));
        vignette.center.Override(new Vector2(0.5f, 0.5f));
        vignette.intensity.Override(0.16f);
        vignette.smoothness.Override(0.5f);
        vignette.rounded.Override(false);

        // 6. Chromatic Aberration (0 at rest for crisp stylized silhouettes)
        ChromaticAberration chromatic = GetOrAdd<ChromaticAberration>(profile);
        chromatic.intensity.Override(0f);

        // 7. Remove Film Grain if it was added previously
        if (profile.TryGet(out FilmGrain existingGrain))
        {
            profile.Remove<FilmGrain>();
            Undo.DestroyObjectImmediate(existingGrain);
        }

        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();
        Debug.Log($"Applied Stylized Souls-like post-processing values to {profile.name}.");
    }

    private static T GetOrAdd<T>(VolumeProfile profile) where T : VolumeComponent
    {
        if (!profile.TryGet(out T component))
        {
            component = profile.Add<T>(true);
            if (!AssetDatabase.Contains(component) && AssetDatabase.Contains(profile))
            {
                AssetDatabase.AddObjectToAsset(component, profile);
            }
        }
        return component;
    }
}
#endif