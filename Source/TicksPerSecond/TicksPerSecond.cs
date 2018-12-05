// Harmony usage based on
// https://github.com/roxxploxx/RimWorldModGuide/wiki/SHORTTUTORIAL%3A-Harmony

using System;
using System.Reflection;
using Verse;
using UnityEngine;
using Harmony;

namespace TicksPerSecond
{
    [StaticConstructorOnStartup]
    static class TicksPerSecond
    {

        private static DateTime PrevTime;
        private static int PrevTicks;
        private static int TPSActual;

        static TicksPerSecond()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.sparr.tickspersecond");

            // find the DoTimespeedControls method of the class RimWorld.GlobalControlsUtility
            MethodInfo targetmethod = AccessTools.Method(typeof(RimWorld.GlobalControlsUtility),"DoTimespeedControls");

            // find the static method to call before (i.e. Prefix) the targetmethod
            HarmonyMethod postfixmethod = new HarmonyMethod(typeof(TicksPerSecond).GetMethod("RimWorld_GlobalControlsUtility_DoTimespeedControls_Postfix"));

            // patch the targetmethod, by calling prefixmethod before it runs, with no postfixmethod (i.e. null)
            harmony.Patch( targetmethod, null, postfixmethod ) ;

            PrevTicks = -1;
            TPSActual = 0;
        }

        public static void RimWorld_GlobalControlsUtility_DoTimespeedControls_Postfix(float leftX, float width, ref float curBaseY) {

            float TRM = Find.TickManager.TickRateMultiplier;
            int TPSTarget = (int)Math.Round((TRM == 0f) ? 0f : (60f * TRM));

            if (PrevTicks == -1) {
                PrevTicks = GenTicks.TicksAbs;
                PrevTime = DateTime.Now;
            } else {
                DateTime CurrTime = DateTime.Now;

                if (CurrTime.Second != PrevTime.Second) {
                    PrevTime = CurrTime;
                    TPSActual = GenTicks.TicksAbs - PrevTicks;
                    PrevTicks = GenTicks.TicksAbs;
                }
            }

            Rect rect = new Rect(leftX - 20f, curBaseY - 26f, width + 20f - 7f, 26f);
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(rect, "TPS: " + TPSActual.ToString() + "(" + TPSTarget.ToString() + ")");
            Text.Anchor = TextAnchor.UpperLeft;
            curBaseY -= 26f;
        }
    }
}