using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

namespace Oculus.Avatar2
{
    /**
     * Per-avatar LOD information.
     * This component is added to every avatar managed by the LOD manager.
     * It is informational and not intended to be changed by the application.
     * @see OvrAvatarLODManager
     */
    public sealed class AvatarLOD : MonoBehaviour
    {
        private bool prevDebug_;

        /// The entity whose LOD is managed by this instance
        public OvrAvatarEntity Entity { get; internal set; } = null;

        // Whether the entity associated with this AvatarLOD is non-null and active
        public bool EntityActive => !ReferenceEquals(Entity, null) && Entity.isActiveAndEnabled;

        /// True if the avatar has been culled by the LOD manager.
        public bool culled { get; private set; }

        /// If enabled, the overrideLevel value will be used instead of the calculated LOD.
        public bool overrideLOD = false;
        private bool prevOverrideLOD_ = false;

        /// Desired level of detail for this avatar.
        public int overrideLevel
        {
            get { return Mathf.Clamp(overrideLevel_, -1, maxLodLevel); }
            set { overrideLevel_ = value; }
        }

        // TODO: Initialize to `int.MinValue`?
        private int overrideLevel_ = default;
        private int prevOverrideLevel_ = default;

        /// Transform on the avatar center joint.
        public Transform centerXform;
        public readonly List<Transform> extraXforms = new List<Transform>();
        private List<AvatarLODGroup> lodGroups_ = new List<AvatarLODGroup>();

        /// Vertex counts for each level of detail for this avatar.
        public readonly List<int> vertexCounts = new List<int>();

        /// Triangle counts for each level of detail for this avatar.
        public readonly List<int> triangleCounts = new List<int>();

        private int _minLodLevel = -1;
        /// Minimum LOD level loaded for this avatar.
        public int minLodLevel => _minLodLevel;
        private int _maxLodLevel = -1;
        /// Maximum LOD level loaded for this avatar.
        public int maxLodLevel => _maxLodLevel;

        /// Distance of avatar center joint from the LOD camera.
        public float distance;

        /// Screen percent occupied by the avatar (0 - 1).
        public float screenPercent;

        /// LOD level calculated based on screen percentage (before dynamic processing).
        public int wantedLevel;

        /// LOD level calculated after dynamic processing.
        public int dynamicLevel;

        ///
        /// Importance of avatar for display purposes (geometric LOD).
        /// This is from a logarithmic function by OvrAvatarLODManager.
        /// @see OvrAvatarLODManager.dynamicLodWantedLogScale
        ///
        public float lodImportance;

        ///
        /// Importance of avatar for update (animation) purposes.
        /// This is from a logarithmic function by OvrAvatarLODManager.
        /// @see OvrAvatarLODManager.screenPercentToUpdateImportanceCurvePower
        ///  @see OvrAvatarLODManager.screenPercentToUpdateImportanceCurveMultiplier.
        ///
        public float updateImportance;

        /// Clock time since last update (in ticks).
        public float lastUpdateAgeTicks;

        /// Total maximum age during previous two updates (in ticks).
        public float previousUpdateAgeWindowTicks;

        /// Clock time since last update (in seconds).
        public float lastUpdateAgeSeconds;

        /// Total maximum age during previous two updates (in seconds).
        public float previousUpdateAgeWindowSeconds;

        public int lastUpdateCost = -1;

        /// Network streaming fidelity for this avatar.
        public OvrAvatarEntity.StreamLOD dynamicStreamLod;

        /// Event invoked when the avatar's cull status has changed.
        /// This event is also available from OvrAvatarLODManager.
        /// @see OvrAvatarLODManager.CullChangedEvent
        public Action<bool> CulledChangedEvent;

        private GameObject debugCanvas_;

        private int level_;
        private int prevLevel_;
        private bool forceDisabled_;

        // There has to be a better way to do this :(
        // ASSUMPTION: Not more than 8 lods, so using bytes as bitfields is sufficient
        private readonly BitArray _levelsWithCost = new BitArray(8);
        private readonly BitArray _levelsWithCostForGroup = new BitArray(8);

