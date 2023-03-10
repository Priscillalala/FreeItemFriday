using System;
using GrooveSharedUtils;
using GrooveSharedUtils.Attributes;
using JetBrains.Annotations;
using RoR2;
using UnityEngine;
using static FreeItemFriday.Assets.Items;
using static FreeItemFriday.Assets.Equipment;
using static FreeItemFriday.Assets.Artifacts;
using static FreeItemFriday.Assets.Achievements;

namespace FreeItemFriday.Language
{
    public static class en
    {
        [LanguageCollectionProvider]
        public static LanguageCollection GetMisc() => new LanguageCollection
        {
            (Assets.expansion.nameToken, $"Free Item Friday"),
            (Assets.expansion.descriptionToken, $"Adds content from the 'Free Item Friday' mod to the game."),
        };
        [LanguageCollectionProvider]
        public static LanguageCollection GetItems() => new LanguageCollection
        {
            (Theremin?.nameToken, $"Theremin"),
            (Theremin?.pickupToken, $"Increase attack speed near the Teleporter."),
            (Theremin?.descriptionToken, $"Increase <style=cIsDamage>attack speed</style> by up to <style=cIsDamage>{Items.Theremin.attackSpeedBonus:0%} <style=cStack>(+{Items.Theremin.attackSpeedBonusPerStack:0%} per stack)</style></style> the closer you are to a Teleporter."),
            (Arrowhead?.nameToken, $"Flint Arrowhead"),
            (Arrowhead?.pickupToken, $"Burn enemies for flat damage on hit."),
            (Arrowhead?.descriptionToken, $"<style=cIsDamage>100%</style> chance to <style=cIsDamage>burn</style> on hit for <style=cIsDamage>{Items.Arrowhead.damage} <style=cStack>(+{Items.Arrowhead.damagePerStack} per stack)</style></style> damage."),
        };
        [LanguageCollectionProvider]
        public static LanguageCollection GetEquipment() => new LanguageCollection
        {
            (DeathEye?.nameToken, $"Godless Eye"),
            (DeathEye?.pickupToken, $"Obliterate all nearby enemies from existence, then yourself. Consumed on use."),
            (DeathEye?.descriptionToken, $"Obliterate enemies within <style=cIsUtility>{Equipment.DeathEye.range}m</style> from existence. Then, <style=cIsHealth>obliterate yourself from existence</style>. Equipment is <style=cIsUtility>consumed</style> on use."),
            (DeathEyeConsumed?.nameToken, $"Godless Eye (Consumed)"),
            (DeathEyeConsumed?.pickupToken, $"Still shocking to the touch. Does nothing."),
            (DeathEyeConsumed?.descriptionToken, $"Still shocking to the touch. Does nothing."),
        };
        [LanguageCollectionProvider]
        public static LanguageCollection GetArtifacts() => new LanguageCollection
        {
            (SlipperyTerrain?.nameToken, $"Artifact of Entropy"),
            (SlipperyTerrain?.descriptionToken, $"Terrain is smooth and frictionless."),
        };
        [LanguageCollectionProvider]
        public static LanguageCollection GetAchievements() => new LanguageCollection
        {
            (BurnMultipleEnemies?.nameToken, "Burn to Kill"),
            (BurnMultipleEnemies?.descriptionToken, "Ignite 10 enemies simultaneously."),
            (ObtainArtifactSlipperyTerrain?.nameToken, "Trial of Entropy"),
            (ObtainArtifactSlipperyTerrain?.descriptionToken, "Complete the Trial of Entropy."),
            (CompleteMultiplayerUnknownEnding?.nameToken, "Fly Away Together"),
            (CompleteMultiplayerUnknownEnding?.descriptionToken, "In multiplayer, obliterate at the Obelisk with a fellow survivor.."),
        };
    }
}
