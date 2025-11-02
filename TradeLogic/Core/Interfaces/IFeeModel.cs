namespace TradeLogic
{
    public interface IFeeModel
    {
        decimal ComputeCommissionPerFill(Fill fill);
    }
}
