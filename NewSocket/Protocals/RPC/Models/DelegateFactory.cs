using System;
using System.Linq;
using System.Reflection;

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

        public static Type? FindDelegate(Assembly[] searchAssemblies, Type returns, params Type[] parameterTypes)
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

                        Console.WriteLine($"Del:[{del.Namespace}] {retType.Name} {del.Name}{(gens.Length != 0 ? $"<{string.Join(", ", gens.Select(x => x.Name))}>" : "")}({string.Join(", ", delParameters.Select(x => $"{x.ParameterType.Name} {x.Name}"))})");

                        var buildParamList = new Type[gens.Length];
                        if (retType.IsGenericParameter)
                        {
                            buildParamList[retType.GenericParameterPosition] = returns;

                            if (parameterTypes.Length == delParameters.Length)
                            {
                                var compatible = true;
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