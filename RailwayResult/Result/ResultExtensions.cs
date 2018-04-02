using System;
using System.Collections.Generic;
using System.Text;

namespace Railway.Result
{
    public static class ResultExtensions
    {
        public static Result<TReturn> ToResult<TReturn>(this TReturn value)
        {
            return Result<TReturn>.ToResult(value);
        }

        public static Result<bool> FromBool(this bool value)
        {
            return Result<bool>.FromBool(value);
        }

        #region Linq
        // SelectMany must have the following signature:
        //   Result<C> SelectMany<A, B, C>(this Result<A> a, Func<A, Result<B>> func, Func<A, B, C> select)
        public static Result<TOut> SelectMany<TReturnA, TReturnB, TOut>(
            this Result<TReturnA> self,
            Func<TReturnA, Result<TReturnB>> func,
            Func<TReturnA, TReturnB, TOut> select)
        {
            if (self.IsSuccess)
            {
                var result = func(self.ReturnValue);
                if (result.IsSuccess)
                    return select(self.ReturnValue, result.ReturnValue).ToResult();
                return Result<TOut>.Failed(result.FailureInfo, func);
            }
            return Result<TOut>.Failed(self.FailureInfo);
        }

        public static IEnumerable<Result<TReturn>> Where<TReturn>(
            this IEnumerable<Result<TReturn>> results,
            Func<TReturn, bool> predicate)
        {
            foreach (var result in results)
            {
                if (result.IsSuccess && predicate(result.ReturnValue))
                    yield return result;
            }
        }

        #endregion Linq
        #region OnFailure 
        //
        // The idea is to chain OnSuccess methods until the final result is returned.
        // OnFailure can be used to correct a failure, a rollback or undo for example, 
        // but should return a result that is compatible with OnSuccess.
        //
        // The signature of OnFailure is to return a Result<TReturn> always.
        // It is not possible to combine, chain, OnSuccess and OnFailure methods
        // that return a different TReturn types. And casting from TReturn input to TOut
        // output is almost never possible. Therefore the OnFailure methods are
        // restricted to return only types of TReturn.
        //
        public static Result<TReturn> OnFailure<TReturn>(this Result<TReturn> result,
            Func<ResultFailure, Result<TReturn>> evaluator)
        {
            try
            {
                if (result.IsFailure)
                {
                    var resultValue = evaluator(result.FailureInfo);
                    return resultValue;
                }
                return result;

            }
            catch (Exception ex)
            {
                return Result<TReturn>.Failed(ex, ex.Message, result);
            }
        }

        public static Result<TReturn> OnFailure<TReturn>(this Result<TReturn> result,
            Func<ResultFailure, TReturn> evaluator)
        {
            try
            {
                if (result.IsFailure)
                {
                    var resultValue = evaluator(result.FailureInfo);
                    return Result<TReturn>.ToResult(resultValue);
                }
                return result;

            }
            catch (Exception ex)
            {
                return Result<TReturn>.Failed(ex, ex.Message, result);
            }
        }

        public static Result<TReturn> OnFailure<TReturn>(this Result<TReturn> result,
            Action<ResultFailure> evaluator)
        {
            try
            {
                if (result.IsFailure)
                {
                    evaluator(result.FailureInfo);
                }
                return result;
            }
            catch (Exception ex)
            {
                return Result<TReturn>.Failed(ex, ex.Message, result);
            }
        }

        #endregion OnFailure
        #region OnException
        public static Result<TReturn> OnException<TReturn>(this Result<TReturn> result,
            Func<ResultFailure, Result<TReturn>> evaluator)
        {
            try
            {
                if (result.IsException)
                {
                    var resultValue = evaluator(result.FailureInfo);
                    return resultValue;
                }
                return result;

            }
            catch (Exception ex)
            {
                return Result<TReturn>.Failed(ex, ex.Message, result);
            }
        }
        public static Result<TReturn> OnException<TException, TReturn>(this Result<TReturn> result,
            Func<ResultFailure, Result<TReturn>> evaluator)
        {
            try
            {
                if (result.IsException && result.FailureInfo.Ex.GetType() == typeof(TException))
                {
                    var resultValue = evaluator(result.FailureInfo);
                    return resultValue;
                }
                return result;

            }
            catch (Exception ex)
            {
                return Result<TReturn>.Failed(ex, ex.Message, result);
            }
        }

