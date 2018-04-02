using System;
using System.Collections.Generic;
using System.Text;
using Railway.Result;

namespace Railway.Result2
{

    public static class Result2Extensions
    {
        #region OnSuccess

        // Map; map a TSuccess result to a TOut result.
        public static Result2<TOut, TFailure> OnSuccess<TSuccess, TFailure, TOut>(
            this Result2<TSuccess, TFailure> input,
            Func<TSuccess, TOut> func)
        {
            return input.IsSuccess
                ? Result2<TOut, TFailure>.Succeeded(func(input.SuccessResult))
                : Result2<TOut, TFailure>.Failed(input.FailureResult);
        }

        // Bind: transforms a TSuccess result into a TOut result.
        public static Result2<TOut, TFailure> OnSuccess<TSuccess, TOut, TFailure>(
            this Result2<TSuccess, TFailure> input,
            Func<TSuccess, Result2<TOut, TFailure>> func)
        {
            return input.IsSuccess
                ? func(input.SuccessResult)
                : Result2<TOut, TFailure>.Failed(input.FailureResult);
        }

        // Performs a void operation on a success result or passes the failure through.
        public static Result2<TSuccess, TFailure> OnSuccess<TSuccess, TFailure>(
            this Result2<TSuccess, TFailure> input,
            Action<TSuccess> action)
        {
            if (input.IsSuccess)
            {
                action(input.SuccessResult);  //note: action can throw, and this does not result into a Failure
            }
            return input;
        }

        #endregion
        #region OnFailure

        // Performs a void operation on a failure result or passes success through.
        public static Result2<TSuccess, TFailure> OnFailure<TSuccess, TFailure>(
            this Result2<TSuccess, TFailure> input,
            Action<TFailure> action)
        {
            if (input.IsFailure)
            {
                action(input.FailureResult);  //note: action can throw, and this does not result into a Failure
            }
            return input;
        }

        // Map: maps a failure result into a success result (correct a failure)
        public static Result2<TSuccess, TFailure> OnFailure<TSuccess, TFailure>(
            this Result2<TSuccess, TFailure> input,
            Func<TFailure, TSuccess> func)
        {
            return input.IsFailure
                ? Result2<TSuccess, TFailure>.Succeeded(func(input.FailureResult))
                : input;
        }


        // Bind: transforms a failure result into a success result (correct a failure)
        public static Result2<TSuccess, TFailure> OnFailure<TSuccess, TFailure>(
            this Result2<TSuccess, TFailure> input,
            Func<TFailure, Result2<TSuccess, TFailure>> func)
        {
            return input.IsFailure
                ? func(input.FailureResult)
                : input;
        }

        // Bind: transforms a TFailure result into a new failure result TFOut.
        public static Result2<TSuccess, TFOut> ConvertFailure<TSuccess, TFOut, TFailure>(
            this Result2<TSuccess, TFailure> input,
            Func<TFailure, Result2<TSuccess, TFOut>> func)
        {
            return input.IsFailure
                ? func(input.FailureResult)
                : Result2<TSuccess, TFOut>.Succeeded(input.SuccessResult);
        }

        // Map: transforms a TFailure result into a new failure result TFOut.
        public static Result2<TSuccess, TFOut> ConvertFailure<TSuccess, TFOut, TFailure>(
            this Result2<TSuccess, TFailure> input,
            Func<TFailure, TFOut> func)
        {
            return input.IsFailure
                ? Result2<TSuccess, TFOut>.Failed(func(input.FailureResult))
                : Result2<TSuccess, TFOut>.Succeeded(input.SuccessResult);
        }

        #endregion
        #region If    

        public static Result2<TOut, TFailure> Continue<TSuccess, TOut, TFailure>(
            this Result2<TSuccess, TFailure> result,
            Func<TSuccess, Result2<TOut, TFailure>> onsucces,
            Func<TFailure, Result2<TOut, TFailure>> onfailure)
        {
            if (onsucces == null)
                throw new ArgumentNullException(nameof(onsucces));
            if (onfailure == null)
                throw new ArgumentNullException(nameof(onfailure));

            if (result.IsSuccess)
                return result.OnSuccess(onsucces);

            return onfailure(result.FailureResult);
        }

        public static Result2<TOut, TFailure> Continue<TSuccess, TOut, TFailure>(
            this Result2<TSuccess, TFailure> result,
            Func<TSuccess, TOut> onsucces,
            Action<TFailure> onfailure)
        {
            if (onsucces == null)
                throw new ArgumentNullException(nameof(onsucces));
            if (onfailure == null)
                throw new ArgumentNullException(nameof(onfailure));

            if (result.IsSuccess)
                return result.OnSuccess(onsucces);

            onfailure(result.FailureResult);
            return Result2<TOut, TFailure>.Failed(result.FailureResult);
        }

