﻿using System;
using GrooveSharedUtils;
using GrooveSharedUtils.Attributes;
using JetBrains.Annotations;
using RoR2;
using UnityEngine;
using FreeItemFriday.ItemModules;
using static FreeItemFriday.Assets;

namespace FreeItemFriday.Language
{
    public static class en
    {
        [LanguageCollectionProvider]
        public static LanguageCollection GetLang() => new LanguageCollection
        {
            (Items.Theremin?.nameToken, $"Theremin"),
            (Items.Theremin?.pickupToken, $"Increase attack speed near the Teleporter."),
            (Items.Theremin?.descriptionToken, $"Increase <style=cIsDamage>attack speed</style> by up to <style=cIsDamage>{Theremin.attackSpeedBonus:0%} <style=cStack>(+{Theremin.attackSpeedBonusPerStack:0%} per stack)</style></style> the closer you are to a Teleporter."),
            (Items.Arrowhead?.nameToken, $"Flint Arrowhead"),
            (Items.Arrowhead?.pickupToken, $"Burn enemies for flat damage on hit."),
            (Items.Arrowhead?.descriptionToken, $"<style=cIsDamage>100%</style> chance to <style=cIsDamage>burn</style> on hit for <style=cIsDamage>{Arrowhead.damage} <style=cStack>(+{Arrowhead.damagePerStack} per stack)</style> damage</style>."),
            (Achievements.BurnMultipleEnemies?.nameToken, "Burn to Kill"),
            (Achievements.BurnMultipleEnemies?.descriptionToken, "Ignite 10 enemies simultaneously."),
        };
    }
}