        public static Result<TReturn> OnException<TReturn>(this Result<TReturn> result,
            Func<ResultFailure, TReturn> evaluator)
        {
            try
            {
                if (result.IsException)
                {
                    var resultValue = evaluator(result.FailureInfo);
                    return Result<TReturn>.ToResult(resultValue);
                }
                return result;

            }
            catch (Exception ex)
            {
                return Result<TReturn>.Failed(ex, ex.Message, result);
            }
        }

        public static Result<TReturn> OnException<TException, TReturn>(this Result<TReturn> result,
            Func<ResultFailure, TReturn> evaluator)
        {
            try
            {
                if (result.IsException && result.FailureInfo.Ex.GetType() == typeof(TException))
                {
                    var resultValue = evaluator(result.FailureInfo);
                    return Result<TReturn>.ToResult(resultValue);
                }
                return result;

            }
            catch (Exception ex)
            {
                return Result<TReturn>.Failed(ex, ex.Message, result);
            }
        }
        public static Result<TReturn> OnException<TReturn>(this Result<TReturn> result,
            Action<ResultFailure> evaluator)
        {
            try
            {
                if (result.IsException)
                {
                    evaluator(result.FailureInfo);
                }
                return result;

            }
            catch (Exception ex)
            {
                return Result<TReturn>.Failed(ex, ex.Message, result);
            }
        }

        public static Result<TReturn> OnException<TException, TReturn>(this Result<TReturn> result,
            Action<ResultFailure> evaluator)
        {
            try
            {
                if (result.IsException && result.FailureInfo.Ex.GetType() == typeof(TException))
                {
                    evaluator(result.FailureInfo);
                }
                return result;

            }
            catch (Exception ex)
            {
                return Result<TReturn>.Failed(ex, ex.Message, result);
            }
        }

        #endregion OnException
        #region OnSuccess

        public static Result<TOut> OnSuccess<TReturn, TOut>(this Result<TReturn> result,
              Func<TReturn, Result<TOut>> evaluator)
        {
            try
            {
                if (result.IsFailure)
                {
                    return Result<TOut>.Failed(result.FailureInfo);
                }
                var returnValue = evaluator(result.ReturnValue);
                return returnValue;
            }
            catch (Exception ex)
            {
                return Result<TOut>.Failed(ex, ex.Message, result);
            }

        }

        public static Result<TOut> OnSuccess<TReturn, TOut>(this Result<TReturn> result,
            Func<TReturn, TOut> evaluator)
        {
            try
            {
                if (result.IsFailure)
                {
                    return Result<TOut>.Failed(result.FailureInfo);
                }
                var returnValue = evaluator(result.ReturnValue);
                return Result<TOut>.ToResult(returnValue);
            }
            catch (Exception ex)
            {
                return Result<TOut>.Failed(ex, ex.Message, result);
            }
        }
        public static Result<TReturn> OnSuccess<TReturn>(this Result<TReturn> result,
            Action<TReturn> action)
        {
            try
            {
                if (result.IsSuccess)
                {
                    action(result.ReturnValue);
                }
                return result;
            }
            catch (Exception ex)
            {
                return Result<TReturn>.Failed(ex, ex.Message, result);
            }
        }


        #endregion OnSuccess
        #region Continue & If
        public static Result<TOut> Continue<TReturn, TOut>(this Result<TReturn> result,
            Func<TReturn, Result<TOut>> onsucces)
        {
            return result.OnSuccess(onsucces);
        }

        public static Result<TOut> Continue<TReturn, TOut>(this Result<TReturn> result,
            Func<TReturn, TOut> onsucces)
        {
            return result.OnSuccess(onsucces);
        }

