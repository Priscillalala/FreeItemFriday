using System;
using GrooveSharedUtils;
using GrooveSharedUtils.Attributes;
using GrooveSharedUtils.ScriptableObjects;
using JetBrains.Annotations;
using RoR2;
using RoR2.ContentManagement;
using RoR2.ExpansionManagement;
using UnityEngine;

namespace FreeItemFriday
{
    [AssetDisplayCase]
    public static class Assets
    {
        [LoadAssetBundle(bundleName = "freeitemfridayassets")]
        public static AssetBundle bundle;

        [GrooveSharedUtils.Frames.Frame.DefaultExpansionDef]
        [TargetAssetName("FreeItemFriday")]
        public static ExpansionDef expansion;

        public static class Items
        {
            [CanBeNull] public static ItemDef Theremin;
            [CanBeNull] public static ItemDef Arrowhead;
        }
        public static class Artifacts
        {
            [CanBeNull] public static ArtifactDef SlipperyTerrain;
        }
        public static class DamageColors
        {
            [CanBeNull] public static ModdedDamageColorDef StrongerBurn;
        }
        public static class Unlockables
        {
            [CanBeNull] [TargetAssetName("FreeItemFriday.Items.Arrowhead")] public static UnlockableDef Arrowhead;
            [CanBeNull] [TargetAssetName("FreeItemFriday.Artifacts.SlipperyTerrain")] public static UnlockableDef SlipperyTerrain;
        }
        public static class Achievements
        {
            [CanBeNull] public static AchievementDef BurnMultipleEnemies;
            [CanBeNull] public static AchievementDef ObtainArtifactSlipperyTerrain;
        }
    }
}
