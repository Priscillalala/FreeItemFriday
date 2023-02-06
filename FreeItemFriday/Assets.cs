using System;
using GrooveSharedUtils;
using GrooveSharedUtils.Attributes;
using GrooveSharedUtils.ScriptableObjects;
using JetBrains.Annotations;
using RoR2;
using UnityEngine;

namespace FreeItemFriday
{
    [AssetDisplayCase]
    public static class Assets
    {
        [LoadAssetBundle(bundleName = "freeitemfridayassets")]
        public static AssetBundle bundle;
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
            [CanBeNull] public static UnlockableDef Items_Arrowhead;
        }
        public static class Achievements
        {
            [CanBeNull] public static AchievementDef BurnMultipleEnemies;
        }
    }
}
