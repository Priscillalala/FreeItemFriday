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
using HG;
using RoR2.Achievements;
using System.Collections.Generic;
using UnityEngine.Networking;
using FreeItemFriday.Achievements;

namespace FreeItemFriday.Items
{
    [Configurable(section = "Flint Arrowhead")]
    public class Arrowhead : ModModule<Arrowhead>
    {
        [Configurable]
        public static float damage = 3f;
        [Configurable]
        public static float damagePerStack = 3f;

        public LazyAddressable<GameObject> omniExplosionVFXQuick = "RoR2/Base/Common/VFX/OmniExplosionVFXQuick.prefab";
        public LazyAddressable<Material> matOmniHitspark3Gasoline = "RoR2/Base/IgniteOnKill/matOmniHitspark3Gasoline.mat";
        public GameObject impactArrowhead;
        public GameObject impactArrowheadStronger;
        public void Awake()
        {
            On.RoR2.DotController.InitDotCatalog += DotController_InitDotCatalog;
            Events.onHitEnemyServer += Events_onHitEnemyServer;
        }

        private void DotController_InitDotCatalog(On.RoR2.DotController.orig_InitDotCatalog orig)
        {
            orig();
            DotController.dotDefs[(int)DotController.DotIndex.StrongerBurn].damageColorIndex = Assets.DamageColors.StrongerBurn.damageColorIndex;
        }

        private void Events_onHitEnemyServer(DamageInfo damageInfo, GameObject victim)
        {
            if (damageInfo.attacker && damageInfo.attacker.TryGetComponent(out CharacterBody attackerBody) && attackerBody.HasItem(Assets.Items.Arrowhead, out int stack) && Util.CheckRoll(100f * damageInfo.procCoefficient, attackerBody.master))
            {
                InflictDotInfo inflictDotInfo = new InflictDotInfo
                {
                    attackerObject = damageInfo.attacker,
                    dotIndex = DotController.DotIndex.Burn,
                    victimObject = victim,
                    totalDamage = GSUtil.StackScaling(damage, damagePerStack, stack),
                };
                StrengthenBurnUtils.CheckDotForUpgrade(attackerBody.inventory, ref inflictDotInfo);
                DotController.DotDef dotDef = DotController.GetDotDef(inflictDotInfo.dotIndex);
                if (dotDef != null) 
                {
                    DamageInfo burnDamageInfo = new DamageInfo();
                    burnDamageInfo.attacker = inflictDotInfo.attackerObject;
                    burnDamageInfo.crit = false;
                    burnDamageInfo.damage = (float)inflictDotInfo.totalDamage;
                    burnDamageInfo.force = Vector3.zero;
                    burnDamageInfo.inflictor = inflictDotInfo.attackerObject;
                    burnDamageInfo.position = damageInfo.position;
                    burnDamageInfo.procCoefficient = 0f;
                    burnDamageInfo.damageColorIndex = dotDef.damageColorIndex;
                    burnDamageInfo.damageType = DamageType.DoT | DamageType.Silent;
                    burnDamageInfo.dotIndex = inflictDotInfo.dotIndex;
                    if (inflictDotInfo.victimObject && inflictDotInfo.victimObject.TryGetComponent(out CharacterBody victimBody) && victimBody.healthComponent)
                    {
                        victimBody.healthComponent.TakeDamage(burnDamageInfo);
                        EffectManager.SpawnEffect(inflictDotInfo.dotIndex == DotController.DotIndex.Burn ? impactArrowhead : impactArrowheadStronger, new EffectData
                        {
                            origin = damageInfo.position,
                            rotation = Util.QuaternionSafeLookRotation(-damageInfo.force),
                            scale = inflictDotInfo.dotIndex == DotController.DotIndex.Burn ? 1.5f : 2.5f
                        }, true) ;
                    }
                }
            }
        }

