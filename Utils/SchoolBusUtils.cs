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
        private static Func<bool, bool> _setExternalSpawnControl;
        private static Func<bool, bool> _setVehicleSupplyEnabled;
        private static Func<Action<ushort, bool>, bool> _registerSchoolLineChanged;
        private static Func<Action<ushort, bool>, bool> _unregisterSchoolLineChanged;

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

        // Master flag. Returns the resulting state (true = engaged).
        public static bool SetExternalSpawnControl(bool engaged)
        {
            if (!_resolved) Resolve();
            return _setExternalSpawnControl != null && _setExternalSpawnControl(engaged);
        }

        // Disable the SCHOOL supplying buses (school-as-depot). When false, School Buses stops
        // spawning/despawning AND stops blocking city depots, so depots / TLM serve the line like a normal line
        public static bool SetVehicleSupplyEnabled(bool enabled)
        {
            if (!_resolved) Resolve();
            return _setVehicleSupplyEnabled != null && _setVehicleSupplyEnabled(enabled);
        }

        // Subscribe to be told when the player flags or unflags a line as a school line from the panel (the "School line" tick)
        public static bool RegisterSchoolLineChanged(Action<ushort, bool> callback)
        {
            if (!_resolved) Resolve();
            return callback != null && _registerSchoolLineChanged != null && _registerSchoolLineChanged(callback);
        }

        // Unsubscribe from being told when the player flags or unflags a line as a school line from the panel (the "School line" tick)
        public static bool UnregisterSchoolLineChanged(Action<ushort, bool> callback)
        {
            if (!_resolved) Resolve();
            return callback != null && _unregisterSchoolLineChanged != null && _unregisterSchoolLineChanged(callback);
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
                _isSchoolLine = (Func<ushort, bool>)CreateQuery(bridge, "IsSchoolLine", [typeof(ushort)], typeof(bool));
                _isSchoolOwnedLine = (Func<ushort, bool>)CreateQuery(bridge, "IsSchoolOwnedLine", [typeof(ushort)], typeof(bool));
                _getSchoolBuilding = (Func<ushort, ushort>)CreateQuery(bridge, "GetSchoolBuilding", [typeof(ushort)], typeof(ushort));
                _setExternalSpawnControl = (Func<bool, bool>)CreateQuery(bridge, "SetExternalSpawnControl", [typeof(bool)], typeof(bool));
                _setVehicleSupplyEnabled = (Func<bool, bool>)CreateQuery(bridge, "SetVehicleSupplyEnabled", [typeof(bool)], typeof(bool));
                _registerSchoolLineChanged = (Func<Action<ushort, bool>, bool>)CreateQuery(bridge, "RegisterSchoolLineChanged", [typeof(Action<ushort, bool>)], typeof(bool));
                _unregisterSchoolLineChanged = (Func<Action<ushort, bool>, bool>)CreateQuery(bridge, "UnregisterSchoolLineChanged", [typeof(Action<ushort, bool>)], typeof(bool));
                if (_isSchoolLine != null && _isSchoolOwnedLine != null && _getSchoolBuilding != null 
                    && _setExternalSpawnControl != null && _setVehicleSupplyEnabled != null
                    && _registerSchoolLineChanged != null && _unregisterSchoolLineChanged != null)
                {
                    LogUtils.DoLog("SchoolBusesUtils: School Buses bridge bound (ApiVersion >= 2) — " +
                             "school lines run as a free school service; school-owned lines hide the depot selector.");
                }
            }
            catch (Exception e)
            {
                _isSchoolLine = null;
                _isSchoolOwnedLine = null;
                _getSchoolBuilding = null;
                _setExternalSpawnControl = null;
                _setVehicleSupplyEnabled = null;
                LogUtils.DoLog("SchoolBusesUtils: School Buses bridge unavailable (" + e.Message + ")");
            }
        }

        // Bound as a typed delegate (not MethodInfo.Invoke) — these run per frame and per
        // SimulationStep, so avoid the boxing/array allocation of reflective invocation.
        private static Delegate CreateQuery(Type bridge, string method, Type[] parameterTypes, Type returnType)
        {
            // Find the static method taking a single ushort parameter
            MethodInfo mi = bridge.GetMethod(method, BindingFlags.Public | BindingFlags.Static, null, parameterTypes, null);

            if(mi == null || mi.ReturnType != returnType) return null;

            Type[] funcTypes = [.. parameterTypes, returnType];

            Type genericFuncDefinition = funcTypes.Length switch
            {
                1 => typeof(Func<>),
                2 => typeof(Func<,>),
                3 => typeof(Func<,,>),
                4 => typeof(Func<,,,>),
                _ => throw new ArgumentException("Too many parameters for standard Func delegate."),
            };

            Type specificFuncType = genericFuncDefinition.MakeGenericType(funcTypes);

            return Delegate.CreateDelegate(specificFuncType, mi, false);
        }
    }
}
