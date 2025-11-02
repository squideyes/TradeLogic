//using System;
//using System.Collections.Generic;

//namespace TradeLogic.Logging
//{
//    public abstract class LogEntryBase
//    {
//        public DateTime LoggedOn { get; set; }

//        protected LogEntryBase()
//        {
//            LoggedOn = DateTime.UtcNow.ToEastern();
//        }

//        public string Serialize()
//        {
//            var data = GetData();

//            data["loggedOn"] = LoggedOn;

//            return JsonSerializer.Serialize(data);
//        }

//        protected abstract Dictionary<string, object> GetData();
//    }
//}

