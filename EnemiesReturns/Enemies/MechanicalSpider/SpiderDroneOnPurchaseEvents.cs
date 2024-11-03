﻿using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EnemiesReturns.Enemies.MechanicalSpider
{
    public class SpiderDroneOnPurchaseEvents : MonoBehaviour
    {
        public PurchaseInteraction purchaseInteraction;

        public SummonMasterBehavior summonMasterBehavior;

        public EventFunctions eventFunctions;

        public Inventory inventory;

        private void Awake()
        {
            if (!purchaseInteraction)
            {
                purchaseInteraction = GetComponent<PurchaseInteraction>();
                if (!purchaseInteraction)
                {
                    return;
                }
            }

            if(!summonMasterBehavior)
            {
                summonMasterBehavior = GetComponent<SummonMasterBehavior>();
            }

            if (!eventFunctions)
            {
                eventFunctions = GetComponent<EventFunctions>();
            }

            if(!inventory)
            {
                inventory = GetComponent<Inventory>();
            }

            purchaseInteraction.onPurchase.AddListener(OnPurchase);
        }

        public void OnPurchase(Interactor activator)
        {
            purchaseInteraction.SetAvailable(false);

            if (summonMasterBehavior)
            {
                var master = summonMasterBehavior.OpenSummonReturnMaster(activator);
                if (master && master.inventory && inventory)
                {
                    master.inventory.CopyEquipmentFrom(inventory);
                    master.inventory.CopyItemsFrom(inventory);
                }
            }

            if(eventFunctions)
            {
                eventFunctions.DestroySelf();
            }
        }

    }
}