        public static Result<TOut> Continue<TReturn, TOut>(this Result<TReturn> result,
            Func<TReturn, Result<TOut>> onsucces,
            Func<ResultFailure, Result<TOut>> onfailure)
        {
            try
            {
                if (result.IsSuccess && onsucces != null)
                    return result.OnSuccess(onsucces);

                if (onfailure != null)
                    return onfailure(result.FailureInfo);

                return Result<TOut>.Failed(result.FailureInfo);
            }
            catch (Exception ex)
            {
                return Result<TOut>.Failed(ex, ex.Message, result);
            }
        }

        public static Result<TOut> Continue<TReturn, TOut>(this Result<TReturn> result,
            Func<TReturn, TOut> onsucces,
            Func<ResultFailure, TOut> onfailure)
        {
            try
            {
                if (result.IsSuccess && onsucces != null)
                    return result.OnSuccess(onsucces);

                if (onfailure != null)
                    return Result<TOut>.ToResult(onfailure(result.FailureInfo));

                return Result<TOut>.Failed(result.FailureInfo);
            }
            catch (Exception ex)
            {
                return Result<TOut>.Failed(ex, ex.Message, result);
            }
        }

        public static Result<TOut> Continue<TReturn, TOut>(this Result<TReturn> result,
            Func<TReturn, TOut> onsucces,
            Action<ResultFailure> onfailure)
        {
            try
            {
                if (result.IsSuccess && onsucces != null)
                    return result.OnSuccess(onsucces);

                if (result.IsFailure && onfailure != null)
                    onfailure(result.FailureInfo);

                return Result<TOut>.Failed(result.FailureInfo);
            }
            catch (Exception ex)
            {
                return Result<TOut>.Failed(ex, ex.Message, result);
            }
        }

        public static Result<TReturn> ContinueIf<TReturn>(this Result<TReturn> result,
            Func<TReturn, bool> predicate,
            Func<TReturn, Result<TReturn>> then)
        {
            try
            {
                if (result.IsSuccess && predicate(result.ReturnValue))
                    return then(result.ReturnValue);

                return result;
            }
            catch (Exception ex)
            {
                return Result<TReturn>.Failed(ex, ex.Message, result);
            }
        }

        // If predicate is true, transforms Result<TSuccess> to Result<TOut>, else pass Failure info
        public static Result<TOut> If<TSuccess, TOut>(
            this Result<TSuccess> result,
            Func<TSuccess, bool> predicate,
            Func<TSuccess, Result<TOut>> thenFunc,
            Func<TSuccess, Result<TOut>> elseFunc
        )
        {
            try
            {
                if (result.IsFailure)
                {
                    return Result<TOut>.Failed(result.FailureInfo);
                }

                if (predicate(result.ReturnValue))
                    return thenFunc(result.ReturnValue);

                return elseFunc(result.ReturnValue);
            }
            catch (Exception ex)
            {
                return Result<TOut>.Failed(ex, ex.Message, result);
            }
        }
        #endregion Continue
        #region Ensure
        public static Result<TReturn> Ensure<TReturn>(
            this TReturn result)
        {
            if (result != null)
                return result.ToResult();

            return Result<TReturn>.NullFailure();
        }
        public static Result<TReturn> Ensure<TReturn>(
            this Result<TReturn> result,
            Func<TReturn, bool> predicate)
        {
            try
            {
                if (result.IsFailure)
                    return Result<TReturn>.Failed(result.FailureInfo);

                if (predicate(result.ReturnValue))
                    return result;

                return Result<TReturn>.Failed(-1, "Not Ensured");
            }
            catch (Exception ex)
            {
                return Result<TReturn>.Failed(ex, ex.Message, result);
            }
        }
        public static Result<TReturn> Ensure<TReturn>(
            this Result<TReturn> result,
            Func<TReturn, Result<bool>> predicate)
        {
            try
            {
                if (result.IsFailure)
                {
                    return Result<TReturn>.Failed(result.FailureInfo);
                }

                var resultValue = predicate(result.ReturnValue);
                if (resultValue.IsSuccess)
                {
                    return result;
                }

                return Result<TReturn>.Failed(resultValue.FailureInfo);
            }
            catch (Exception ex)
            {
                return Result<TReturn>.Failed(ex, ex.Message, result);
            }
        }


