using System;
using Unity;
using UnityEngine;
using GrooveSharedUtils;
using GrooveSharedUtils.Frames;
using GrooveSharedUtils.Attributes;
using GrooveSharedUtils.Interfaces;
using RoR2;
using RoR2.Items;
using static FreeItemFriday.Assets;
using static GrooveSharedUtils.Common;
using R2API;
using System.Collections;
using HG;
using RoR2.Achievements;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace FreeItemFriday.ItemModules
{
    [Configurable]
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
            DotController.dotDefs[(int)DotController.DotIndex.StrongerBurn].damageColorIndex = DamageColors.StrongerBurn.damageColorIndex;
        }

        private void Events_onHitEnemyServer(DamageInfo damageInfo, GameObject victim)
        {
            if (damageInfo.attacker && damageInfo.attacker.TryGetComponent(out CharacterBody attackerBody) && attackerBody.HasItem(Items.Arrowhead, out int stack) && Util.CheckRoll(100f * damageInfo.procCoefficient, attackerBody.master))
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
                identifier = nameof(Achievements.BurnMultipleEnemies),
                unlockableRewardName = "Items.Arrowhead",
                achievementIcon = bundle.LoadAsset<Sprite>("texBurnMultipleEnemiesIcon")
            }.SetTrackerTypes<BurnMultipleEnemiesAchievement, BurnMultipleEnemiesAchievement.ServerAchievement>().Build();

            yield return new ItemFrame
            {
                name = nameof(Items.Arrowhead),
                icon = bundle.LoadAsset<Sprite>("texArrowheadIcon"),
                itemTier = ItemTier.Tier1,
                pickupModelPrefab = bundle.LoadAsset<GameObject>("PickupArrowhead"),
                unlockableDef = Unlockables.Items_Arrowhead,
            }.SetItemTags(ItemTag.Damage).Build();
            GSUtil.SetupModelPanelParameters(Items.Arrowhead.pickupModelPrefab, Vector3.zero, 1, 8);

            GameObject displayPrefab = bundle.LoadAsset<GameObject>("DisplayArrowhead");
            GSUtil.SetupItemDisplay(displayPrefab);
            SingleItemDisplayFrame.Create(Items.Arrowhead, displayPrefab)
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
                .Add(Idrs.Scav)
                .Build();

            yield return DamageColorFrame.Create(nameof(DamageColors.StrongerBurn), new Color32(244, 113, 80, 255)).Build();

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

        public class BurnMultipleEnemiesAchievement : BaseAchievement
        {
            public static bool IsBurnDot(DotController.DotIndex index) => index == DotController.DotIndex.Burn || index == DotController.DotIndex.PercentBurn || index == DotController.DotIndex.Helfire || index == DotController.DotIndex.StrongerBurn;
            public override void OnInstall()
            {
                base.OnInstall();
                base.SetServerTracked(true);
            }
            public override void OnUninstall()
            {
                base.SetServerTracked(false);
                base.OnUninstall();
            }
            public class ServerAchievement : BaseServerAchievement
            {
                private List<DotController> affectedDotControllers;
                public override void OnInstall()
                {
                    base.OnInstall();
                    affectedDotControllers = new List<DotController>();
                    DotController.onDotInflictedServerGlobal += DotController_onDotInflictedServerGlobal;
                }

                private void DotController_onDotInflictedServerGlobal(DotController dotController, ref InflictDotInfo inflictDotInfo)
                {
                    GameObject currentBodyObject = GetCurrentBody()?.gameObject;
                    if (!currentBodyObject) return;
                    if (IsBurnDot(inflictDotInfo.dotIndex) && currentBodyObject == inflictDotInfo.attackerObject && currentBodyObject != inflictDotInfo.victimObject)
                    {
                        if (!affectedDotControllers.Contains(dotController)) affectedDotControllers.Add(dotController);

                        HashSet<NetworkInstanceId> affectedBodyObjects = new HashSet<NetworkInstanceId>();
                        int burningEnemiesCount = 0;
                        for (int i = affectedDotControllers.Count - 1; i >= 0; i--)
                        {
                            DotController otherDotController = affectedDotControllers[i];
                            if (!otherDotController)
                            {
                                affectedDotControllers.RemoveAt(i);
                                continue;
                            }

                            if (affectedBodyObjects.Contains(otherDotController.victimObjectId)) continue;

                            affectedBodyObjects.Add(otherDotController.victimObjectId);

                            if (IsDotControllerBurning(otherDotController, currentBodyObject))
                            {
                                burningEnemiesCount++;
                            }
                            else
                            {
                                affectedDotControllers.RemoveAt(i);
                            }
                        }
                        if (burningEnemiesCount >= 10)
                        {
                            base.Grant();
                        }
                    }
                }
                public bool IsDotControllerBurning(DotController dotController, GameObject currentBodyObject)
                {
                    for (int i = 0; i < dotController.dotStackList.Count; i++)
                    {
                        DotController.DotStack dotStack = dotController.dotStackList[i];
                        if (IsBurnDot(dotStack.dotIndex) && dotStack.attackerObject == currentBodyObject)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                public override void OnUninstall()
                {
                    DotController.onDotInflictedServerGlobal -= DotController_onDotInflictedServerGlobal;
                    affectedDotControllers = null;
                    base.OnUninstall();
                }
            }
        }

    }
}
