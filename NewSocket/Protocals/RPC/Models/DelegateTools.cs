using System;
using System.Reflection;
using System.Threading.Tasks;

namespace NewSocket.Protocals.RPC.Models
{
    public class DelegateTools
    {
        public static void GetReturnTypeInfo(Type t, out bool async, out Type underlying, out bool returns, out PropertyInfo? taskResultInfo)
        {
            returns = true;
            underlying = t;
            taskResultInfo = null;
            async = false;
            if (t == typeof(Task))
            {
                returns = false;
                underlying = typeof(void);
                async = true;
                return;
            }

            if (t == typeof(void))
            {
                returns = false;
                async = false;
                return;
            }

            var generics = t.GenericTypeArguments;
            if (generics.Length == 1)
            {
                var generic = generics[0];
                var gtask = typeof(Task<>).MakeGenericType(generic);
                taskResultInfo = gtask.GetProperty("Result", BindingFlags.Instance | BindingFlags.Public);
                if (gtask == t)
                {
                    underlying = generic;
                    async = true;
                    return;
                }
            }
            return;
        }

        public static bool GetDelegateInfo(Type delegateType, out Type returnType, out ParameterInfo[] parameters)
        {
            parameters = new ParameterInfo[0];
            returnType = typeof(void);
            if (!delegateType.IsSubclassOf(typeof(Delegate)))
            {
                return false;
            }

            var invokeInfo = delegateType.GetMethod("Invoke");
            if (invokeInfo == null)
            {
                return false;
            }

            returnType = invokeInfo.ReturnType;
            parameters = invokeInfo.GetParameters();

            return true;
        }
    }
}