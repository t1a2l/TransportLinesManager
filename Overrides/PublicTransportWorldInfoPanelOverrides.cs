using HarmonyLib;
using Commons.Extensions;
using Commons.Utils;
using TransportLinesManager.CommonsWindow;
using TransportLinesManager.WorldInfoPanels;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace TransportLinesManager.Overrides
{
	[HarmonyPatch(typeof(PublicTransportWorldInfoPanel))]
	public static class PublicTransportWorldInfoPanelOverrides
	{
		[HarmonyPatch(typeof(PublicTransportWorldInfoPanel), "Start")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TranspileStart(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var CheckEnabled = typeof(UVMPublicTransportWorldInfoPanel).GetMethod("CheckEnabled", Patcher.allFlags);
            var OverrideStart = typeof(UVMPublicTransportWorldInfoPanel).GetMethod("OverrideStart", Patcher.allFlags);
            var inst = new List<CodeInstruction>(instructions);
            Label label = il.DefineLabel();
            inst[2].labels.Add(label);
            inst.InsertRange(2, new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Call, CheckEnabled),
                new CodeInstruction(OpCodes.Brfalse, label),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call,OverrideStart),
                new CodeInstruction(OpCodes.Ret ),
            });
            LogUtils.PrintMethodIL(inst);
            return inst;
        }

        [HarmonyPatch(typeof(PublicTransportWorldInfoPanel), "UpdateBindings")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TranspileUpdateBindings(IEnumerable<CodeInstruction> instructions)
        {
            var UpdateBindings = typeof(UVMPublicTransportWorldInfoPanel).GetMethod("UpdateBindings", Patcher.allFlags);
            var inst = new List<CodeInstruction>(instructions);
            inst.InsertRange(2, new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Call, UpdateBindings),
                new CodeInstruction(OpCodes.Ret),
            });
            return inst;
        }

        [HarmonyPatch(typeof(PublicTransportWorldInfoPanel), "ResetScrollPosition")]
        [HarmonyPrefix]
        public static bool ResetScrollPosition() => false;

        [HarmonyPatch(typeof(PublicTransportWorldInfoPanel), "OnLineColorChanged")]
        [HarmonyPrefix]
        public static bool OnLineColorChanged()
        {
            return false;
        }
        
        [HarmonyPatch(typeof(PublicTransportWorldInfoPanel), "OnLineNameChanged")]
        [HarmonyPrefix]
        public static bool OnLineNameChanged()
        {
            return false;
        }

        [HarmonyPatch(typeof(PublicTransportWorldInfoPanel), "OnEnable")]
        [HarmonyPrefix]
        public static bool OnEnable(PublicTransportWorldInfoPanel __instance)
        {
			UVMPublicTransportWorldInfoPanel.OnEnableOverride();
            return false;
        }

        [HarmonyPatch(typeof(PublicTransportWorldInfoPanel), "OnDisable")]
        [HarmonyPrefix]
        public static bool OnDisable(PublicTransportWorldInfoPanel __instance, ref InstanceID ___m_InstanceID)
        {
            UVMPublicTransportWorldInfoPanel.OnDisableOverride();
            return false;
        }

        [HarmonyPatch(typeof(PublicTransportWorldInfoPanel), "OnLinesOverviewClicked")]
        [HarmonyPrefix]
        public static bool OnLinesOverviewClicked()
        {
            TransportLinesManagerMod.Instance.OpenPanelAtModTab();
            TLMPanel.Instance.OpenAt(UVMPublicTransportWorldInfoPanel.GetCurrentTSD());
            return false;
        }

        [HarmonyPatch(typeof(PublicTransportWorldInfoPanel), "OnSetTarget")]
        [HarmonyPrefix]
        public static bool PreOnSetTarget()
        {
            UVMPublicTransportWorldInfoPanel.OnSetTarget();
            return false;
        }

        [HarmonyPatch(typeof(PublicTransportWorldInfoPanel), "OnGotFocus")]
        [HarmonyPrefix]
        public static bool OnGotFocus()
        {
            UVMPublicTransportWorldInfoPanel.PreOnGotFocus();
            return false;
        }

	}
}
