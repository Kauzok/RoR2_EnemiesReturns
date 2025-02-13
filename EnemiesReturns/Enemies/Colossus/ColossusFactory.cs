﻿using RoR2.Networking;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using KinematicCharacterController;
using System.Linq;
using Newtonsoft.Json.Utilities;
using R2API;
using RoR2.CharacterAI;
using EnemiesReturns.EditorHelpers;
using RoR2.Skills;
using RoR2.Projectile;
using HG;
using EntityStates;
using static RoR2.ItemDisplayRuleSet;
using EnemiesReturns.Projectiles;
using ThreeEyedGames;
using static EnemiesReturns.Utils;
using RoR2.Mecanim;
using EnemiesReturns.ModdedEntityStates.Colossus.Stomp;
using EnemiesReturns.ModdedEntityStates.Colossus.RockClap;

namespace EnemiesReturns.Enemies.Colossus
{
    public class ColossusFactory
    {
        public struct Skills
        {
            public static SkillDef Stomp;

            public static SkillDef StoneClap;

            public static SkillDef LaserClap;

            public static SkillDef HeadLaser;
        }

        public struct SkillFamilies
        {
            public static SkillFamily Primary;

            public static SkillFamily Secondary;

            public static SkillFamily Utility;

            public static SkillFamily Special;
        }

        public struct SkinDefs
        {

        }

        public struct SpawnCards
        {

        }

        public static GameObject colossusBody;

        public static GameObject colossusMaster;

        internal static GameObject stompProjectile;

        internal static GameObject stompEffect;

