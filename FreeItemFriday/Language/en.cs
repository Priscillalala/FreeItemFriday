using System;
using GrooveSharedUtils;
using GrooveSharedUtils.Attributes;
using JetBrains.Annotations;
using RoR2;
using UnityEngine;
using FreeItemFriday.Items;

namespace FreeItemFriday.Language
{
    public static class en
    {
        [LanguageCollectionProvider]
        public static LanguageCollection GetLang() => new LanguageCollection
        {
            (Assets.expansion.nameToken, $"Free Item Friday"),
            (Assets.expansion.descriptionToken, $"Adds content from the 'Free Item Friday' mod to the game."),
            (Assets.Items.Theremin?.nameToken, $"Theremin"),
            (Assets.Items.Theremin?.pickupToken, $"Increase attack speed near the Teleporter."),
            (Assets.Items.Theremin?.descriptionToken, $"Increase <style=cIsDamage>attack speed</style> by up to <style=cIsDamage>{Theremin.attackSpeedBonus:0%} <style=cStack>(+{Theremin.attackSpeedBonusPerStack:0%} per stack)</style></style> the closer you are to a Teleporter."),
            (Assets.Items.Arrowhead?.nameToken, $"Flint Arrowhead"),
            (Assets.Items.Arrowhead?.pickupToken, $"Burn enemies for flat damage on hit."),
            (Assets.Items.Arrowhead?.descriptionToken, $"<style=cIsDamage>100%</style> chance to <style=cIsDamage>burn</style> on hit for <style=cIsDamage>{Arrowhead.damage} <style=cStack>(+{Arrowhead.damagePerStack} per stack)</style> damage</style>."),
            (Assets.Achievements.BurnMultipleEnemies?.nameToken, "Burn to Kill"),
            (Assets.Achievements.BurnMultipleEnemies?.descriptionToken, "Ignite 10 enemies simultaneously."),
            ("Saturnian Feast", "Gain 25 max health. Revive after the Teleporter event; consumed on use."),
            ("???", "Gain a powerful recharging shield while a boss is alive."),
        };
    }
}
