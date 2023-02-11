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
        public static class DamageColors
        {
            [CanBeNull] public static ModdedDamageColorDef StrongerBurn;
        }
        public static class Unlockables
        {
            [CanBeNull] [TargetAssetName("FreeItemFriday.Items.Arrowhead")] public static UnlockableDef Arrowhead;
        }
        public static class Achievements
        {
            [CanBeNull] public static AchievementDef BurnMultipleEnemies;
        }
    }
}