        public GameObject CreateColossusBody(GameObject bodyPrefab, Texture2D icon, UnlockableDef log, Dictionary<string, Material> skinsLookup)
        {
            var aimOrigin = bodyPrefab.transform.Find("AimOrigin");
            var modelTransform = bodyPrefab.transform.Find("ModelBase/mdlColossus");
            var modelBase = bodyPrefab.transform.Find("ModelBase");
            var crouchController = bodyPrefab.transform.Find("ModelBase/CrouchController");

            var focusPoint = bodyPrefab.transform.Find("ModelBase/mdlColossus/LogBookTarget");
            var cameraPosition = bodyPrefab.transform.Find("ModelBase/mdlColossus/LogBookTarget/LogBookCamera");

            var modelRenderer = bodyPrefab.transform.Find("ModelBase/mdlColossus/Colossus").gameObject.GetComponent<SkinnedMeshRenderer>();
            var headRenderer = bodyPrefab.transform.Find("ModelBase/mdlColossus/Colossus Head").gameObject.GetComponent<SkinnedMeshRenderer>();

            var headTransform = bodyPrefab.transform.Find("ModelBase/mdlColossus/Armature/root/root_pelvis_control/spine/spine.001/head");
            var rootTransform = bodyPrefab.transform.Find("ModelBase/mdlColossus/Armature/root");

            var rocksInitialTransform = bodyPrefab.transform.Find("ModelBase/mdlColossus/Points/RocksSpawnPoint");

            #region ColossusBody

            #region NetworkIdentity
            bodyPrefab.AddComponent<NetworkIdentity>().localPlayerAuthority = true;
            #endregion

            #region CharacterDirection
            var characterDirection = bodyPrefab.AddComponent<CharacterDirection>();
            characterDirection.targetTransform = modelBase;
            characterDirection.turnSpeed = 90f;
            #endregion

            #region CharacterMotor
            var characterMotor = bodyPrefab.AddComponent<CharacterMotor>();
            characterMotor.characterDirection = characterDirection;
            characterMotor.muteWalkMotion = false;
            characterMotor.mass = 2000f;
            characterMotor.airControl = 0.25f;
            characterMotor.disableAirControlUntilCollision = false;
            characterMotor.generateParametersOnAwake = true;
            #endregion

            #region InputBankTest
            var inputBank = bodyPrefab.AddComponent<InputBankTest>();
            #endregion

            #region CharacterBody
            CharacterBody characterBody = null;
            if (!bodyPrefab.TryGetComponent(out characterBody))
            {
                characterBody = bodyPrefab.AddComponent<CharacterBody>();
            }
            characterBody.baseNameToken = "ENEMIES_RETURNS_COLOSSUS_BODY_NAME";
            characterBody.subtitleNameToken = "ENEMIES_RETURNS_COLOSSUS_BODY_SUBTITLE";
            characterBody.bodyFlags = CharacterBody.BodyFlags.IgnoreFallDamage;
            characterBody.rootMotionInMainState = false;
            characterBody.mainRootSpeed = 7.5f;

            characterBody.baseMaxHealth = 2100f;
            characterBody.baseRegen = 0f;
            characterBody.baseMaxShield = 0f;
            characterBody.baseMoveSpeed = 4f;
            characterBody.baseAcceleration = 20f;
            characterBody.baseJumpPower = 5f;
            characterBody.baseDamage = 40f;
            characterBody.baseAttackSpeed = 1f;
            characterBody.baseCrit = 0f;
            characterBody.baseArmor = 20f;
            characterBody.baseVisionDistance = float.PositiveInfinity;
            characterBody.baseJumpCount = 1;
            characterBody.sprintingSpeedMultiplier = 1.45f;

            characterBody.autoCalculateLevelStats = true;
            characterBody.levelMaxHealth = 630f;
            characterBody.levelDamage = 8f;
            characterBody.levelArmor = 0f;

            characterBody.wasLucky = false;
            characterBody.spreadBloomDecayTime = 0.45f;
            characterBody._defaultCrosshairPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/StandardCrosshair.prefab").WaitForCompletion();
            characterBody.aimOriginTransform = aimOrigin;
            characterBody.hullClassification = HullClassification.Golem;
            characterBody.portraitIcon = icon;
            characterBody.bodyColor = new Color(0.36f, 0.36f, 0.44f);
            characterBody.isChampion = true;
            characterBody.preferredInitialStateType = new EntityStates.SerializableEntityStateType(typeof(EntityStates.Uninitialized));
            #endregion

            #region CameraTargetParams
            var cameraTargetParams = bodyPrefab.AddComponent<CameraTargetParams>();
            cameraTargetParams.cameraParams = Addressables.LoadAssetAsync<CharacterCameraParams>("RoR2/Base/Common/ccpStandardRaidboss.asset").WaitForCompletion(); // TODO: maybe huge
            #endregion

            #region ModelLocator
            var modelLocator = bodyPrefab.AddComponent<ModelLocator>();
            modelLocator.modelTransform = modelTransform;
            modelLocator.modelBaseTransform = modelBase;

            modelLocator.autoUpdateModelTransform = true;
            modelLocator.dontDetatchFromParent = false;

            modelLocator.noCorpse = false;
            modelLocator.dontReleaseModelOnDeath = true;
            modelLocator.preserveModel = false;

            modelLocator.normalizeToFloor = false;
            modelLocator.normalSmoothdampTime = 0.1f;
            modelLocator.normalMaxAngleDelta = 90f;
            #endregion

            #region EntityStateMachineBody
            var esmBody = bodyPrefab.AddComponent<EntityStateMachine>();
            esmBody.customName = "Body";
            esmBody.initialStateType = new EntityStates.SerializableEntityStateType(typeof(ModdedEntityStates.Colossus.SpawnState));
            esmBody.mainStateType = new EntityStates.SerializableEntityStateType(typeof(EntityStates.GenericCharacterMain));
            #endregion

            #region EntityStateMachineWeapon
            var esmWeapon = bodyPrefab.AddComponent<EntityStateMachine>();
            esmWeapon.customName = "Weapon";
            esmWeapon.initialStateType = new EntityStates.SerializableEntityStateType(typeof(EntityStates.Idle));
            esmWeapon.mainStateType = new EntityStates.SerializableEntityStateType(typeof(EntityStates.Idle));
            #endregion

            #region GenericSkills

            #region Primary
            var gsPrimary = bodyPrefab.AddComponent<GenericSkill>();
            gsPrimary._skillFamily = SkillFamilies.Primary;
            gsPrimary.skillName = "Stomp";
            gsPrimary.hideInCharacterSelect = false;
            #endregion

            #region Secondary
            var gsSecondary = bodyPrefab.AddComponent<GenericSkill>();
            gsSecondary._skillFamily = SkillFamilies.Secondary;
            gsSecondary.skillName = "StoneClap";
            gsSecondary.hideInCharacterSelect = false;
            #endregion

            #region Utility
            var gsUtility = bodyPrefab.AddComponent<GenericSkill>();
            gsUtility._skillFamily = SkillFamilies.Utility;
            gsUtility.skillName = "LaserClap";
            gsUtility.hideInCharacterSelect = false;
            #endregion

            #region Special
            var gsSpecial = bodyPrefab.AddComponent<GenericSkill>();
            gsSpecial._skillFamily = SkillFamilies.Special;
            gsSpecial.skillName = "LaserSpin";
            gsSpecial.hideInCharacterSelect = false;
            #endregion

            #endregion

            #region SkillLocator
            SkillLocator skillLocator = null;
            if (!bodyPrefab.TryGetComponent(out skillLocator))
            {
                skillLocator = bodyPrefab.AddComponent<SkillLocator>();
            }
            skillLocator.primary = gsPrimary;
            skillLocator.secondary = gsSecondary;
            skillLocator.utility = gsUtility;
            skillLocator.special = gsSpecial;
            #endregion

            #region TeamComponent
            TeamComponent teamComponent = null;
            if (!bodyPrefab.TryGetComponent(out teamComponent))
            {
                teamComponent = bodyPrefab.AddComponent<TeamComponent>();
            }
            teamComponent.teamIndex = TeamIndex.None;
            #endregion

            #region HealthComponent
            var healthComponent = bodyPrefab.AddComponent<HealthComponent>();
            healthComponent.dontShowHealthbar = false;
            healthComponent.globalDeathEventChanceCoefficient = 1f;
            #endregion

            #region Interactor
            bodyPrefab.AddComponent<Interactor>().maxInteractionDistance = 3f;
            #endregion

            #region InteractionDriver
            bodyPrefab.AddComponent<InteractionDriver>();
            #endregion

            #region CharacterDeathBehavior
            var characterDeathBehavior = bodyPrefab.AddComponent<CharacterDeathBehavior>();
            characterDeathBehavior.deathStateMachine = esmBody;
            characterDeathBehavior.deathState = new EntityStates.SerializableEntityStateType(typeof(EntityStates.GenericCharacterDeath));
            characterDeathBehavior.idleStateMachine = new EntityStateMachine[] { esmWeapon };
            #endregion

            #region CharacterNetworkTransform
            var characterNetworkTransform = bodyPrefab.AddComponent<CharacterNetworkTransform>();
            characterNetworkTransform.positionTransmitInterval = 0.1f;
            characterNetworkTransform.interpolationFactor = 2f;
            #endregion

            #region NetworkStateMachine
            var networkStateMachine = bodyPrefab.AddComponent<NetworkStateMachine>();
            networkStateMachine.stateMachines = new EntityStateMachine[] { esmBody, esmWeapon };
            #endregion

            #region DeathRewards
            bodyPrefab.AddComponent<DeathRewards>().logUnlockableDef = log;
            #endregion

            #region EquipmentSlot
            bodyPrefab.AddComponent<EquipmentSlot>();
            #endregion

            #region SfxLocator
            var sfxLocator = bodyPrefab.AddComponent<SfxLocator>();
            sfxLocator.deathSound = ""; // TODO
            sfxLocator.barkSound = ""; // TODO
            #endregion

            #region KinematicCharacterMotor
            var kinematicCharacterMotor = bodyPrefab.AddComponent<KinematicCharacterMotor>();
            kinematicCharacterMotor.CharacterController = characterMotor;
            kinematicCharacterMotor.Capsule = bodyPrefab.GetComponent<CapsuleCollider>();
            kinematicCharacterMotor.Rigidbody = bodyPrefab.GetComponent<Rigidbody>();

            kinematicCharacterMotor.CapsuleRadius = 7.15f;
            kinematicCharacterMotor.CapsuleHeight = 28.15f;
            kinematicCharacterMotor.CapsuleYOffset = 0f;

            kinematicCharacterMotor.DetectDiscreteCollisions = false;
            kinematicCharacterMotor.GroundDetectionExtraDistance = 0f;
            kinematicCharacterMotor.MaxStepHeight = 1f;
            kinematicCharacterMotor.MinRequiredStepDepth = 0.1f;
            kinematicCharacterMotor.MaxStableSlopeAngle = 55f;
            kinematicCharacterMotor.MaxStableDistanceFromLedge = 0.5f;
            kinematicCharacterMotor.PreventSnappingOnLedges = false;
            kinematicCharacterMotor.MaxStableDenivelationAngle = 55f;

            kinematicCharacterMotor.RigidbodyInteractionType = RigidbodyInteractionType.None;
            kinematicCharacterMotor.PreserveAttachedRigidbodyMomentum = true;

            kinematicCharacterMotor.HasPlanarConstraint = false;
            kinematicCharacterMotor.PlanarConstraintAxis = new Vector3(0f, 0f, 1f);

            kinematicCharacterMotor.StepHandling = StepHandlingMethod.Standard;
            kinematicCharacterMotor.LedgeHandling = true;
            kinematicCharacterMotor.InteractiveRigidbodyHandling = true;
            kinematicCharacterMotor.SafeMovement = false;
            #endregion

            //#region SpitterDeathDanceController // TODO: maybe use GPReference for this
            //var spitterDeathDance = colossusPrefab.AddComponent<SpitterDeathDanceController>();
            //spitterDeathDance.body = characterBody;
            //spitterDeathDance.modelLocator = modelLocator;
            //#endregion

            #endregion

            #region SetupBoxes

            var golemSurfaceDef = Addressables.LoadAssetAsync<SurfaceDef>("RoR2/Base/Golem/sdGolem.asset").WaitForCompletion();

            var hurtBoxesTransform = bodyPrefab.GetComponentsInChildren<Transform>().Where(t => t.name == "Hurtbox").ToArray();
            List<HurtBox> hurtBoxes = new List<HurtBox>();
            foreach (Transform t in hurtBoxesTransform)
            {
                var hurtBox = t.gameObject.AddComponent<HurtBox>();
                hurtBox.healthComponent = healthComponent;
                hurtBox.damageModifier = HurtBox.DamageModifier.Normal;
                hurtBoxes.Add(hurtBox);

                t.gameObject.AddComponent<SurfaceDefProvider>().surfaceDef = golemSurfaceDef;
            }

            var sniperHurtBoxes = bodyPrefab.GetComponentsInChildren<Transform>().Where(t => t.name == "SniperHurtbox").ToArray();
            foreach (Transform t in sniperHurtBoxes)
            {
                var hurtBox = t.gameObject.AddComponent<HurtBox>();
                hurtBox.healthComponent = healthComponent;
                hurtBox.damageModifier = HurtBox.DamageModifier.Normal;
                hurtBox.isSniperTarget = true;
                hurtBoxes.Add(hurtBox);

                t.gameObject.AddComponent<SurfaceDefProvider>().surfaceDef = golemSurfaceDef;
            }

            // TODO
            var mainHurtboxTransform = bodyPrefab.transform.Find("ModelBase/mdlColossus/Armature/root/root_pelvis_control/MainHurtBox");
            var mainHurtBox = mainHurtboxTransform.gameObject.AddComponent<HurtBox>();
            mainHurtBox.healthComponent = healthComponent;
            mainHurtBox.damageModifier = HurtBox.DamageModifier.Normal;
            mainHurtBox.isBullseye = true;
            hurtBoxes.Add(mainHurtBox);

            mainHurtboxTransform.gameObject.AddComponent<SurfaceDefProvider>().surfaceDef = golemSurfaceDef;

            //var hitBox = bodyPrefab.transform.Find("ModelBase/mdlSpitter/Armature/Root/Root_Pelvis_Control/Bone.001/Bone.002/Bone.003/Head/Hitbox").gameObject.AddComponent<HitBox>();
            #endregion

            #region mdlColossus
            var mdlColossus = modelTransform.gameObject;
            var animator = modelTransform.gameObject.GetComponent<Animator>();

            #region AimAnimator
            // if you are having issues with AimAnimator,
            // * just add Additive Reference Pose for your pitch and yaw animations in the middle of the animation
            // * make both animations loop
            // * set them both to zero speed in your animation controller
            // * I haven't found how to add poses to "separate" animation files, so those have to be in fbx
            var aimAnimator = mdlColossus.AddComponent<AimAnimator>();
            aimAnimator.inputBank = inputBank;
            aimAnimator.directionComponent = characterDirection;

            aimAnimator.pitchRangeMin = -70f; // its looking up, not down, for fuck sake
            aimAnimator.pitchRangeMax = 70f;

            aimAnimator.yawRangeMin = -135f;
            aimAnimator.yawRangeMax = 135f;

            aimAnimator.pitchGiveupRange = 50f;
            aimAnimator.yawGiveupRange = 50f;

            aimAnimator.giveupDuration = 5f;

            aimAnimator.raisedApproachSpeed = 180f;
            aimAnimator.loweredApproachSpeed = 180f;
            aimAnimator.smoothTime = 0.3f;

            aimAnimator.fullYaw = false;
            aimAnimator.aimType = AimAnimator.AimType.Direct;

            aimAnimator.enableAimWeight = false;
            aimAnimator.UseTransformedAimVector = false;
            #endregion

            #region ChildLocator
            var childLocator = mdlColossus.AddComponent<ChildLocator>();
            var ourChildLocator = mdlColossus.GetComponent<OurChildLocator>();
            childLocator.transformPairs = Array.ConvertAll(ourChildLocator.transformPairs, item =>
            {
                return new ChildLocator.NameTransformPair
                {
                    name = item.name,
                    transform = item.transform,
                };
            });
            UnityEngine.Object.Destroy(ourChildLocator);
            #endregion

            #region HurtBoxGroup
            var hurtboxGroup = mdlColossus.AddComponent<HurtBoxGroup>();
            hurtboxGroup.hurtBoxes = hurtBoxes.ToArray();
            for (short i = 0; i < hurtboxGroup.hurtBoxes.Length; i++)
            {
                hurtboxGroup.hurtBoxes[i].hurtBoxGroup = hurtboxGroup;
                hurtboxGroup.hurtBoxes[i].indexInGroup = i;
                if (hurtboxGroup.hurtBoxes[i].isBullseye)
                {
                    hurtboxGroup.bullseyeCount++;
                }
            }
            hurtboxGroup.mainHurtBox = mainHurtBox;
            #endregion

            #region AnimationEvents
            if (!mdlColossus.TryGetComponent<AnimationEvents>(out _))
            {
                mdlColossus.AddComponent<AnimationEvents>();
            }
            #endregion

            #region DestroyOnUnseen
            mdlColossus.AddComponent<DestroyOnUnseen>().cull = false;
            #endregion

            #region CharacterModel
            var characterModel = mdlColossus.AddComponent<CharacterModel>();
            characterModel.body = characterBody;
            characterModel.itemDisplayRuleSet = CreateItemDisplayRuleSet();
            characterModel.autoPopulateLightInfos = true;
            characterModel.baseRendererInfos = new CharacterModel.RendererInfo[]
            {
                new CharacterModel.RendererInfo
                {
                    renderer = modelRenderer,
                    defaultMaterial = modelRenderer.material,
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false,
                    hideOnDeath = false
                },
                new CharacterModel.RendererInfo
                {
                    renderer = headRenderer,
                    defaultMaterial = headRenderer.material,
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false,
                    hideOnDeath = false
                }
            };
            #endregion

            #region HitBoxGroupLeftStomp
            var leftstompHitbox = mdlColossus.transform.Find("Armature/foot.l/LeftStompHitbox").gameObject.AddComponent<HitBox>();

            var hbgLeftStomp = mdlColossus.AddComponent<HitBoxGroup>();
            hbgLeftStomp.groupName = "LeftStomp";
            hbgLeftStomp.hitBoxes = new HitBox[] { leftstompHitbox };
            #endregion

            #region HitBoxGroupLeftStomp
            var rightStompHitbox = mdlColossus.transform.Find("Armature/foot.r/RightStompHitbox").gameObject.AddComponent<HitBox>();

            var hbgRightStomp = mdlColossus.AddComponent<HitBoxGroup>();
            hbgRightStomp.groupName = "RightStomp";
            hbgRightStomp.hitBoxes = new HitBox[] { rightStompHitbox };
            #endregion

            #region FootstepHandler
            FootstepHandler footstepHandler = null;
            if (!mdlColossus.TryGetComponent(out footstepHandler))
            {
                footstepHandler = mdlColossus.AddComponent<FootstepHandler>();
            }
            footstepHandler.enableFootstepDust = true;
            footstepHandler.baseFootstepString = "Play_titanboss_step";
            footstepHandler.footstepDustPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/GenericHugeFootstepDust.prefab").WaitForCompletion();
            #endregion

            #region ModelPanelParameters
            var modelPanelParameters = mdlColossus.AddComponent<ModelPanelParameters>();
            modelPanelParameters.focusPointTransform = focusPoint;
            modelPanelParameters.cameraPositionTransform = cameraPosition;
            modelPanelParameters.modelRotation = new Quaternion(0, 0, 0, 1);
            modelPanelParameters.minDistance = 7.5f;
            modelPanelParameters.maxDistance = 22f;
            #endregion

            #region SkinDefs

            //            RenderInfo[] defaultRender = Array.ConvertAll(characterModel.baseRendererInfos, item => new RenderInfo
            //            {
            //                renderer = (SkinnedMeshRenderer)item.renderer,
            //                material = item.defaultMaterial,
            //                ignoreOverlays = item.ignoreOverlays

            //            });
            //            SkinDefs.Default = CreateSkinDef("skinSpitterDefault", mdlColossus, defaultRender);

            //            RenderInfo[] lakesRender = new RenderInfo[]
            //            {
            //                new RenderInfo
            //                {
            //                    renderer = modelRenderer,
            //                    material = skinsLookup["matSpitterLakes"],
            //                    ignoreOverlays = false
            //                },
            //                new RenderInfo
            //                {
            //                    renderer = gumsRenderer,
            //                    material = skinsLookup["matSpitterGutsLakes"],
            //                    ignoreOverlays = true
            //                },
            //                new RenderInfo
            //                {
            //                    renderer = teethenderer,
            //                    material = skinsLookup["matSpitterLakes"],
            //                    ignoreOverlays = true
            //                }
            //            };
            //            SkinDefs.Lakes = CreateSkinDef("skinSpitterLakes", mdlColossus, lakesRender, SkinDefs.Default);

            //            RenderInfo[] sulfurRender = new RenderInfo[]
            //            {
            //                new RenderInfo
            //                {
            //                    renderer = modelRenderer,
            //                    material = skinsLookup["matSpitterSulfur"],
            //                    ignoreOverlays = false
            //                },
            //                new RenderInfo
            //                {
            //                    renderer = gumsRenderer,
            //                    material = skinsLookup["matSpitterGutsSulfur"],
            //                    ignoreOverlays = true
            //                },
            //                new RenderInfo
            //                {
            //                    renderer = teethenderer,
            //                    material = skinsLookup["matSpitterSulfur"],
            //                    ignoreOverlays = true
            //                }
            //            };
            //            SkinDefs.Sulfur = CreateSkinDef("skinSpitterSulfur", mdlColossus, sulfurRender, SkinDefs.Default);

            //            RenderInfo[] depthsRender = new RenderInfo[]
            //{
            //                new RenderInfo
            //                {
            //                    renderer = modelRenderer,
            //                    material = skinsLookup["matSpitterDepths"],
            //                    ignoreOverlays = false
            //                },
            //                new RenderInfo
            //                {
            //                    renderer = gumsRenderer,
            //                    material = skinsLookup["matSpitterGutsDepths"],
            //                    ignoreOverlays = true
            //                },
            //                new RenderInfo
            //                {
            //                    renderer = teethenderer,
            //                    material = skinsLookup["matSpitterDepths"],
            //                    ignoreOverlays = true
            //                }
            //};
            //            SkinDefs.Depths = CreateSkinDef("skinSpitterDepths", mdlColossus, depthsRender, SkinDefs.Default);

            //            var modelSkinController = mdlColossus.AddComponent<ModelSkinController>();
            //            modelSkinController.skins = new SkinDef[]
            //            {
            //                SkinDefs.Default,
            //                SkinDefs.Lakes,
            //                SkinDefs.Sulfur,
            //                SkinDefs.Depths
            //            };
            #endregion

            //var helper = mdlColossus.AddComponent<AnimationParameterHelper>();
            //helper.animator = animator;
            //helper.animationParameters = new string[] { "walkSpeedDebug" };

            mdlColossus.AddComponent<FloatingRocksController>().initialPosition = rocksInitialTransform;
            #endregion

            #region AimAssist
            var aimAssistTarget = bodyPrefab.transform.Find("ModelBase/mdlColossus/AimAssist").gameObject.AddComponent<AimAssistTarget>();
            aimAssistTarget.point0 = headTransform;
            aimAssistTarget.point1 = rootTransform;
            aimAssistTarget.assistScale = 4f;
            aimAssistTarget.healthComponent = healthComponent;
            aimAssistTarget.teamComponent = teamComponent;
            #endregion

            #region CrouchController
            // all numbers are taken from titan
            var crouchMecanim = crouchController.gameObject.AddComponent<CrouchMecanim>();
            crouchMecanim.duckHeight = 7.74f;
            crouchMecanim.animator = animator;
            crouchMecanim.smoothdamp = 0.3f;
            crouchMecanim.initialVerticalOffset = 4.91f;
            #endregion

            bodyPrefab.RegisterNetworkPrefab();

            return bodyPrefab;
        }

