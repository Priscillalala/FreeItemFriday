using System;
using GrooveSharedUtils;
using GrooveSharedUtils.Attributes;
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
        }
    }
}
