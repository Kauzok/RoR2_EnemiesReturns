﻿using EnemiesReturns.Enemies.Colossus;
using EntityStates;
using Rewired.HID;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace EnemiesReturns.ModdedEntityStates.Colossus.Death
{
    public abstract class DeathFallBase : BaseDeath
    {
        public override float duration => 3.6f;

        public override float fallEffectSpawnTime => 3.2f;

        public override string fallEffectChild => "Death1FallEffect";

        public static float fallBlastAttackRadius = 15f;

        public static float fallBlastAttackDamage = 0.5f;

        public static float fallBlastAttackForce = 3000f;

        public abstract string fallAnimation { get; }

        private bool hasFiredAttack;

        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlaySound("ER_Colossus_Death1_Play", gameObject);
            PlayAnimation("Death, Override", fallAnimation);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(NetworkServer.active && fixedAge >= fallEffectSpawnTime && !hasFiredAttack)
            {
                BlastAttack blastAttack = new BlastAttack();
                blastAttack.radius = fallBlastAttackRadius;
                blastAttack.procCoefficient = 0f;
                blastAttack.position = fallTransform.position;
                blastAttack.attacker = characterBody.gameObject;
                blastAttack.crit = false;
                blastAttack.baseDamage = fallBlastAttackDamage * damageStat;
                blastAttack.canRejectForce = false;
                blastAttack.falloffModel = BlastAttack.FalloffModel.SweetSpot;
                blastAttack.baseForce = fallBlastAttackForce;
                blastAttack.teamIndex = characterBody.teamComponent.teamIndex;
                blastAttack.damageType = DamageType.NonLethal;
                blastAttack.attackerFiltering = AttackerFiltering.Default;
                blastAttack.Fire();

                hasFiredAttack = true;
            }
        }
    }
}