        public GameObject CreateColossusMaster(GameObject masterPrefab, GameObject bodyPrefab)
        {
            #region NetworkIdentity
            masterPrefab.AddComponent<NetworkIdentity>().localPlayerAuthority = true;
            #endregion

            #region CharacterMaster
            var characterMaster = masterPrefab.AddComponent<CharacterMaster>();
            characterMaster.bodyPrefab = bodyPrefab;
            characterMaster.spawnOnStart = false;
            characterMaster.teamIndex = TeamIndex.Monster;
            characterMaster.destroyOnBodyDeath = true;
            characterMaster.isBoss = false;
            characterMaster.preventGameOver = true;
            #endregion

            #region Inventory
            masterPrefab.AddComponent<Inventory>();
            #endregion

            #region EntityStateMachineAI
            var esmAI = masterPrefab.AddComponent<EntityStateMachine>();
            esmAI.customName = "AI";
            esmAI.initialStateType = new EntityStates.SerializableEntityStateType(typeof(EntityStates.AI.Walker.Wander));
            esmAI.mainStateType = new EntityStates.SerializableEntityStateType(typeof(EntityStates.AI.Walker.Wander));
            #endregion

            #region BaseAI
            var baseAI = masterPrefab.AddComponent<BaseAI>();
            baseAI.fullVision = false;
            baseAI.neverRetaliateFriendlies = true;
            baseAI.enemyAttentionDuration = 5f;
            baseAI.desiredSpawnNodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;
            baseAI.stateMachine = esmAI;
            baseAI.scanState = new EntityStates.SerializableEntityStateType(typeof(EntityStates.AI.Walker.Wander));
            baseAI.isHealer = false;
            baseAI.enemyAttention = 0f;
            baseAI.aimVectorDampTime = 0.05f;
            baseAI.aimVectorMaxSpeed = 180f;
            #endregion

            #region MinionOwnership
            if (!masterPrefab.TryGetComponent<MinionOwnership>(out _))
            {
                masterPrefab.AddComponent<MinionOwnership>();
            }
            #endregion

            #region AISkillDriver_FireStomp
            var asdFireStomp = masterPrefab.AddComponent<AISkillDriver>();
            asdFireStomp.customName = "FireStomp";
            asdFireStomp.skillSlot = SkillSlot.Primary;

            asdFireStomp.requiredSkill = null;
            asdFireStomp.requireSkillReady = true;
            asdFireStomp.requireEquipmentReady = false;
            asdFireStomp.minUserHealthFraction = float.NegativeInfinity;
            asdFireStomp.maxUserHealthFraction = float.PositiveInfinity;
            asdFireStomp.minTargetHealthFraction = float.NegativeInfinity;
            asdFireStomp.maxTargetHealthFraction = float.PositiveInfinity;
            asdFireStomp.minDistance = 0f;
            asdFireStomp.maxDistance = 60f;
            asdFireStomp.selectionRequiresTargetLoS = true;
            asdFireStomp.selectionRequiresOnGround = false;
            asdFireStomp.selectionRequiresAimTarget = false;
            asdFireStomp.maxTimesSelected = -1;

            asdFireStomp.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            asdFireStomp.activationRequiresTargetLoS = true;
            asdFireStomp.activationRequiresAimTargetLoS = false;
            asdFireStomp.activationRequiresAimConfirmation = false;
            asdFireStomp.movementType = AISkillDriver.MovementType.Stop;
            asdFireStomp.moveInputScale = 1;
            asdFireStomp.aimType = AISkillDriver.AimType.AtMoveTarget;
            asdFireStomp.ignoreNodeGraph = false;
            asdFireStomp.shouldSprint = false;
            asdFireStomp.shouldFireEquipment = false;
            asdFireStomp.buttonPressType = AISkillDriver.ButtonPressType.Hold;

            asdFireStomp.driverUpdateTimerOverride = 0.5f;
            asdFireStomp.resetCurrentEnemyOnNextDriverSelection = false;
            asdFireStomp.noRepeat = false;
            asdFireStomp.nextHighPriorityOverride = null;
            #endregion

            #region AISkillDriver_ChaseOffNodeGraph
            var asdChaseOffNodeGraph = masterPrefab.AddComponent<AISkillDriver>();
            asdChaseOffNodeGraph.customName = "ChaseOffNodegraph";
            asdChaseOffNodeGraph.skillSlot = SkillSlot.None;

            asdChaseOffNodeGraph.requiredSkill = null;
            asdChaseOffNodeGraph.requireSkillReady = false;
            asdChaseOffNodeGraph.requireEquipmentReady = false;
            asdChaseOffNodeGraph.minUserHealthFraction = float.NegativeInfinity;
            asdChaseOffNodeGraph.maxUserHealthFraction = float.PositiveInfinity;
            asdChaseOffNodeGraph.minTargetHealthFraction = float.NegativeInfinity;
            asdChaseOffNodeGraph.maxTargetHealthFraction = float.PositiveInfinity;
            asdChaseOffNodeGraph.minDistance = 0f;
            asdChaseOffNodeGraph.maxDistance = 7f;
            asdChaseOffNodeGraph.selectionRequiresTargetLoS = true;
            asdChaseOffNodeGraph.selectionRequiresOnGround = false;
            asdChaseOffNodeGraph.selectionRequiresAimTarget = false;
            asdChaseOffNodeGraph.maxTimesSelected = -1;

            asdChaseOffNodeGraph.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            asdChaseOffNodeGraph.activationRequiresTargetLoS = false;
            asdChaseOffNodeGraph.activationRequiresAimTargetLoS = false;
            asdChaseOffNodeGraph.activationRequiresAimConfirmation = false;
            asdChaseOffNodeGraph.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            asdChaseOffNodeGraph.moveInputScale = 1;
            asdChaseOffNodeGraph.aimType = AISkillDriver.AimType.AtMoveTarget;
            asdChaseOffNodeGraph.ignoreNodeGraph = true;
            asdChaseOffNodeGraph.shouldSprint = false;
            asdChaseOffNodeGraph.shouldFireEquipment = false;
            asdChaseOffNodeGraph.buttonPressType = AISkillDriver.ButtonPressType.Hold;

            asdChaseOffNodeGraph.driverUpdateTimerOverride = -1;
            asdChaseOffNodeGraph.resetCurrentEnemyOnNextDriverSelection = false;
            asdChaseOffNodeGraph.noRepeat = false;
            asdChaseOffNodeGraph.nextHighPriorityOverride = null;
            #endregion

            #region AISkillDriver_PathFromAfar
            var asdPathFromAfar = masterPrefab.AddComponent<AISkillDriver>();
            asdPathFromAfar.customName = "PathFromAfar";
            asdPathFromAfar.skillSlot = SkillSlot.None;

            asdPathFromAfar.requiredSkill = null;
            asdPathFromAfar.requireSkillReady = false;
            asdPathFromAfar.requireEquipmentReady = false;
            asdPathFromAfar.minUserHealthFraction = float.NegativeInfinity;
            asdPathFromAfar.maxUserHealthFraction = float.PositiveInfinity;
            asdPathFromAfar.minTargetHealthFraction = float.NegativeInfinity;
            asdPathFromAfar.maxTargetHealthFraction = float.PositiveInfinity;
            asdPathFromAfar.minDistance = 0f;
            asdPathFromAfar.maxDistance = float.PositiveInfinity;
            asdPathFromAfar.selectionRequiresTargetLoS = false;
            asdPathFromAfar.selectionRequiresOnGround = false;
            asdPathFromAfar.selectionRequiresAimTarget = false;
            asdPathFromAfar.maxTimesSelected = -1;

            asdPathFromAfar.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            asdPathFromAfar.activationRequiresTargetLoS = false;
            asdPathFromAfar.activationRequiresAimTargetLoS = false;
            asdPathFromAfar.activationRequiresAimConfirmation = false;
            asdPathFromAfar.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            asdPathFromAfar.moveInputScale = 1;
            asdPathFromAfar.aimType = AISkillDriver.AimType.AtMoveTarget;
            asdPathFromAfar.ignoreNodeGraph = false;
            asdPathFromAfar.shouldSprint = false;
            asdPathFromAfar.shouldFireEquipment = false;
            asdPathFromAfar.buttonPressType = AISkillDriver.ButtonPressType.Hold;

            asdPathFromAfar.driverUpdateTimerOverride = -1;
            asdPathFromAfar.resetCurrentEnemyOnNextDriverSelection = false;
            asdPathFromAfar.noRepeat = false;
            asdPathFromAfar.nextHighPriorityOverride = null;
            #endregion

            masterPrefab.RegisterNetworkPrefab();

            return masterPrefab;
        }

