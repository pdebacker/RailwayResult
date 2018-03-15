using System;


namespace Railway.Result2
{
    public class Result2<TSuccess, TFailure>
    {
        public bool IsSuccess { get; private set; }

        public bool IsFailure => !IsSuccess;

        public TSuccess SuccessResult { get; private set; }

        public TFailure FailureResult { get; private set; }

        private Result2()
        {
        }

        public static Result2<TSuccess, TFailure> Succeeded(TSuccess successResult)
        {
            if (successResult == null)
                throw new ArgumentNullException(nameof(successResult));

            return new Result2<TSuccess, TFailure>()
            {
                SuccessResult = successResult,
                IsSuccess = true
            };
        }

        public static Result2<TSuccess, TFailure> Failed(TFailure failureResult)
        {
            if (failureResult == null)
                throw new ArgumentNullException(nameof(failureResult));

            return new Result2<TSuccess, TFailure>()
            {
                FailureResult = failureResult,
                IsSuccess = false
            };
        }


    }

}
