namespace WickScalper.Common
{
    public class GuardContext<T>
    {
        public T Value { get; }
        public string ParameterName { get; }

        public GuardContext(T value, string parameterName = null)
        {
            Value = value;
            ParameterName = parameterName ?? "value";
        }
    }
}