        #region GameObjects

        public GameObject CreateStompEffect()
        {
            var clonedEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleGuardGroundSlam.prefab").WaitForCompletion().InstantiateClone("ColossusStompEffect", false);

            var components = clonedEffect.GetComponentsInChildren<ParticleSystem>();
            foreach (var component in components)
            {
                var main = component.main;
                main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            }

            clonedEffect.transform.localScale = new Vector3(2f, 2f, 2f);

            return clonedEffect;
        }

        public GameObject CreateStompProjectile()
        {
            var clonedEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/Sunder.prefab").WaitForCompletion().InstantiateClone("ColossusStompProjectile", true);
            var clonedEffectGhost = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/SunderGhost.prefab").WaitForCompletion().InstantiateClone("ColossusStompProjectileGhost", false);

            var components = clonedEffectGhost.GetComponentsInChildren<ParticleSystem>();
            foreach(var component in components)
            {
                var main = component.main;
                main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            }

            var ghostAnchor = new GameObject();
            ghostAnchor.name = "Anchor";
            ghostAnchor.transform.parent = clonedEffect.transform;
            ghostAnchor.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            ghostAnchor.transform.localScale = new Vector3(1f, 1f, 1f);

            var projectileController = clonedEffect.GetComponent<ProjectileController>();
            projectileController.ghostPrefab = clonedEffectGhost;
            projectileController.ghostTransformAnchor = ghostAnchor.transform;

            var hitbox = clonedEffect.transform.Find("Hitbox");
            hitbox.transform.localScale = new Vector3(hitbox.transform.localScale.x, 1.7f, hitbox.transform.localScale.z);

            clonedEffect.transform.localScale = new Vector3(2f, 2f, 2f);
            clonedEffectGhost.transform.localScale = new Vector3(2f, 2f, 2f);

            return clonedEffect;
        }

