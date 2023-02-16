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
using Mono.Cecil.Cil;
using MonoMod.Cil;
using KinematicCharacterController;
using UnityEngine.SceneManagement;
using ThreeEyedGames;
using RoR2.Projectile;

namespace FreeItemFriday.Artifacts
{
    [Configurable(section = "Artifact of Entropy")]
    public class SlipperyTerrain : ModModule<SlipperyTerrain>
    {
        [Configurable]
        public static float baseAccelerationMultiplier = 0.05f;
        [Configurable]
        public static float airAccelerationCoefficient = 2f;
        [Configurable]
        public static float positiveAccelerationCoefficient = 10f;
        [Configurable]
        public static float horizontalJumpBoostCoefficient = 0.5f;

        public GameObject slipperyTerrainFormulaDisplay;
        public bool didUpdateSceneVisuals = false;
        public Dictionary<Material, Material> slipperyMaterialInstances = new Dictionary<Material, Material>();

        public LazyAddressable<GameObject> artifactFormulaDisplay = "RoR2/Base/artifactworld/ArtifactFormulaDisplay.prefab";
        public LazyAddressable<Material> matArtifact = "RoR2/Base/artifactworld/matArtifact.mat";
        public LazyAddressable<GameObject> engiBubbleShield = "RoR2/Base/Engi/EngiBubbleShield.prefab";
        public LazyAddressable<Shader> standard = "RoR2/Base/Shaders/HGStandard.shader";
        public LazyAddressable<Shader> snowTopped = "RoR2/Base/Shaders/HGSnowTopped.shader";
        public LazyAddressable<Shader> triplanar = "RoR2/Base/Shaders/HGTriplanarTerrainBlend.shader";
        public override IEnumerator LoadContent()
        {
            yield return new AchievementFrame
            {
                identifier = nameof(Assets.Achievements.ObtainArtifactSlipperyTerrain),
                unlockableRewardName = "FreeItemFriday.Artifacts.SlipperyTerrain",
                achievementIcon = Assets.bundle.LoadAsset<Sprite>("texObtainArtifactSlipperyTerrainIcon")
            }.SetTrackerType<ObtainArtifactSlipperyTerrainAchievement>();

            ArtifactCodeInfo artifactCodeInfo = default;
            artifactCodeInfo.SetTopRow(ArtifactCompounds.Circle, ArtifactCompounds.Circle, ArtifactCompounds.Circle);
            artifactCodeInfo.SetMiddleRow(ArtifactCompounds.Square, ArtifactCompounds.Square, ArtifactCompounds.Square);
            artifactCodeInfo.SetBottomRow(ArtifactCompounds.Triangle, ArtifactCompounds.Diamond, ArtifactCompounds.Triangle);

            yield return new ArtifactFrame
            {
                name = nameof(Assets.Artifacts.SlipperyTerrain),
                unlockableDef = Assets.Unlockables.SlipperyTerrain,
                enabledIcon = Assets.bundle.LoadAsset<Sprite>("texArtifactSlipperyTerrainEnabled"),
                disabledIcon = Assets.bundle.LoadAsset<Sprite>("texArtifactSlipperyTerrainDisabled"),
                enabledAction = OnArtifactEnabled,
                disabledAction = OnArtifactDisabled,
                pickupModelPrefab = Assets.bundle.LoadAsset<GameObject>("PickupSlipperyTerrain"),
                artifactCode = artifactCodeInfo,
            };

            slipperyTerrainFormulaDisplay = PrefabAPI.InstantiateClone(artifactFormulaDisplay, "SlipperyTerrainFormulaDisplay", false);
            artifactCodeInfo.CopyToFormulaDisplay(slipperyTerrainFormulaDisplay.GetComponent<ArtifactFormulaDisplay>());
            foreach(Decal decal in slipperyTerrainFormulaDisplay.GetComponentsInChildren<Decal>())
            {
                decal.Fade = 0.15f;
            }
            if (slipperyTerrainFormulaDisplay.transform.TryFind("Frame", out Transform frame))
            {
                frame.gameObject.SetActive(false);
            }
            if (slipperyTerrainFormulaDisplay.transform.TryFind("ArtifactFormulaHolderMesh", out Transform mesh))
            {
                mesh.gameObject.SetActive(false);
            }

            if (Assets.Artifacts.SlipperyTerrain.pickupModelPrefab.transform.TryFind("mdlSlipperyTerrainArtifact", out Transform mdl) && mdl.TryGetComponent(out MeshRenderer renderer))
            {
                renderer.material = matArtifact;
            }

            if(engiBubbleShield.WaitForCompletion().transform.TryFind("Collision/ActiveVisual", out Transform activeVisual))
            {
                activeVisual.gameObject.AddComponent<FreezeRotationWhenArtifactEnabled>();
            }
        }
        public void Awake()
        {
            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        }
        private void SceneManager_activeSceneChanged(Scene oldScene, Scene newScene)
        {
            if(newScene.name == "snowyforest" && slipperyTerrainFormulaDisplay)
            {
                //Instantiate(slipperyTerrainFormulaDisplay, new Vector3(120, 33, 198), Quaternion.Euler(new Vector3(340, 200, 25))).transform.localScale = Vector3.one * 10f;
                Instantiate(slipperyTerrainFormulaDisplay, new Vector3(150, 67, 237), Quaternion.Euler(new Vector3(276, 10, 190))).transform.localScale = Vector3.one * 12f;
            }
            if (didUpdateSceneVisuals = (RunArtifactManager.instance && RunArtifactManager.instance.IsArtifactEnabled(Assets.Artifacts.SlipperyTerrain)))
            {
                UpdateSceneVisuals();
            }
        }
        public void UpdateSceneVisuals()
        {
            foreach (Material materialInstance in slipperyMaterialInstances.Values)
            {
                Destroy(materialInstance);
            }
            slipperyMaterialInstances.Clear();

            MeshRenderer[] meshRenderers = FindObjectsOfType<MeshRenderer>();
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                MeshRenderer meshRenderer = meshRenderers[i];
                if (meshRenderer.gameObject.layer != LayerIndex.world.intVal || !meshRenderer.gameObject.activeInHierarchy || meshRenderer.GetComponent<Decal>())
                {
                    continue;
                }
                Material mat = meshRenderer.sharedMaterial;
                if (!mat)
                {
                    continue;
                }
                if (!slipperyMaterialInstances.TryGetValue(mat, out Material matInstance) && (mat.shader == standard || mat.shader == snowTopped || mat.shader == triplanar))
                {
                    matInstance = Instantiate(mat);
                    if (mat.shader == standard)
                    {
                        matInstance.SetFloat("_SpecularStrength", 0.6f);
                        matInstance.SetFloat("_SpecularExponent ", 10f);
                    }
                    else if (mat.shader == snowTopped)
                    {
                        if (matInstance.GetTexture("_SnowNormalTex"))
                        {
                            matInstance.SetFloat("_SpecularStrength", 0.1f);
                            matInstance.SetFloat("_SpecularExponent", 20f);
                            matInstance.SetFloat("_SnowSpecularStrength", 0.4f);
                            matInstance.SetFloat("_SnowSpecularExponent", 8f);
                        }
                        else
                        {
                            matInstance.SetFloat("_SpecularStrength", 0.4f);
                            matInstance.SetFloat("_SpecularExponent", 3f);
                        }
                    }
                    else if (mat.shader == triplanar)
                    {
                        matInstance.SetFloat("_GreenChannelSpecularStrength", 0.15f);
                        matInstance.SetFloat("_GreenChannelSpecularExponent", 8f);
                    }
                    matInstance.SetInt("_RampInfo", 1);
                    slipperyMaterialInstances[mat] = matInstance;
                }
                if (matInstance)
                {
                    meshRenderers[i].sharedMaterial = matInstance;
                }
            }
        }
        public void OnArtifactEnabled(RunArtifactManager runArtifactManager)
        {
            if (!didUpdateSceneVisuals)
            {
                UpdateSceneVisuals();
            }
            On.RoR2.CharacterMotor.OnGroundHit += CharacterMotor_OnGroundHit;
            IL.EntityStates.GenericCharacterMain.ApplyJumpVelocity += GenericCharacterMain_ApplyJumpVelocity;
            IL.RoR2.CharacterMotor.PreMove += CharacterMotor_PreMove;
            On.RoR2.Projectile.ProjectileStickOnImpact.UpdateSticking += ProjectileStickOnImpact_UpdateSticking;
        }

