﻿using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EnemiesReturns.Enemies.MechanicalSpider
{
    public class ExecuteSkillOnDamage : MonoBehaviour, IOnTakeDamageServerReceiver
    {
        public GenericSkill skillToExecute;

        public CharacterBody characterBody;

        public EntityStateMachine mainStateMachine;

        private void Awake()
        {
            if(!characterBody)
            {
                characterBody = GetComponent<CharacterBody>();
            }

            if (!mainStateMachine && characterBody)
            {
                mainStateMachine = EntityStateMachine.FindByCustomName(characterBody.gameObject, "Body");
            }
        }

        public void OnTakeDamageServer(DamageReport damageReport)
        {
            if(!characterBody || characterBody.isPlayerControlled || !characterBody.hasEffectiveAuthority)
            {
                this.enabled = false;
                return;
            }

            if (!skillToExecute)
            {
                // no skill - no reason to do anything
                this.enabled = false;
                return;
            }

            if(mainStateMachine.IsInMainState() && characterBody.healthComponent.alive)
            {
                skillToExecute.ExecuteIfReady();
            }
        }
    }
}
