using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Railway.Result
{
    public class ResultFailure
    {
        public ResultFailure()
        {
            IsNull = false;
            Errors = new List<Error>();
            StackTrace = Environment.StackTrace;
            ReturnType = typeof(bool);
            LogResultError();
        }
        public ResultFailure(Type returnType)
        {
            IsNull = true;
            Errors = new List<Error>();
            StackTrace = Environment.StackTrace;
            ReturnType = returnType;
            LogResultError();
        }

        public ResultFailure(Type returnType, Exception ex)
        {
            Errors = new List<Error>();
            Errors.Add(new Error(ex.HResult, ex.Message));
            Ex = ex;
            StackTrace = Environment.StackTrace;
            ReturnType = returnType;
            LogResultError();
        }

        public ResultFailure(Type returnType, Exception ex, string message)
        {
            Errors = new List<Error>();
            Errors.Add(new Error(ex.HResult, message));
            Ex = ex;
            StackTrace = Environment.StackTrace;
            ReturnType = returnType;
            LogResultError();
        }

        public ResultFailure(Type returnType, Exception ex, string message, object callerInstance = null)
        {
            Errors = new List<Error>();
            Errors.Add(new Error(ex.HResult, message));
            Ex = ex;
            Object = callerInstance;
            StackTrace = Environment.StackTrace;
            ReturnType = returnType;
            LogResultError();
        }
        public ResultFailure(Type returnType, string message, object callerInstance = null)
        {
            Errors = new List<Error>();
            Errors.Add(new Error(0, message));
            Object = callerInstance;
            StackTrace = Environment.StackTrace;
            ReturnType = returnType;
            LogResultError();
        }
        public ResultFailure(Type returnType, int code, string message, object callerInstance = null)
        {
            Errors = new List<Error>();
            Errors.Add(new Error(code, message));
            Object = callerInstance;
            StackTrace = Environment.StackTrace;
            ReturnType = returnType;
            LogResultError();
        }

        public bool IsNull { get; }
        public Exception Ex { get; }
        public string StackTrace { get; private set; }
        public string CallStack { get; private set; }
        public Type ReturnType { get; }
        public object Object { get; set; }
        public List<Error> Errors { get; }

        public long Code
        {
            get
            {
                if (Errors.Count > 0)
                    return Errors[0].Code;
                return 0;
            }
        }

        public string Message
        {
            get
            {
                if (Errors.Count > 0)
                    return Errors[0].Message;
                return null;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            if (IsNull)
                sb.AppendLine("IsNull failure");
            else if (Ex != null)
                sb.AppendLine("Exception failure: " + Ex.GetType().Name);
            else
                sb.AppendLine("Result failure");

            if (ReturnType != null)
            {
                sb.AppendLine("Return Type = " + ReturnType.ToString());
            }
            if (Errors.Any())
            {
                sb.AppendLine("Result error(s):");
                foreach (var error in Errors)
                {
                    sb.Append(" Message = " + error.Message + ";");
                    sb.AppendLine(" ErrorCode = " + error.Code);
                }
            }
            sb.AppendLine();
            if (Ex != null)
            {
                sb.AppendLine(Ex.GetType().FullName);
                sb.AppendLine(Ex.Message);
                sb.AppendLine(Ex.StackTrace);
                sb.AppendLine();
            }
            if (StackTrace != null)
            {
                sb.AppendLine("Environment stack:");
                sb.AppendLine(StackTrace);
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private void LogResultError()
        {
            try
            {
                if (ResultLogger.Logger != null)
                {
                    if (ResultLogger.ImplicitLoggingLevel == ResultLoggerLevel.All)
                    {
                        ResultLogger.Logger.LogFailure(this);
                    }
                    else if (ResultLogger.ImplicitLoggingLevel == ResultLoggerLevel.OnlyExceptionsAndNull &&
                             (this.IsNull || this.Ex != null))
                    {
                        ResultLogger.Logger.LogFailure(this);
                    }
                    else if (ResultLogger.ImplicitLoggingLevel == ResultLoggerLevel.OnlyExceptions &&
                             this.Ex != null)
                    {
                        ResultLogger.Logger.LogFailure(this);
                    }
                }
            }
            catch
            {
                // do not fail on failed logging
            }
        }

        public class Error
        {
            public Error(long code, string message)
            {
                Code = code;
                Message = message;
            }
            public long Code { get; }
            public string Message { get; }
        }
    }
}
