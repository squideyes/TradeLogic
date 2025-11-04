using System;
using System.Collections.Generic;

namespace TradeLogic
{
    public sealed class StateTransitionLogEntry : LogEntryBase
    {
        public Guid PositionId { get; set; }
        public string FromState { get; set; }
        public string ToState { get; set; }
        public string Trigger { get; set; }
        public string Message { get; set; }

        public StateTransitionLogEntry(
            Guid positionId,
            string fromState,
            string toState,
            string trigger,
            string message,
            LogLevel level = LogLevel.Info) : base(level)
        {
            PositionId = positionId;
            FromState = fromState;
            ToState = toState;
            Trigger = trigger;
            Message = message;
        }

        protected override Dictionary<string, object> GetData()
        {
            return new Dictionary<string, object>
            {
                { "positionId", PositionId },
                { "fromState", FromState },
                { "toState", ToState },
                { "trigger", Trigger },
                { "message", Message }
            };
        }
    }
}

