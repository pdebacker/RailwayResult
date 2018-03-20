namespace Railway.Result
{
    public interface IResultFailureLogger
    {
        void LogFailure(ResultFailure failureInfo);
    }
    public static class ResultLogger
    {
        // Provide a thread safe global logger
        public static IResultFailureLogger Logger { get; set; }
    }
}
