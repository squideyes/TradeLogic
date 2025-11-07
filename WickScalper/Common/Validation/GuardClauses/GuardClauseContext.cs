using System;

namespace WickScalper.Common
{
    public class GuardClauseContext<T>
    {
        public T Value { get; }
        public string ParameterName { get; }

        public GuardClauseContext(T value, string parameterName = null)
        {
            Value = value;
            ParameterName = parameterName ?? "value";
        }
    }
}

