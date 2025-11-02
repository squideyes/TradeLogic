using System;

namespace TradeLogic
{
    public sealed class FlatFeeModel : IFeeModel
    {
        private readonly decimal _perContractPerSide;
        public FlatFeeModel(decimal perContractPerSide) { _perContractPerSide = perContractPerSide; }
        public decimal ComputeCommissionPerFill(Fill fill) => _perContractPerSide * Math.Abs(fill.Quantity);
    }
}
