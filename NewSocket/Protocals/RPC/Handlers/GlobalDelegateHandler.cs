using NewSocket.Protocals.RPC.Interfaces;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace NewSocket.Protocals.RPC.Handlers
{
    /// <summary>
    /// A catch-all unsafe handler for any delegate
    /// </summary>
    public class GlobalDelegateHandler : IRPCHandler
    {
        public string Name { get; }

        public Type[] Parameters { get; }

        private Type m_ReturnType;
        public Type ReturnType => m_ReturnType;

        public bool IsAsync => m_IsAsync;

        public bool HasReturn => m_HasReturn;

        private bool m_IsAsync;

        private bool m_HasReturn;

        private PropertyInfo m_ResultInfo;

        private Delegate Handler;

        public GlobalDelegateHandler(string name, Delegate handler)
        {
            Name = name;
            Handler = handler;
            var param = handler.Method.GetParameters();
            if (param.Length > 0)
            {
                Parameters = param.Select(x => x.ParameterType).ToArray();
            }
            else
            {
                Parameters = new Type[0];
            }

            m_IsAsync = IsTask(handler.Method.ReturnType, out m_ReturnType, out m_HasReturn);
        }

        public async Task<object> Execute(object[] parameters)
        {
            if (m_IsAsync)
            {
                var obj = Handler.DynamicInvoke(parameters);
                if (obj != null && obj is Task task)
                {
                    await task;
                    if (m_HasReturn && m_ResultInfo != null)
                    {
                        return m_ResultInfo.GetValue(task);
                    }
                    return null;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return Handler.DynamicInvoke(parameters);
            }
        }

        private bool IsTask(Type t, out Type underlying, out bool returns)
        {
            returns = true;
            underlying = t;
            if (t == typeof(Task))
            {
                returns = false;
                return true;
            }

            if (t == typeof(void))
            {
                returns = false;
                return false;
            }

            var generics = t.GenericTypeArguments;
            if (generics.Length == 1)
            {
                var generic = generics[0];
                var gtask = typeof(Task<>).MakeGenericType(generic);
                m_ResultInfo = gtask.GetProperty("Result", BindingFlags.Instance | BindingFlags.Public);
                if (gtask == t)
                {
                    underlying = generic;
                    return true;
                }
            }

            return false;
        }
    }
}