using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TransportLinesManager.Utils
{
    public static class RealTimeUtils
    {
        private static bool _resolved;

        public delegate void GetSchoolOperationHoursDelegate(out float schoolStartHour, out float schoolEndHour);

        private static GetSchoolOperationHoursDelegate _getSchoolOperationHours;

        private static Func<bool> _isWeekendEnabled;

        private static Func<bool> _isWeekend;

        // return real time mod school operation hours.
        public static void GetSchoolOperationHours(out float schoolStartHour, out float schoolEndHour)
        {
            if (!_resolved) Resolve();
            if(_getSchoolOperationHours != null)
            {
                _getSchoolOperationHours(out schoolStartHour, out schoolEndHour);
                return;
            }
            schoolStartHour = 0f;
            schoolEndHour = 0f;
        }

        public static bool IsWeekendEnabled()
        {
            if (!_resolved) Resolve();
            return _isWeekendEnabled != null && _isWeekendEnabled();
        }

        public static bool IsWeekend()
        {
            if (!_resolved) Resolve();
            return _isWeekend != null && _isWeekend();
        }

        private static void Resolve()
        {
            _resolved = true;
            try
            {
                var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "RealTime");
                if (assembly == null)
                {
                    Debug.Log("RealTimeUtils: Real Time assembly not found");
                    return;
                }
                Type bridge = assembly.GetType("RealTime.Integration.RealTimeBridge", false);
                if (bridge == null)
                {
                    Debug.Log("RealTimeUtils: Real Time bridge is null");
                    return; 
                }
                MethodInfo version = bridge.GetMethod("GetApiVersion", BindingFlags.Public | BindingFlags.Static);
                if (version == null || (int)version.Invoke(null, null) < 1)
                {
                    Debug.Log("RealTimeUtils: Real Time bridge ApiVersion less then 1");
                    return; 
                }
                Type[] paramTypes = [typeof(float).MakeByRefType(), typeof(float).MakeByRefType()];
                _getSchoolOperationHours = CreateQuery<GetSchoolOperationHoursDelegate>(bridge, "GetSchoolOperationHours", paramTypes, typeof(void));
                _isWeekendEnabled = CreateQuery<Func<bool>>(bridge, "IsWeekendEnabled", Type.EmptyTypes, typeof(bool));
                _isWeekend = CreateQuery<Func<bool>>(bridge, "IsWeekend", Type.EmptyTypes, typeof(bool));
                if (_getSchoolOperationHours != null && _isWeekendEnabled != null && _isWeekend != null)
                {
                    Debug.Log("RealTimeUtils: Real Time bridge bound (ApiVersion >= 1) Success");
                }
                else
                {
                    Debug.Log("RealTimeUtils: Real Time bridge bound (ApiVersion >= 1) Failed");
                }
            }
            catch (Exception e)
            {
                _getSchoolOperationHours = null;
                Debug.Log("RealTimeUtils: Real Time bridge unavailable (" + e.Message + ")");
            }
        }

        private static MethodInfo FindStaticMethod(Type bridge, string method, Type[] parameterTypes, Type returnType)
        {
            MethodInfo mi = bridge.GetMethod(method, BindingFlags.Public | BindingFlags.Static, null, parameterTypes, null);
            return mi != null && mi.ReturnType == returnType ? mi : null;
        }

        // Bound as a typed delegate (not MethodInfo.Invoke) — these run per frame and per
        // SimulationStep, so avoid the boxing/array allocation of reflective invocation.
        private static TDelegate CreateQuery<TDelegate>(Type bridge, string method, Type[] parameterTypes, Type returnType) where TDelegate : class
        {
            // Find the static method taking a single ushort parameter
            MethodInfo mi = FindStaticMethod(bridge, method, parameterTypes, returnType);

            if (mi == null) return null;

            try
            {
                return Delegate.CreateDelegate(typeof(TDelegate), mi, false) as TDelegate;
            }
            catch
            {
                return null;
            }
        }
    }
}
