namespace TradeLogic
{
    /// <summary>
    /// Interface for tick-driven components.
    /// PositionManager only handles ticks; bars are synthesized via BarConsolidator.
    /// </summary>
    public interface ITickHandler
    {
        void OnTick(Tick tick);
    }
}

