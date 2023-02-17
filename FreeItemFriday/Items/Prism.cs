using System;
using Unity;
using UnityEngine;
using GrooveSharedUtils;
using GrooveSharedUtils.Frames;
using GrooveSharedUtils.Attributes;
using GrooveSharedUtils.Interfaces;
using RoR2;
using RoR2.Items;
using static GrooveSharedUtils.Common;
using R2API;
using System.Collections;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine.Networking;

namespace FreeItemFriday.Items
{
    [IgnoreModule]
    [Configurable]
    public class Prism : ModModule<Prism>
    {
        public float bonusBarrierDuration = 0.5f;
        public float bonusBarrierDurationPerStack = 0.5f;
        public override IEnumerator LoadContent()
        {
            yield return new ItemFrame
            {
                name = nameof(Assets.Items.Prism),
                icon = Assets.bundle.LoadAsset<Sprite>("texThereminIcon"),
                itemTier = ItemTier.Lunar,
                pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("PickupTheremin"),
            }
            .SetItemTags(ItemTag.Utility, ItemTag.Healing)
            .SetLogbookModelParameters(new Vector3(0, 0, 0), 1, 5);

            GameObject displayPrefab = Assets.bundle.LoadAsset<GameObject>("DisplayTheremin");
            GSUtil.SetupItemDisplay(displayPrefab);
            yield return SingleItemDisplayFrame.Create(Assets.Items.Theremin, displayPrefab)
                .Add(Idrs.Commando)
                .Add(Idrs.Huntress)
                .Add(Idrs.Bandit2)
                .Add(Idrs.Toolbot)
                .Add(Idrs.Engi)
                .Add(Idrs.EngiTurret)
                .Add(Idrs.EngiWalkerTurret)
                .Add(Idrs.Mage)
                .Add(Idrs.Merc)
                .Add(Idrs.Treebot)
                .Add(Idrs.Loader)
                .Add(Idrs.Croco)
                .Add(Idrs.Captain)
                .Add(Idrs.RailGunner)
                .Add(Idrs.VoidSurvivor)
                .Add(Idrs.Scav);
        }
        public void Awake()
        {
            On.RoR2.UI.HealthBar.UpdateBarInfos += HealthBar_UpdateBarInfos;
            On.RoR2.HealthComponent.AddBarrier += HealthComponent_AddBarrier;
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            //IL.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
        }

        private void HealthBar_UpdateBarInfos(On.RoR2.UI.HealthBar.orig_UpdateBarInfos orig, RoR2.UI.HealthBar self)
        {
            orig(self);
            if (self.barInfoCollection.barrierBarInfo.enabled && self.source && self.source.body && self.source.body.HasItem(Assets.Items.Prism))
            {
                self.barInfoCollection.barrierBarInfo.normalizedXMin = Mathf.Lerp(1f, self.barInfoCollection.instantHealthbarInfo.normalizedXMax, self.source.barrier / self.source.fullBarrier);
                self.barInfoCollection.barrierBarInfo.normalizedXMax = 1f;
            }
        }

        private void HealthComponent_AddBarrier(On.RoR2.HealthComponent.orig_AddBarrier orig, HealthComponent self, float value)
        {
            if (self.body && self.body.HasItem(Assets.Items.Prism, out int stack))
            {
                value += self.body.barrierDecayRate * GSUtil.StackScaling(bonusBarrierDuration, bonusBarrierDurationPerStack, stack);
            }
            orig(self, value);
        }

        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            if (self.HasItem(Assets.Items.Prism))
            {
                float maxBarrier = CalculatePrismMaxbarrier(self);
                self.barrierDecayRate *= maxBarrier / self.maxBarrier;
                self.maxBarrier = maxBarrier;
                if (self.healthComponent.barrier > self.maxBarrier && NetworkServer.active)
                {
                    self.healthComponent.Networkbarrier = self.maxBarrier;
                }
            }
        }
        public static float CalculatePrismMaxbarrier(CharacterBody body)
        {
            float maxBarrier = body.healthComponent.missingCombinedHealth;
            if (body.cursePenalty > 1f)
            {
                maxBarrier += body.healthComponent.fullCombinedHealth * (body.cursePenalty - 1);
            }
            return maxBarrier;
        }
        public class PrismBehaviour : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnServer = true, useOnClient = true)]
            public static ItemDef GetItemDef() => Assets.Items.Prism;
            public float lastMaxBarrier;
            public void FixedUpdate()
            {
                if (lastMaxBarrier != (lastMaxBarrier = CalculatePrismMaxbarrier(body)))
                {
                    body.MarkAllStatsDirty();
                }   
            }
        }
        /*private void CharacterBody_RecalculateStats(ILContext il)
{
   ILCursor c = new ILCursor(il);
   bool ilFound = c.TryGotoNext(MoveType.Before,
       x => x.MatchCallOrCallvirt<CharacterBody>("set_maxBarrier")
       ); 
   if (ilFound)
   {
       c.Emit(OpCodes.Ldarg, 0);
       c.EmitDelegate<Func<float, CharacterBody, float>>((maxBarrier, body) =>
       {
           if (body.HasItem(Assets.Items.Prism))
           {

           }
       });
   }
   else
   {
       Log(BepInEx.Logging.LogLevel.Error, nameof(CharacterBody_RecalculateStats) + "IL hook failed!");
   }
}*/
    }
}
