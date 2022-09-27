using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Oculus.Avatar2;
using Oculus.Platform;
using UnityEngine;
using CAPI = Oculus.Avatar2.CAPI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SampleEntity : OvrAvatarEntity
{
    private const string logScope = "sampleAvatar";
    public enum AssetSource
    {
        Zip,
        StreamingAssets,
    }

    [System.Serializable]
    private struct AssetData
    {
        public AssetSource source;
        public string path;
    }

    [Header("Sample Avatar Entity")]
    [Tooltip("A version of the avatar with additional textures will be loaded to portray more accurate human materials (requiring shader support).")]
    [SerializeField] private bool _highQuality = false;
    [Tooltip("Attempt to load the Avatar model file from the Content Delivery Network (CDN) based on a userID, as opposed to loading from disc.")]
    [SerializeField] private bool _loadUserFromCdn = true;
    [Tooltip("Make initial requests for avatar and then defer loading until other avatars can make their requests.")]
    [SerializeField] private bool _deferLoading = false;

    [Header("Assets")]
    [Tooltip("Asset paths to load, and whether each asset comes from a preloaded zip file or directly from StreamingAssets")]
    [SerializeField] private List<AssetData> _assets = new List<AssetData> { new AssetData { source = AssetSource.Zip, path = "0" } };

    [Tooltip("Adds an underscore between the path and the postfix.")]
    [SerializeField] private bool _underscorePostfix = true;

    [Tooltip("Filename Postfix (WARNING: Typically the postfix is Platform specific, such as \"_rift.glb\")")]
    [SerializeField] private string _overridePostfix = String.Empty;

    [Header("CDN")]
    [Tooltip("Automatically retry LoadUser download request on failure")]
    [SerializeField] private bool _autoCdnRetry = true;

    [Tooltip("Automatically check for avatar changes")]
    [SerializeField] private bool _autoCheckChanges = false;
    [Tooltip("How frequently to check for avatar changes")]
    [SerializeField] [Range(4.0f, 320.0f)] private float _changeCheckInterval = 8.0f;

#pragma warning disable CS0414
    [Header("Debug Drawing")]
    [Tooltip("Draw debug visualizations for avatar gaze targets")]
    [SerializeField] private bool _debugDrawGazePos;
#pragma warning restore CS0414

    private enum OverrideStreamLOD
    {
        Default,
        ForceHigh,
        ForceMedium,
        ForceLow,
    }

    [Header("Sample Networking")]
    [Tooltip("Streaming quality override, default will not override")]
    [SerializeField] private OverrideStreamLOD _overrideStreamLod = OverrideStreamLOD.Default;

    private static readonly int DESAT_AMOUNT_ID = Shader.PropertyToID("_DesatAmount");
    private static readonly int DESAT_TINT_ID = Shader.PropertyToID("_DesatTint");
    private static readonly int DESAT_LERP_ID = Shader.PropertyToID("_DesatLerp");

    private bool HasLocalAvatarConfigured => _assets.Count > 0;

    private Stopwatch _loadTime = new Stopwatch();

    protected void Start()
    {
        LoadLocalAvatar();
        

        switch (_overrideStreamLod)
        {
            case OverrideStreamLOD.ForceHigh:
                ForceStreamLod(StreamLOD.High);
                break;
            case OverrideStreamLOD.ForceMedium:
                ForceStreamLod(StreamLOD.Medium);
                break;
            case OverrideStreamLOD.ForceLow:
                ForceStreamLod(StreamLOD.Low);
                break;
        }
    }

    public string GetAssetPath()
    {
        return _assets[0].path;
    }

    private void LoadLocalAvatar()
    {
        if (!HasLocalAvatarConfigured)
        {
            OvrAvatarLog.LogInfo("No local avatar asset configured", logScope, this);
            return;
        }

        // Zip asset paths are relative to the inside of the zip.
        // Zips can be loaded from the OvrAvatarManager at startup or by calling OvrAvatarManager.Instance.AddZipSource
        // Assets can also be loaded individually from Streaming assets
        var path = new string[1];
        foreach (var asset in _assets)
        {
            bool isFromZip = (asset.source == AssetSource.Zip);

            string assetPostfix = (_underscorePostfix ? "_" : "")
                + OvrAvatarManager.Instance.GetPlatformGLBPostfix(isFromZip)
                + OvrAvatarManager.Instance.GetPlatformGLBVersion(_highQuality, isFromZip)
                + OvrAvatarManager.Instance.GetPlatformGLBExtension(isFromZip);
            if (!String.IsNullOrEmpty(_overridePostfix))
            {
                assetPostfix = _overridePostfix;
            }

            path[0] = asset.path + assetPostfix;
            if (isFromZip)
            {
                LoadAssetsFromZipSource(path);
            }
            else
            {
                LoadAssetsFromStreamingAssets(path);
            }
        }
    }

    public void ReloadAvatarManually(string newAssetPaths, AssetSource newAssetSource)
    {
        string[] tempStringArray = new string[1];
        tempStringArray[0] = newAssetPaths;
        ReloadAvatarManually(tempStringArray, newAssetSource);
    }

    public void ReloadAvatarManually(string[] newAssetPaths, AssetSource newAssetSource)
    {
        Teardown();
        CreateEntity();

        bool isFromZip = (newAssetSource == AssetSource.Zip);
        string assetPostfix = (_underscorePostfix ? "_" : "")
            + OvrAvatarManager.Instance.GetPlatformGLBPostfix(isFromZip)
            + OvrAvatarManager.Instance.GetPlatformGLBVersion(_highQuality, isFromZip)
            + OvrAvatarManager.Instance.GetPlatformGLBExtension(isFromZip);

        string[] combinedPaths = new string[newAssetPaths.Length];
        for (var index = 0; index < newAssetPaths.Length; index++)
        {
            combinedPaths[index] = $"{newAssetPaths[index]}{assetPostfix}";
        }

        if (isFromZip)
        {
            LoadAssetsFromZipSource(combinedPaths);
        }
        else
        {
            LoadAssetsFromStreamingAssets(combinedPaths);
        }
    }

    #region Fade/Desat

    private static readonly Color AVATAR_FADE_DEFAULT_COLOR = new Color(33 / 255f, 50 / 255f, 99 / 255f, 0f); // "#213263"
    private static readonly float AVATAR_FADE_DEFAULT_COLOR_BLEND = 0.7f; // "#213263"
    private static readonly float AVATAR_FADE_DEFAULT_GRAYSCALE_BLEND = 0;

    [Header("Rendering")]
    [SerializeField] [Range(0, 1)] private float shaderGrayToSolidColorBlend_ = AVATAR_FADE_DEFAULT_COLOR_BLEND;
    [SerializeField] [Range(0, 1)] private float shaderDesatBlend_ = AVATAR_FADE_DEFAULT_GRAYSCALE_BLEND;
    [SerializeField] private Color shaderSolidColor_ = AVATAR_FADE_DEFAULT_COLOR;

    public float ShaderGrayToSolidColorBlend
    {
        // Blends grayscale to solid color
        get => shaderGrayToSolidColorBlend_;
        set
        {
            if (Mathf.Approximately(value, shaderGrayToSolidColorBlend_))
            {
                shaderGrayToSolidColorBlend_ = value;
                UpdateMaterialsWithDesatModifiers();
            }
        }
    }

    public float ShaderDesatBlend
    {
        // Blends shader color to result of ShaderGrayToSolidColorBlend
        get => shaderDesatBlend_;
        set
        {
            if (Mathf.Approximately(value, shaderDesatBlend_))
            {
                shaderDesatBlend_ = value;
                UpdateMaterialsWithDesatModifiers();
            }
        }
    }

    public Color ShaderSolidColor
    {
        get => shaderSolidColor_;
        set
        {
            if (shaderSolidColor_ != value)
            {
                shaderSolidColor_ = value;
                UpdateMaterialsWithDesatModifiers();
            }
        }
    }

    public void SetShaderDesat(float desatBlend, float? grayToSolidBlend = null, Color? solidColor = null)
    {
        if (solidColor.HasValue)
        {
            shaderSolidColor_ = solidColor.Value;
        }
        if (grayToSolidBlend.HasValue)
        {
            shaderGrayToSolidColorBlend_ = grayToSolidBlend.Value;
        }
        shaderDesatBlend_ = desatBlend;
        UpdateMaterialsWithDesatModifiers();
    }

    private void UpdateMaterialsWithDesatModifiers()
    {
        // TODO: Migrate to `OvrAvatarMaterial` system
#pragma warning disable 618 // disable deprecated method call warnings
        SetMaterialKeyword("DESAT", shaderDesatBlend_ > 0.0f);
        SetMaterialProperties((block, entity) =>
        {
            block.SetFloat(DESAT_AMOUNT_ID, entity.shaderDesatBlend_);
            block.SetColor(DESAT_TINT_ID, entity.shaderSolidColor_);
            block.SetFloat(DESAT_LERP_ID, entity.shaderGrayToSolidColorBlend_);
        }, this);
#pragma warning restore 618 // restore deprecated method call warnings
    }

    #endregion

    #region Unity Transforms

    public Transform GetSkeletonTransform(CAPI.ovrAvatar2JointType jointType)
    {
        if (!_criticalJointTypes.Contains(jointType))
        {
            OvrAvatarLog.LogError($"Can't access joint {jointType} unless it is in critical joint set");
            return null;
        }

        return GetSkeletonTransformByType(jointType);
    }

    public CAPI.ovrAvatar2JointType[] GetCriticalJoints()
    {
        return _criticalJointTypes;
    }

    #endregion
}