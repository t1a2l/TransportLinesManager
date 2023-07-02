using ColossalFramework;
using HarmonyLib;
using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using Klyte.Commons.Utils.UtilitiesClasses;
using Klyte.TransportLinesManager.Data.Base;
using Klyte.TransportLinesManager.Data.DataContainers;
using Klyte.TransportLinesManager.Data.Managers;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static EconomyManager;

namespace Klyte.TransportLinesManager.Overrides
{
    internal enum TLMTransportLineFlags
    {
        ZERO_BUDGET_CURRENT = 0x40000000
    }

    [HarmonyPatch(typeof(TransportLine))]
    public static class TransportLineOverrides
    {
        private static readonly MethodInfo m_targetVehicles = typeof(TransportLine).GetMethod("CalculateTargetVehicleCount", Patcher.allFlags);
        private static readonly MethodInfo m_setActive = typeof(TransportLine).GetMethod("SetActive", Patcher.allFlags);
        private static readonly MethodInfo m_newTargetVehicles = typeof(TransportLineOverrides).GetMethod("NewCalculateTargetVehicleCount", Patcher.allFlags);
        private static readonly MethodInfo m_economyManagerCallFetch = typeof(EconomyManager).GetMethod("FetchResource", Patcher.allFlags, null, new Type[] { typeof(Resource), typeof(int), typeof(ItemClass) }, null);
        private static readonly MethodInfo m_doTransportLineEconomyManagement = typeof(TransportLineOverrides).GetMethod("DoTransportLineEconomyManagement", Patcher.allFlags);
        private static readonly MethodInfo m_getBudgetInt = typeof(TLMLineUtils).GetMethod("GetEffectiveBudgetInt", Patcher.allFlags);

        [HarmonyPatch(typeof(TransportLine), "AddStop")]
        [HarmonyPrefix]
        public static void PreDoAutomation(ushort lineID, ref TransportLine.Flags __state) => __state = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags;

        [HarmonyPatch(typeof(TransportLine), "AddStop")]
        [HarmonyPostfix]
        public static void DoAutomation(ushort lineID, TransportLine.Flags __state)
        {
            LogUtils.DoLog("OLD: " + __state + " ||| NEW: " + Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags);
            if (lineID > 0 && (__state & TransportLine.Flags.Complete) == TransportLine.Flags.None && (__state & TransportLine.Flags.Temporary) == TransportLine.Flags.None)
            {
                if ((Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags & TransportLine.Flags.Complete) != TransportLine.Flags.None
                    && (Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags & (TransportLine.Flags.Temporary)) == TransportLine.Flags.None)
                {
                    if (TLMBaseConfigXML.Instance.UseAutoColor)
                    {
                        TLMController.AutoColor(lineID);
                    }
                    if (TLMBaseConfigXML.Instance.UseAutoName)
                    {
                        TLMController.AutoName(lineID);
                    }
                    TLMController.Instance.LineCreationToolbox.IncrementNumber();
                }
            }
            if ((Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags & TransportLine.Flags.Complete) == TransportLine.Flags.None)
            {
                Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags &= ~TransportLine.Flags.CustomColor;
                TLMTransportLineExtension.Instance.SafeCleanEntry(lineID);
            }

        }
       
