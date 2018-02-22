# Railway Programming in C#.
<span>
Inspired by the Railway programming talk from <a href="https://vimeo.com/113707214">Scott Wlaschin</a> and the presentation from <a href="https://youtu.be/uM906cqdFWE ">Marcus Denny</a> I decided to create my own C# implementation, just for fun and to learn. 
</span>



This project contains two approaches how to handle a two track result: 
<span>`Result<TSuccess>` and </span>
<span>`Result2<TSuccess, TFailure>` </span>


## The Railway programming concept ##

A function normally returns either single result, a null, or throw's an exception. This requires if-not-null checks and try-catch blocks which can sometimes obfuscate the intention of the business logic. The idea of Railway programming is to create a Success track and a Failure track. 

 
     Success<T1> ---> f()---> T2 ---> f() ---> T3 ---> f()---> T4 
                                        \						
                                         \						
     Failure<F>  -----------> F  ------------> F  -----------> F 

This is done by wrapping a function's return value or into a Result monad. Result contains either a Success result or a Failure result. The Result monad let you chain operations on the Success track and express the bussiness logic as a sequences of steps. In case an error occurs, the flow continues on the Failure track basically bypassing the next operations in the flow. An example of updating the email addess of an existing Customer:

		 Result<bool> result = 
			ValidateEmail(newEmailAddress)
			.OnSuccess(_ => Repository.GetCustomer(customerId))
			.OnSuccess(result => customer = result)                                             
			.OnSuccess(result => oldEmail = customer.EmailAddress)                              
			.OnSuccess(_ => Repository.UpdateCustomer(customer))                     
			.OnSuccess(_ => customer.EmailAddress = newEmailAddress)
			.OnSuccess(_ => SendMailChangeVerification(newEmailAddress, customer))        
			.OnSuccess(_ => SendMailChangeVerification(oldEmail, customer)); 

In case of a Failure the OnSuccess delegates are not executed and skipped. Of course, in the end you need to handle failures, so the Result<T> will pass the error information along the chain of functions. 

This git project contains two implementations of the Result monad:

	Result<TSuccess>
	Result2<TSuccess, TFailure>
  
Both approaches have their pro's and cons. Obviously the **Result2** monad is flexible in the definition of the Failure result type, where the **Result** monad uses a predefined ResultFailure type. An example of using a specific failure type: `Result2<HttpStatusCode, HttpStatusCode>` where a Success result is a `HttpStatus.OK` and a Failure result is a `HttpStatus.NotFound`. However the **Result** monad handles exceptions automatically, optionally handles failure logging automatically, and can transform Null or a Boolean False result into a Failure result. It is not possible to transform an Exception into a generic TFailure type if no transformation function `F<Exception, TFailure>` is provided. Therefore **Result2** does not provide exception handling nor Null result handling. 

## Constructing Result / Result2 ## 
	public static Result<TSuccess> ToResult(TSuccess value)
	public static Result<TSuccess> ToResult(Result<TSuccess> value)
	public static Result<TSuccess> ToResult(Func<TSuccess> func)
	public static Result<TSuccess> ToResult<TSuccess>(this TSuccess value)

	public static Result<bool> FromBool(bool value)
	public static Result<bool> FromBool(Func<bool> func)
	public static Result<bool> FromBool(this bool value)

	public static Result<bool> Succeeded()
	public static Result<bool> Failed()
	public static Result<TSuccess> Failed(string message)
	public static Result<TSuccess> Failed(int code, string message = null)
	public static Result<TSuccess> Failed(Exception ex)
	public static Result<TSuccess> Failed(Exception ex, string message, object callerInstance = null)
	public static Result<TSuccess> Failed(ResultFailure failureInfo)
	public static Result<TSuccess> Failed(ResultFailure failureInfo, object instance)


	public static Result2<TSuccess, TFailure> Succeeded(TSuccess successResult)
	public static Result2<TSuccess, TFailure> Failed(TFailure failureResult)

The `FromBool()` function treats a False as Failure and a True as Success.
The `ToResult()` methods transform a Null result into a Null Failure. 
When using __Result__, all lambda functions are evaluated inside a try-catch block. Any exception is transformed automatically into a ResultFailure. 

## ResultFailure ##
The standard ResultFailure class contains a list of Errors; an (optional) message and error code. Or an Exception result. It also keeps track of the callstack and the TSuccess type. You can optionally provide an object containing information which caused the failure, for example the Customer object that could not be saved into the repository.

    public class ResultFailure
    {
		public ResultFailure()
		...
		
		public bool IsNull { get; }
		public Exception Ex { get; }
		public string StackTrace { get; private set; }
		public string CallStack { get; private set; }
		public Type ReturnType { get; }
		public object Object { get; set; }
		public List<Error> Errors { get; }
	}

	public class Error
	{
	   public long Code { get; }
	   public string Message { get; }
	}