        public static Result<TReturn> Ensure<TReturn, T1>(
            this Result<TReturn> result,
            Result<T1> r1)
        {
            try
            {
                if (result.IsFailure)
                    return result;

                if (r1.IsFailure)
                    return Result<TReturn>.Failed(r1.FailureInfo);

                return result;
            }
            catch (Exception ex)
            {
                return Result<TReturn>.Failed(ex, ex.Message, result);
            }
        }
        public static Result<TReturn> Ensure<TReturn, T1, T2>(
            this Result<TReturn> result,
            Result<T1> r1,
            Result<T2> r2)
        {
            try
            {
                if (result.IsFailure)
                    return result;

                if (r1.IsFailure)
                    return Result<TReturn>.Failed(r1.FailureInfo);

                if (r2.IsFailure)
                    return Result<TReturn>.Failed(r2.FailureInfo);

                return result;
            }
            catch (Exception ex)
            {
                return Result<TReturn>.Failed(ex, ex.Message, result);
            }
        }

        public static Result<TReturn> Ensure<TReturn, T1, T2, T3>(
            this Result<TReturn> result,
            Result<T1> r1,
            Result<T2> r2,
            Result<T3> r3)
        {
            try
            {
                if (result.IsFailure)
                    return result;

                if (r1.IsFailure)
                    return Result<TReturn>.Failed(r1.FailureInfo);

                if (r2.IsFailure)
                    return Result<TReturn>.Failed(r2.FailureInfo);

                if (r3.IsFailure)
                    return Result<TReturn>.Failed(r3.FailureInfo);

                return result;
            }
            catch (Exception ex)
            {
                return Result<TReturn>.Failed(ex, ex.Message, result);
            }
        }
        #endregion
        #region OnNull
        public static Result<TReturn> OnNull<TReturn>(this Result<TReturn> result,
            Func<ResultFailure, Result<TReturn>> evaluator)
        {
            try
            {
                if (result.IsFailure && result.IsNull)
                {
                    var resultValue = evaluator(result.FailureInfo);
                    return resultValue;
                }
                return result;

            }
            catch (Exception ex)
            {
                return Result<TReturn>.Failed(ex, ex.Message, result);
            }
        }

        public static Result<TReturn> OnNull<TReturn>(this Result<TReturn> result,
            Func<ResultFailure, TReturn> evaluator)
        {
            try
            {
                if (result.IsFailure && result.IsNull)
                {
                    var resultValue = evaluator(result.FailureInfo);
                    return Result<TReturn>.ToResult(resultValue);
                }
                return result;

            }
            catch (Exception ex)
            {
                return Result<TReturn>.Failed(ex, ex.Message, result);
            }
        }
        #endregion OnNull
        #region Finally
        public static TResult FinallyOrNull<TResult>(this Result<TResult> result)
        {
            if (result.IsSuccess)
                return result.ReturnValue;

            return default(TResult);
        }

        public static TResult FinallyOrThrow<TResult>(this Result<TResult> result)
        {
            if (result.IsSuccess)
                return result.ReturnValue;

            throw new ResultException(result.FailureInfo);
        }

        public static Result<TResult> ThrowOnFailure<TResult>(this Result<TResult> result)
        {
            if (result.IsFailure)
                throw new ResultException(result.FailureInfo);

            return result;
        }

        public static Result<TResult> ThrowOnException<TResult>(this Result<TResult> result)
        {
            if (result.IsException)
                throw result.FailureInfo.Ex;

            return result;
        }
        #endregion Finally
        #region Logging
        public static Result<TReturn> LogFailure<TReturn>(this Result<TReturn> result)
        {
            try
            {
                if (result.IsFailure && ResultLogger.Logger != null)
                {
                    ResultLogger.Logger.LogFailure(result.FailureInfo);
                }
            }
            catch
            {
                // do not fail on failed logging
            }

            return result;
        }
        public static Result<TReturn> LogFailure<TReturn>(this Result<TReturn> result,
            string message)
        {
            try
            {
                if (result.IsFailure && ResultLogger.Logger != null)
                {
                    result.AddFailure(message);
                    ResultLogger.Logger.LogFailure(result.FailureInfo);
                }
            }
            catch
            {
                // do not fail on failed logging
            }

            return result;
        }
        #endregion
    }
}
