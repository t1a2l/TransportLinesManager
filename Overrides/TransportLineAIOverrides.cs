using HarmonyLib;
using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Klyte.TransportLinesManager.Overrides
{
    [HarmonyPatch(typeof(TransportLineAI))]
	public static class TransportLineAIOverrides 
	{
        private static readonly FieldInfo m_budgetField = typeof(TransportLine).GetField("m_budget", Patcher.allFlags);
        private static readonly MethodInfo m_getBudgetInt = typeof(TLMLineUtils).GetMethod("GetEffectiveBudgetInt", Patcher.allFlags);

		[HarmonyPatch(typeof(TransportLineAI), "SimulationStep", new Type[] { typeof(ushort), typeof(NetNode) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref })]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TranspileSimulationStepAI(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);

            for (int i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Ldfld && inst[i].operand == m_budgetField)
                {
                    inst[i] = new CodeInstruction(OpCodes.Call, m_getBudgetInt);
                    inst[i + 1] = new CodeInstruction(OpCodes.Stloc_S, 4);
                    inst.RemoveAt(i + 9);
                    inst.RemoveAt(i + 8);
                    inst.RemoveAt(i + 7);
                    inst.RemoveAt(i + 6);
                    inst.RemoveAt(i + 5);
                    inst.RemoveAt(i + 4);
                    inst.RemoveAt(i + 3);
                    inst.RemoveAt(i + 2);
                    inst.RemoveAt(i - 1);
                    inst.RemoveAt(i - 4);
                    inst.RemoveAt(i - 5);
                    inst.RemoveAt(i - 6);
                    break;
                }
            }
            LogUtils.PrintMethodIL(inst);
            return inst;
        }
	}
}
