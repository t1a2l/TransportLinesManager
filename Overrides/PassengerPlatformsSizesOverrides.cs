using HarmonyLib;
using Commons.Utils;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace TransportLinesManager.Overrides
{
    [HarmonyPatch]
    public static class PassengerPlatformsSizesOverrides
    {
      
        [HarmonyPatch(typeof(PassengerTrainAI), "LoadPassengers")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TranspliePassengerTrainAIRaiseHalfGrid(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);
            for (int i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Ldc_R4
                    && inst[i].operand is float sg)
                {
                    if (sg == 64f)
                    {
                        inst[i].operand = 68f;
                    }
                    else if (sg == 32f)
                    {
                        inst[i].operand = 36f;
                    }
                    else if (sg == 4096f)
                    {
                        inst[i].operand = 4625f;
                    }
                    else if (sg == 1024f)
                    {
                        inst[i].operand = 1297f;
                    }
                }
            }
            LogUtils.PrintMethodIL(inst);
            return inst;
        }

        [HarmonyPatch(typeof(PassengerTrainAI), "CheckPassengers")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TranspliePassengerTrainAIRaiseHalfGrid1(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);
            for (int i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Ldc_R4
                    && inst[i].operand is float sg)
                {
                    if (sg == 64f)
                    {
                        inst[i].operand = 68f;
                    }
                    else if (sg == 32f)
                    {
                        inst[i].operand = 36f;
                    }
                    else if (sg == 4096f)
                    {
                        inst[i].operand = 4625f;
                    }
                    else if (sg == 1024f)
                    {
                        inst[i].operand = 1297f;
                    }
                }
            }
            LogUtils.PrintMethodIL(inst);
            return inst;
        }

        [HarmonyPatch(typeof(PassengerBlimpAI), "LoadPassengers")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TranspliePassengerBlimpAIRaiseHalfGrid(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);
            for (int i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Ldc_R4
                    && inst[i].operand is float sg)
                {
                    if (sg == 64f)
                    {
                        inst[i].operand = 68f;
                    }
                    else if (sg == 32f)
                    {
                        inst[i].operand = 36f;
                    }
                    else if (sg == 4096f)
                    {
                        inst[i].operand = 4625f;
                    }
                    else if (sg == 1024f)
                    {
                        inst[i].operand = 1297f;
                    }
                }
            }
            LogUtils.PrintMethodIL(inst);
            return inst;
        }

        [HarmonyPatch(typeof(PassengerFerryAI), "LoadPassengers")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TranspliePassengerFerryAIRaiseHalfGrid(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);
            for (int i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Ldc_R4
                    && inst[i].operand is float sg)
                {
                    if (sg == 64f)
                    {
                        inst[i].operand = 68f;
                    }
                    else if (sg == 32f)
                    {
                        inst[i].operand = 36f;
                    }
                    else if (sg == 4096f)
                    {
                        inst[i].operand = 4625f;
                    }
                    else if (sg == 1024f)
                    {
                        inst[i].operand = 1297f;
                    }
                }
            }
            LogUtils.PrintMethodIL(inst);
            return inst;
        }

        [HarmonyPatch(typeof(PassengerHelicopterAI), "LoadPassengers")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TranspliePassengerHelicopterAIRaiseHalfGrid(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);
            for (int i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Ldc_R4
                    && inst[i].operand is float sg)
                {
                    if (sg == 64f)
                    {
                        inst[i].operand = 68f;
                    }
                    else if (sg == 32f)
                    {
                        inst[i].operand = 36f;
                    }
                    else if (sg == 4096f)
                    {
                        inst[i].operand = 4625f;
                    }
                    else if (sg == 1024f)
                    {
                        inst[i].operand = 1297f;
                    }
                }
            }
            LogUtils.PrintMethodIL(inst);
            return inst;
        }

        [HarmonyPatch(typeof(PassengerPlaneAI), "LoadPassengers")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TranspliePassengerPlaneAIRaiseHalfGrid(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);
            for (int i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Ldc_R4
                    && inst[i].operand is float sg)
                {
                    if (sg == 64f)
                    {
                        inst[i].operand = 68f;
                    }
                    else if (sg == 32f)
                    {
                        inst[i].operand = 36f;
                    }
                    else if (sg == 4096f)
                    {
                        inst[i].operand = 4625f;
                    }
                    else if (sg == 1024f)
                    {
                        inst[i].operand = 1297f;
                    }
                }
            }
            LogUtils.PrintMethodIL(inst);
            return inst;
        }

        [HarmonyPatch(typeof(PassengerPlaneAI), "CheckPassengers")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TranspliePassengerPlaneAIRaiseHalfGrid1(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);
            for (int i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Ldc_R4
                    && inst[i].operand is float sg)
                {
                    if (sg == 64f)
                    {
                        inst[i].operand = 68f;
                    }
                    else if (sg == 32f)
                    {
                        inst[i].operand = 36f;
                    }
                    else if (sg == 4096f)
                    {
                        inst[i].operand = 4625f;
                    }
                    else if (sg == 1024f)
                    {
                        inst[i].operand = 1297f;
                    }
                }
            }
            LogUtils.PrintMethodIL(inst);
            return inst;
        }

        [HarmonyPatch(typeof(PassengerShipAI), "LoadPassengers")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TranspliePassengerShipAIRaiseHalfGrid(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);
            for (int i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Ldc_R4
                    && inst[i].operand is float sg)
                {
                    if (sg == 64f)
                    {
                        inst[i].operand = 68f;
                    }
                    else if (sg == 32f)
                    {
                        inst[i].operand = 36f;
                    }
                    else if (sg == 4096f)
                    {
                        inst[i].operand = 4625f;
                    }
                    else if (sg == 1024f)
                    {
                        inst[i].operand = 1297f;
                    }
                }
            }
            LogUtils.PrintMethodIL(inst);
            return inst;
        }

        [HarmonyPatch(typeof(PassengerShipAI), "CheckPassengers")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TranspliePassengerShipAIRaiseHalfGrid1(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);
            for (int i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Ldc_R4
                    && inst[i].operand is float sg)
                {
                    if (sg == 64f)
                    {
                        inst[i].operand = 68f;
                    }
                    else if (sg == 32f)
                    {
                        inst[i].operand = 36f;
                    }
                    else if (sg == 4096f)
                    {
                        inst[i].operand = 4625f;
                    }
                    else if (sg == 1024f)
                    {
                        inst[i].operand = 1297f;
                    }
                }
            }
            LogUtils.PrintMethodIL(inst);
            return inst;
        }

        [HarmonyPatch(typeof(TramAI), "LoadPassengers")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TransplieTramAIRaiseHalfGrid(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);
            for (int i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Ldc_R4
                    && inst[i].operand is float sg)
                {
                    if (sg == 64f)
                    {
                        inst[i].operand = 68f;
                    }
                    else if (sg == 32f)
                    {
                        inst[i].operand = 36f;
                    }
                    else if (sg == 4096f)
                    {
                        inst[i].operand = 4625f;
                    }
                    else if (sg == 1024f)
                    {
                        inst[i].operand = 1297f;
                    }
                }
            }
            LogUtils.PrintMethodIL(inst);
            return inst;
        }

        [HarmonyPatch(typeof(TramAI), "CheckPassengers")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TransplieTramAIRaiseHalfGrid1(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);
            for (int i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Ldc_R4
                    && inst[i].operand is float sg)
                {
                    if (sg == 64f)
                    {
                        inst[i].operand = 68f;
                    }
                    else if (sg == 32f)
                    {
                        inst[i].operand = 36f;
                    }
                    else if (sg == 4096f)
                    {
                        inst[i].operand = 4625f;
                    }
                    else if (sg == 1024f)
                    {
                        inst[i].operand = 1297f;
                    }
                }
            }
            LogUtils.PrintMethodIL(inst);
            return inst;
        }

        [HarmonyPatch(typeof(TrolleybusAI), "LoadPassengers")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TransplieTrolleybusAIRaiseHalfGrid(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);
            for (int i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Ldc_R4
                    && inst[i].operand is float sg)
                {
                    if (sg == 64f)
                    {
                        inst[i].operand = 68f;
                    }
                    else if (sg == 32f)
                    {
                        inst[i].operand = 36f;
                    }
                    else if (sg == 4096f)
                    {
                        inst[i].operand = 4625f;
                    }
                    else if (sg == 1024f)
                    {
                        inst[i].operand = 1297f;
                    }
                }
            }
            LogUtils.PrintMethodIL(inst);
            return inst;
        }

        [HarmonyPatch(typeof(TrolleybusAI), "CheckPassengers")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TransplieTrolleybusAIRaiseHalfGrid1(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);
            for (int i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Ldc_R4
                    && inst[i].operand is float sg)
                {
                    if (sg == 64f)
                    {
                        inst[i].operand = 68f;
                    }
                    else if (sg == 32f)
                    {
                        inst[i].operand = 36f;
                    }
                    else if (sg == 4096f)
                    {
                        inst[i].operand = 4625f;
                    }
                    else if (sg == 1024f)
                    {
                        inst[i].operand = 1297f;
                    }
                }
            }
            LogUtils.PrintMethodIL(inst);
            return inst;
        }

        [HarmonyPatch(typeof(BusAI), "LoadPassengers")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TransplieBusAIRaiseHalfGrid(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);
            for (int i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Ldc_R4
                    && inst[i].operand is float sg)
                {
                    if (sg == 64f)
                    {
                        inst[i].operand = 68f;
                    }
                    else if (sg == 32f)
                    {
                        inst[i].operand = 36f;
                    }
                    else if (sg == 4096f)
                    {
                        inst[i].operand = 4625f;
                    }
                    else if (sg == 1024f)
                    {
                        inst[i].operand = 1297f;
                    }
                }
            }
            LogUtils.PrintMethodIL(inst);
            return inst;
        }

        [HarmonyPatch(typeof(CableCarAI), "LoadPassengers")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TransplieCableCarAIRaiseHalfGrid(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);
            for (int i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Ldc_R4
                    && inst[i].operand is float sg)
                {
                    if (sg == 64f)
                    {
                        inst[i].operand = 68f;
                    }
                    else if (sg == 32f)
                    {
                        inst[i].operand = 36f;
                    }
                    else if (sg == 4096f)
                    {
                        inst[i].operand = 4625f;
                    }
                    else if (sg == 1024f)
                    {
                        inst[i].operand = 1297f;
                    }
                }
            }
            LogUtils.PrintMethodIL(inst);
            return inst;
        }
    }
}