        public override IEnumerator LoadContent()
        {
            yield return new AchievementFrame
            {
                identifier = nameof(Assets.Achievements.BurnMultipleEnemies),
                unlockableRewardName = "FreeItemFriday.Items.Arrowhead",
                achievementIcon = Assets.bundle.LoadAsset<Sprite>("texBurnMultipleEnemiesIcon")
            }.SetTrackerTypes<BurnMultipleEnemiesAchievement, BurnMultipleEnemiesAchievement.ServerAchievement>();

            yield return new ItemFrame
            {
                name = nameof(Assets.Items.Arrowhead),
                icon = Assets.bundle.LoadAsset<Sprite>("texArrowheadIcon"),
                itemTier = ItemTier.Tier1,
                pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("PickupArrowhead"),
                unlockableDef = Assets.Unlockables.Arrowhead,
            }.SetItemTags(ItemTag.Damage);
            GSUtil.SetupModelPanelParameters(Assets.Items.Arrowhead.pickupModelPrefab, Vector3.zero, 1, 8);

            GameObject displayPrefab = Assets.bundle.LoadAsset<GameObject>("DisplayArrowhead");
            GSUtil.SetupItemDisplay(displayPrefab);
            yield return SingleItemDisplayFrame.Create(Assets.Items.Arrowhead, displayPrefab)
                .Add(Idrs.Commando, "Pelvis", new Vector3(-0.162F, -0.09F, -0.053F), new Vector3(7.522F, 244.056F, 358.818F), new Vector3(0.469F, 0.469F, 0.469F))
                .Add(Idrs.Huntress, "Arrow", new Vector3(0.343F, 0F, 0F), new Vector3(87.415F, 144.866F, 55.112F), new Vector3(0.388F, 0.388F, 0.388F))
                .Add(Idrs.Bandit2, "Chest", new Vector3(0.153F, -0.144F, 0.066F), new Vector3(355.538F, 89.398F, 170.59F), new Vector3(0.507F, 0.507F, 0.507F))
                .Add(Idrs.Toolbot, "Head", new Vector3(-0.925F, 2.842F, 1.601F), new Vector3(45.327F, 331.491F, 198.947F), new Vector3(3.118F, 3.118F, 3.118F))
                .Add(Idrs.Engi, "Pelvis", new Vector3(0.205F, 0.05F, -0.102F), new Vector3(0F, 114.381F, 354.036F), new Vector3(0.523F, 0.523F, 0.523F))
                .Add(Idrs.EngiTurret, "Head", new Vector3(0.681F, 1.016F, -0.988F), new Vector3(0.775F, 180F, 202.127F), new Vector3(1.588F, 1.588F, 1.588F))
                .Add(Idrs.EngiWalkerTurret, "Head", new Vector3(0.566F, 1.491F, -0.94F), new Vector3(7.103F, 180F, 204.769F), new Vector3(1.588F, 1.588F, 1.588F))
                .Add(Idrs.Mage, "Pelvis", new Vector3(-0.159F, -0.085F, -0.09F), new Vector3(356.235F, 252.299F, 344.311F), new Vector3(0.46F, 0.46F, 0.46F))
                .Add(Idrs.Merc, "UpperArmL", new Vector3(0.161F, -0.006F, 0.001F), new Vector3(29.587F, 212.128F, 321.824F), new Vector3(0.493F, 0.493F, 0.493F))
                .Add(Idrs.Treebot, "PlatformBase", new Vector3(1.062F, 0.782F, 0.174F), new Vector3(337.728F, 201.301F, 224.188F), new Vector3(1.056F, 1.056F, 1.056F))
                .Add(Idrs.Loader, "MechUpperArmL", new Vector3(0.037F, 0.053F, -0.154F), new Vector3(335.055F, 244.872F, 293.27F), new Vector3(0.547F, 0.547F, 0.547F))
                .Add(Idrs.Croco, "Head", new Vector3(1.926F, -0.053F, -0.112F), new Vector3(45.85F, 17.71F, 113.992F), new Vector3(5.36F, 5.36F, 5.36F))
                .Add(Idrs.Captain, "ClavicleL", new Vector3(0.021F, 0.136F, -0.226F), new Vector3(52.975F, 287.284F, 287.388F), new Vector3(0.587F, 0.587F, 0.587F))
                .Add(Idrs.RailGunner, "Pelvis", new Vector3(0.155F, 0.079F, -0.029F), new Vector3(10.264F, 100.904F, 358.845F), new Vector3(0.434F, 0.434F, 0.434F))
                .Add(Idrs.VoidSurvivor, "ShoulderL", new Vector3(0.063F, 0.289F, 0.052F), new Vector3(13.815F, 321.452F, 169.227F), new Vector3(0.597F, 0.597F, 0.597F))
                .Add(Idrs.Scav, "Weapon", new Vector3(3.037F, 8.08F, 2.629F), new Vector3(45.304F, 318.616F, 106.156F), new Vector3(5.5F, 5.5F, 5.5F));

            yield return DamageColorFrame.Create(nameof(Assets.DamageColors.StrongerBurn), new Color32(244, 113, 80, 255));

            impactArrowhead = omniExplosionVFXQuick.WaitForCompletion().InstantiateClone("ImpactArrowhead", false);
            /*DestroyImmediate(impactArrowhead.transform.Find("ScaledSmoke, Billboard").gameObject);
            DestroyImmediate(impactArrowhead.transform.Find("Unscaled Smoke, Billboard").gameObject);
            DestroyImmediate(impactArrowhead.transform.Find("Unscaled Flames").gameObject);
            DestroyImmediate(impactArrowhead.transform.Find("ScaledSmoke, Billboard").gameObject);*/
            if (impactArrowhead.TryGetComponent(out EffectComponent effectComponent))
            {
                effectComponent.soundName = "Play_item_proc_strengthenBurn";
            }
            if (impactArrowhead.TryGetComponent(out VFXAttributes vFXAttributes))
            {
                vFXAttributes.vfxPriority = VFXAttributes.VFXPriority.Low;
            }
            if (impactArrowhead.TryGetComponent(out OmniEffect omniEffect))
            {
                for(int i = omniEffect.omniEffectGroups.Length - 1; i >= 0; i--)
                {
                    switch (omniEffect.omniEffectGroups[i].name)
                    {
                        case "Scaled Smoke":
                        case "Smoke Ring":
                        case "Area Indicator Ring":
                        case "Unscaled Smoke":
                        case "Flames":
                            ArrayUtils.ArrayRemoveAtAndResize(ref omniEffect.omniEffectGroups, i);
                            break;
                    }
                }
            }
            yield return impactArrowhead;

            impactArrowheadStronger = impactArrowhead.InstantiateClone("ImpactArrowHeadStronger", false);
            impactArrowheadStronger.transform.GetChild(0).GetComponent<Renderer>().material = matOmniHitspark3Gasoline.WaitForCompletion();
            yield return impactArrowheadStronger;

        }
    }
}