        // AvatarLODManager.Initialize doesn't run for all the avatars added
        // in LODScene so assign a unique id internally on construction.
        private static Int32 _avatarIdSource = 1;
        public AvatarLOD()
        {
            _avatarId = _avatarIdSource++;
        }

        public int Level
        {
            get { return level_; }
            set
            {
                if (value == prevLevel_)
                {
                    return;
                }
                level_ = value;

                if (!overrideLOD)
                {
                    UpdateLOD();
                    UpdateDebugLabel();
                }

                prevLevel_ = level_;
            }
        }

        public UInt32 UpdateCost
        {
            get
            {
                // Clear
                _levelsWithCost.SetAll(false);

                // Check costs for all lod groups
                foreach (var lodGroup in lodGroups_)
                {
                    var levelBitField = lodGroup.LevelsWithAnimationUpdateCost;
                    // Set bits in bit array from byte's value
                    for (byte i = 0; i < 8; i++)
                    {
                        _levelsWithCostForGroup.Set(i, (levelBitField & (1 << i)) != 0);
                    }

                    _levelsWithCost.Or(_levelsWithCostForGroup);
                }

                UInt32 cost = 0;
                for (int i = minLodLevel; i < maxLodLevel; i++)
                {
                    cost += _levelsWithCost.Get(i) ? (UInt32)vertexCounts[i] : 0;
                }

                return cost;
            }
        }


        public void SetForceDisable(bool tf)
        {
            forceDisabled_ = tf;
        }

        private void Start()
        {
            AvatarLODManager.Instance.AddLOD(this);
            if (centerXform == null)
            {
                centerXform = transform;
            }
        }

        // Returns true upon a state transition
        public bool SetCulled(bool nextCulled)
        {
            if (nextCulled != culled)
            {
                culled = nextCulled;
                CulledChangedEvent?.Invoke(culled);
                return true;
            }
            return false;
        }

        private bool HasValidLODParent()
        {
            bool found = false;
            var lodParents = gameObject.transform.parent.GetComponents<AvatarLODParent>();
            foreach (AvatarLODParent lodParent in lodParents)
            {
                found |= !lodParent.beingDestroyed;
            }

            return found;
        }

        private void DestroyLODParentIfOnlyChild()
        {
            if (gameObject.transform.parent)
            {
                var lodParents = gameObject.transform.parent.gameObject.GetComponents<AvatarLODParent>();
                foreach (AvatarLODParent lodParent in lodParents)
                {
                    lodParent.DestroyIfOnlyLODChild(this);
                }
            }
        }


        private void OnBeforeTransformParentChanged()
        {
            DestroyLODParentIfOnlyChild();
        }

        private void OnTransformParentChanged()
        {
            Transform t = gameObject.transform.parent;
            if (t != null && !HasValidLODParent())
            {
                t.gameObject.AddComponent<AvatarLODParent>();
            }

            AvatarLODManager.ParentStateChanged(this);
        }

        // This behaviour is manually updated at a specific time during OvrAvatarManager::Update()
        // to prevent issues with Unity script update ordering
        internal void UpdateOverride()
        {
            if (!isActiveAndEnabled || forceDisabled_) { return; }

            Profiler.BeginSample("AvatarLOD::UpdateOverride");

            bool needsDebugLabelUpdate = false;
            if (overrideLevel != prevOverrideLevel_)
            {
                if (overrideLOD)
                {
                    UpdateLOD();
                    needsDebugLabelUpdate = true;
                }
                prevOverrideLevel_ = overrideLevel;
            }

            if (overrideLOD != prevOverrideLOD_)
            {
                UpdateLOD();
                needsDebugLabelUpdate = true;
                prevOverrideLOD_ = overrideLOD;
            }

            bool debug = AvatarLODManager.Instance.debug.displayLODLabels || AvatarLODManager.Instance.debug.displayAgeLabels;
            if (debug || debug != prevDebug_)
            {
                needsDebugLabelUpdate = true;
                prevDebug_ = debug;
            }

            if (needsDebugLabelUpdate)
            {
                UpdateDebugLabel();
            }

            Profiler.EndSample();
        }

