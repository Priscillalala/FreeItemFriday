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
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using FreeItemFriday.Achievements;

namespace FreeItemFriday.Equipment
{
    [Configurable(section = "Godless Eye")]
    public class DeathEye : ModModule<DeathEye>
    {
        [Configurable]
        public static float range = 200f;
        [Configurable]
        public static float duration = 2f;
        [Configurable]
        public static int maxConsecutiveEnemies = 10;

        public LazyAddressable<GameObject> msObelisk = "RoR2/Base/mysteryspace/MSObelisk.prefab";
        public LazyAddressable<Material> matMSObeliskLightning = "RoR2/Base/mysteryspace/matMSObeliskLightning.mat";
        public LazyAddressable<Material> matMSObeliskHeart = "RoR2/Base/mysteryspace/matMSObeliskHeart.mat";
        public LazyAddressable<Material> matJellyfishLightning = "RoR2/Base/Common/VFX/matJellyfishLightning.mat";
        public LazyAddressable<Material> matMSStarsLink = "RoR2/Base/mysteryspace/matMSStarsLink.mat";

        public GameObject delayedDeathHandler;

        public override IEnumerator LoadContent()
        {
            yield return new AchievementFrame
            {
                identifier = nameof(Assets.Achievements.CompleteMultiplayerUnknownEnding),
                unlockableRewardName = "FreeItemFriday.Items.DeathEye",
                achievementIcon = Assets.bundle.LoadAsset<Sprite>("texCompleteMultiplayerUnknownEndingIcon")
            }.SetTrackerTypes<CompleteMultiplayerUnknownEndingAchievement, CompleteMultiplayerUnknownEndingAchievement.ServerAchievement>();

            GameObject pickupPrefab = Assets.bundle.LoadAsset<GameObject>("PickupDeathEye");

            MeshRenderer modelRenderer = pickupPrefab.transform.Find("mdlDeathEye").GetComponent<MeshRenderer>();
            List<Material> sharedMats = new List<Material>(); 
            modelRenderer.GetSharedMaterials(sharedMats);
            sharedMats[1] = matMSObeliskLightning;
            modelRenderer.SetSharedMaterials(sharedMats);

            GameObject pickupConsumedPrefab = pickupPrefab.InstantiateClone("PickupDeathEyeConsumed", false);
            DestroyImmediate(pickupConsumedPrefab.transform.Find("EyeBallFX").gameObject);

            pickupPrefab.transform.Find("EyeBallFX/Weird Sphere").GetComponent<ParticleSystemRenderer>().sharedMaterial = matMSObeliskHeart;
            pickupPrefab.transform.Find("EyeBallFX/LongLifeNoiseTrails, Bright").GetComponent<ParticleSystemRenderer>().trailMaterial = matMSStarsLink;
            pickupPrefab.transform.Find("EyeBallFX/Lightning").GetComponent<ParticleSystemRenderer>().sharedMaterial = matJellyfishLightning;

            yield return new EquipmentFrame
            {
                name = nameof(Assets.Equipment.DeathEye),
                icon = Assets.bundle.LoadAsset<Sprite>("texDeathEyeIcon"),
                equipmentType = EquipmentType.Lunar,
                appearsInSingleplayer = false,
                appearsInMultiplayer = true,
                canBeRandomlyTriggered = false,
                enigmaCompatible = false,
                cooldown = 60f,
                pickupModelPrefab = pickupPrefab,
                performEquipmentActionServer = FireDeathEye,
                unlockableDef = Assets.Unlockables.DeathEye
            }.SetLogbookModelParameters(Vector3.zero, 3, 10);

            yield return new EquipmentFrame
            {
                name = nameof(Assets.Equipment.DeathEyeConsumed),
                icon = Assets.bundle.LoadAsset<Sprite>("texDeathEyeConsumedIcon"),
                equipmentType = EquipmentType.Lunar,
                overrideColorIndex = ColorCatalog.ColorIndex.Unaffordable,
                appearsInSingleplayer = false,
                appearsInMultiplayer = true,
                canBeRandomlyTriggered = false,
                enigmaCompatible = false,
                canDrop = false,
                pickupModelPrefab = pickupConsumedPrefab,
            };

            GameObject displayPrefab = new GameObject().InstantiateClone("DisplayDeathEye", false);
            displayPrefab.AddComponent<ItemDisplay>();
            //displayPrefab.AddComponent<UpdateItemFollowerScaleDebug>();
            ItemFollower itemFollower = displayPrefab.AddComponent<ItemFollower>();
            itemFollower.targetObject = displayPrefab;
            itemFollower.followerPrefab = pickupPrefab;
            itemFollower.distanceDampTime = 0.005f;
            itemFollower.distanceMaxSpeed = 200f;
            yield return SingleItemDisplayFrame.Create(Assets.Equipment.DeathEye, displayPrefab)
                .Add(Idrs.Commando, "Head", new Vector3(0.001F, 0.545F, -0.061F), new Vector3(0F, 90F, 0F), new Vector3(0.069F, 0.069F, 0.069F))
                .Add(Idrs.Huntress, "Head", new Vector3(-0.002F, 0.486F, -0.158F), new Vector3(359.97F, 89.949F, 345.155F), new Vector3(0.067F, 0.067F, 0.067F))
                .Add(Idrs.Bandit2, "Head", new Vector3(-0.001F, 0.367F, -0.002F), new Vector3(0F, 89.995F, 0.001F), new Vector3(0.066F, 0.066F, 0.066F))
                .Add(Idrs.Toolbot, "Head", new Vector3(-0.002F, 4.049F, 3.237F), new Vector3(0.708F, 89.264F, 50.748F), new Vector3(0.111F, 0.111F, 0.111F))
                .Add(Idrs.Engi, "Chest", new Vector3(-0.001F, 1.049F, 0.174F), new Vector3(0F, 90F, 0F), new Vector3(0.089F, 0.089F, 0.089F))
                .Add(Idrs.Mage, "Head", new Vector3(-0.002F, 0.328F, 0.003F), new Vector3(0F, 90F, 0F), new Vector3(0.055F, 0.055F, 0.055F))
                .Add(Idrs.Merc, "Head", new Vector3(-0.002F, 0.452F, -0.01F), new Vector3(0F, 90F, 0F), new Vector3(0.06F, 0.06F, 0.06F))
                .Add(Idrs.Treebot, "Chest", new Vector3(0.157F, 3.44F, 0F), new Vector3(0F, 90F, 0F), new Vector3(0.148F, 0.148F, 0.148F))
                .Add(Idrs.Loader, "Head", new Vector3(-0.002F, 0.442F, 0F), new Vector3(0F, 90F, 0F), new Vector3(0.089F, 0.089F, 0.089F))
                .Add(Idrs.Croco, "Head", new Vector3(-0.036F, -0.134F, 3.731F), new Vector3(0.141F, 89.889F, 298.828F), new Vector3(0.152F, 0.152F, 0.152F))
                .Add(Idrs.Captain, "Base", new Vector3(-0.03F, 0.199F, -1.281F), new Vector3(0F, 90F, 90F), new Vector3(0.062F, 0.062F, 0.062F))
                .Add(Idrs.RailGunner, "Head", new Vector3(-0.001F, 0.363F, -0.089F), new Vector3(0F, 90F, 0F), new Vector3(0.056F, 0.056F, 0.056F))
                .Add(Idrs.VoidSurvivor, "Head", new Vector3(-0.006F, 0.322F, -0.217F), new Vector3(357.745F, 91.815F, 321.156F), new Vector3(0.069F, 0.069F, 0.069F))
                .Add(Idrs.EquipmentDrone, "HeadCenter", new Vector3(0F, 0F, 1.987F), new Vector3(0F, 90F, 90F), new Vector3(0.351F, 0.351F, 0.351F));

            GameObject consumedDisplayPrefab = displayPrefab.InstantiateClone("DisplayDeathEyeConsumed", false);
            ItemFollower consumedItemFollower = consumedDisplayPrefab.GetComponent<ItemFollower>();
            consumedItemFollower.targetObject = consumedDisplayPrefab;
            consumedItemFollower.followerPrefab = pickupConsumedPrefab;
            yield return SingleItemDisplayFrame.Create(Assets.Equipment.DeathEyeConsumed, consumedDisplayPrefab)
                .Add(Idrs.Commando, "Head", new Vector3(0.001F, 0.545F, -0.061F), new Vector3(0F, 90F, 0F), new Vector3(0.069F, 0.069F, 0.069F))
                .Add(Idrs.Huntress, "Head", new Vector3(-0.002F, 0.486F, -0.158F), new Vector3(359.97F, 89.949F, 345.155F), new Vector3(0.067F, 0.067F, 0.067F))
                .Add(Idrs.Bandit2, "Head", new Vector3(-0.001F, 0.367F, -0.002F), new Vector3(0F, 89.995F, 0.001F), new Vector3(0.066F, 0.066F, 0.066F))
                .Add(Idrs.Toolbot, "Head", new Vector3(-0.002F, 4.049F, 3.237F), new Vector3(0.708F, 89.264F, 50.748F), new Vector3(0.111F, 0.111F, 0.111F))
                .Add(Idrs.Engi, "Chest", new Vector3(-0.001F, 1.049F, 0.174F), new Vector3(0F, 90F, 0F), new Vector3(0.089F, 0.089F, 0.089F))
                .Add(Idrs.Mage, "Head", new Vector3(-0.002F, 0.328F, 0.003F), new Vector3(0F, 90F, 0F), new Vector3(0.055F, 0.055F, 0.055F))
                .Add(Idrs.Merc, "Head", new Vector3(-0.002F, 0.452F, -0.01F), new Vector3(0F, 90F, 0F), new Vector3(0.06F, 0.06F, 0.06F))
                .Add(Idrs.Treebot, "Chest", new Vector3(0.157F, 3.44F, 0F), new Vector3(0F, 90F, 0F), new Vector3(0.148F, 0.148F, 0.148F))
                .Add(Idrs.Loader, "Head", new Vector3(-0.002F, 0.442F, 0F), new Vector3(0F, 90F, 0F), new Vector3(0.089F, 0.089F, 0.089F))
                .Add(Idrs.Croco, "Head", new Vector3(-0.036F, -0.134F, 3.731F), new Vector3(0.141F, 89.889F, 298.828F), new Vector3(0.152F, 0.152F, 0.152F))
                .Add(Idrs.Captain, "Base", new Vector3(-0.03F, 0.199F, -1.281F), new Vector3(0F, 90F, 90F), new Vector3(0.062F, 0.062F, 0.062F))
                .Add(Idrs.RailGunner, "Head", new Vector3(-0.001F, 0.363F, -0.089F), new Vector3(0F, 90F, 0F), new Vector3(0.056F, 0.056F, 0.056F))
                .Add(Idrs.VoidSurvivor, "Head", new Vector3(-0.006F, 0.322F, -0.217F), new Vector3(357.745F, 91.815F, 321.156F), new Vector3(0.069F, 0.069F, 0.069F))
                .Add(Idrs.EquipmentDrone, "HeadCenter", new Vector3(0F, 0F, 1.987F), new Vector3(0F, 90F, 90F), new Vector3(0.351F, 0.351F, 0.351F));

            delayedDeathHandler = msObelisk.WaitForCompletion().transform.Find("Stage1FX").gameObject.InstantiateClone("DelayedDeathHandler", false);
            delayedDeathHandler.SetActive(true);
            delayedDeathHandler.AddComponent<NetworkIdentity>();
            PrefabAPI.RegisterNetworkPrefab(delayedDeathHandler);
            delayedDeathHandler.AddComponent<DelayedDeathEye>();    
            delayedDeathHandler.AddComponent<DestroyOnTimer>().duration = duration;
            DestroyImmediate(delayedDeathHandler.transform.Find("LongLifeNoiseTrails, Bright").gameObject);
            DestroyImmediate(delayedDeathHandler.transform.Find("PersistentLight").gameObject);
        }
        public bool FireDeathEye(EquipmentSlot equipmentSlot)
        {
            if (!equipmentSlot.healthComponent || !equipmentSlot.healthComponent.alive)
            {
                return false;
            }
            Vector3 position = equipmentSlot.characterBody?.corePosition ?? equipmentSlot.transform.position;
            DelayedDeathEye delayedDeathEye = Instantiate(delayedDeathHandler, position, Quaternion.identity).GetComponent<DelayedDeathEye>();

            TeamMask teamMask = TeamMask.allButNeutral;
            if (equipmentSlot.teamComponent) 
            {
                teamMask.RemoveTeam(equipmentSlot.teamComponent.teamIndex);
            }
            delayedDeathEye.cleanupTeams = teamMask;

            List<DelayedDeathEye.DeathGroup> deathGroups = new List<DelayedDeathEye.DeathGroup>();
            int consecutiveEnemies = 0;
            BodyIndex currentBodyIndex = BodyIndex.None;
            List<CharacterBody> currentVictims = new List<CharacterBody>();
            foreach (CharacterBody body in CharacterBody.readOnlyInstancesList)
            {
                if (teamMask.HasTeam(body.teamComponent.teamIndex) && (body.corePosition - position).sqrMagnitude <= range * range)
                {
                    if (body.bodyIndex != currentBodyIndex || consecutiveEnemies >= maxConsecutiveEnemies)
                    {
                        currentBodyIndex = body.bodyIndex;
                        consecutiveEnemies = 0;
                        if (currentVictims.Count > 0)
                        {
                            deathGroups.Add(new DelayedDeathEye.DeathGroup
                            {
                                victimBodies = new List<CharacterBody>(currentVictims),
                            });
                        }
                        currentVictims.Clear();
                    }
                    currentVictims.Add(body);
                    consecutiveEnemies++;
                }
            }
            if (currentVictims.Count > 0)
            {
                deathGroups.Add(new DelayedDeathEye.DeathGroup
                {
                    victimBodies = new List<CharacterBody>(currentVictims),
                });
            }
            currentVictims.Clear();
            deathGroups.Add(new DelayedDeathEye.DeathGroup 
            { 
                victimBodies = new List<CharacterBody>() { equipmentSlot.characterBody } 
            });
            if (deathGroups.Count > 0) 
            {
                float durationBetweenDeaths = duration / deathGroups.Count;
                for (int i = 0; i < deathGroups.Count; i++)
                {
                    DelayedDeathEye.DeathGroup group = deathGroups[i];
                    group.time = Run.FixedTimeStamp.now + (durationBetweenDeaths * i);
                    delayedDeathEye.EnqueueDeath(group);
                }
            }
            NetworkServer.Spawn(delayedDeathEye.gameObject);

            if (equipmentSlot.characterBody?.inventory)
            {
                CharacterMasterNotificationQueue.SendTransformNotification(equipmentSlot.characterBody.master, equipmentSlot.characterBody.inventory.currentEquipmentIndex, Assets.Equipment.DeathEyeConsumed.equipmentIndex, CharacterMasterNotificationQueue.TransformationType.Default);
                equipmentSlot.characterBody.inventory.SetEquipmentIndex(Assets.Equipment.DeathEyeConsumed.equipmentIndex);
            }
            return true;
        }
        public class DelayedDeathEye : MonoBehaviour
        {
            public struct DeathGroup
            {
                public Run.FixedTimeStamp time;
                public List<CharacterBody> victimBodies;
            }
            public Queue<DeathGroup> deathQueue = new Queue<DeathGroup>();
            public TeamMask cleanupTeams = TeamMask.none;
            private bool hasRunCleanup;
            private GameObject destroyEffectPrefab;
            public void EnqueueDeath(DeathGroup death)
            {
                deathQueue.Enqueue(death);
            }
            public void Awake()
            {
                destroyEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/BrittleDeath.prefab").WaitForCompletion();
            }
            public void Start()
            {
                Util.PlaySound("Play_vagrant_R_explode", base.gameObject);
            }
            public void FixedUpdate() 
            {
                if (!NetworkServer.active)
                {
                    return;
                }
                if (deathQueue.Count <= 0) 
                {
                    RunCleanup();
                    enabled = false;
                    return;
                }
                if (deathQueue.Peek().time.hasPassed)
                {
                    List<CharacterBody> victimBodies = deathQueue.Dequeue().victimBodies;
                    foreach (CharacterBody victim in victimBodies)
                    {
                        DestroyVictim(victim);
                    }
                }
                
            }
            
            public void RunCleanup()
            {
                if (hasRunCleanup)
                {
                    return;
                }
                if (CharacterBody.readOnlyInstancesList.Count > 0)
                {
                    for (int i = CharacterBody.readOnlyInstancesList.Count - 1; i >= 0; i--)
                    {
                        CharacterBody body = CharacterBody.readOnlyInstancesList[i];
                        if (body.teamComponent && cleanupTeams.HasTeam(body.teamComponent.teamIndex) && (body.corePosition - transform.position).sqrMagnitude <= range * range)
                        {
                            DestroyVictim(body);
                        }
                    }
                }
                hasRunCleanup = true;
            }

            public void DestroyVictim(CharacterBody victim)
            {
                if (!victim)
                {
                    return;
                }
                if (victim.master)
                {
                    victim.master.preventGameOver = false;
                }
                EffectManager.SpawnEffect(destroyEffectPrefab, new EffectData
                {
                    origin = victim.corePosition,
                    scale = victim.radius
                }, true);
                Destroy(victim.gameObject);
            }
        }
    }
}
