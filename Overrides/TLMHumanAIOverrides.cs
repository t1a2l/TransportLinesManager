using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using ColossalFramework;
using HarmonyLib;
using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Data.Base;
using Klyte.TransportLinesManager.Data.Managers;
using Klyte.TransportLinesManager.Utils;
using static EconomyManager;

namespace Klyte.TransportLinesManager.Overrides
{
    [HarmonyPatch(typeof(HumanAI))]
	public static class TLMHumanAIOverrides 
	{
        private static readonly MethodInfo m_doHumanAiEconomyManagement = typeof(TLMHumanAIOverrides).GetMethod("DoHumanAiEconomyManagement", Patcher.allFlags);
        private static readonly MethodInfo m_economyManagerCallAdd = typeof(EconomyManager).GetMethod("AddResource", Patcher.allFlags, null, new Type[] { typeof(Resource), typeof(int), typeof(ItemClass) }, null);
        private static readonly MethodInfo m_getTicketPriceForPrefix = typeof(TLMHumanAIOverrides).GetMethod("GetTicketPriceForVehicle", Patcher.allFlags);
        private static readonly MethodInfo m_getTicketPriceDefault = typeof(VehicleAI).GetMethod("GetTicketPrice", Patcher.allFlags);

		[HarmonyPatch(typeof(HumanAI), "EnterVehicle")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TranspileEnterVehicle(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);

            for (int i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Callvirt && inst[i].operand == m_economyManagerCallAdd)
                {
                    inst[i] = new CodeInstruction(OpCodes.Ldloc_2);
                    inst.InsertRange(i + 1, new List<CodeInstruction>(){
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Call, m_doHumanAiEconomyManagement)
                    });
                    break;
                }
            }
            LogUtils.PrintMethodIL(inst);
            return inst;
        }

        [HarmonyPatch(typeof(HumanAI), "EnterVehicle")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TicketPriceTranspilerEnterVehicle(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);

            for (int i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Callvirt && inst[i].operand == m_getTicketPriceDefault)
                {
                    inst[i] = new CodeInstruction(OpCodes.Call, m_getTicketPriceForPrefix);
                    inst.RemoveAt(i + 3);
                    inst.RemoveAt(i + 2);
                    break;
                }
            }
            LogUtils.PrintMethodIL(inst);
            return inst;
        }

        private static int DoHumanAiEconomyManagement(EconomyManager instance, Resource resource, int amount, ItemClass itemClass, ushort vehicleId, ushort citizenId)
        {
            LogUtils.DoLog($"DoHumanAiEconomyManagement : vehicleId {vehicleId}");
            ushort lineId = VehicleManager.instance.m_vehicles.m_buffer[vehicleId].m_transportLine;
            ref Citizen citizen = ref CitizenManager.instance.m_citizens.m_buffer[citizenId];
            instance.AddResource(resource, amount, itemClass);
            if (lineId != 0)
            {
                ushort stopId = TransportLine.GetPrevStop(VehicleManager.instance.m_vehicles.m_buffer[vehicleId].m_targetBuilding);
                TLMTransportLineStatusesManager.instance.AddToLine(lineId, amount, 0, ref citizen, citizenId);
                TLMTransportLineStatusesManager.instance.AddToVehicle(vehicleId, amount, 0, ref citizen);
                TLMTransportLineStatusesManager.instance.AddToStop(stopId, amount, ref citizen);
                LogUtils.DoLog($"DoHumanAiEconomyManagement : line {lineId};amount = {amount}; citizen = {citizenId}");
            }

            return 0;
        }

        private static int GetTicketPriceForVehicle(VehicleAI ai, ushort vehicleID, ref Vehicle vehicleData)
        {
            var def = TransportSystemDefinition.From(vehicleData.Info);

            if (def == default)
            {
                LogUtils.DoLog($"GetTicketPriceForVehicle ({vehicleID}):DEFAULT TSD FOR {ai}");
                return ai.GetTicketPrice(vehicleID, ref vehicleData);
            }

            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte district = instance.GetDistrict(vehicleData.m_targetPos3);
            DistrictPolicies.Services servicePolicies = instance.m_districts.m_buffer[district].m_servicePolicies;
            DistrictPolicies.Event @event = instance.m_districts.m_buffer[district].m_eventPolicies & Singleton<EventManager>.instance.GetEventPolicyMask();
            float multiplier;
            if (vehicleData.Info.m_class.m_subService == ItemClass.SubService.PublicTransportTours)
            {
                multiplier = 1;
            }
            else
            {
                if ((servicePolicies & DistrictPolicies.Services.FreeTransport) != DistrictPolicies.Services.None)
                {
                    LogUtils.DoLog($"GetTicketPriceForVehicle ({vehicleID}): FreeTransport at district!");
                    return 0;
                }
                if ((@event & DistrictPolicies.Event.ComeOneComeAll) != DistrictPolicies.Event.None)
                {
                    LogUtils.DoLog($"GetTicketPriceForVehicle ({vehicleID}): ComeOneComeAll at district!");
                    return 0;
                }
                if ((servicePolicies & DistrictPolicies.Services.HighTicketPrices) != DistrictPolicies.Services.None)
                {
                    LogUtils.DoLog($"GetTicketPriceForVehicle ({vehicleID}): HighTicketPrices at district!");
                    instance.m_districts.m_buffer[district].m_servicePoliciesEffect = (instance.m_districts.m_buffer[district].m_servicePoliciesEffect | DistrictPolicies.Services.HighTicketPrices);
                    multiplier = 5f / 4f;
                }
                else
                {
                    multiplier = 1;
                }
            }
            uint ticketPriceDefault = TLMLineUtils.GetTicketPriceForLine(def, vehicleData.m_transportLine).First.Value;
            LogUtils.DoLog($"GetTicketPriceForVehicle ({vehicleID}): multiplier = {multiplier}, ticketPriceDefault = {ticketPriceDefault}");

            return (int)(multiplier * ticketPriceDefault);

        }
	}
}