        #endregion

        #region SkillDefs

        internal SkillDef CreateStompSkill()
        {
            var skillDef = ScriptableObject.CreateInstance<SkillDef>();

            (skillDef as ScriptableObject).name = "ColossusBodyStomp";
            skillDef.skillName = "Stomp";

            skillDef.skillNameToken = "ENEMIES_RETURNS_COLOSSUS_STOMP_NAME";
            skillDef.skillDescriptionToken = "ENEMIES_RETURNS_COLOSSUS_STOMP_DESCRIPTION";
            //bite.icon = ; yeah, right

            skillDef.activationStateMachineName = "Body";
            skillDef.activationState = new EntityStates.SerializableEntityStateType(typeof(StompEnter));
            skillDef.interruptPriority = EntityStates.InterruptPriority.Skill;

            skillDef.baseRechargeInterval = 6f;
            skillDef.baseMaxStock = 1;
            skillDef.rechargeStock = 1;
            skillDef.requiredStock = 1;
            skillDef.stockToConsume = 1;

            skillDef.resetCooldownTimerOnUse = false;
            skillDef.fullRestockOnAssign = true;
            skillDef.dontAllowPastMaxStocks = false;
            skillDef.beginSkillCooldownOnSkillEnd = false;

            skillDef.cancelSprintingOnActivation = true;
            skillDef.forceSprintDuringState = false;
            skillDef.canceledFromSprinting = false;

            skillDef.isCombatSkill = true;
            skillDef.mustKeyPress = false;

            return skillDef;
        }