        private void OnDestroy()
        {
            DestroyLODParentIfOnlyChild();
            AvatarLODManager.RemoveLOD(this);
        }

        [Conditional("UNITY_DEVELOPMENT")]
        [Conditional("UNITY_EDITOR")]
        internal void TrackUpdateAge(float deltaTime)
        {
            // Track of the last update time for debug tools
            if (EntityActive)
            {
                previousUpdateAgeWindowSeconds = lastUpdateAgeSeconds + deltaTime;
                lastUpdateAgeSeconds = 0;
                previousUpdateAgeWindowTicks = lastUpdateAgeTicks + 1;
                lastUpdateAgeTicks = 0;
            }
            else
            {
                lastUpdateAgeSeconds += Time.deltaTime;
                lastUpdateAgeTicks += 1;
            }
        }

        [Conditional("UNITY_DEVELOPMENT")]
        [Conditional("UNITY_EDITOR")]
        public void UpdateDebugLabel()
        {
            if (AvatarLODManager.Instance.debug.displayLODLabels || AvatarLODManager.Instance.debug.displayAgeLabels)
            {
                if (debugCanvas_ == null && AvatarLODManager.Instance.avatarLodDebugCanvas != null)
                {
                    GameObject canvasPrefab = AvatarLODManager.Instance.avatarLodDebugCanvas; //LoadAssetWithFullPath<GameObject>($"{AvatarPaths.ASSET_SOURCE_PATH}/LOD/Prefabs/AVATAR_LOD_DEBUG_CANVAS.prefab");
                    if (canvasPrefab != null)
                    {
                        debugCanvas_ = Instantiate(canvasPrefab, centerXform);

                        // Set position instead of localPosition to keep the label in a steady readable location.
                        debugCanvas_.transform.position = debugCanvas_.transform.parent.position + AvatarLODManager.Instance.debug.displayLODLabelOffset;

                        debugCanvas_.SetActive(true);
                    }
                    else
                    {
                        OvrAvatarLog.LogWarning("DebugLOD will require the avatarLodDebugCanvas prefab to be specified. This has a simple UI card that allows for world space display of LOD.");
                    }
                }

                if (debugCanvas_ != null)
                {
                    var text = debugCanvas_.GetComponentInChildren<UnityEngine.UI.Text>();

                    // Set position instead of localPosition to keep the label in a steady readable location.
                    debugCanvas_.transform.position = debugCanvas_.transform.parent.position + AvatarLODManager.Instance.debug.displayLODLabelOffset;

                    if (AvatarLODManager.Instance.debug.displayLODLabels)
                    {
                        int actualLevel = overrideLOD ? overrideLevel : Level;
                        text.color = actualLevel == -1 ? Color.gray : AvatarLODManager.LOD_COLORS[actualLevel];
                        text.text = actualLevel.ToString();
                        text.fontSize = 40;
                    }

                    if (AvatarLODManager.Instance.debug.displayAgeLabels)
                    {
                        text.text = previousUpdateAgeWindowSeconds.ToString();
                        text.color = new Color(Math.Max(Math.Min(-1.0f + 2.0f * previousUpdateAgeWindowSeconds, 1.0f), 0.0f), Math.Max(Math.Min(previousUpdateAgeWindowSeconds * 2.0f, 2.0f - 2.0f * previousUpdateAgeWindowSeconds), 0f), Math.Max(Math.Min(1.0f - 2.0f * previousUpdateAgeWindowSeconds, 1.0f), 0.0f));
                        text.fontSize = 10;
                    }
                }
            }
            else
            {
                if (debugCanvas_ != null)
                {
                    debugCanvas_.SetActive(false);
                    debugCanvas_ = null;
                }
            }
        }

