using System;
using System.Reflection;
using Commons.Utils;

namespace TransportLinesManager.Utils
{
    public static class RealTimeUtils
    {
        private static bool _resolved;

        public delegate void GetSchoolOperationHoursDelegate(out float schoolStartHour, out float schoolEndHour);

        private static GetSchoolOperationHoursDelegate _getSchoolOperationHours;

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

        private static void Resolve()
        {
            _resolved = true;
            try
            {
                Type bridge = Type.GetType("RealTime.Integration.RealTimeBridge, RealTime", false);
                if (bridge == null)
                { 
                    return; 
                }
                MethodInfo version = bridge.GetMethod("GetApiVersion", BindingFlags.Public | BindingFlags.Static);
                if (version == null || (int)version.Invoke(null, null) < 1)
                { 
                    return; 
                }
                Type[] paramTypes = [typeof(float).MakeByRefType(), typeof(float).MakeByRefType()];
                _getSchoolOperationHours = (GetSchoolOperationHoursDelegate)CreateQuery(bridge, "GetSchoolOperationHours", paramTypes, typeof(void));
                if (_getSchoolOperationHours != null)
                {
                    LogUtils.DoLog("RealTimeUtils: Real Time bridge bound (ApiVersion >= 1) — ");
                }
            }
            catch (Exception e)
            {
                _getSchoolOperationHours = null;
                LogUtils.DoLog("RealTimeUtils: Real Time bridge unavailable (" + e.Message + ")");
            }
        }

        // Bound as a typed delegate (not MethodInfo.Invoke) — these run per frame and per
        // SimulationStep, so avoid the boxing/array allocation of reflective invocation.
        private static Delegate CreateQuery(Type bridge, string method, Type[] parameterTypes, Type returnType)
        {
            // Find the static method taking a single ushort parameter
            MethodInfo mi = bridge.GetMethod(method, BindingFlags.Public | BindingFlags.Static, null, parameterTypes, null);

            if(mi == null || mi.ReturnType != returnType) return null;

            try
            {
                if (returnType == typeof(void))
                {
                    Type genericActionDefinition;
                    switch (parameterTypes.Length)
                    {
                        case 0: return Delegate.CreateDelegate(typeof(Action), mi, false);
                        case 1: genericActionDefinition = typeof(Action<>); break;
                        case 2: genericActionDefinition = typeof(Action<,>); break;
                        case 3: genericActionDefinition = typeof(Action<,,>); break;
                        default: throw new ArgumentException("Too many parameters for standard Action delegate.");
                    }

                    Type specificActionType = genericActionDefinition.MakeGenericType(parameterTypes);
                    return Delegate.CreateDelegate(specificActionType, mi, false);
                }

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
            catch
            {
                return null;
            }
        }
    }
}
