using System;

namespace Railway.Result
{
    public class ResultException : Exception
    {
        public ResultException(ResultFailure failureInfo)
        {
            FailureInfo = failureInfo;
        }

        public ResultFailure FailureInfo { get; }
    }
}
