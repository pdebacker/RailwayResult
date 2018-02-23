using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailwayResultTests.StubDomain
{
    public static class Const
    {
        public const int CustomerId = 123;
        public const int NullCustomerId = 0;
        public const int ExceptionCustomerId = -1;
        public const int NullMailCustomerId = 20;
        public const int ValidMailCustomerId = 21;
        public const int InvalidMailCustomerId = 22;
        public const int ExcptionMailCustomerId = 23;
        public const int CustomerWithLowLimitId = 30;

        public const int OfferId = 123;
        public const int NullOfferId = 0;
        public const int ExceptionOfferId = -1;
        public const int OfferWithNullCustomerId = 1;

        public const int ProductId = 123;
        public const int NullProductId = 0;
        public const int ExceptionProductId = -1;

        public const int OrderId = 1;
        public const int NullOrderId = 0;
        public const int ExceptionOrderId = -1;
        public const int UpdateExceptionOrderId = -2;
        public const int OrderWithNullCustomerId = 10;
        public const int OrderWithExceptionCustomerId = 11;
        public const int OrderWithNullMailMailCustomerId = NullMailCustomerId;
        public const int OrderWithValidMailCustomerId = ValidMailCustomerId;
        public const int OrderWithInvalidMailCustomerId = InvalidMailCustomerId;
        public const int OrderWithExceptionMailCustomerId = ExcptionMailCustomerId;
        public const int OrderWithErpProcessFailure = 30;
        public const int OrderWithErpProcessException = 31;
        public const int OrderWithLowLimitCustomerCustomerId = 40;

    }
}
