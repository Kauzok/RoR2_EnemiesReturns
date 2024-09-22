﻿using RoR2.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static RoR2.Console;
using UnityEngine.AddressableAssets;
using RoR2.Skills;
using RoR2;
using EnemiesReturns.Enemies.Spitter;
//
using EnemiesReturns.Enemies.Colossus;
using Rewired.Utils.Classes.Utility;
using EnemiesReturns.EditorHelpers;
using EnemiesReturns.Items.ColossalKnurl;
using EnemiesReturns.Enemies.Ifrit;

namespace EnemiesReturns
{
    public class ContentProvider : IContentPackProvider
    {
        public string identifier => EnemiesReturnsPlugin.GUID + "." + nameof(ContentProvider);

        private readonly ContentPack _contentPack = new ContentPack();

        public const string AssetBundleName = "EnemiesReturns";
        public const string AssetBundleFolder = "AssetBundles";

        public const string SoundbankFolder = "Soundbanks";
        public const string SoundsSoundBankFileName = "EnemiesReturnsSounds";

        public static Dictionary<string, string> ShaderLookup = new Dictionary<string, string>()
        {
            {"stubbedror2/base/shaders/hgstandard", "RoR2/Base/Shaders/HGStandard.shader"},
            {"stubbedror2/base/shaders/hgsnowtopped", "RoR2/Base/Shaders/HGSnowTopped.shader"},
            {"stubbedror2/base/shaders/hgtriplanarterrainblend", "RoR2/Base/Shaders/HGTriplanarTerrainBlend.shader"},
            {"stubbedror2/base/shaders/hgintersectioncloudremap", "RoR2/Base/Shaders/HGIntersectionCloudRemap.shader" },
            {"stubbedror2/base/shaders/hgcloudremap", "RoR2/Base/Shaders/HGCloudRemap.shader" },
            {"stubbedror2/base/shaders/hgopaquecloudremap", "RoR2/Base/Shaders/HGOpaqueCloudRemap.shader" },
            {"stubbedror2/base/shaders/hgdistortion", "RoR2/Base/Shaders/HGDistortion.shader" },
            {"stubbedcalm water/calmwater - dx11 - doublesided", "Calm Water/CalmWater - DX11 - DoubleSided.shader" },
            {"stubbedcalm water/calmwater - dx11", "Calm Water/CalmWater - DX11.shader" },
            {"stubbednature/speedtree", "RoR2/Base/Shaders/SpeedTreeCustom.shader"},
            {"stubbeddecalicious/decaliciousdeferreddecal", "Decalicious/DecaliciousDeferredDecal.shader" }
        };

        public static List<Material> MaterialCache = new List<Material>(); //apparently you need it because reasons?

        public static List<Texture2D> TextureCache = new List<Texture2D>();

        public struct MonsterCategories
        {
            public const string Champions = "Champions";
            public const string Minibosses = "Minibosses";
            public const string BasicMonsters = "Basic Monsters";
            public const string Special = "Special";
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(_contentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            Stopwatch totalStopwatch = new Stopwatch();
            totalStopwatch.Start();

            _contentPack.identifier = identifier;

            Stopwatch segmentStopWatch = new Stopwatch();
            segmentStopWatch.Start();
            string soundbanksFolderPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(ContentProvider).Assembly.Location), SoundbankFolder);
            LoadSoundBanks(soundbanksFolderPath);
            segmentStopWatch.Stop();
            //TimeSpan ts = stopwatch.elapsedSeconds;
            Log.Info("Soundbanks loaded in " + segmentStopWatch.elapsedSeconds);

