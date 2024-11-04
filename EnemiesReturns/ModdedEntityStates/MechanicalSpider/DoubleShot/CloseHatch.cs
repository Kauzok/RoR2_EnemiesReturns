﻿using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnemiesReturns.ModdedEntityStates.MechanicalSpider.DoubleShot
{
    public class CloseHatch : BaseState
    {
        public static float baseDuration = 0.7f;

        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            //PlayAnimation("Hatch", "CloseHatch", "Fire.playbackRate", duration);
            GetModelAnimator().SetBool("hatchOpen", false);
            Util.PlaySound("ER_Spider_Hatch_Close_Play", base.gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}