        internal SkillDef CreateStoneClapSkill()
        {
            var skillDef = ScriptableObject.CreateInstance<SkillDef>();

            (skillDef as ScriptableObject).name = "ColossusBodyStoneClap";
            skillDef.skillName = "StoneClap";

            skillDef.skillNameToken = "ENEMIES_RETURNS_COLOSSUS_STONE_CLAP_NAME";
            skillDef.skillDescriptionToken = "ENEMIES_RETURNS_COLOSSUS_STONE_CLAP_DESCRIPTION";
            //bite.icon = ; yeah, right

            skillDef.activationStateMachineName = "Body";
            skillDef.activationState = new EntityStates.SerializableEntityStateType(typeof(RockClapStart));
            skillDef.interruptPriority = EntityStates.InterruptPriority.Skill;

            skillDef.baseRechargeInterval = 6f;
            skillDef.baseMaxStock = 1;
            skillDef.rechargeStock = 1;
            skillDef.requiredStock = 1;
            skillDef.stockToConsume = 1;

            skillDef.resetCooldownTimerOnUse = false;
            skillDef.fullRestockOnAssign = true;
            skillDef.dontAllowPastMaxStocks = false;
            skillDef.beginSkillCooldownOnSkillEnd = false;

            skillDef.cancelSprintingOnActivation = true;
            skillDef.forceSprintDuringState = false;
            skillDef.canceledFromSprinting = false;

            skillDef.isCombatSkill = true;
            skillDef.mustKeyPress = false;

            return skillDef;
        }