        public void UpdateLOD()
        {
            if (forceDisabled_) { return; }

            if (lodGroups_ != null && lodGroups_.Count > 0)
            {
                foreach (var t in lodGroups_)
                {
                    t.Level = overrideLOD ? overrideLevel : Level;
                }
            }
        }

        [Conditional("UNITY_DEVELOPMENT")]
        [Conditional("UNITY_EDITOR")]
        public void ForceUpdateLOD<T>()
        {
            foreach (var t in lodGroups_)
            {
                if (t is T)
                {
                    t.UpdateLODGroup();
                }
            }
        }

        public void ForceUpdateLOD()
        {
            foreach (var t in lodGroups_)
            {
                t.UpdateLODGroup();
            }
        }

        public void ResetLODGroups()
        {
            foreach (var t in lodGroups_)
            {
                t.ResetLODGroup();
            }
        }

        private void AddLODGroup(AvatarLODGroup group)
        {
            lodGroups_.Add(group);
            group.parentLOD = this;
            group.Level = overrideLOD ? overrideLevel : Level;
        }

        public void RemoveLODGroup(AvatarLODGroup group)
        {
            lodGroups_.Remove(group);
        }

        public void ClearLODGameObjects()
        {
            // Vertex counts will be reset by this function.
            vertexCounts.Clear();
            triangleCounts.Clear();

            _minLodLevel = -1;
            _maxLodLevel = -1;

            CAPI.ovrAvatar2LOD_UnregisterAvatar(_avatarId);
        }

        public void AddLODGameObjectGroupByAvatarSkinnedMeshRenderers(GameObject parentGameObject, Dictionary<string, List<GameObject>> suffixToObj)
        {
            foreach (var kvp in suffixToObj)
            {
                AvatarLODSkinnableGroup gameObjectGroup = parentGameObject.GetOrAddComponent<AvatarLODSkinnableGroup>();
                gameObjectGroup.GameObjects = kvp.Value.ToArray();
                AddLODGroup(gameObjectGroup);
            }
        }

        public void AddLODGameObjectGroupBySdkRenderers(Dictionary<int, Oculus.Avatar2.OvrAvatarEntity.LodData> lodObjects)
        {
            // Vertex counts will be reset by this function.
            vertexCounts.Clear();
            triangleCounts.Clear();

            // first see what the limits could be...
            _minLodLevel = lodObjects.First().Key;
            _maxLodLevel = minLodLevel;
            foreach (var entry in lodObjects)
            {
                if (_minLodLevel > entry.Key) _minLodLevel = entry.Key;
                if (_maxLodLevel < entry.Key) _maxLodLevel = entry.Key;
            }

            // first find common parent and children;
            GameObject commonParent = null;
            GameObject[] children = new GameObject[maxLodLevel + 1];
            if (maxLodLevel >= vertexCounts.Count || maxLodLevel >= triangleCounts.Count)
            {
                vertexCounts.Capacity = maxLodLevel;
                triangleCounts.Capacity = maxLodLevel;
                while (maxLodLevel >= vertexCounts.Count)
                {
                    vertexCounts.Add(0);
                }
                while (maxLodLevel >= triangleCounts.Count)
                {
                    triangleCounts.Add(0);
                }
            }

            for (int i = minLodLevel; i < maxLodLevel + 1; i++)
            {
                if (lodObjects.TryGetValue(i, out var lodData))
                {
                    var go = lodData.gameObject;
                    vertexCounts[i] += lodData.vertexCount;
                    triangleCounts[i] += lodData.triangleCount;
                    children[i] = go;
                    var localParent = go.transform.parent.gameObject;
                    if (commonParent == null)
                    {
                        commonParent = localParent;
                    }
                    else if (commonParent != localParent)
                    {
                        OvrAvatarLog.LogError("Expected all lodObjects to have the same parent object.");
                    }
                }
            }

            var gameObjectGroup = commonParent.GetOrAddComponent<AvatarLODSkinnableGroup>();
            gameObjectGroup.GameObjects = children;
            AddLODGroup(gameObjectGroup);

            // Register avatar with native runtime LOD scheme
            // Temporary for LOD editing bringup
            CAPI.ovrAvatar2LODRegistration reg;
            reg.avatarId = _avatarId;
            reg.lodWeights = vertexCounts.ToArray();
            reg.lodThreshold = _maxLodLevel;

            CAPI.ovrAvatar2LOD_RegisterAvatar(reg);
        }