        [HarmonyPatch(typeof(TransportLine), "SimulationStep")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TranspileSimulationStepLine(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);

            inst.InsertRange(0, new List<CodeInstruction>
                    {
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Call, m_getBudgetInt),
                        new CodeInstruction(OpCodes.Ldc_I4_0),
                        new CodeInstruction(OpCodes.Cgt),
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Call, m_getBudgetInt),
                        new CodeInstruction(OpCodes.Ldc_I4_0),
                        new CodeInstruction(OpCodes.Cgt),
                        new CodeInstruction(OpCodes.Call,m_setActive ),
                    });
            for (int i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Call && inst[i].operand == m_targetVehicles)
                {
                    inst[i - 1].opcode = OpCodes.Ldarg_1;
                    inst[i] = new CodeInstruction(OpCodes.Call, m_newTargetVehicles);
                    inst.RemoveRange(i - 6, 5);
                }
            }
            LogUtils.PrintMethodIL(inst);
            return inst;
        }

        [HarmonyPatch(typeof(TransportLine), "SimulationStep")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TranspileSimulationStepLine_GoingBack(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);

            for (int i = 1; i < inst.Count - 3; i++)
            {
                if (
                    inst[i - 1].opcode == OpCodes.Br
                    && inst[i].opcode == OpCodes.Ldloc_S
                    && inst[i].operand is LocalBuilder lb1
                    && lb1.LocalIndex == 11
                    && inst[i + 1].opcode == OpCodes.Ldloc_S
                    && inst[i + 1].operand is LocalBuilder lb2
                    && lb2.LocalIndex == 32
                    && inst[i + 2].opcode == OpCodes.Ble
             )
                {
                    LogUtils.DoLog($"Found @ line {i}");
                    var targetLabel = (Label)inst[i + 2].operand;
                    var labelsToAdd = new List<Label>();
                    while (!inst[i].labels.Contains(targetLabel))
                    {
                        labelsToAdd.AddRange(inst[i].labels);
                        inst.RemoveAt(i);
                    }
                    LogUtils.DoLog($"Moved labels: {labelsToAdd.Count}");
                    inst[i].labels.AddRange(labelsToAdd);
                    break;
                }
            }
            LogUtils.PrintMethodIL(inst);
            return inst;
        }

        [HarmonyPatch(typeof(TransportLine), "SimulationStep")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TranspileSimulationStep(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);

            for (int i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Callvirt && inst[i].operand == m_economyManagerCallFetch)
                {
                    inst[i - 1] = new CodeInstruction(OpCodes.Ldarg_1);
                    inst[i] = new CodeInstruction(OpCodes.Ldarg_0);
                    inst[i + 1] = new CodeInstruction(OpCodes.Call, m_doTransportLineEconomyManagement);
                    //inst.RemoveAt(i + 1);
                    inst.RemoveAt(i);
                    //inst.RemoveAt(i - 1);
                    inst.RemoveAt(i - 2);
                    inst.RemoveAt(i - 3);
                    inst.RemoveAt(i - 4);
                    inst.RemoveAt(i - 5);
                    break;
                }
            }
            LogUtils.PrintMethodIL(inst);
            return inst;
        }
        
        [HarmonyPatch(typeof(TransportLine), "AddVehicle")]
        [HarmonyPostfix]
        public static void BusUnbuncher(ushort vehicleID, ref Vehicle data, bool findTargetStop)
        {
            if (findTargetStop && (data.Info.GetAI() is BusAI || data.Info.GetAI() is TramAI || data.Info.GetAI() is TrolleybusAI) && data.m_transportLine > 0)
            {
                TransportLine t = Singleton<TransportManager>.instance.m_lines.m_buffer[data.m_transportLine];
                if (TransportSystemDefinition.GetDefinitionForLine(ref t).GetConfig().RequireLineStartTerminal)
                {
                    var terminalMarkedStops = new ushort[t.CountStops(data.m_transportLine) - 1].Select((x, i) => Tuple.New(t.GetStop(i + 1), TLMStopDataContainer.Instance.SafeGet(t.GetStop(i + 1)).IsTerminal)).Where(x => x.Second).Select(x => x.First);
                    var terminalStops = new ushort[] { t.m_stops }.Union(terminalMarkedStops).ToList();
                    data.m_targetBuilding = terminalStops[SimulationManager.instance.m_randomizer.Int32((uint)terminalStops.Count)];
                }
                else
                {
                    data.m_targetBuilding = t.GetStop(SimulationManager.instance.m_randomizer.Int32((uint)t.CountStops(data.m_transportLine)));
                }
            }
        }

        [HarmonyPatch(typeof(TransportLine), "CanLeaveStop")]
        [HarmonyPrefix]
        public static bool PreCanLeaveStop(TransportLine __instance, ushort nextStop, int waitTime, ref bool __result)
        {
            if (__instance.m_vehicles == 0 || (__instance.m_flags & TransportLine.Flags.Created) == 0)
            {
                __result = CanLeaveStop(__instance, nextStop, waitTime);
                return false;
            }
            var info = VehicleManager.instance.m_vehicles.m_buffer[__instance.m_vehicles].Info;
            var validType = (info.m_vehicleType == VehicleInfo.VehicleType.Car && TLMBaseConfigXML.CurrentContextConfig.ExpressBusesEnabled)
                || (info.m_vehicleType == VehicleInfo.VehicleType.Tram && TLMBaseConfigXML.CurrentContextConfig.ExpressTramsEnabled)
                || (info.m_vehicleType == VehicleInfo.VehicleType.Trolleybus && TLMBaseConfigXML.CurrentContextConfig.ExpressTrolleybusesEnabled);
            var currentStop = TransportLine.GetPrevStop(nextStop);
            __result = validType && currentStop != __instance.m_stops && !TLMStopDataContainer.Instance.SafeGet(currentStop).IsTerminal ? true : CanLeaveStop(__instance, nextStop, waitTime);
            return false;
        }

        private static bool CanLeaveStop(TransportLine __instance, ushort nextStop, int waitTime)
		{
            if (nextStop == 0)
	        {
		        return true;
	        }
	        ushort prevSegment = TransportLine.GetPrevSegment(nextStop);
	        if (prevSegment == 0)
	        {
		        return true;
	        }
	        NetManager instance = Singleton<NetManager>.instance;
	        int trafficLightState = instance.m_segments.m_buffer[prevSegment].m_trafficLightState0;
	        int num = (__instance.m_averageInterval - trafficLightState + 2) / 4;
	        return num <= 0 || waitTime >= 4;
		}

        private static void DoTransportLineEconomyManagement(ushort lineId)
        {
            LogUtils.DoLog($"DoTransportLineEconomyManagement : line {lineId}");
            ref TransportLine tl = ref TransportManager.instance.m_lines.m_buffer[lineId];
            VehicleManager instance3 = VehicleManager.instance;
            ushort currVehicle = tl.m_vehicles;
            int loopCounter = 0;
            var capacities = new Dictionary<ushort, int>();
            while (currVehicle != 0)
            {
                ushort nextLineVehicle = instance3.m_vehicles.m_buffer[currVehicle].m_nextLineVehicle;
                capacities[currVehicle] = VehicleUtils.GetCapacity(instance3.m_vehicles.m_buffer[currVehicle].Info);
                currVehicle = nextLineVehicle;
                if (++loopCounter > 16384)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            int amount = 0;
            var tsd = TransportSystemDefinition.GetDefinitionForLine(ref tl);
            Citizen refNull = default;
            foreach (KeyValuePair<ushort, int> entry in capacities)
            {
                int cost = (int) (entry.Value * tsd.GetEffectivePassengerCapacityCost());
                TLMTransportLineStatusesManager.instance.AddToVehicle(entry.Key, 0, cost, ref refNull);
                amount += cost;
            }


            LogUtils.DoLog($"DoTransportLineEconomyManagement : line {lineId} ({tsd} {tl.m_lineNumber}) ;amount = {amount}");
            TLMTransportLineStatusesManager.instance.AddToLine(lineId, 0, amount, ref refNull, 0);
            EconomyManager.instance.FetchResource(Resource.Maintenance, amount, tl.Info.m_class);
        }

        public static int NewCalculateTargetVehicleCount(ushort lineId)
        {
            ref TransportLine t = ref TransportManager.instance.m_lines.m_buffer[lineId];
            float lineLength = t.m_totalLength;
            if (lineLength == 0f && t.m_stops != 0)
            {
                NetManager instance = Singleton<NetManager>.instance;
                ushort stops = t.m_stops;
                ushort num2 = stops;
                int num3 = 0;
                while (num2 != 0)
                {
                    ushort num4 = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        ushort segment = instance.m_nodes.m_buffer[num2].GetSegment(i);
                        if (segment != 0 && instance.m_segments.m_buffer[segment].m_startNode == num2)
                        {
                            lineLength += instance.m_segments.m_buffer[segment].m_averageLength;
                            num4 = instance.m_segments.m_buffer[segment].m_endNode;
                            break;
                        }
                    }
                    num2 = num4;
                    if (num2 == stops)
                    {
                        break;
                    }
                    if (++num3 >= 32768)
                    {
                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                        break;
                    }
                }
                t.m_totalLength = lineLength;
            }
            return TLMLineUtils.ProjectTargetVehicleCount(t.Info, lineLength, TLMLineUtils.GetEffectiveBudget(lineId));
        }

    }
}
