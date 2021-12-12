using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace NewSocket.Protocals.RPC.Models
{
    public class DelegateFactory
    {
        public static Delegate? CreateDelegate(MethodInfo info, object? declaringInstance, Assembly[] sourceAssemblies)
        {
            var parameters = info.GetParameters();
            Type[] paramtypes;
            if (parameters.Any())
            {
                paramtypes = parameters.Select(x => x.ParameterType).ToArray();
            }
            else
            {
                paramtypes = new Type[0];
            }

            DelegateTools.GetReturnTypeInfo(info.ReturnType, out var async, out var returnType, out var returns, out _);

            var delType = FindDelegate(sourceAssemblies, info.ReturnType, paramtypes);

            if (delType == null)
            {
                return null;
            }

            if (info.IsStatic)
            {
                return Delegate.CreateDelegate(delType, info);
            }
            else
            {
                return Delegate.CreateDelegate(delType, declaringInstance, info);
            }
        }

        public static Delegate? CreateDelegate(MethodInfo info, object? declaringInstance)
        {
            return CreateDelegate(info, declaringInstance, new[] { Assembly.GetExecutingAssembly() });
        }

        public static Type? FindDelegate(Type returns, params Type[] parameterTypes)
        {
            return FindDelegate(new[] { Assembly.GetExecutingAssembly() }, returns, parameterTypes: parameterTypes);
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public static Type? FindDelegate(Assembly[] searchAssemblies, Type returns, params Type[] parameterTypes)
        {
            try
            {
                foreach (var search in searchAssemblies)
                {
                    var delegates = search.GetTypes().Where(x => x.IsSubclassOf(typeof(Delegate))).ToArray();

                    foreach (var del in delegates)
                    {
                        if (DelegateTools.GetDelegateInfo(del, out var retType, out var delParameters))
                        {
                            var gens = del.GetGenericArguments();
                            if (returns == retType && MatchesDirect(parameterTypes, delParameters))
                            {
                                return del;
                            }

                            var compatible = true;

                            var buildParamList = new Type[gens.Length];

                            if (returns == typeof(void))
                            {
                                compatible = retType == typeof(void);
                            }
                            else if (retType.IsGenericParameter)
                            {
                                buildParamList[retType.GenericParameterPosition] = returns;
                            }
                            else if (returns != retType)
                            {
                                compatible = false;
                            }

                            if (parameterTypes.Length == delParameters.Length && compatible)
                            {
                                for (int i = 0; i < parameterTypes.Length; i++)
                                {
                                    var requestedType = parameterTypes[i];
                                    var delParamType = delParameters[i].ParameterType;

                                    if (delParamType == requestedType)
                                    {
                                        continue;
                                    }
                                    else if (delParamType.IsGenericParameter)
                                    {
                                        var existingParamtype = buildParamList[delParamType.GenericParameterPosition];
                                        if (existingParamtype != null)
                                        {
                                            if (existingParamtype != requestedType)
                                            {
                                                compatible = false;
                                                break;
                                            }
                                            else
                                            {
                                                buildParamList[delParamType.GenericParameterPosition] = requestedType;
                                            }
                                        }

                                        buildParamList[delParamType.GenericParameterPosition] = requestedType;
                                    } else
                                    {
                                        compatible = false;
                                    }
                                }


                                if (compatible)
                                {
                                    if (buildParamList.Length > 0)
                                    {
                                        return del.MakeGenericType(typeArguments: buildParamList);
                                    }
                                    else
                                    {
                                        return del;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }

        private static bool MatchesDirect(Type[] parameters, ParameterInfo[] paramInfo)
        {
            if (parameters.Length != paramInfo.Length)
            {
                return false;
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i] != paramInfo[i].ParameterType)
                {
                    return false;
                }
            }
            return true;
        }
    }
}