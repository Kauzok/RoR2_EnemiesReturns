﻿using EntityStates;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EnemiesReturns.ModdedEntityStates.Colossus.HeadLaser
{
    public class HeadLaserStart : BaseState
    {
        public static float baseDuration = 5.5f;

        private float duration;

        private static readonly int aimYawCycleHash = Animator.StringToHash("aimYawCycle");

        private static readonly int aimPitchCycleHash = Animator.StringToHash("aimPitchCycle");

        public static float targetPitch = EnemiesReturnsConfiguration.Colossus.HeadLaserPitchStart.Value;

        private float startYaw;

        private float startPitch;

        private Animator modelAnimator;

        public override void OnEnter()
        {
            base.OnEnter();
            modelAnimator = GetModelAnimator();
            if(modelAnimator)
            {
                startYaw = modelAnimator.GetFloat(aimPitchCycleHash);
                startPitch = modelAnimator.GetFloat(aimPitchCycleHash);
            }
            duration = baseDuration / attackSpeedStat;
            PlayCrossfade("Body", "LaserBeamStart", "Laser.playbackrate", duration, 0.1f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (modelAnimator)
            {
                modelAnimator.SetFloat(aimYawCycleHash, Mathf.Clamp(Mathf.Lerp(startYaw, 0f, fixedAge / duration), 0f, 0.99f));
                modelAnimator.SetFloat(aimPitchCycleHash, Mathf.Clamp(Mathf.Lerp(startPitch, targetPitch, fixedAge / duration), 0f, 0.99f));
            }

            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextState(new HeadLaserAttack());
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}
