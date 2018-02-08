﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HugsLib;
using HugsLib.Settings;

using RunAndGun.Utilities;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;

namespace RunAndGun
{
    public class Base : ModBase
    {
        public override string ModIdentifier
        {
            get { return "RunAndGun"; }
        }
        internal static SettingHandle<int> accuracyPenalty;
        internal static SettingHandle<int> movementPenaltyHeavy;
        internal static SettingHandle<int> movementPenaltyLight;
        public static SettingHandle<int> enableRGForFleeChance;
        public static SettingHandle<bool> enableRGForAI;
        internal static SettingHandle<DictWeaponRecordHandler> weaponSelecter;
        internal static SettingHandle<DictWeaponRecordHandler> weaponForbidder;
        internal static SettingHandle<String> tabsHandler;

        internal static SettingHandle<float> weightLimitFilter;


        private const int minPercentage = 0;
        private const int maxPercentage = 100;
        private static Color highlight1 = new Color(0.5f, 0, 0, 0.1f);
        String[] tabNames = { "RG_tab1".Translate(), "RG_tab2".Translate()};

        public override void DefsLoaded()
        {
            float maxWeightMelee;
            float maxWeightRanged;
            List<ThingDef> allWeapons = WeaponUtility.getAllWeapons();
            WeaponUtility.getHeaviestWeapons(allWeapons, out maxWeightMelee, out maxWeightRanged);
            maxWeightMelee += 1;
            maxWeightRanged += 1;
            float maxWeightTotal = Math.Max(maxWeightMelee, maxWeightRanged);



            enableRGForAI = Settings.GetHandle<bool>("enableRGForAI", "RG_EnableRGForAI_Title".Translate(), "RG_EnableRGForAI_Description".Translate(), true);
            enableRGForFleeChance = Settings.GetHandle<int>("enableRGForFleeChance", "RG_EnableRGForFleeChance_Title".Translate(), "RG_EnableRGForFleeChance_Description".Translate(), 100, Validators.IntRangeValidator(minPercentage, maxPercentage));
            enableRGForFleeChance.VisibilityPredicate = delegate { return enableRGForAI.Value; };

            accuracyPenalty = Settings.GetHandle<int>("accuracyPenalty", "RG_AccuracyPenalty_Title".Translate(), "RG_AccuracyPenalty_Description".Translate(), 10, Validators.IntRangeValidator(minPercentage, maxPercentage));

            movementPenaltyHeavy = Settings.GetHandle<int>("movementPenaltyHeavy", "RG_MovementPenaltyHeavy_Title".Translate(), "RG_MovementPenaltyHeavy_Description".Translate(), 40, Validators.IntRangeValidator(minPercentage, maxPercentage));
            movementPenaltyLight = Settings.GetHandle<int>("movementPenaltyLight", "RG_MovementPenaltyLight_Title".Translate(), "RG_MovementPenaltyLight_Description".Translate(), 10, Validators.IntRangeValidator(minPercentage, maxPercentage));

            tabsHandler = Settings.GetHandle<String>("tabs", "RG_Tabs_Title".Translate(), "", "none");
            tabsHandler.CustomDrawer = rect => { return DrawUtility.CustomDrawer_Tabs(rect, tabsHandler, tabNames); };

            weightLimitFilter = Settings.GetHandle<float>("weightLimitFilter", "RG_WeightLimitFilter_Title".Translate(), "RG_WeightLimitFilter_Description".Translate(), 3.4f);
            weightLimitFilter.CustomDrawer = rect => { return DrawUtility.CustomDrawer_Filter(rect, weightLimitFilter, false, 0, maxWeightTotal, highlight1); };
            weightLimitFilter.VisibilityPredicate = delegate { return tabsHandler.Value == tabNames[0]; };

            weaponSelecter = Settings.GetHandle<DictWeaponRecordHandler>("weaponSelecter_new", "RG_WeaponSelection_Title".Translate(), "RG_WeaponSelection_Description".Translate(), null);
            weaponSelecter.VisibilityPredicate = delegate { return tabsHandler.Value == tabNames[0]; };

            weaponForbidder = Settings.GetHandle<DictWeaponRecordHandler>("weaponForbidder_new", "RG_WeaponForbidder_Title".Translate(), "RG_WeaponForbidder_Description".Translate(), null);
            weaponForbidder.VisibilityPredicate = delegate { return tabsHandler.Value == tabNames[1]; };

            weaponSelecter.CustomDrawer = rect => { return DrawUtility.CustomDrawer_MatchingWeapons_active(rect, weaponSelecter, allWeapons, weightLimitFilter, "RG_ConsideredLight".Translate(), "RG_ConsideredHeavy".Translate()); };
            weaponForbidder.CustomDrawer = rect => { return DrawUtility.CustomDrawer_MatchingWeapons_active(rect, weaponForbidder, allWeapons, null, "RG_Allow".Translate(), "RG_Forbid".Translate() ); };

            DrawUtility.filterWeapons(ref weaponSelecter, allWeapons, weightLimitFilter);
            DrawUtility.filterWeapons(ref weaponForbidder, allWeapons, null);

        }



    }
}
