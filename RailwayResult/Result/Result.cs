using System;
using System.Collections;
using System.Collections.Generic;

namespace Railway.Result
{
    public class Result<TReturn> : IEnumerable<Result<TReturn>>
    {
        private Result(TReturn returnValue, Type returnType)
        {
            ReturnValue = returnValue;
            if (ReturnValue == null)
                _failureInfo = new ResultFailure(returnType);
            IsSuccess = (ReturnValue != null);
        }

        private Result(ResultFailure failureInfo)
        {
            _failureInfo = failureInfo;
            IsSuccess = false;
        }

        private Result(ResultFailure failureInfo, object instance)
        {
            _failureInfo = failureInfo;
            _failureInfo.Object = instance;
            IsSuccess = false;
        }

        private ResultFailure _failureInfo;
        public TReturn ReturnValue { get; private set; }

        public ResultFailure FailureInfo => _failureInfo;

        public bool IsSuccess { get; private set; }

        public bool IsFailure => !IsSuccess;

        public bool IsException => (_failureInfo?.Ex != null);

        public bool IsNull => (_failureInfo != null && _failureInfo.IsNull);

        public void AddFailure(string message)
        {
            AddFailure(0, message);
        }
        public void AddFailure(int code, string message = null)
        {
            IsSuccess = false;
            if (_failureInfo == null)
            {
                _failureInfo = new ResultFailure(typeof(TReturn), code, message);
            }
            else
            {
                _failureInfo.Errors.Add(new ResultFailure.Error(code, message));
            }
        }

        public static Result<TReturn> ToResult(TReturn value)
        {
            return new Result<TReturn>(value, typeof(TReturn));
        }

        public static Result<TReturn> ToResult(Result<TReturn> value)
        {
            if (value.IsSuccess)
                return value;

            return Result<TReturn>.Failed(value.FailureInfo);
        }

        public static Result<TReturn> ToResult(Func<TReturn> evaluator)
        {
            try
            {
                return new Result<TReturn>(evaluator(), typeof(TReturn));
            }
            catch (Exception ex)
            {
                var b = evaluator.ToString();
                return Failed(ex, ex.Message, b);
            }
        }
        public static Result<bool> Succeeded()
        {
            return new Result<bool>(true, typeof(bool));
        }
        public static Result<bool> Failed()
        {
            var result = new Result<bool>(new ResultFailure());
            result.ReturnValue = false;
            return result;
        }
        // interpret bool as success or failure value
        public static Result<bool> FromBool(bool value)
        {
            return value ? Succeeded() : Failed();
        }
        public static Result<bool> FromBool(Func<bool> evaluator)
        {
            try
            {
                Type returnType = typeof(bool);
                return FromBool(evaluator());
            }
            catch (Exception ex)
            {
                return new Result<bool>(new ResultFailure(typeof(bool), ex));
            }
        }

        public static Result<TReturn> Failed(string message)
        {
            return new Result<TReturn>(new ResultFailure(typeof(TReturn), message));
        }

        public static Result<TReturn> Failed(int code, string message = null)
        {
            return new Result<TReturn>(new ResultFailure(typeof(TReturn), code, message));
        }

        public static Result<TReturn> Failed(Exception ex)
        {
            return new Result<TReturn>(new ResultFailure(typeof(TReturn), ex));
        }
        public static Result<TReturn> Failed(Exception ex, string message, object callerInstance = null)
        {
            return new Result<TReturn>(new ResultFailure(typeof(TReturn), ex, message, callerInstance));
        }

        public static Result<TReturn> Failed(ResultFailure failureInfo)
        {
            return new Result<TReturn>(failureInfo);
        }

        public static Result<TReturn> Failed(ResultFailure failureInfo, object instance)
        {
            return new Result<TReturn>(failureInfo, instance);
        }

        public override string ToString()
        {
            if (IsSuccess)
                return ReturnValue.ToString();

            return FailureInfo.ToString();
        }

        public IEnumerator<Result<TReturn>> GetEnumerator()
        {
            yield return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return this;
        }
    }







}