        internal SkillDef CreateLaserClapSkill()
        {
            var skillDef = ScriptableObject.CreateInstance<SkillDef>();

            (skillDef as ScriptableObject).name = "ColossusBodyLaserClap";
            skillDef.skillName = "LaserClap";

            skillDef.skillNameToken = "ENEMIES_RETURNS_COLOSSUS_LASER_CLAP_NAME";
            skillDef.skillDescriptionToken = "ENEMIES_RETURNS_COLOSSUS_LASER_CLAP_DESCRIPTION";
            //bite.icon = ; yeah, right

            skillDef.activationStateMachineName = "Body";
            skillDef.activationState = new EntityStates.SerializableEntityStateType(typeof(ModdedEntityStates.Colossus.LaserClapStart));
            skillDef.interruptPriority = EntityStates.InterruptPriority.Skill;

            skillDef.baseRechargeInterval = 6f;
            skillDef.baseMaxStock = 1;
            skillDef.rechargeStock = 1;
            skillDef.requiredStock = 1;
            skillDef.stockToConsume = 1;

            skillDef.resetCooldownTimerOnUse = false;
            skillDef.fullRestockOnAssign = true;
            skillDef.dontAllowPastMaxStocks = false;
            skillDef.beginSkillCooldownOnSkillEnd = false;

            skillDef.cancelSprintingOnActivation = true;
            skillDef.forceSprintDuringState = false;
            skillDef.canceledFromSprinting = false;

            skillDef.isCombatSkill = true;
            skillDef.mustKeyPress = false;

            return skillDef;
        }

        internal SkillDef CreateHeadLaserSkill()
        {
            var skillDef = ScriptableObject.CreateInstance<SkillDef>();

            (skillDef as ScriptableObject).name = "ColossusBodyHeadLaser";
            skillDef.skillName = "HeadLaser";

            skillDef.skillNameToken = "ENEMIES_RETURNS_COLOSSUS_HEAD_LASER_NAME";
            skillDef.skillDescriptionToken = "ENEMIES_RETURNS_COLOSSUS_HEAD_LASER_DESCRIPTION";
            //bite.icon = ; yeah, right

            skillDef.activationStateMachineName = "Body";
            skillDef.activationState = new EntityStates.SerializableEntityStateType(typeof(ModdedEntityStates.Colossus.HeadLaserStart));
            skillDef.interruptPriority = EntityStates.InterruptPriority.Skill;

            skillDef.baseRechargeInterval = 6f;
            skillDef.baseMaxStock = 1;
            skillDef.rechargeStock = 1;
            skillDef.requiredStock = 1;
            skillDef.stockToConsume = 1;

            skillDef.resetCooldownTimerOnUse = false;
            skillDef.fullRestockOnAssign = true;
            skillDef.dontAllowPastMaxStocks = false;
            skillDef.beginSkillCooldownOnSkillEnd = false;

            skillDef.cancelSprintingOnActivation = true;
            skillDef.forceSprintDuringState = false;
            skillDef.canceledFromSprinting = false;

            skillDef.isCombatSkill = true;
            skillDef.mustKeyPress = false;

            return skillDef;
        }

        #endregion

        public CharacterSpawnCard CreateCard(string name, GameObject master, SkinDef skin = null, GameObject bodyGameObject = null)
        {
            var card = ScriptableObject.CreateInstance<CharacterSpawnCard>();
            (card as ScriptableObject).name = name;
            card.prefab = master;
            card.sendOverNetwork = true;
            card.hullSize = HullClassification.BeetleQueen;
            card.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;
            card.requiredFlags = RoR2.Navigation.NodeFlags.None;
            card.forbiddenFlags = RoR2.Navigation.NodeFlags.NoCharacterSpawn;
            card.directorCreditCost = 600;
            card.occupyPosition = false;
            card.eliteRules = SpawnCard.EliteRules.Default;
            card.noElites = false;
            card.forbiddenAsBoss = false;
            if (skin && bodyGameObject && bodyGameObject.TryGetComponent<CharacterBody>(out var body))
            {
                card.loadout = new SerializableLoadout
                {
                    bodyLoadouts = new SerializableLoadout.BodyLoadout[]
                    {
                        new SerializableLoadout.BodyLoadout()
                        {
                            body = body,
                            skinChoice = skin,
                            skillChoices = Array.Empty<SerializableLoadout.BodyLoadout.SkillChoice>() // yes, we need it
                        }
                    }
                };
            };

            return card;
        }

        private ItemDisplayRuleSet CreateItemDisplayRuleSet()
        {
            var idrs = ScriptableObject.CreateInstance<ItemDisplayRuleSet>();

            #region FireElite
            var fireEquipDisplay = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteFire/DisplayEliteHorn.prefab").WaitForCompletion();

            var displayRuleGroupFire = new DisplayRuleGroup();
            displayRuleGroupFire.AddDisplayRule(new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = fireEquipDisplay,
                childName = "JawR",
                localPos = new Vector3(-0.32686F, 2.51006F, -0.21041F),
                localAngles = new Vector3(354.7525F, 340F, 7.12234F),
                localScale = new Vector3(0.6F, 0.6F, 0.6F),
                limbMask = LimbFlags.None
            });

            displayRuleGroupFire.AddDisplayRule(new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = fireEquipDisplay,
                childName = "JawL",
                localPos = new Vector3(-0.12522F, 2.55699F, -0.28728F),
                localAngles = new Vector3(354.7525F, 20.00001F, 351.6044F),
                localScale = new Vector3(-0.6F, 0.6F, 0.6F),
                limbMask = LimbFlags.None
            });

            ArrayUtils.ArrayAppend(ref idrs.keyAssetRuleGroups, new KeyAssetRuleGroup
            {
                keyAsset = Addressables.LoadAssetAsync<EquipmentDef>("RoR2/Base/EliteFire/EliteFireEquipment.asset").WaitForCompletion(),
                displayRuleGroup = displayRuleGroupFire,
            });
            #endregion

