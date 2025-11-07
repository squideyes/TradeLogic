namespace WickScalper.Common
{
    public class ValidatorBase<T>
    {
        protected GuardContext<T> Context { get; }

        public ValidatorBase(GuardContext<T> context) =>
            Context = context;

        protected void Throw(string message) =>
            new GuardException(message);

        protected string GetMessage(string defaultMessage) =>
            $"{Context.ParameterName} {defaultMessage}";
    }
}

