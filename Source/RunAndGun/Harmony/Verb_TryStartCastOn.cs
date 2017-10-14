﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using Verse;
using RimWorld;

namespace RunAndGun.Harmony
{
    [HarmonyPatch(typeof(Verb), "TryStartCastOn")]
    static class Verb_TryStartCastOn
    {
        static bool Prefix(Verb __instance, ref LocalTargetInfo castTarg, ref bool surpriseAttack, ref bool canFreeIntercept)
        {
            Pawn pawn = __instance.CasterPawn;

            if (__instance.caster == null)
            {
                Log.Error("Verb " + __instance.GetUniqueLoadID() + " needs caster to work (possibly lost during saving/loading).");
                return false;
            }
            if (!__instance.caster.Spawned)
            {
                return false;
            }
            if (__instance.state == VerbState.Bursting || !__instance.CanHitTarget(castTarg))
            {
                return false;
            }
            if (__instance.verbProps.CausesTimeSlowdown && castTarg.HasThing && (castTarg.Thing.def.category == ThingCategory.Pawn || (castTarg.Thing.def.building != null && castTarg.Thing.def.building.IsTurret)) && castTarg.Thing.Faction == Faction.OfPlayer && __instance.caster.HostileTo(Faction.OfPlayer))
            {
                Find.TickManager.slower.SignalForceNormalSpeed();
            }

            if (__instance.CasterIsPawn)
            {
                CompRunAndGun comp = pawn.TryGetComp<CompRunAndGun>();
                if (comp == null || !comp.isEnabled || pawn.jobs.curJob.def != JobDefOf.Goto)
                {
                    return true;
                }
            }


            Traverse.Create(__instance).Field("surpriseAttack").SetValue(surpriseAttack);
            Traverse.Create(__instance).Field("canFreeInterceptNow").SetValue(canFreeIntercept);
            Traverse.Create(__instance).Field("currentTarget").SetValue(castTarg);


            if (__instance.CasterIsPawn && __instance.verbProps.warmupTime > 0f)
            {

                ShootLine newShootLine;
                if (!__instance.TryFindShootLineFromTo(__instance.caster.Position, castTarg, out newShootLine))
                {
                    return false;
                }
                __instance.CasterPawn.Drawer.Notify_WarmingCastAlongLine(newShootLine, __instance.caster.Position);
                float statValue = __instance.CasterPawn.GetStatValue(StatDefOf.AimingDelayFactor, true);
                int ticks = (__instance.verbProps.warmupTime * statValue).SecondsToTicks();
                __instance.CasterPawn.stances.SetStance(new Stance_RunAndGun(ticks, castTarg, __instance));
            }
            else
            {
                __instance.WarmupComplete();
            }
            return false;
        }
    }
}