You can optionally provide a thread safe logger that implements the `IResultFailureLogger`
interface. If a logger is provided, each time a ResultFailure instance is created, it will call the logger.

	public static class ResultLogger
	{
	   // Provide a thread safe global logger
	   public static IResultFailureLogger Logger { get; set; }
	}
	
# Chaining and Binding #

## OnSuccess & OnFailure ##
You can chain operations via de OnSuccess() or OnFailure() extension methods. The OnSuccess operations are only excuted on the Succes track, when Result.IsSuccess is true. The OnFailure operations are executed on the Failure track, when Result.IsFailure is true. Note that all OnSuccess and OnFailure extension methods return a __Result__ or __Result2__ instance where the TSuccess type might be changed from TIn to TOut. If a failure occurs, you change tracks; from Success to the Failure track. However, when on the Failure track, it is possible to correct failures and return a Success result. The flow will then switch back to the Success track. 

	public static Result<TOut> OnSuccess<TReturn, TOut>(this Result<TReturn> result,
			   Func<TReturn, TOut> evaluator)
	public static Result<TOut> OnSuccess<TReturn, TOut>(this Result<TReturn> result,
				  Func<TReturn, Result<TOut>> evaluator)
	public static Result<TReturn> OnSuccess<TReturn>(this Result<TReturn> result,
				Action<TReturn> action)

	public static Result<TReturn> OnFailure<TReturn>(this Result<TReturn> result,
			   Func<ResultFailure, TReturn> evaluator)
	public static Result<TReturn> OnFailure<TReturn>(this Result<TReturn> result,
				Func<ResultFailure, Result<TReturn>> evaluator)
	public static Result<TReturn> OnFailure<TReturn>(this Result<TReturn> result,
				Action<ResultFailure> evaluator)


				
	public static Result2<TOut, TFailure> OnSuccess<TSuccess, TFailure, TOut>(
				this Result2<TSuccess, TFailure> input,
				Func<TSuccess, TOut> func)
	public static Result2<TOut, TFailure> OnSuccess<TSuccess, TOut, TFailure>(
			   this Result2<TSuccess, TFailure> input,
			   Func<TSuccess, Result2<TOut, TFailure>> func)
	public static Result2<TSuccess, TFailure> OnSuccess<TSuccess, TFailure>(
				this Result2<TSuccess, TFailure> input,
				Action<TSuccess> action)

	public static Result2<TSuccess, TFailure> OnFailure<TSuccess, TFailure>(
			   this Result2<TSuccess, TFailure> input,
			   Func<TFailure, Result2<TSuccess, TFailure>> func)
	public static Result2<TSuccess, TFailure> OnFailure<TSuccess, TFailure>(
				this Result2<TSuccess, TFailure> input,
				Func<TFailure, TSuccess> func)
	public static Result2<TSuccess, TFailure> OnFailure<TSuccess, TFailure>(
				this Result2<TSuccess, TFailure> input,
				Action<TFailure> action)



## Null handling ##
A Null is not a valid return value and is treated as a failure. Nulls are not allowed. However, in some cases a Null result is signaling that something does not exist. In those cases you might want to handle Null values explicit For example: 

	return Repository.GetCustomerByName(name).OnNull( new Customer(name) );
	
will create a new Customer when the repository returns a Null result _(assuming Repository returns a __Result__ monad)_.

Handling Null results and using the IsNull() operation is only possible with the __Result__ monad. __Result2__ will throw.

	public static Result<TReturn> OnNull<TReturn>(this Result<TReturn> result,
				Func<ResultFailure, Result<TReturn>> evaluator)
	public static Result<TReturn> OnNull<TReturn>(this Result<TReturn> result,
				Func<ResultFailure, TReturn> evaluator)			

## Continue & If ##

The two Continue methods below are exactly the same as OnSuccess. Just another name. Its a matter of taste.

	public static Result<TOut> Continue<TReturn, TOut>(this Result<TReturn> result,
            Func<TReturn, Result<TOut>> onsucces)
	public static Result<TOut> Continue<TReturn, TOut>(this Result<TReturn> result,
            Func<TReturn, TOut> onsucces)
			
However the Continue operations listed below will accept two delegates, one for each track, and either execute the Success delegate or the Failure delegate. This is not the same as chaining an Onsuccess().OnFailure() operation. Lets have a look at the following example:

	Repository.GetCustomer(id).OnSuccess( doA() ).OnFailure( doB() );

If the Repository returns a Success result you expect that doA() is executed and doB() not. But this is not true because doA() can fail as well. In that case doB() is still executed due to the failure of doA() and not because of a failure of the Repository. In such a case you want to continue the flow with either doA() or doB(). And this can be achieved with a Continue and two delegates:

	Repository.GetCustomer(id)
		.Continue( customer => doA(),
		           error => doB() 
				 );