            #region HauntedElite
            var displayRuleGroupHaunted = new DisplayRuleGroup();
            displayRuleGroupHaunted.AddDisplayRule(new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteHaunted/DisplayEliteStealthCrown.prefab").WaitForCompletion(),
                childName = "Head",
                localPos = new Vector3(0F, -0.29125F, -0.36034F),
                localAngles = new Vector3(270F, 0F, 0F),
                localScale = new Vector3(0.43975F, 0.43975F, 0.43975F),
                limbMask = LimbFlags.None
            });

            ArrayUtils.ArrayAppend(ref idrs.keyAssetRuleGroups, new KeyAssetRuleGroup
            {
                displayRuleGroup = displayRuleGroupHaunted,
                keyAsset = Addressables.LoadAssetAsync<EquipmentDef>("RoR2/Base/EliteHaunted/EliteHauntedEquipment.asset").WaitForCompletion()
            });
            #endregion

            #region IceElite
            var displayRuleGroupIce = new DisplayRuleGroup();
            displayRuleGroupIce.AddDisplayRule(new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteIce/DisplayEliteIceCrown.prefab").WaitForCompletion(),
                childName = "Head",
                localPos = new Vector3(-0.36417F, 4.08597F, -0.81975F),
                localAngles = new Vector3(88.15041F, 342.9204F, 152.0255F),
                localScale = new Vector3(0.28734F, 0.28734F, 0.28734F),
                limbMask = LimbFlags.None
            });

            ArrayUtils.ArrayAppend(ref idrs.keyAssetRuleGroups, new KeyAssetRuleGroup
            {
                displayRuleGroup = displayRuleGroupIce,
                keyAsset = Addressables.LoadAssetAsync<EquipmentDef>("RoR2/Base/EliteIce/EliteIceEquipment.asset").WaitForCompletion()
            });
            #endregion

            #region LightningElite
            var displayRuleGroupLightning = new DisplayRuleGroup();
            displayRuleGroupLightning.AddDisplayRule(new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteLightning/DisplayEliteRhinoHorn.prefab").WaitForCompletion(),
                childName = "TailEnd",
                localPos = new Vector3(-0.00302F, 0.77073F, 0.00143F),
                localAngles = new Vector3(284.2227F, 198.9412F, 159.205F),
                localScale = new Vector3(1.15579F, 1.15579F, 1.15579F),
                limbMask = LimbFlags.None
            });
            displayRuleGroupLightning.AddDisplayRule(new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteLightning/DisplayEliteRhinoHorn.prefab").WaitForCompletion(),
                childName = "JawL",
                localPos = new Vector3(-0.25677F, 2.49244F, -0.13195F),
                localAngles = new Vector3(323.8193F, 261.7038F, 7.48606F),
                localScale = new Vector3(1.45f, 1.45f, 1.45f),
                limbMask = LimbFlags.None
            });
            displayRuleGroupLightning.AddDisplayRule(new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteLightning/DisplayEliteRhinoHorn.prefab").WaitForCompletion(),
                childName = "JawR",
                localPos = new Vector3(-0.00804F, 2.49258F, -0.13194F),
                localAngles = new Vector3(322.8305F, 89.99672F, 7F),
                localScale = new Vector3(1.45f, 1.45f, 1.45f),
                limbMask = LimbFlags.None
            });


            ArrayUtils.ArrayAppend(ref idrs.keyAssetRuleGroups, new KeyAssetRuleGroup
            {
                displayRuleGroup = displayRuleGroupLightning,
                keyAsset = Addressables.LoadAssetAsync<EquipmentDef>("RoR2/Base/EliteLightning/EliteLightningEquipment.asset").WaitForCompletion()
            });
            #endregion

            #region LunarElite
            var displayRuleGroupLunar = new DisplayRuleGroup();
            displayRuleGroupLunar.AddDisplayRule(new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteLunar/DisplayEliteLunar, Fire.prefab").WaitForCompletion(),
                childName = "Head",
                localPos = new Vector3(0F, 0F, 0F),
                localAngles = new Vector3(270F, 0F, 0F),
                localScale = new Vector3(0.8F, 0.8F, 0.8F),
                limbMask = LimbFlags.None
            });

            ArrayUtils.ArrayAppend(ref idrs.keyAssetRuleGroups, new KeyAssetRuleGroup
            {
                displayRuleGroup = displayRuleGroupLunar,
                keyAsset = Addressables.LoadAssetAsync<EquipmentDef>("RoR2/Base/EliteLunar/EliteLunarEquipment.asset").WaitForCompletion()
            });
            #endregion

            #region PoisonElite
            var displayRuleGroupPoison = new DisplayRuleGroup();
            displayRuleGroupPoison.AddDisplayRule(new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ElitePoison/DisplayEliteUrchinCrown.prefab").WaitForCompletion(),
                childName = "JawL",
                localPos = new Vector3(0F, 0.38638F, -0.00001F),
                localAngles = new Vector3(0F, 270F, 0F),
                localScale = new Vector3(0.5F, 0.5F, 0.5F),
                limbMask = LimbFlags.None
            });
            displayRuleGroupPoison.AddDisplayRule(new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ElitePoison/DisplayEliteUrchinCrown.prefab").WaitForCompletion(),
                childName = "JawR",
                localPos = new Vector3(0F, 0.38638F, -0.00001F),
                localAngles = new Vector3(0F, 90F, 0F),
                localScale = new Vector3(0.5F, 0.5F, 0.5F),
                limbMask = LimbFlags.None
            });

            ArrayUtils.ArrayAppend(ref idrs.keyAssetRuleGroups, new KeyAssetRuleGroup
            {
                displayRuleGroup = displayRuleGroupPoison,
                keyAsset = Addressables.LoadAssetAsync<EquipmentDef>("RoR2/Base/ElitePoison/ElitePoisonEquipment.asset").WaitForCompletion()
            });
            #endregion

            #region EliteEarth
            var displayRuleGroupEarth = new DisplayRuleGroup();
            displayRuleGroupEarth.AddDisplayRule(new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/EliteEarth/DisplayEliteMendingAntlers.prefab").WaitForCompletion(),
                childName = "Head",
                localPos = new Vector3(-0.12323F, 2.48183F, -0.47279F),
                localAngles = new Vector3(7.00848F, 2.28661F, 1.77239F),
                localScale = new Vector3(4.42437F, 4.42437F, 4.42437F),
                limbMask = LimbFlags.None
            });

            ArrayUtils.ArrayAppend(ref idrs.keyAssetRuleGroups, new KeyAssetRuleGroup
            {
                displayRuleGroup = displayRuleGroupEarth,
                keyAsset = Addressables.LoadAssetAsync<EquipmentDef>("RoR2/DLC1/EliteEarth/EliteEarthEquipment.asset").WaitForCompletion()
            });
            #endregion

            #region VoidElite
            var displayRuleGroupVoid = new DisplayRuleGroup();
            displayRuleGroupVoid.AddDisplayRule(new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                followerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/EliteVoid/DisplayAffixVoid.prefab").WaitForCompletion(),
                childName = "Head",
                localPos = new Vector3(0F, 1.1304F, 0.00001F),
                localAngles = new Vector3(0F, 0F, 0F),
                localScale = new Vector3(0.84412F, 0.84412F, 0.84412F),
                limbMask = LimbFlags.None
            });

            ArrayUtils.ArrayAppend(ref idrs.keyAssetRuleGroups, new KeyAssetRuleGroup
            {
                displayRuleGroup = displayRuleGroupVoid,
                keyAsset = Addressables.LoadAssetAsync<EquipmentDef>("RoR2/DLC1/EliteVoid/EliteVoidEquipment.asset").WaitForCompletion()
            });
            #endregion

            return idrs;
        }

        private void Renamer(GameObject object1)
        {

        }
    }
}

