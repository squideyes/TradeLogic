using System;

namespace WickScalper.Common
{
    public class GuardClauseResult<T>
    {
        protected GuardClauseContext<T> Context { get; }

        public GuardClauseResult(GuardClauseContext<T> context)
        {
            Context = context;
        }

        protected void Throw(string message)
        {
            throw new ValidationException(message);
        }

        protected string GetMessage(string defaultMessage)
        {
            return $"{Context.ParameterName} {defaultMessage}";
        }
    }
}

