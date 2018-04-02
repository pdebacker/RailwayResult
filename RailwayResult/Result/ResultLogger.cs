namespace Railway.Result
{
    public interface IResultFailureLogger
    {
        void LogFailure(ResultFailure failureInfo);
    }

    public enum ResultLoggerLevel
    {
        All,
        OnlyExceptionsAndNull,
        OnlyExceptions,
        None
    }
    public static class ResultLogger
    {
        // Provide a thread safe global logger
        public static IResultFailureLogger Logger { get; set; }

        public static ResultLoggerLevel ImplicitLoggingLevel { get; set; }

  

    }
}