            string assetBundleFolderPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(ContentProvider).Assembly.Location), AssetBundleFolder);

            AssetBundle assetbundle = null;
            yield return LoadAssetBundle(System.IO.Path.Combine(assetBundleFolderPath, AssetBundleName), args.progressReceiver, (resultAssetBundle) => assetbundle = resultAssetBundle);

            Dictionary<string, Material> materialLookup = new Dictionary<string, Material>();

            yield return LoadAllAssetsAsync(assetbundle, args.progressReceiver, (Action<Material[]>)((assets) =>
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                var materials = assets;

                if (materials != null)
                {
                    foreach (Material material in materials)
                    {
                        materialLookup.Add(material.name, material);

                        if (!material.shader.name.StartsWith("Stubbed")) { continue; }

                        var replacementShader = Addressables.LoadAssetAsync<Shader>(ShaderLookup[material.shader.name.ToLower()]).WaitForCompletion();
                        var oldName = material.shader.name.ToLower();
                        if (replacementShader)
                        {
                            material.shader = replacementShader;
                            MaterialCache.Add(material);
                        }
                        else
                        {
                            Log.Warning("Couldn't find replacement shader for " + material.shader.name.ToLower());
                        }
                    }
                }
                stopwatch.Stop();
                Log.Info("Materials swapped in " + stopwatch.elapsedSeconds);
            }));

            Dictionary<string, Sprite> iconLookup = new Dictionary<string, Sprite>();

            Texture2D texLavaCrackRound = null;
            yield return LoadAllAssetsAsync(assetbundle, args.progressReceiver, (Action<Texture2D[]>)((textures) =>
            {
                texLavaCrackRound = textures.First(texture => texture.name == "texLavaCrackRound");
            }));

            yield return LoadAllAssetsAsync(assetbundle, args.progressReceiver, (Action<Sprite[]>)((assets) =>
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                iconLookup = assets.ToDictionary(item => item.name, item => item);
                stopwatch.Stop();
                Log.Info("Icons loaded in " + stopwatch.elapsedSeconds);
            }));

            AnimationCurveDef adcIfritPylonLight = null;
            yield return LoadAllAssetsAsync(assetbundle, args.progressReceiver, (Action<AnimationCurveDef[]>)((assets) =>
            {
                adcIfritPylonLight = assets.First(acd => acd.name == "adcIfritPylonLightIntencityCurve");
                var acdLaserBarrage = assets.First(acd => acd.name == "LaserBarrageLightIntencity");
                ModdedEntityStates.Colossus.HeadLaserBarrage.HeadLaserBarrageAttack.intencityGraph = acdLaserBarrage.curve;
            }));

            yield return LoadAllAssetsAsync(assetbundle, args.progressReceiver, (Action<GameObject[]>)((assets) =>
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                //var escList = new List<EntityStateConfiguration>();
                var bodyList = new List<GameObject>();
                var masterList = new List<GameObject>();
                var stateList = new List<Type>();
                var sfList = new List<SkillFamily>();
                var sdList = new List<SkillDef>();
                var effectsList = new List<EffectDef>();
                var projectilesList = new List<GameObject>();
                var unlockablesList = new List<UnlockableDef>();
                var itemList = new List<ItemDef>();

                stateList.Add(typeof(ModdedEntityStates.BaseEmoteState)); // dunno if I need it, but just in case

                #region Spitter
                var spitterFactory = new SpitterFactory();

                var biteEffectPrefab = spitterFactory.CreateBiteEffect();
                ModdedEntityStates.Spitter.Bite.biteEffectPrefab = biteEffectPrefab;
                effectsList.Add(new EffectDef(biteEffectPrefab));

                var chargedSpitSmallDoTZone = spitterFactory.CreatedChargedSpitSmallDoTZone();
                var chargedSpitDoTZone = spitterFactory.CreateChargedSpitDoTZone();
                var chargedSpitChunkProjectile = spitterFactory.CreateChargedSpitSplitProjectile(chargedSpitSmallDoTZone);
                var chargedSpitProjectile = spitterFactory.CreateChargedSpitProjectile(chargedSpitDoTZone, chargedSpitChunkProjectile); ;
                ModdedEntityStates.Spitter.FireChargedSpit.projectilePrefab = chargedSpitProjectile;
                projectilesList.Add(chargedSpitProjectile);
                projectilesList.Add(chargedSpitSmallDoTZone);
                projectilesList.Add(chargedSpitDoTZone);
                projectilesList.Add(chargedSpitChunkProjectile);

                Junk.ModdedEntityStates.Spitter.NormalSpit.normalSpitProjectile = spitterFactory.CreateNormalSpitProjectile();
                projectilesList.Add(Junk.ModdedEntityStates.Spitter.NormalSpit.normalSpitProjectile);

                var spitterLog = Utils.CreateUnlockableDef("Logs.SpitterBody.0", "ENEMIES_RETURNS_UNLOCKABLE_LOG_SPITTER");
                unlockablesList.Add(spitterLog);

                SpitterFactory.Skills.NormalSpit = spitterFactory.CreateNormalSpitSkill();
                SpitterFactory.Skills.Bite = spitterFactory.CreateBiteSkill();
                SpitterFactory.Skills.ChargedSpit = spitterFactory.CreateChargedSpitSkill();

                sdList.Add(SpitterFactory.Skills.NormalSpit);
                sdList.Add(SpitterFactory.Skills.Bite);
                sdList.Add(SpitterFactory.Skills.ChargedSpit);

                SpitterFactory.SkillFamilies.Primary = Utils.CreateSkillFamily("SpitterPrimaryFamily", SpitterFactory.Skills.NormalSpit);
                SpitterFactory.SkillFamilies.Secondary = Utils.CreateSkillFamily("SpitterSecondaryFamily", SpitterFactory.Skills.Bite);
                SpitterFactory.SkillFamilies.Special = Utils.CreateSkillFamily("SpitterSpecialFamily", SpitterFactory.Skills.ChargedSpit);

                sfList.Add(SpitterFactory.SkillFamilies.Primary);
                sfList.Add(SpitterFactory.SkillFamilies.Secondary);
                sfList.Add(SpitterFactory.SkillFamilies.Special);

                var spitterBody = assets.First(body => body.name == "SpitterBody");
                SpitterFactory.SpitterBody = spitterFactory.CreateBody(spitterBody, iconLookup["texSpitterIcon"], spitterLog, materialLookup);
                bodyList.Add(SpitterFactory.SpitterBody);

                var spitterMaster = assets.First(master => master.name == "SpitterMaster");
                SpitterFactory.SpitterMaster = spitterFactory.CreateMaster(spitterMaster, spitterBody);
                masterList.Add(SpitterFactory.SpitterMaster);

                SpitterFactory.SpawnCards.cscSpitterDefault = spitterFactory.CreateCard("cscSpitterDefault", spitterMaster, SpitterFactory.SkinDefs.Default, spitterBody);
                var dcSpitterDefault = new DirectorCard
                {
                    spawnCard = SpitterFactory.SpawnCards.cscSpitterDefault,
                    selectionWeight = EnemiesReturnsConfiguration.Spitter.SelectionWeight.Value,
                    spawnDistance = DirectorCore.MonsterSpawnDistance.Standard,
                    preventOverhead = true,
                    minimumStageCompletions = EnemiesReturnsConfiguration.Spitter.MinimumStageCompletion.Value
                };
                AddMonsterToCardCategory(dcSpitterDefault, MonsterCategories.Minibosses, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/Base/foggyswamp/dccsFoggySwampMonsters.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcSpitterDefault, MonsterCategories.Minibosses, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/Base/foggyswamp/dccsFoggySwampMonstersDLC.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcSpitterDefault, MonsterCategories.Minibosses, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC2/dccsFoggySwampMonstersDLC2.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcSpitterDefault, MonsterCategories.Minibosses, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC2/dccsFoggySwampMonstersDLC2Only.asset").WaitForCompletion());

                AddMonsterToCardCategory(dcSpitterDefault, MonsterCategories.Minibosses, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/Base/arena/dccsArenaMonsters.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcSpitterDefault, MonsterCategories.Minibosses, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/Base/arena/dccsArenaMonstersDLC1.asset").WaitForCompletion());

                AddMonsterToCardCategory(dcSpitterDefault, MonsterCategories.Minibosses, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/Base/artifactworld/dccsArtifactWorldMonsters.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcSpitterDefault, MonsterCategories.Minibosses, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/Base/artifactworld/dccsArtifactWorldMonstersDLC1.asset").WaitForCompletion());

                AddMonsterToCardCategory(dcSpitterDefault, MonsterCategories.Minibosses, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC2/artifactworld01/dccsArtifactWorld01Monsters.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcSpitterDefault, MonsterCategories.Minibosses, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC2/artifactworld01/dccsArtifactWorld01Monsters_DLC1.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcSpitterDefault, MonsterCategories.Minibosses, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC2/artifactworld02/dccsArtifactWorld02Monsters.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcSpitterDefault, MonsterCategories.Minibosses, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC2/artifactworld02/dccsArtifactWorld02Monsters_DLC1.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcSpitterDefault, MonsterCategories.Minibosses, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC2/artifactworld03/dccsArtifactWorld03Monsters.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcSpitterDefault, MonsterCategories.Minibosses, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC2/artifactworld03/dccsArtifactWorld03MonstersDLC1.asset").WaitForCompletion());

                //DirectorAPI.DirectorCardHolder dchSpitterDefault = new DirectorAPI.DirectorCardHolder
                //{
                //    Card = dcSpitterDefault,
                //    MonsterCategory = DirectorAPI.MonsterCategory.Minibosses,
                //};
                //DirectorAPI.Helpers.AddNewMonsterToStage(dchSpitterDefault, false, DirectorAPI.Stage.WetlandAspect);
                //DirectorAPI.Helpers.AddNewMonsterToStage(dchSpitterDefault, false, DirectorAPI.Stage.Custom, "FBLScene");
                //DirectorAPI.Helpers.AddNewMonsterToStage(dchSpitterDefault, false, DirectorAPI.Stage.VoidCell);
                //DirectorAPI.Helpers.AddNewMonsterToStage(dchSpitterDefault, false, DirectorAPI.Stage.ArtifactReliquary);

                SpitterFactory.SpawnCards.cscSpitterLakes = spitterFactory.CreateCard("cscSpitterLakes", spitterMaster, SpitterFactory.SkinDefs.Lakes, spitterBody);
                var dcSpitterLakes = new DirectorCard
                {
                    spawnCard = SpitterFactory.SpawnCards.cscSpitterLakes,
                    selectionWeight = EnemiesReturnsConfiguration.Spitter.SelectionWeight.Value,
                    spawnDistance = DirectorCore.MonsterSpawnDistance.Standard,
                    preventOverhead = true,
                    minimumStageCompletions = EnemiesReturnsConfiguration.Spitter.MinimumStageCompletion.Value
                };
                AddMonsterToCardCategory(dcSpitterLakes, MonsterCategories.Minibosses, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC2/lakes/dccsLakesMonsters.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcSpitterLakes, MonsterCategories.Minibosses, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC2/lakes/dccsLakesMonstersDLC1.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcSpitterLakes, MonsterCategories.Minibosses, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC2/lakes/dccsLakesMonstersDLC2.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcSpitterLakes, MonsterCategories.Minibosses, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC2/lakes/dccsLakesMonstersDLC2Only.asset").WaitForCompletion());

                AddMonsterToCardCategory(dcSpitterLakes, MonsterCategories.Minibosses, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/CU8/lakes/dccsLakesMonsters.asset").WaitForCompletion());

                //DirectorAPI.DirectorCardHolder dchSpitterLakes = new DirectorAPI.DirectorCardHolder
                //{
                //    Card = dcSpitterLakes,
                //    MonsterCategory = DirectorAPI.MonsterCategory.Minibosses,
                //};
                //DirectorAPI.Helpers.AddNewMonsterToStage(dchSpitterLakes, false, DirectorAPI.Stage.VerdantFalls);

                SpitterFactory.SpawnCards.cscSpitterSulfur = spitterFactory.CreateCard("cscSpitterSulfur", spitterMaster, SpitterFactory.SkinDefs.Sulfur, spitterBody);
                var dcSpitterSulfur = new DirectorCard
                {
                    spawnCard = SpitterFactory.SpawnCards.cscSpitterSulfur,
                    selectionWeight = EnemiesReturnsConfiguration.Spitter.SelectionWeight.Value,
                    spawnDistance = DirectorCore.MonsterSpawnDistance.Standard,
                    preventOverhead = true,
                    minimumStageCompletions = EnemiesReturnsConfiguration.Spitter.MinimumStageCompletion.Value
                };
                AddMonsterToCardCategory(dcSpitterSulfur, MonsterCategories.Minibosses, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC1/sulfurpools/dccsSulfurPoolsMonstersDLC1.asset").WaitForCompletion());

                //DirectorAPI.DirectorCardHolder dhcSpitterSulfur = new DirectorAPI.DirectorCardHolder
                //{
                //    Card = dcSpitterSulfur,
                //    MonsterCategory = DirectorAPI.MonsterCategory.Minibosses,
                //};


                SpitterFactory.SpawnCards.cscSpitterDepths = spitterFactory.CreateCard("cscSpitterDepths", spitterMaster, SpitterFactory.SkinDefs.Depths, spitterBody);
                var dcSpitterDepth = new DirectorCard
                {
                    spawnCard = SpitterFactory.SpawnCards.cscSpitterDepths,
                    selectionWeight = EnemiesReturnsConfiguration.Spitter.SelectionWeight.Value,
                    spawnDistance = DirectorCore.MonsterSpawnDistance.Standard,
                    preventOverhead = true,
                    minimumStageCompletions = EnemiesReturnsConfiguration.Spitter.MinimumStageCompletion.Value
                };
                AddMonsterToCardCategory(dcSpitterDepth, MonsterCategories.Minibosses, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/Base/dampcave/dccsDampCaveMonsters.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcSpitterDepth, MonsterCategories.Minibosses, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/Base/dampcave/dccsDampCaveMonstersDLC1.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcSpitterDepth, MonsterCategories.Minibosses, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC2/dccsDampCaveMonstersDLC2.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcSpitterDepth, MonsterCategories.Minibosses, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC2/dccsDampCaveMonstersDLC2Only.asset").WaitForCompletion());

                AddMonsterToCardCategory(dcSpitterDepth, MonsterCategories.Minibosses, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC1/itdampcave/dccsITDampCaveMonsters.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcSpitterDepth, MonsterCategories.Minibosses, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC1/itdampcave/dccsITDampCaveMonstersDLC2.asset").WaitForCompletion());

                //DirectorAPI.DirectorCardHolder dhcSpitterDepths = new DirectorAPI.DirectorCardHolder
                //{
                //    Card = dcSpitterDepth,
                //    MonsterCategory = DirectorAPI.MonsterCategory.Minibosses,
                //};
                //DirectorAPI.Helpers.AddNewMonsterToStage(dhcSpitterDepths, false, DirectorAPI.Stage.AbyssalDepths);
                //DirectorAPI.Helpers.AddNewMonsterToStage(dhcSpitterDepths, false, DirectorAPI.Stage.AbyssalDepthsSimulacrum);

                stateList.Add(typeof(ModdedEntityStates.Spitter.Bite));
                stateList.Add(typeof(ModdedEntityStates.Spitter.SpawnState));
                stateList.Add(typeof(Junk.ModdedEntityStates.Spitter.NormalSpit));
                stateList.Add(typeof(ModdedEntityStates.Spitter.ChargeChargedSpit));
                stateList.Add(typeof(ModdedEntityStates.Spitter.FireChargedSpit));
                stateList.Add(typeof(ModdedEntityStates.Spitter.DeathDance));
                stateList.Add(typeof(ModdedEntityStates.Spitter.SpitterMain));
                stateList.Add(typeof(ModdedEntityStates.Spitter.DeathDancePlayer));

                //escList.Add(spitterFactory.CreateSpawnStateConfiguration());
                //escList.Add(spitterFactory.CreateBiteStateConfiguration());
                #endregion

                #region Colossus

                #region ColossalKnurl
                var knurlFactory = new ColossalKnurlFactory();

                ColossalKnurlFactory.itemDef = knurlFactory.CreateItem(assets.First(item => item.name == "PickupColossalCurl"), iconLookup["texColossalKnurlIcon"]);
                itemList.Add(ColossalKnurlFactory.itemDef);

                var dtColossus = ScriptableObject.CreateInstance<ExplicitPickupDropTable>();
                (dtColossus as ScriptableObject).name = "epdtColossus";
                dtColossus.canDropBeReplaced = true;
                dtColossus.pickupEntries = new ExplicitPickupDropTable.PickupDefEntry[]
                {
                    new ExplicitPickupDropTable.PickupDefEntry
                    {
                        pickupWeight = 1,
                        pickupDef = ColossalKnurlFactory.itemDef
                    }
                };

                var knurlProjectileGhost = assets.First(item => item.name == "ColossalKnurlFistProjectileGhost");
                knurlProjectileGhost = knurlFactory.CreateFistGhostPrefab(knurlProjectileGhost);

                var knurlProjectile = assets.First(item => item.name == "ColossalKnurlFistProjectile");
                ColossalKnurlFactory.projectilePrefab = knurlFactory.CreateFistProjectile(knurlProjectile, knurlProjectileGhost);

                projectilesList.Add(ColossalKnurlFactory.projectilePrefab);
                #endregion

                var colossusFactory = new ColossusFactory();

                var stompEffect = colossusFactory.CreateStompEffect();
                ModdedEntityStates.Colossus.Stomp.StompBase.stompEffectPrefab = stompEffect;
                effectsList.Add(new EffectDef(stompEffect));

                var stompProjectile = colossusFactory.CreateStompProjectile();
                ModdedEntityStates.Colossus.Stomp.StompBase.projectilePrefab = stompProjectile;
                projectilesList.Add(stompProjectile);

                var deathFallEffect = colossusFactory.CreateDeathFallEffect();
                effectsList.Add(new EffectDef(deathFallEffect));
                ModdedEntityStates.Colossus.Death.Death2.fallEffect = deathFallEffect;

                var death2Effect = colossusFactory.CreateDeath2Effect();
                effectsList.Add(new EffectDef(death2Effect));
                ModdedEntityStates.Colossus.Death.Death2.deathEffect = death2Effect;

                var clapEffect = colossusFactory.CreateClapEffect();
                ModdedEntityStates.Colossus.RockClap.RockClapEnd.clapEffect = clapEffect;

                var flyingRockGhost = colossusFactory.CreateFlyingRocksGhost();
                Enemies.Colossus.FloatingRocksController.flyingRockPrefab = flyingRockGhost;
                var flyingRockProjectile = colossusFactory.CreateFlyingRockProjectile(flyingRockGhost);
                ModdedEntityStates.Colossus.RockClap.RockClapEnd.projectilePrefab = flyingRockProjectile;
                projectilesList.Add(flyingRockProjectile);

                var laserBarrageProjectile = colossusFactory.CreateLaserBarrageProjectile();
                ModdedEntityStates.Colossus.HeadLaserBarrage.HeadLaserBarrageAttack.projectilePrefab = laserBarrageProjectile;
                projectilesList.Add(laserBarrageProjectile);

                var spawnEffect = colossusFactory.CreateSpawnEffect();
                ModdedEntityStates.Colossus.SpawnState.burrowPrefab = spawnEffect;
                effectsList.Add(new EffectDef(spawnEffect));

                var colossusLog = Utils.CreateUnlockableDef("Logs.ColossusBody.0", "ENEMIES_RETURNS_UNLOCKABLE_LOG_COLOSSUS");
                unlockablesList.Add(colossusLog);

                var laserEffect = colossusFactory.CreateLaserEffect();
                Junk.ModdedEntityStates.Colossus.HeadLaser.HeadLaserAttack.beamPrefab = laserEffect;

                ColossusFactory.Skills.Stomp = colossusFactory.CreateStompSkill();
                ColossusFactory.Skills.StoneClap = colossusFactory.CreateStoneClapSkill();
                ColossusFactory.Skills.LaserBarrage = colossusFactory.CreateLaserBarrageSkill();
                ColossusFactory.Skills.HeadLaser = colossusFactory.CreateHeadLaserSkill();
                sdList.Add(ColossusFactory.Skills.Stomp);
                sdList.Add(ColossusFactory.Skills.StoneClap);
                sdList.Add(ColossusFactory.Skills.LaserBarrage);
                sdList.Add(ColossusFactory.Skills.HeadLaser);

                ColossusFactory.SkillFamilies.Primary = Utils.CreateSkillFamily("ColossusPrimaryFamily", ColossusFactory.Skills.Stomp);
                ColossusFactory.SkillFamilies.Secondary = Utils.CreateSkillFamily("ColossusSecondaryFamily", ColossusFactory.Skills.StoneClap);
                ColossusFactory.SkillFamilies.Utility = Utils.CreateSkillFamily("ColossusUtilityFamily", ColossusFactory.Skills.LaserBarrage);
                ColossusFactory.SkillFamilies.Special = Utils.CreateSkillFamily("ColossusSpecialFamily", ColossusFactory.Skills.HeadLaser);
                sfList.Add(ColossusFactory.SkillFamilies.Primary);
                sfList.Add(ColossusFactory.SkillFamilies.Secondary);
                sfList.Add(ColossusFactory.SkillFamilies.Utility);
                sfList.Add(ColossusFactory.SkillFamilies.Special);

                var colossusBody = assets.First(body => body.name == "ColossusBody");
                ColossusFactory.ColossusBody = colossusFactory.CreateBody(colossusBody, iconLookup["texColossusIcon"], colossusLog, materialLookup, dtColossus);
                bodyList.Add(ColossusFactory.ColossusBody);

                var colossusMaster = assets.First(master => master.name == "ColossusMaster");
                ColossusFactory.ColossusMaster = colossusFactory.CreateMaster(colossusMaster, colossusBody);
                masterList.Add(ColossusFactory.ColossusMaster);

                stateList.Add(typeof(ModdedEntityStates.Colossus.ColossusMain));
                stateList.Add(typeof(ModdedEntityStates.Colossus.SpawnState));
                stateList.Add(typeof(ModdedEntityStates.Colossus.DancePlayer));
                stateList.Add(typeof(ModdedEntityStates.Colossus.Death.InitialDeathState));
                stateList.Add(typeof(ModdedEntityStates.Colossus.Death.BaseDeath));
                stateList.Add(typeof(ModdedEntityStates.Colossus.Death.DeathFallBase));
                stateList.Add(typeof(ModdedEntityStates.Colossus.Death.Death1));
                stateList.Add(typeof(ModdedEntityStates.Colossus.Death.Death2));
                stateList.Add(typeof(ModdedEntityStates.Colossus.Death.DeathBoner));
                stateList.Add(typeof(ModdedEntityStates.Colossus.RockClap.RockClapEnd));
                stateList.Add(typeof(ModdedEntityStates.Colossus.RockClap.RockClapStart));
                stateList.Add(typeof(ModdedEntityStates.Colossus.Stomp.StompEnter));
                stateList.Add(typeof(ModdedEntityStates.Colossus.Stomp.StompBase));
                stateList.Add(typeof(ModdedEntityStates.Colossus.Stomp.StompL));
                stateList.Add(typeof(ModdedEntityStates.Colossus.Stomp.StompR));
                stateList.Add(typeof(ModdedEntityStates.Colossus.HeadLaserBarrage.HeadLaserBarrageStart));
                stateList.Add(typeof(ModdedEntityStates.Colossus.HeadLaserBarrage.HeadLaserBarrageAttack));
                stateList.Add(typeof(ModdedEntityStates.Colossus.HeadLaserBarrage.HeadLaserBarrageEnd));
                stateList.Add(typeof(Junk.ModdedEntityStates.Colossus.HeadLaser.HeadLaserStart));
                stateList.Add(typeof(Junk.ModdedEntityStates.Colossus.HeadLaser.HeadLaserAttack));
                stateList.Add(typeof(Junk.ModdedEntityStates.Colossus.HeadLaser.HeadLaserEnd));

                Enemies.Colossus.ColossusFactory.SpawnCards.cscColossusDefault = colossusFactory.CreateCard("cscColossusDefault", colossusMaster, ColossusFactory.SkinDefs.Default, colossusBody);
                DirectorCard dcColossusDefault = new DirectorCard
                {
                    spawnCard = ColossusFactory.SpawnCards.cscColossusDefault,
                    selectionWeight = EnemiesReturnsConfiguration.Colossus.SelectionWeight.Value,
                    spawnDistance = DirectorCore.MonsterSpawnDistance.Standard,
                    preventOverhead = true,
                    minimumStageCompletions = EnemiesReturnsConfiguration.Colossus.MinimumStageCompletion.Value
                };
                AddMonsterToCardCategory(dcColossusDefault, MonsterCategories.Champions, Addressables.LoadAssetAsync<FamilyDirectorCardCategorySelection>("RoR2/Base/Common/dccsGolemFamily.asset").WaitForCompletion());
                //DirectorAPI.DirectorCardHolder dchColossusDefault = new DirectorAPI.DirectorCardHolder
                //{
                //    Card = dcColossusDefault,
                //    MonsterCategory = DirectorAPI.MonsterCategory.Champions,
                //};
                //AddMonsterToFamily(dcColossusDefault, Addressables.LoadAssetAsync<FamilyDirectorCardCategorySelection>("RoR2/Base/Common/dccsGolemFamily.asset").WaitForCompletion());

                Enemies.Colossus.ColossusFactory.SpawnCards.cscColossusSkyMeadow = colossusFactory.CreateCard("cscColossusSkyMeadow", colossusMaster, ColossusFactory.SkinDefs.SkyMeadow, colossusBody);
                DirectorCard dcColossusSkyMeadow = new DirectorCard
                {
                    spawnCard = ColossusFactory.SpawnCards.cscColossusSkyMeadow,
                    selectionWeight = EnemiesReturnsConfiguration.Colossus.SelectionWeight.Value,
                    spawnDistance = DirectorCore.MonsterSpawnDistance.Standard,
                    preventOverhead = true,
                    minimumStageCompletions = EnemiesReturnsConfiguration.Colossus.MinimumStageCompletion.Value
                };
                AddMonsterToCardCategory(dcColossusSkyMeadow, MonsterCategories.Champions, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/Base/skymeadow/dccsSkyMeadowMonsters.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcColossusSkyMeadow, MonsterCategories.Champions, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/Base/skymeadow/dccsSkyMeadowMonstersDLC1.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcColossusSkyMeadow, MonsterCategories.Champions, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC2/dccsSkyMeadowMonstersDLC2.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcColossusSkyMeadow, MonsterCategories.Champions, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC2/dccsSkyMeadowMonstersDLC2Only.asset").WaitForCompletion());

                AddMonsterToCardCategory(dcColossusSkyMeadow, MonsterCategories.Champions, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC1/itskymeadow/dccsITSkyMeadowMonsters.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcColossusSkyMeadow, MonsterCategories.Champions, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC1/itskymeadow/dccsITSkyMeadowMonstersDLC2.asset").WaitForCompletion());

                //DirectorAPI.DirectorCardHolder dchColossusSkyMeadow = new DirectorAPI.DirectorCardHolder
                //{
                //    Card = dcColossusSkyMeadow,
                //    MonsterCategory = DirectorAPI.MonsterCategory.Champions,
                //};
                //DirectorAPI.Helpers.AddNewMonsterToStage(dchColossusSkyMeadow, false, DirectorAPI.Stage.SkyMeadow);
                //DirectorAPI.Helpers.AddNewMonsterToStage(dchColossusSkyMeadow, false, DirectorAPI.Stage.SkyMeadowSimulacrum);

                Enemies.Colossus.ColossusFactory.SpawnCards.cscColossusGrassy = colossusFactory.CreateCard("cscColossusGrassy", colossusMaster, ColossusFactory.SkinDefs.Grassy, colossusBody);
                DirectorCard dcColossusGrassy = new DirectorCard
                {
                    spawnCard = ColossusFactory.SpawnCards.cscColossusGrassy,
                    selectionWeight = EnemiesReturnsConfiguration.Colossus.SelectionWeight.Value,
                    spawnDistance = DirectorCore.MonsterSpawnDistance.Standard,
                    preventOverhead = true,
                    minimumStageCompletions = EnemiesReturnsConfiguration.Colossus.MinimumStageCompletion.Value
                };
                //DirectorAPI.DirectorCardHolder dchColossusGrassy = new DirectorAPI.DirectorCardHolder
                //{
                //    Card = dcColossusGrassy,
                //    MonsterCategory = DirectorAPI.MonsterCategory.Champions,
                //};
                //AddMonsterToFamily(dcColossusGrassy, Addressables.LoadAssetAsync<FamilyDirectorCardCategorySelection>("RoR2/Base/Common/dccsGolemFamilyNature.asset").WaitForCompletion());

                AddMonsterToCardCategory(dcColossusGrassy, MonsterCategories.Champions, Addressables.LoadAssetAsync<FamilyDirectorCardCategorySelection>("RoR2/Base/Common/dccsGolemFamilyNature").WaitForCompletion());

                AddMonsterToCardCategory(dcColossusGrassy, MonsterCategories.Champions, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/Base/golemplains/dccsGolemplainsMonsters.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcColossusGrassy, MonsterCategories.Champions, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/Base/golemplains/dccsGolemplainsMonstersDLC1.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcColossusGrassy, MonsterCategories.Champions, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC2/dccsGolemplainsMonstersDLC2.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcColossusGrassy, MonsterCategories.Champions, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC2/dccsGolemplainsMonstersDLC2Only.asset").WaitForCompletion());

                AddMonsterToCardCategory(dcColossusGrassy, MonsterCategories.Champions, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/Base/arena/dccsArenaMonsters.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcColossusGrassy, MonsterCategories.Champions, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/Base/arena/dccsArenaMonstersDLC1.asset").WaitForCompletion());

                AddMonsterToCardCategory(dcColossusGrassy, MonsterCategories.Champions, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/Base/dampcave/dccsDampCaveMonsters.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcColossusGrassy, MonsterCategories.Champions, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/Base/dampcave/dccsDampCaveMonstersDLC1.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcColossusGrassy, MonsterCategories.Champions, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC2/dccsDampCaveMonstersDLC2.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcColossusGrassy, MonsterCategories.Champions, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC2/dccsDampCaveMonstersDLC2Only.asset").WaitForCompletion());

                AddMonsterToCardCategory(dcColossusGrassy, MonsterCategories.Champions, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC1/itgolemplains/dccsITGolemplainsMonsters.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcColossusGrassy, MonsterCategories.Champions, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC1/itgolemplains/dccsITGolemplainsMonstersDLC2.asset").WaitForCompletion());

                AddMonsterToCardCategory(dcColossusGrassy, MonsterCategories.Champions, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC1/itdampcave/dccsITDampCaveMonsters.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcColossusGrassy, MonsterCategories.Champions, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC1/itdampcave/dccsITDampCaveMonstersDLC2.asset").WaitForCompletion());
                //DirectorAPI.Helpers.AddNewMonsterToStage(dchColossusGrassy, false, DirectorAPI.Stage.TitanicPlains);
                //DirectorAPI.Helpers.AddNewMonsterToStage(dchColossusGrassy, false, DirectorAPI.Stage.TitanicPlainsSimulacrum);
                //DirectorAPI.Helpers.AddNewMonsterToStage(dchColossusGrassy, false, DirectorAPI.Stage.VoidCell);
                //DirectorAPI.Helpers.AddNewMonsterToStage(dchColossusGrassy, false, DirectorAPI.Stage.AbyssalDepths);
                //DirectorAPI.Helpers.AddNewMonsterToStage(dchColossusGrassy, false, DirectorAPI.Stage.AbyssalDepthsSimulacrum);
                //DirectorAPI.Helpers.AddNewMonsterToStage(dchColossusGrassy, false, DirectorAPI.Stage.Custom, "FBLScene");

                Enemies.Colossus.ColossusFactory.SpawnCards.cscColossusCastle = colossusFactory.CreateCard("cscColossusCastle", colossusMaster, ColossusFactory.SkinDefs.Castle, colossusBody);
                DirectorCard dcColossusCastle = new DirectorCard
                {
                    spawnCard = ColossusFactory.SpawnCards.cscColossusCastle,
                    selectionWeight = EnemiesReturnsConfiguration.Colossus.SelectionWeight.Value,
                    spawnDistance = DirectorCore.MonsterSpawnDistance.Standard,
                    preventOverhead = true,
                    minimumStageCompletions = EnemiesReturnsConfiguration.Colossus.MinimumStageCompletion.Value
                };
                //DirectorAPI.DirectorCardHolder dchColossusCastle = new DirectorAPI.DirectorCardHolder
                //{
                //    Card = dcColossusCastle,
                //    MonsterCategory = DirectorAPI.MonsterCategory.Champions,
                //};
                //DirectorAPI.Helpers.AddNewMonsterToStage(dchColossusCastle, false, DirectorAPI.Stage.Custom, "sm64_bbf_SM64_BBF");

                Enemies.Colossus.ColossusFactory.SpawnCards.cscColossusSandy = colossusFactory.CreateCard("cscColossusSandy", colossusMaster, ColossusFactory.SkinDefs.Sandy, colossusBody);
                DirectorCard dcColossusSandy = new DirectorCard
                {
                    spawnCard = ColossusFactory.SpawnCards.cscColossusSandy,
                    selectionWeight = EnemiesReturnsConfiguration.Colossus.SelectionWeight.Value,
                    spawnDistance = DirectorCore.MonsterSpawnDistance.Standard,
                    preventOverhead = true,
                    minimumStageCompletions = EnemiesReturnsConfiguration.Colossus.MinimumStageCompletion.Value
                };
                AddMonsterToCardCategory(dcColossusSandy, MonsterCategories.Champions, Addressables.LoadAssetAsync<FamilyDirectorCardCategorySelection>("RoR2/Base/Common/dccsGolemFamilySandy.asset").WaitForCompletion());

                AddMonsterToCardCategory(dcColossusSandy, MonsterCategories.Champions, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/Base/goolake/dccsGooLakeMonsters.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcColossusSandy, MonsterCategories.Champions, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/Base/goolake/dccsGooLakeMonstersDLC1.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcColossusSandy, MonsterCategories.Champions, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC2/dccsGooLakeMonstersDLC2.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcColossusSandy, MonsterCategories.Champions, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC2/dccsGooLakeMonstersDLC2Only.asset").WaitForCompletion());

                AddMonsterToCardCategory(dcColossusSandy, MonsterCategories.Champions, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC1/itgoolake/dccsITGooLakeMonsters.asset").WaitForCompletion());
                //DirectorAPI.DirectorCardHolder dchColossusSandy = new DirectorAPI.DirectorCardHolder
                //{
                //    Card = dcColossusSandy,
                //    MonsterCategory = DirectorAPI.MonsterCategory.Champions,
                //};
                //AddMonsterToFamily(dcColossusSandy, Addressables.LoadAssetAsync<FamilyDirectorCardCategorySelection>("RoR2/Base/Common/dccsGolemFamilySandy.asset").WaitForCompletion());
                //DirectorAPI.Helpers.AddNewMonsterToStage(dchColossusSandy, false, DirectorAPI.Stage.AbandonedAqueduct); 
                //DirectorAPI.Helpers.AddNewMonsterToStage(dchColossusSandy, false, DirectorAPI.Stage.AbandonedAqueductSimulacrum);

                Enemies.Colossus.ColossusFactory.SpawnCards.cscColossusSnowy = colossusFactory.CreateCard("cscColossusSnowy", colossusMaster, ColossusFactory.SkinDefs.Snowy, colossusBody);
                DirectorCard dcColossusSnowy = new DirectorCard
                {
                    spawnCard = ColossusFactory.SpawnCards.cscColossusSnowy,
                    selectionWeight = EnemiesReturnsConfiguration.Colossus.SelectionWeight.Value,
                    spawnDistance = DirectorCore.MonsterSpawnDistance.Standard,
                    preventOverhead = true,
                    minimumStageCompletions = EnemiesReturnsConfiguration.Colossus.MinimumStageCompletion.Value
                };
                AddMonsterToCardCategory(dcColossusSnowy, MonsterCategories.Champions, Addressables.LoadAssetAsync<FamilyDirectorCardCategorySelection>("RoR2/Base/Common/dccsGolemFamilySnowy.asset").WaitForCompletion());

                AddMonsterToCardCategory(dcColossusSnowy, MonsterCategories.Champions, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC1/snowyforest/dccsSnowyForestMonstersDLC1.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcColossusSnowy, MonsterCategories.Champions, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC2/dccsSnowyForestMonstersDLC2.asset").WaitForCompletion());

                AddMonsterToCardCategory(dcColossusSnowy, MonsterCategories.Champions, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/Base/frozenwall/dccsFrozenWallMonsters.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcColossusSnowy, MonsterCategories.Champions, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/Base/frozenwall/dccsFrozenWallMonstersDLC1.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcColossusSnowy, MonsterCategories.Champions, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC2/dccsFrozenWallMonstersDLC2.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcColossusSnowy, MonsterCategories.Champions, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC2/dccsFrozenWallMonstersDLC2Only.asset").WaitForCompletion());

                AddMonsterToCardCategory(dcColossusSnowy, MonsterCategories.Champions, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC1/itfrozenwall/dccsITFrozenWallMonsters.asset").WaitForCompletion());
                AddMonsterToCardCategory(dcColossusSnowy, MonsterCategories.Champions, Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/DLC1/itfrozenwall/dccsITFrozenWallMonstersDLC2.asset").WaitForCompletion());

                //DirectorAPI.DirectorCardHolder dchColossusSnowy = new DirectorAPI.DirectorCardHolder
                //{
                //    Card = dcColossusSnowy,
                //    MonsterCategory = DirectorAPI.MonsterCategory.Champions,
                //};
                //AddMonsterToFamily(dcColossusSnowy, Addressables.LoadAssetAsync<FamilyDirectorCardCategorySelection>("RoR2/Base/Common/dccsGolemFamilySnowy.asset").WaitForCompletion());
                //DirectorAPI.Helpers.AddNewMonsterToStage(dchColossusSnowy, false, DirectorAPI.Stage.SiphonedForest);
                //DirectorAPI.Helpers.AddNewMonsterToStage(dchColossusSnowy, false, DirectorAPI.Stage.RallypointDelta);
                //DirectorAPI.Helpers.AddNewMonsterToStage(dchColossusSnowy, false, DirectorAPI.Stage.RallypointDeltaSimulacrum);
                #endregion

                #region Ifrit

                #region IfritPylon
                var ifritPylonFactory = new IfritPylonFactory();

                //ModdedEntityStates.Ifrit.Pylon.FireExplosion.explosionPrefab = ifritPylonFactory.CreateExplosionEffect();
                ModdedEntityStates.Ifrit.Pylon.FireExplosion.explosionPrefab = ifritPylonFactory.CreateExlosionEffectAlt();
                effectsList.Add(new EffectDef(ModdedEntityStates.Ifrit.Pylon.FireExplosion.explosionPrefab));

                var pylonBody = assets.First(body => body.name == "IfritPylonBody");
                IfritPylonFactory.IfritPylonBody = ifritPylonFactory.CreateBody(pylonBody, adcIfritPylonLight);
                bodyList.Add(IfritPylonFactory.IfritPylonBody);

                var pylonMaster = assets.First(master => master.name == "IfritPylonMaster");
                IfritPylonFactory.IfritPylonMaster = ifritPylonFactory.CreateMaster(pylonMaster, IfritPylonFactory.IfritPylonBody);
                masterList.Add(IfritPylonFactory.IfritPylonMaster);

                IfritPylonFactory.scIfritPylon = ifritPylonFactory.CreateCard("cscIfritPylon", IfritPylonFactory.IfritPylonMaster);
                stateList.Add(typeof(ModdedEntityStates.Ifrit.Pylon.ChargingExplosion));
                stateList.Add(typeof(ModdedEntityStates.Ifrit.Pylon.FiringExplosion));
                stateList.Add(typeof(ModdedEntityStates.Ifrit.Pylon.FireExplosion));
                #endregion

                var ifritFactory = new IfritFactory();

                var ifritManePrefab = assets.First(mane => mane.name == "IfritManeFireParticle");
                var maneMaterial = ifritFactory.CreateManeMaterial();
                materialLookup.Add(maneMaterial.name, maneMaterial);
                ifritManePrefab.GetComponent<Renderer>().material = maneMaterial;

                var ifritHellzonePillarProjectile = assets.First(projectile => projectile.name == "IfritHellzonePillarProjectile");
                var ifritHellzonePillarProjectileGhost = assets.First(projectile => projectile.name == "IfritHellzonePillarProjectileGhost");
                var pillarProjectile = ifritFactory.CreateHellzonePillarProjectile(ifritHellzonePillarProjectile, ifritHellzonePillarProjectileGhost);
                var dotZoneProjectile = ifritFactory.CreateHellfireDotZoneProjectile(pillarProjectile, texLavaCrackRound);
                var hellzoneProjectile = ifritFactory.CreateHellzoneProjectile(dotZoneProjectile);

                projectilesList.Add(dotZoneProjectile);
                projectilesList.Add(hellzoneProjectile);
                projectilesList.Add(pillarProjectile);

                ModdedEntityStates.Ifrit.FlameCharge.FlameCharge.flamethrowerEffectPrefab = ifritFactory.CreateFlameBreath();
                ModdedEntityStates.Ifrit.Hellzone.FireHellzoneFire.projectilePrefab = hellzoneProjectile;

                IfritFactory.Skills.Hellzone = ifritFactory.CreateHellzoneSkill();
                IfritFactory.SkillFamilies.Secondary = Utils.CreateSkillFamily("IfritSecondaryFamily", IfritFactory.Skills.Hellzone);

                IfritFactory.Skills.SummonPylon = ifritFactory.CreateSummonPylonSkill();
                IfritFactory.SkillFamilies.Special = Utils.CreateSkillFamily("IfritSpecialFamily", IfritFactory.Skills.SummonPylon);

                IfritFactory.Skills.FlameCharge = ifritFactory.CreateFlameChargeSkill();
                IfritFactory.SkillFamilies.Utility = Utils.CreateSkillFamily("IfritUtilityFamily", IfritFactory.Skills.FlameCharge);

                var ifritLog = Utils.CreateUnlockableDef("Logs.IfritBody.0", "ENEMIES_RETURNS_UNLOCKABLE_LOG_IFRIT");
                unlockablesList.Add(ifritLog);

                var ifritBody = assets.First(body => body.name == "IfritBody");
                IfritFactory.IfritBody = ifritFactory.CreateBody(ifritBody, null, ifritLog, materialLookup, null);
                bodyList.Add(IfritFactory.IfritBody);

                var ifritMaster = assets.First(master => master.name == "IfritMaster");
                IfritFactory.IfritMaster = ifritFactory.CreateMaster(ifritMaster, IfritFactory.IfritBody);
                masterList.Add(IfritFactory.IfritMaster);

                stateList.Add(typeof(ModdedEntityStates.Ifrit.SpawnState));
                stateList.Add(typeof(ModdedEntityStates.Ifrit.SummonPylon));
                stateList.Add(typeof(ModdedEntityStates.Ifrit.Hellzone.FireHellzoneStart));
                stateList.Add(typeof(ModdedEntityStates.Ifrit.Hellzone.FireHellzoneFire));
                stateList.Add(typeof(ModdedEntityStates.Ifrit.Hellzone.FireHellzoneEnd));
                stateList.Add(typeof(ModdedEntityStates.Ifrit.FlameCharge.FlameChargeBegin));
                stateList.Add(typeof(ModdedEntityStates.Ifrit.FlameCharge.FlameCharge));
                #endregion

                _contentPack.bodyPrefabs.Add(bodyList.ToArray());
                _contentPack.masterPrefabs.Add(masterList.ToArray());
                _contentPack.skillDefs.Add(sdList.ToArray());
                _contentPack.skillFamilies.Add(sfList.ToArray());
                _contentPack.entityStateTypes.Add(stateList.ToArray());
                _contentPack.effectDefs.Add(effectsList.ToArray());
                _contentPack.projectilePrefabs.Add(projectilesList.ToArray());
                _contentPack.unlockableDefs.Add(unlockablesList.ToArray());
                _contentPack.itemDefs.Add(itemList.ToArray());
                //_contentPack.entityStateConfigurations.Add(escList.ToArray());
                stopwatch.Stop();
                Log.Info("Characters created in " + stopwatch.elapsedSeconds);
            }));

            totalStopwatch.Stop();
            Log.Info("Total loading time: " + totalStopwatch.elapsedSeconds);

            yield break;
        }

        private void AddMonsterToCardCategory(DirectorCard card, string categoryName, DirectorCardCategorySelection stageCard)
        {
            int num = Utils.FindCategoryIndexByName(stageCard, categoryName);
            if(num >= 0)
            {
                stageCard.AddCard(num, card);
            }
        }

        private IEnumerator LoadAssetBundle(string assetBundleFullPath, IProgress<float> progress, Action<AssetBundle> onAssetBundleLoaded)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(assetBundleFullPath);
            while (!assetBundleCreateRequest.isDone)
            {
                progress.Report(assetBundleCreateRequest.progress);
                yield return null;
            }

            onAssetBundleLoaded(assetBundleCreateRequest.assetBundle);
            stopwatch.Stop();
            Log.Info("Asset bundle " + assetBundleFullPath + " loaded in " + stopwatch.elapsedSeconds);

            yield break;
        }

        private static IEnumerator LoadAllAssetsAsync<T>(AssetBundle assetBundle, IProgress<float> progress, Action<T[]> onAssetsLoaded) where T : UnityEngine.Object
        {
            var sceneDefsRequest = assetBundle.LoadAllAssetsAsync<T>();
            while (!sceneDefsRequest.isDone)
            {
                progress.Report(sceneDefsRequest.progress);
                yield return null;
            }

            onAssetsLoaded(sceneDefsRequest.allAssets.Cast<T>().ToArray());

            yield break;
        }


        private static void LoadSoundBanks(string soundbanksFolderPath)
        {
            var akResult = AkSoundEngine.AddBasePath(soundbanksFolderPath);
            if (akResult == AKRESULT.AK_Success)
            {
                Log.Info($"Added bank base path : {soundbanksFolderPath}");
            }
            else
            {
                Log.Error(
                    $"Error adding base path : {soundbanksFolderPath} " +
                    $"Error code : {akResult}");
            }

            //akResult = AkSoundEngine.LoadBank(InitSoundBankFileName, out var _);
            //if (akResult == AKRESULT.AK_Success)
            //{
            //    Log.Info($"Added bank : {InitSoundBankFileName}");
            //}
            //else
            //{
            //    Log.Error(
            //        $"Error loading bank : {InitSoundBankFileName} " +
            //        $"Error code : {akResult}");
            //}

            //akResult = AkSoundEngine.LoadBank(MusicSoundBankFileName, out var _);
            //if (akResult == AKRESULT.AK_Success)
            //{
            //    Log.Info($"Added bank : {MusicSoundBankFileName}");
            //}
            //else
            //{
            //    Log.Error(
            //        $"Error loading bank : {MusicSoundBankFileName} " +
            //        $"Error code : {akResult}");
            //}

            akResult = AkSoundEngine.LoadBank(SoundsSoundBankFileName, out var _);
            if (akResult == AKRESULT.AK_Success)
            {
                Log.Info($"Added bank : {SoundsSoundBankFileName}");
            }
            else
            {
                Log.Error(
                    $"Error loading bank : {SoundsSoundBankFileName} " +
                    $"Error code : {akResult}");
            }
        }

    }
}
