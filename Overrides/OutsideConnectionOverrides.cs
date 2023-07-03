using Commons.Utils;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Commons.Extensions;

namespace TransportLinesManager.Overrides
{
    [HarmonyPatch(typeof(OutsideConnectionAI))]
    public static class OutsideConnectionOverrides
    {

        [HarmonyPatch(typeof(OutsideConnectionAI), "StartConnectionTransferImpl")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TranspileStartConnectionTransferImpl(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo GetRandomVehicle = typeof(TransportStationAIOverrides).GetMethod("GetRandomVehicle", Patcher.allFlags);

            var inst = new List<CodeInstruction>(instructions);
            for (int i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Callvirt
                    && inst[i].operand is MethodInfo mi
                    && mi.Name == "GetRandomVehicleInfo")
                {
                    inst.RemoveAt(i);
                    inst.InsertRange(i, new List<CodeInstruction> {
                        new CodeInstruction(OpCodes.Ldarg_2),
                        new CodeInstruction(OpCodes.Call, GetRandomVehicle),
                    });
                }
            }
            LogUtils.PrintMethodIL(inst);
            return inst;
        }
    }
}