        public static Result2<TReturn, TFailure> ContinueIf<TReturn, TFailure>(
            this Result2<TReturn, TFailure> result,
            Func<TReturn, bool> predicate,
            Func<TReturn, Result2<TReturn, TFailure>> then)
        {
            if (result.IsSuccess && predicate(result.SuccessResult))
                return then(result.SuccessResult);

            return result;
        }

        // If predicate is true, transforms Success<T1> to Success<T2>, else pass failure F
        public static Result2<TOut, TFailure> If<TSuccess, TOut, TFailure>(
            this Result2<TSuccess, TFailure> input,
            Func<TSuccess, bool> predicate,
            Func<TSuccess, Result2<TOut, TFailure>> thenFunc,
            Func<TSuccess, Result2<TOut, TFailure>> elseFunc
            )
        {
            if (input.IsFailure)
            {
                return Result2<TOut, TFailure>.Failed(input.FailureResult);
            }

            if (predicate(input.SuccessResult))
                return thenFunc(input.SuccessResult);

            return elseFunc(input.SuccessResult);
        }
        #endregion
        #region Linq
        // SelectMany must have the following signature:
        //   Result<C> SelectMany<A, B, C>(this Result<A> a, Func<A, Result<B>> func, Func<A, B, C> select)
        public static Result2<TOut, TFailure> SelectMany<TReturnA, TReturnB, TOut, TFailure>(
            this Result2<TReturnA, TFailure> self,
            Func<TReturnA, Result2<TReturnB, TFailure>> func,
            Func<TReturnA, TReturnB, TOut> select)
        {
            if (self.IsSuccess)
            {
                var result = func(self.SuccessResult);
                if (result.IsSuccess)
                    return Result2<TOut, TFailure>.Succeeded(select(self.SuccessResult, result.SuccessResult));
                return Result2<TOut, TFailure>.Failed(result.FailureResult);
            }
            return Result2<TOut, TFailure>.Failed(self.FailureResult);
        }

        public static IEnumerable<Result2<TReturn, TFailure>> Where<TReturn, TFailure>(
            this IEnumerable<Result2<TReturn, TFailure>> results,
            Func<TReturn, bool> predicate)
        {
            foreach (var result in results)
            {
                if (result.IsSuccess && predicate(result.SuccessResult))
                    yield return result;
            }
        }

        #endregion Linq
        #region Ensure
        public static Result2<TSuccess, TFailure> Ensure<TSuccess, TFailure>(
            this TSuccess result)
        {
            if (result != null)
                return Result2<TSuccess, TFailure>.Succeeded(result);

            return Result2<TSuccess, TFailure>.Failed(default(TFailure));
        }
        
        public static Result2<TSuccess, TFailure> Ensure<TSuccess, T1, TFailure>(
            this Result2<TSuccess, TFailure> result,
            Result2<T1, TFailure> r1)
        {
                if (result.IsFailure)
                    return result;

                if (r1.IsFailure)
                    return Result2<TSuccess,TFailure>.Failed(r1.FailureResult);

                return result;
        }

        public static Result2<TSuccess, TFailure> Ensure<TSuccess, T1, T2, TFailure>(
            this Result2<TSuccess, TFailure> result,
            Result2<T1, TFailure> r1,
            Result2<T1, TFailure> r2)
        {
            if (result.IsFailure)
                return result;

            if (r1.IsFailure)
                return Result2<TSuccess, TFailure>.Failed(r1.FailureResult);

            if (r2.IsFailure)
                return Result2<TSuccess, TFailure>.Failed(r2.FailureResult);

            return result;
        }

        public static Result2<TSuccess, TFailure> Ensure<TSuccess, T1, T2, TFailure>(
            this Result2<TSuccess, TFailure> result,
            Result2<T1, TFailure> r1,
            Result2<T1, TFailure> r2,
            Result2<T1, TFailure> r3)
        {
            if (result.IsFailure)
                return result;

            if (r1.IsFailure)
                return Result2<TSuccess, TFailure>.Failed(r1.FailureResult);

            if (r2.IsFailure)
                return Result2<TSuccess, TFailure>.Failed(r2.FailureResult);

            if (r3.IsFailure)
                return Result2<TSuccess, TFailure>.Failed(r3.FailureResult);

            return result;
        }
        #endregion
    }
}
