using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript.Strategies;
using System;
using System.Collections.Generic;
using TL = TradeLogic;

namespace NinjaTrader.NinjaScript.Strategies
{
    public class TestStrategy : Strategy
    {
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "Test strategy to verify TradeLogic.dll reference";
                Name = "TestStrategy";
                Calculate = Calculate.OnBarClose;
                EntriesPerDirection = 1;
                EntryHandling = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy = false;
                OrderFillResolution = OrderFillResolution.Standard;
                StartBehavior = StartBehavior.WaitUntilFlat;
                TimeInForce = NinjaTrader.Cbi.TimeInForce.Gtc;
                BarsRequiredToTrade = 20;
            }
            else if (State == State.DataLoaded)
            {
                // Just try to create a simple TradeLogic object
                try
                {
                    var idGen = new TL.GuidIdGenerator();
                    Print("Successfully created GuidIdGenerator from TradeLogic.dll");
                }
                catch (Exception ex)
                {
                    Print($"Error creating GuidIdGenerator: {ex.Message}");
                }
            }
        }

        protected override void OnBarUpdate()
        {
            // Do nothing
        }
    }
}