        public AvatarLODBehaviourStateGroup AddLODBehaviourStateGroup(
          GameObject go,
          Behaviour[] behaviours,
          bool[] lodStates)
        {
            AvatarLODBehaviourStateGroup behaviourLODGroup = go.GetOrAddComponent<AvatarLODBehaviourStateGroup>();
            behaviourLODGroup.LodBehaviours = new List<Behaviour>(behaviours);
            behaviourLODGroup.EnabledStates = new List<bool>(lodStates);
            AddLODGroup(behaviourLODGroup);
            return behaviourLODGroup;
        }

        public AvatarLODBehaviourStateGroup AddLODBehaviourStateGroup(
          Behaviour behaviour,
          bool[] lodStates)
        {
            return AddLODBehaviourStateGroup(behaviour.gameObject, new Behaviour[] { behaviour }, lodStates);
        }

        public AvatarLODBehaviourStateGroup AddLODBehaviourStateGroup(GameObject go, Type[] behaviourTypes,
          bool[] lodStates)
        {
            var behaviours = new List<Behaviour>();

            foreach (var type in behaviourTypes)
            {
                Component[] cmpts = go.GetComponentsInChildren(type, true);
                foreach (var component in cmpts)
                {
                    behaviours.Add((Behaviour) component);
                }
            }

            return behaviours.Count > 0 ? AddLODBehaviourStateGroup(go, behaviours.ToArray(), lodStates) : null;
        }

        public AvatarLODActionGroup AddLODActionGroup(GameObject go, Action[] actions)
        {
            var actionLODGroup = go.GetOrAddComponent<AvatarLODActionGroup>();
            if (actions?.Length > 0)
            {
                actionLODGroup.Actions = new List<Action>(actions);
            }
            AddLODGroup(actionLODGroup);
            return actionLODGroup;
        }

        public AvatarLODActionGroup AddLODActionGroup(GameObject go, Action action, int levels)
        {
            var actions = new Action[levels];
            if (action != null)
            {
                for (int i = 0; i < levels; i++)
                {
                    actions[i] = action;
                }
            }

            return AddLODActionGroup(go, actions);
        }

        // Find a valid LOD near the requested one
        public int CalcAdjustedLod(int lod)
        {
            var adjustedLod = Math.Min(Math.Max(lod, minLodLevel), maxLodLevel);
            if (adjustedLod != -1 && vertexCounts[adjustedLod] == 0)
            {
                adjustedLod = GetNextLod(lod);
                if (adjustedLod == -1)
                {
                    adjustedLod = GetPreviousLod(lod);
                }
            }
            return adjustedLod;
        }

        private int GetNextLod(int lod)
        {
            if (maxLodLevel >= 0)
            {
                for (int nextLod = lod + 1; nextLod <= maxLodLevel; ++nextLod)
                {
                    if (vertexCounts[nextLod] != 0)
                    {
                        return nextLod;
                    }
                }
            }
            return -1;
        }

        public int GetPreviousLod(int lod)
        {
            if (minLodLevel >= 0)
            {
                for (int prevLod = lod - 1; prevLod >= minLodLevel; --prevLod)
                {
                    if (vertexCounts[prevLod] != 0)
                    {
                        return prevLod;
                    }
                }
            }
            return -1;
        }

        public void Reset()
        {
            ResetXforms();
        }

        private void ResetXforms()
        {
            centerXform = transform;
            extraXforms.Clear();
        }

        // Temporary to bring up runtime LOD system
        // Unique ID for this avatar
        public Int32 _avatarId;
    }
}