        public void OnArtifactDisabled(RunArtifactManager runArtifactManager)
        {
            On.RoR2.Projectile.ProjectileStickOnImpact.UpdateSticking -= ProjectileStickOnImpact_UpdateSticking;
            IL.RoR2.CharacterMotor.PreMove -= CharacterMotor_PreMove;
            IL.EntityStates.GenericCharacterMain.ApplyJumpVelocity -= GenericCharacterMain_ApplyJumpVelocity;
            On.RoR2.CharacterMotor.OnGroundHit -= CharacterMotor_OnGroundHit;
        }

        private void ProjectileStickOnImpact_UpdateSticking(On.RoR2.Projectile.ProjectileStickOnImpact.orig_UpdateSticking orig, ProjectileStickOnImpact self)
        {
            if (self.hitHurtboxIndex == -2 && !self.GetComponent<ProjectileGrappleController>())
            {
                if (self.rigidbody.isKinematic)
                {
                    self.rigidbody.isKinematic = false;
                    self.rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                }
                return;
            }
            orig(self);
        }

        private void CharacterMotor_OnGroundHit(On.RoR2.CharacterMotor.orig_OnGroundHit orig, CharacterMotor self, Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            orig(self, hitCollider, hitNormal, hitPoint, ref hitStabilityReport);
            self.isAirControlForced = true;
        }