Continue continued:
				 
	public static Result<TOut> Continue<TReturn, TOut>(this Result<TReturn> result,
            Func<TReturn, Result<TOut>> onsucces,
            Func<ResultFailure, Result<TOut>> onfailure)
	public static Result<TOut> Continue<TReturn, TOut>(this Result<TReturn> result,
            Func<TReturn, TOut> onsucces,
            Func<ResultFailure, TOut> onfailure)
	public static Result<TOut> Continue<TReturn, TOut>(this Result<TReturn> result,
            Func<TReturn, TOut> onsucces,
            Action<ResultFailure> onfailure)
		
	public static Result2<TOut, TFailure> Continue<TSuccess, TOut, TFailure>(
            this Result2<TSuccess, TFailure> result,
            Func<TSuccess, Result2<TOut, TFailure>> onsucces,
            Func<TFailure, Result2<TOut, TFailure>> onfailure)
	public static Result2<TOut, TFailure> Continue<TSuccess, TOut, TFailure>(
            this Result2<TSuccess, TFailure> result,
            Func<TSuccess, TOut> onsucces,
            Action<TFailure> onfailure)
	
ContinueIf, executes the _then_ function if the predicate returns true. Else the Success result is returned.

	public static Result<TReturn> ContinueIf<TReturn>(
			this Result<TReturn> result,
            Func<TReturn, bool> predicate,
            Func<TReturn, Result<TReturn>> then)
			
	 public static Result2<TReturn, TFailure> ContinueIf<TReturn, TFailure>(
            this Result2<TReturn, TFailure> result,
            Func<TReturn, bool> predicate,
            Func<TReturn, Result2<TReturn, TFailure>> then)

And the classic If-Then-Else operation. If there is a Success result, either the _then_ or the _else_ operation is executed depending on the outcome of the predicate. Example:

	Repository.GetAccount( id )
		.If( account => account.Activated == true,
		     account => doThen( account ), 
			 account => doElse( account )
			);
			
a Then and Else expression must be supplied and both must return the same Result type.
			
	public static Result<TOut> If<TSuccess, TOut>(
            this Result<TSuccess> result,
            Func<TSuccess, bool> predicate,
            Func<TSuccess, Result<TOut>> thenFunc,
            Func<TSuccess, Result<TOut>> elseFunc )
			
	public static Result2<TOut, TFailure> If<TSuccess, TOut, TFailure>(
            this Result2<TSuccess, TFailure> input,
            Func<TSuccess, bool> predicate,
            Func<TSuccess, Result2<TOut, TFailure>> thenFunc,
            Func<TSuccess, Result2<TOut, TFailure>> elseFunc )
			
## Convert Failures ##

When using the __Result2__ monad, you might want to convert the TFailure output into a different TFailure type. Else, all functions on the Success track must return the same TFailure type. And this might not be always desired. For example when using libraries which you cannot change. Therefore there are two ConvertFailure methods that operate on the Failure track and transforms a TFailureIn into a TFailureOut:

 
     Success<T1> --------------> T1 
                                        						
                                        						
     Failure<F1> ------f()-----> F2 
	 

The methods:

	public static Result2<TSuccess, TFOut> ConvertFailure<TSuccess, TFOut, TFailure>(
            this Result2<TSuccess, TFailure> input,
            Func<TFailure, Result2<TSuccess, TFOut>> func)
    
    public static Result2<TSuccess, TFOut> ConvertFailure<TSuccess, TFOut, TFailure>(
            this Result2<TSuccess, TFailure> input,
            Func<TFailure, TFOut> func)
			

## Finally or Throw ## 

The Finally method will "unbox" the Success result and return a value of TSuccess. Of course this is only possible if there is a Success result. If there is no Success result you have two options, return a Null or Throw an `ResultException`.

	public static TResult FinallyOrNull<TResult>(this Result<TResult> result)
	public static TResult FinallyOrThrow<TResult>(this Result<TResult> result)
	
The Finally and Throw methods are only available for the __Result__ monad.
	
	public static Result<TResult> ThrowOnFailure<TResult>(this Result<TResult> result)
	public static Result<TResult> ThrowOnException<TResult>(this Result<TResult> result)

## Linq ##

Both the __Result__ and __Result2__ monads provide a SelectMany extension method which will allow the monad to be used with Linq. An example:

		var result =
			from product in GetProductResult(productId)
			from order in GetOrderResult(orderId)
			from customer in GetCustomerResult(order.CustomerId)
			from added in AddProductToCustomerOrder(order, customer, product)
			from ok in UpdateCustomerOrder(added, order)
			select ok;

		if (result.IsFailure)
			throw new ApplicationException(...);
				

SelectMany:

	public static Result<TOut> SelectMany<TReturnA, TReturnB, TOut>(
            this Result<TReturnA> self,
            Func<TReturnA, Result<TReturnB>> func,
            Func<TReturnA, TReturnB, TOut> select)
				
	public static Result2<TOut,TFailure> SelectMany<TReturnA, TReturnB, TOut, TFailure>(
            this Result2<TReturnA, TFailure> self,
            Func<TReturnA, Result2<TReturnB, TFailure>> func,
            Func<TReturnA, TReturnB, TOut> select)

				