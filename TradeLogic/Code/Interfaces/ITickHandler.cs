namespace TradeLogic
{
    public interface ITickHandler
    {
        void OnTick(Tick tick);
        void OnBar(Bar bar);
    }
}