        private void GenericCharacterMain_ApplyJumpVelocity(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            bool ilFound = c.TryGotoNext(MoveType.Before,
                x => x.MatchCallOrCallvirt<KinematicCharacterMotor>(nameof(KinematicCharacterMotor.ForceUnground))
                ) && c.TryGotoPrev(MoveType.Before,
                x => x.MatchStfld<CharacterMotor>(nameof(CharacterMotor.velocity))
                );
            if (ilFound)
            {
                c.Emit(OpCodes.Ldarg, 0);
                c.Emit(OpCodes.Ldarg, 2);
                c.EmitDelegate<Func<Vector3, CharacterMotor, float, Vector3>>((newVelocity, motor, horizontalMultiplier) =>
                {
                    float adjustedHorizontalMultiplier = ((horizontalMultiplier - 1) / horizontalMultiplier) * horizontalJumpBoostCoefficient;
                    return new Vector3(motor.velocity.x + newVelocity.x * adjustedHorizontalMultiplier, newVelocity.y, motor.velocity.z + newVelocity.z * adjustedHorizontalMultiplier);
                });
            }
            else
            {
                Log(BepInEx.Logging.LogLevel.Error, nameof(GenericCharacterMain_ApplyJumpVelocity) + "IL hook failed!");
            }
        }

        private void CharacterMotor_PreMove(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            int locTargetIndex = -1;
            int locAcclerationIndex = -1;
            bool ilFound = c.TryGotoNext(MoveType.Before,
                x => x.MatchCallOrCallvirt(typeof(Vector3).GetMethod(nameof(Vector3.MoveTowards))),
                x => x.MatchStfld<CharacterMotor>(nameof(CharacterMotor.velocity))
                ) && c.TryGotoPrev(MoveType.Before, 
                x => x.MatchLdfld<CharacterMotor>(nameof(CharacterMotor.velocity)),
                x => x.MatchLdloc(out locTargetIndex),
                x => x.MatchLdloc(out locAcclerationIndex)
                );
            if (ilFound)
            {
                c.Emit(OpCodes.Ldloc, locTargetIndex);
                c.Emit(OpCodes.Ldloc, locAcclerationIndex);
                c.Emit(OpCodes.Ldarg, 0);
                c.EmitDelegate<Func<Vector3, float, CharacterMotor, float>>((target, acceleration, motor) =>
                {
                    if (true)
                    {
                        float multiplier = baseAccelerationMultiplier;
                        if (target.sqrMagnitude > motor.velocity.sqrMagnitude)
                        {
                            multiplier *= positiveAccelerationCoefficient;
                        }
                        else if (!motor.isGrounded)
                        {
                            multiplier *= airAccelerationCoefficient;
                        }
                        if (!motor.body || motor.body.moveSpeed * motor.body.moveSpeed >= motor.velocity.sqrMagnitude)
                        {
                            acceleration *= multiplier;
                        }
                        else
                        {
                            acceleration *= 1 - ((1 - multiplier) * Mathf.Sqrt(motor.body.moveSpeed / motor.velocity.magnitude));
                        }
                    }
                    return acceleration;
                });
                c.Emit(OpCodes.Stloc, locAcclerationIndex);
            }
            else
            {
                Log(BepInEx.Logging.LogLevel.Error, nameof(CharacterMotor_PreMove) + "IL hook failed!");
            }
        }
        public class FreezeRotationWhenArtifactEnabled : MonoBehaviour
        {
            private Quaternion rotation;
            public void Start()
            {
                rotation = transform.rotation;
                if (!RunArtifactManager.instance || !RunArtifactManager.instance.IsArtifactEnabled(Assets.Artifacts.SlipperyTerrain))
                {
                    enabled = false;
                }
            }
            public void Update()
            {
                transform.rotation = rotation;
            }
        }
    }
}
