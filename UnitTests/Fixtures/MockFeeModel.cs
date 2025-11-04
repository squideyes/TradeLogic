using TradeLogic;

namespace TradeLogic.UnitTests
{
    public class MockFeeModel : IFeeModel
    {
        private readonly decimal _feePerFill;

        public MockFeeModel(decimal feePerFill = 0m)
        {
            _feePerFill = feePerFill;
        }

        public decimal ComputeCommissionPerFill(Fill fill)
        {
            return _feePerFill;
        }
    }
}

