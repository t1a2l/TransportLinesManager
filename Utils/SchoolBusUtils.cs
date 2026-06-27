using System;
using System.Reflection;
using ColossalFramework;
using Commons.Utils;

namespace TransportLinesManager.Utils
{
    public static class SchoolBusUtils
    {
        private static bool _resolved;
        private static Func<ushort, bool> _isSchoolLine;
        private static Func<ushort, bool> _isSchoolOwnedLine;
        private static Func<ushort, ushort> _getSchoolBuilding;

        // True if the line is a registered school line (generated OR manually flagged).
        public static bool IsSchoolLine(ushort lineId)
        {
            if (!_resolved) Resolve();
            return lineId != 0 && _isSchoolLine != null && _isSchoolLine(lineId);
        }

        // True if this line's bus is supplied by its school (school-as-depot): mod-generated,
        // feature enabled, school still standing. City depots never serve such a line.
        public static bool IsSchoolOwnedLine(ushort lineId)
        {
            if (!_resolved) Resolve();
            return lineId != 0 && _isSchoolOwnedLine != null && _isSchoolOwnedLine(lineId);
        }

        // School (Education building) this line serves, or 0 if it is not a registered school line.
        // The id indexes BuildingManager.m_buildings. NOTE: it is the bound building and is not
        // re-validated, so check Building.Flags.Created if you need a guaranteed-live building.
        public static ushort GetSchoolBuilding(ushort lineId)
        {
            if (!_resolved) Resolve();
            ushort buildingId = 0;
            if (lineId != 0 && _getSchoolBuilding != null)
            {
                buildingId = _getSchoolBuilding(lineId);
                var building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingId];
                if((building.m_flags & Building.Flags.Created) == 0)
                {
                    buildingId = 0;
                }
            }
            return buildingId;
        }

        private static void Resolve()
        {
            _resolved = true;
            try
            {
                Type bridge = Type.GetType("SchoolBuses.Integration.SchoolBusBridge, SchoolBuses", false);
                if (bridge == null)
                { 
                    return; 
                }
                MethodInfo version = bridge.GetMethod("GetApiVersion", BindingFlags.Public | BindingFlags.Static);
                if (version == null || (int)version.Invoke(null, null) < 2)
                { 
                    return; 
                }
                _isSchoolLine = (Func<ushort, bool>)CreateQuery(bridge, "IsSchoolLine");
                _isSchoolOwnedLine = (Func<ushort, bool>)CreateQuery(bridge, "IsSchoolOwnedLine");
                _getSchoolBuilding = (Func<ushort, ushort>)CreateQuery(bridge, "GetSchoolBuilding");
                if (_isSchoolLine != null && _isSchoolOwnedLine != null)
                {
                    LogUtils.DoLog("SchoolBusesUtil: School Buses bridge bound (ApiVersion >= 2) — " +
                             "school lines run as a free school service; school-owned lines hide the depot selector.");
                }
            }
            catch (Exception e)
            {
                _isSchoolLine = null;
                _isSchoolOwnedLine = null;
                LogUtils.DoLog("SchoolBusesUtil: School Buses bridge unavailable (" + e.Message + ")");
            }
        }

        // Bound as a typed delegate (not MethodInfo.Invoke) — these run per frame and per
        // SimulationStep, so avoid the boxing/array allocation of reflective invocation.
        private static Delegate CreateQuery(Type bridge, string method)
        {
            // Find the static method taking a single ushort parameter
            MethodInfo mi = bridge.GetMethod(method, BindingFlags.Public | BindingFlags.Static, null, [typeof(ushort)], null);

            if (mi == null) return null;

            // Check if the return type is bool
            if (mi.ReturnType == typeof(bool))
            {
                return Delegate.CreateDelegate(typeof(Func<ushort, bool>), mi, false);
            }

            // Check if the return type is ushort
            if (mi.ReturnType == typeof(ushort))
            {
                return Delegate.CreateDelegate(typeof(Func<ushort, ushort>), mi, false);
            }

            return null;
        }
    }
}
