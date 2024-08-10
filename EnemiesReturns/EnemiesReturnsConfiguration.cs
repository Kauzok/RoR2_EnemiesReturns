﻿using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnemiesReturns
{
    public static class EnemiesReturnsConfiguration
    {
        public static ConfigEntry<float> DebugWalkSpeedValue;

        public static ConfigEntry<float> testconfig;

        public struct Spitter
        {
            public static ConfigEntry<int> DirectorCost;
            public static ConfigEntry<int> SelectionWeight;
            public static ConfigEntry<int> MinimumStageCompletion;

            public static ConfigEntry<float> BaseMaxHealth;
            public static ConfigEntry<float> BaseMoveSpeed;
            public static ConfigEntry<float> BaseJumpPower;
            public static ConfigEntry<float> BaseDamage;
            public static ConfigEntry<float> BaseArmor;
            public static ConfigEntry<float> LevelMaxHealth;
            public static ConfigEntry<float> LevelDamage;
            public static ConfigEntry<float> LevelArmor;

            public static ConfigEntry<float> BiteDamageModifier;
            public static ConfigEntry<float> BiteDamageForce;
            public static ConfigEntry<float> BiteRadius;

            [Obsolete]
            public static ConfigEntry<float> NormalSpitDamage;
            [Obsolete]
            public static ConfigEntry<float> NormalSpitSpeed;
            [Obsolete]
            public static ConfigEntry<float> NormalSpitForce;

            public static ConfigEntry<float> ChargedProjectileCooldown;
            public static ConfigEntry<float> ChargedProjectileDamage;
            public static ConfigEntry<float> ChargedProjectileForce;
            public static ConfigEntry<float> ChargedProjectileLargeDoTZoneDamage;
            public static ConfigEntry<float> ChargedProjectileLargeDoTZoneScale;
            public static ConfigEntry<float> ChargedProjectileFlyTime;
            public static ConfigEntry<float> ChargedProjectileSmallDoTZoneDamage;
            public static ConfigEntry<float> ChargedProjectileSmallDoTZoneScale;
        }

        public struct Colossus
        {
            public static ConfigEntry<int> DirectorCost;
            public static ConfigEntry<int> SelectionWeight;
            public static ConfigEntry<int> MinimumStageCompletion;

            public static ConfigEntry<float> BaseMaxHealth;
            public static ConfigEntry<float> BaseMoveSpeed;
            public static ConfigEntry<float> BaseJumpPower;
            public static ConfigEntry<float> BaseDamage;
            public static ConfigEntry<float> BaseArmor;
            public static ConfigEntry<float> LevelMaxHealth;
            public static ConfigEntry<float> LevelDamage;
            public static ConfigEntry<float> LevelArmor;

            public static ConfigEntry<float> StompOverlapAttackDamage;
            public static ConfigEntry<float> StompOverlapAttackForce;
            public static ConfigEntry<float> StompProjectileDamage;
            public static ConfigEntry<float> StompProjectileForce;
            public static ConfigEntry<float> StompProjectileSpeed;
            public static ConfigEntry<int> StompProjectileCount;

            public static ConfigEntry<float> RockClapProjectileDamage;
            public static ConfigEntry<float> RockClapProjectileForce;
            public static ConfigEntry<float> RockClapProjectileSpeed;
            public static ConfigEntry<float> RockClapProjectileSpeedDelta;
            public static ConfigEntry<float> RockClapProjectileDistanceFraction;
            public static ConfigEntry<float> RockClapProjectileDistanceFractionDelta;
            public static ConfigEntry<float> RockClapProjectileSpawnDistance;
            public static ConfigEntry<int> RockClapProjectileCount;
            public static ConfigEntry<float> RockClapDamage;
            public static ConfigEntry<float> RockClapForce;
            public static ConfigEntry<float> RockClapRadius;

            public static ConfigEntry<float> HeadLaserDuration;
            public static ConfigEntry<float> HeadLaserFireFrequency;
            public static ConfigEntry<float> HeadLaserDamage;
            public static ConfigEntry<float> HeadLaserForce;
            public static ConfigEntry<float> HeadLaserRadius;
            public static ConfigEntry<int> HeadLaserTurnCount;
            public static ConfigEntry<float> HeadLaserPitchStart;
            public static ConfigEntry<float> HeadLaserPitchStep;
        }

        public static void PopulateConfig(ConfigFile config) 
        {
            config.Clear();

            DebugWalkSpeedValue = config.Bind("Debug", "walkSpeed value", 1f, "Value speed for walkSpeed animation. For debugging.");
            testconfig = config.Bind("test", "test", 5f, "test");

            #region Spitter

            Spitter.SelectionWeight = config.Bind("Director", "Selection Weight", 1, "Selection weight of Spitter.");
            Spitter.MinimumStageCompletion = config.Bind("Director", "Minimum Stage Completion", 0, "Minimum stages players need to complete before monster starts spawning.");
            Spitter.DirectorCost = config.Bind("Director", "Director Cost", 30, "Director cost of Spitter.");
            
            Spitter.BaseMaxHealth = config.Bind("Character Stats", "Base Max Health", 300f, "Spitter's base health.");
            Spitter.BaseMoveSpeed = config.Bind("Character Stats", "Base Movement Speed", 7f, "Spitter's base movement speed.");
            Spitter.BaseJumpPower = config.Bind("Character Stats", "Base Jump Power", 20f, "Spitter's base jump power.");
            Spitter.BaseDamage = config.Bind("Character Stats", "Base Damage", 20f, "Spitter's base damage.");
            Spitter.BaseArmor = config.Bind("Character Stats", "Base Armor", 0f, "Spitter's base armor.");

            Spitter.LevelMaxHealth = config.Bind("Character Stats", "Health per Level", 90f, "Spitter's health increase per level.");
            Spitter.LevelDamage = config.Bind("Character Stats", "Damage per Level", 4f, "Spitter's damage increase per level.");
            Spitter.LevelArmor = config.Bind("Character Stats", "Armor per Level", 0f, "Spitter's armor increase per level.");

            Spitter.BiteDamageModifier = config.Bind("Spitter Bite", "Bite Damage", 1.5f, "Spitter's Bite damage.");
            Spitter.BiteDamageForce = config.Bind("Spitter Bite", "Bite Force", 200f, "Spitter's Bite force, by default equal to that of Lemurian.");
            Spitter.BiteRadius = config.Bind("Spitter Bite", "Bite Radius", 0f, "Spitter's Bite radius.");

            //Spitter.NormalSpitDamage = config.Bind("Spitter Normal Spit", "Spit Damage", 2f, "Spitter's Normal Spit projectile damage.");
            //Spitter.NormalSpitForce = config.Bind("Spitter Normal Spit", "Spit Force", 1000f, "Spitter's Normal Spit force.");
            //Spitter.NormalSpitSpeed = config.Bind("Spitter Normal Spit", "Spit Speed", 50f, "Spitter's Normal Spit projectile speed.");

            Spitter.ChargedProjectileCooldown = config.Bind("Spitter Charged Spit", "Charged Spit Cooldown", 6f, "Charged Spit Cooldown");
            Spitter.ChargedProjectileDamage = config.Bind("Spitter Charged Spit", "Charged Spit Damage", 1.6f, "Spitter's Charged projectile damage.");
            Spitter.ChargedProjectileForce = config.Bind("Spitter Charged Spit", "Charged Spit Force", 0f, "Spitter's Charged projectile force.");
            Spitter.ChargedProjectileFlyTime = config.Bind("Spitter Charged Spit", "Charged Spit Fly Time", 0.75f, "Spitter's Charged Projectile fly time. The higher the value the bigger the arc and the slower the projectile will be. Don't set it too low otherwise projectile will fly pass targes and puddles will be somewhere in the back.");

            Spitter.ChargedProjectileLargeDoTZoneDamage = config.Bind("Spitter Charged Spit", "Charged Spit Large DoT Zone Damage", 0.15f, "Spitter's Charged Large DoT zone damage off projectile's damage.");
            Spitter.ChargedProjectileLargeDoTZoneScale = config.Bind("Spitter Charged Spit", "Charged Spit Large DoT Zone Size Scale", 0.55f, "Spitter's Charged Large DoT Zone size scale off Mini Mushrim's DoT zone (since it was used as basis). Also controls projectile's blast radius.");
            Spitter.ChargedProjectileSmallDoTZoneDamage = config.Bind("Spitter Charged Spit", "Charged Spit Small DoT Zone Damage", 0.15f, "Spitter's Charged Large DoT zone damage off projectile's damage.");
            Spitter.ChargedProjectileSmallDoTZoneScale = config.Bind("Spitter Charged Spit", "Charged Spit Small DoT Zone Scale", 0.3f, "Spitter's Charged Large DoT Zone scale off Mini Mushrim's DoT zone (since it was used as basis). Also controls projectile's blast radius.");

            #endregion

            #region Colossus

            Colossus.StompOverlapAttackDamage = config.Bind("Colossus Stomp", "Stomp Overlap Damage", 6f, "Colossus' Stomp Overlap (aka stomp itself) damage.");
            Colossus.StompOverlapAttackForce = config.Bind("Colossus Stomp", "Stomp Overlap Force", 6000f, "Colossus' Stomp Overlap (aka stomp itself) force.");
            Colossus.StompProjectileDamage = config.Bind("Colossus Stomp", "Stomp Projectile Damage", 3f, "Colossus' Stomp Projectile damage.");
            Colossus.StompProjectileForce = config.Bind("Colossus Stomp", "Stomp Projectile Force", -2500f, "Colossus' Stomp Projectile force. Default number is negative, that means it pulls towards Colossus.");
            Colossus.StompProjectileSpeed = config.Bind("Colossus Stomp", "Stomp Projectile Speed", 60f, "Colossus' Stomp Projectile speed.");
            Colossus.StompProjectileCount = config.Bind("Colossus Stomp", "Stomp Projectile Count", 16, "Colossus' Stomp Projectile count.");

            Colossus.RockClapProjectileDamage = config.Bind("Colossus Rock Clap", "Rock Clap Projectile Damage", 3f, "Colossus' Rock Clap projectile damage.");
            Colossus.RockClapProjectileForce = config.Bind("Colossus Rock Clap", "Rock Clap Projectile Force", 3000f, "Colossus' Rock Clap projectile force."); // TODO: might be too much
            Colossus.RockClapProjectileSpeed = config.Bind("Colossus Rock Clap", "Rock Clap Projectile Speed", 50f, "Colossus' Rock Clap projectile speed.");
            Colossus.RockClapProjectileSpeedDelta = config.Bind("Colossus Rock Clap", "Rock Clap Projectile Speed Delta", 5f, "Colossus' Rock Clap projectile speed delta (speed = Random(speed - delta, speed + delta)).");
            Colossus.RockClapProjectileDistanceFraction = config.Bind("Colossus Rock Clap", "Rock Clap Projectile Distance Fraction", 0.8f, "Basically determines angle at which rocks fly upwards. Uses colossus' position and rock initial position and takes a distance between them at this fraction.");
            Colossus.RockClapProjectileDistanceFractionDelta = config.Bind("Colossus Rock Clap", "Rock Clap Projectile Distance Fraction Delta", 0.1f, "Projectile distance delta. See Projectile distance for explanation and see Speed delta for the formula.");
            Colossus.RockClapProjectileSpawnDistance = config.Bind("Colossus Rock Clap", "Rock Clap Projectile Spawn Distance", 15f, "Colossus' Rock Clap projectile distance from body. Basically controls how far rocks spawn from the center of the body.");
            Colossus.RockClapProjectileCount = config.Bind("Colossus Rock Clap", "Rock Clap Projectile Count", 20, "Colossus' Rock Clap projectile count.");
            Colossus.RockClapDamage = config.Bind("Colossus Rock Clap", "Rock Clap Damage", 6f, "Colossus' Rock Clap damage.");
            Colossus.RockClapForce = config.Bind("Colossus Rock Clap", "Rock Clap Force", 6000f, "Colossus' Rock Clap force."); // TODO: might be too much
            Colossus.RockClapRadius = config.Bind("Colossus Rock Clap", "Rock Clap Radius", 16f, "Colossus' Rock Clap radius.");

            Colossus.HeadLaserDuration = config.Bind("Colossus Head Laser", "Head Laser Duration", 15f, "Colossus' Head Laser duration. Only includes firing laser itself, pre and post states are not included.");
            Colossus.HeadLaserFireFrequency = config.Bind("Colossus Head Laser", "Head Laser Fire Frequency", 0.03f, "How frequently Colossus' Head Laser fires. Has no effect on visuals.");
            Colossus.HeadLaserDamage = config.Bind("Colossus Head Laser", "Head Laser Damage", 6f, "Colossus' Head Laser Damage");
            Colossus.HeadLaserForce = config.Bind("Colossus Head Laser", "Head Laser Force", 0f, "Colossus' Head Laser force");
            Colossus.HeadLaserRadius = config.Bind("Colossus Head Laser", "Head Laser Radius", 15f, "Colossus' Head Laser radius.");
            Colossus.HeadLaserTurnCount = config.Bind("Colossus Head Laser", "Head Laser Head Turn Count", 3, "How many times Colossus turns its head left to right and back during Head Laser attack. Duration of each turn is (Head Laser Duration)/(Head Laser Head Turn Count).");
            Colossus.HeadLaserPitchStart = config.Bind("Colossus Head Laser", "Head Laser Starting Pitch", 0.1f, "Determines starting pitch of Colossus' head. Values (including total value) above 1 will be limited to 1.");
            Colossus.HeadLaserPitchStep = config.Bind("Colossus Head Laser", "Head Laser Head Pitch Step", 0.25f, "Determines how much higher Colossus' head gets after each turn. Final value is (Head Laser Starting Pitch)+(Head Laser Head Turn Count)*(Head Laser Head Pitch Step). Values (including total value) above 1 will be limited to 1.");
            #endregion
        }


    }
}
