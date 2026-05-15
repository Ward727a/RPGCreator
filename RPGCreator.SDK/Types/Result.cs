// RPG Creator - Open-source RPG Engine.
// (c) 2026 Ward
// 
// This file is part of RPG Creator and is distributed under the Apache 2.0 License.
// You are free to use, modify, and distribute this file under the terms of the Apache 2.0 License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence Apache 2.0.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence Apache 2.0.
// Voir LICENSE pour plus de détails.
// 
// Contact:
// => Mail: Ward727a@gmail.com
//    Please use this object: "RPG Creator [YourObject]"
// => Discord: ward727
// 
// For urgent inquiries, sending both an email and a message on Discord is highly recommended for a quicker response.

namespace RPGCreator.SDK.Types;

public readonly struct Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    
    public string Error { get; }

    public static Result Empty => new Result(false, "Result.Empty PROVIDED!!! This should NEVER happen!");
    
    private Result(bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Alias for Success.
    /// </summary>
    /// <returns>
    /// A new <see cref="Result"/> object representing a successful operation.
    /// </returns>   
    public static Result Ok() => Success();
    
    /// <summary>
    /// Creates a new <see cref="Result"/> object representing a successful operation.
    /// </summary>
    /// <returns>
    /// A new <see cref="Result"/> object representing a successful operation.
    /// </returns>  
    public static Result Success() => new(true, "");
    
    /// <summary>
    /// Alias for Failure.
    /// </summary>
    /// <param name="message">Message to associate with the failure result.</param>
    /// <returns>
    /// A new <see cref="Result"/> object representing a failure with the specified error message.
    /// </returns>
    public static Result Fail(string message = "No error message provided.") => Failure(message);
    
    /// <summary>
    /// Creates a new <see cref="Result"/> object representing a failure with the specified error message.
    /// </summary>
    /// <param name="message">Message to associate with the failure result.</param>   
    /// <returns>
    /// A new <see cref="Result"/> object representing a failure with the specified error message.
    /// </returns>
    public static Result Failure(string message = "No error message provided.") => new(false, message);

    public override string ToString()
    {
        return IsSuccess ? $"Success" : $"Failure: {Error}";
    }

    public Result OnFailure(Action<string> onFailure)
    {
        if (IsFailure) onFailure(Error);
        return this;
    }
    
    public Result OnSuccess(Action onSuccess)
    {
        if (IsSuccess) onSuccess();
        return this;
    }
    
    public void Match(Action onSuccess, Action<string> onFailure)
    {
        if (IsSuccess) onSuccess();
        else onFailure(Error);
    }

    public void Finally(Action onFinally)
    {
        onFinally();
    }
};

public readonly struct Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    
    public string Error { get; }
    public T Value => IsSuccess ? field! : throw new InvalidOperationException("Cannot access value of a failed result.");
    
    public static Result<T> Empty => new Result<T>(false, "Result.Empty PROVIDED!!! This should NEVER happen!", default);
    
    private Result(bool isSuccess, string error, T? value)
    {
        IsSuccess = isSuccess;
        Error = error;
        Value = value;
    }
    
    /// <summary>
    /// Alias for Success.
    /// </summary>
    /// <param name="value">The value to return.</param>  
    /// <returns>
    /// A new <see cref="Result"/> object representing a successful operation with the specified value.
    /// </returns>
    /// <typeparam name="T">The type of the value to return.</typeparam>
    public static Result<T> Ok(T value) => Success(value);
    
    /// <summary>
    /// Creates a new <see cref="Result"/> object representing a successful operation with the specified value.
    /// </summary>
    /// <param name="value">The value to return.</param>
    /// <returns>
    /// A new <see cref="Result"/> object representing a successful operation with the specified value.
    /// </returns>
    /// <typeparam name="T">The type of the value to return.</typeparam>
    public static Result<T> Success(T value) => new(true, "", value);
    
    /// <summary>
    /// Alias for Failure.
    /// </summary>
    /// <param name="message">Message to associate with the failure result.</param>
    /// <returns>
    /// A new <see cref="Result"/> object representing a failure with the specified error message.
    /// </returns>
    /// <typeparam name="T">The type of the value to return.</typeparam>
    public static Result<T> Fail(string message = "No error message provided.") => Failure(message);
    
    /// <summary>
    /// Creates a new <see cref="Result"/> object representing a failure with the specified error message.
    /// </summary>
    /// <param name="message">Message to associate with the failure result.</param>
    /// <returns>
    /// A new <see cref="Result"/> object representing a failure with the specified error message.
    /// </returns>
    /// <typeparam name="T">The type of the value to return.</typeparam>
    public static Result<T> Failure(string message = "No error message provided.") => new(false, message, default);
    
    public static implicit operator Result<T>(Result result) => result.IsSuccess ? Failure("You can't return a result directly without specifying a value while awaiting a Result<T>!") : Failure(result.Error);
    public static implicit operator Result<T>(T value) => Success(value);
    
    public override string ToString()
    {
        return IsSuccess ? $"Success: {Value}" : $"Failure: {Error}";
    }
    
    public Result<T> OnFailure(Action<string> onFailure)
    {
        if (IsFailure) onFailure(Error);
        return this;
    }
    
    public Result<T> OnSuccess(Action<T> onSuccess)
    {
        if (IsSuccess) onSuccess(Value);
        return this;
    }
    
    public T? Match(Func<T, T> onSuccess, Func<string, T?> onFailure) => IsSuccess ? onSuccess(Value) : onFailure(Error);
    public U? Match<U>(Func<T, U> onSuccess, Func<string, U?> onFailure) => IsSuccess ? onSuccess(Value) : onFailure(Error);
    
    public void Finally(Action onFinally)
    {
        onFinally();
    }
    
    /// <summary>
    /// Allow to 'extract' a value from a <see cref="Result{T}"/> object, and return a new <see cref="Result{U}"/> object.<br/>
    /// In the case of a failure on the original <see cref="Result{T}"/>, the new <see cref="Result{U}"/> will be a failure with the same error message.
    /// </summary>
    /// <param name="mapper">The function to apply to the value of the <see cref="Result{T}"/> object.</param> 
    /// <typeparam name="U">The type of the value to return.</typeparam>
    /// <returns>
    /// A new <see cref="Result{U}"/> object representing the result of applying the function to the value of the original <see cref="Result{T}"/> object.<br/>
    /// Or a failure with the same error message if the original <see cref="Result{T}"/> object was a failure.
    /// </returns>
    public Result<U> Map<U>(Func<T, U> mapper)
    {
        return IsSuccess ? Result<U>.Success(mapper(Value)) : Result<U>.Failure(Error);
    }

    /// <summary>
    /// Allows to bind a function to a <see cref="Result{T}"/> object, and return a new <see cref="Result{U}"/> object.<br/>
    /// This can be used to chain method that return a <see cref="Result{T}"/> object.<br/>
    /// For example:
    /// <code>
    /// <![CDATA[
    /// public Result<int> FailedMethod(){ return Result<int>.Failure("Failed"); };
    /// public Result<int> SomeMethod(){ return Result<int>.Ok(10); };
    /// public Result<int> ResultAddFive(int originValue){ return Result<int>.Ok(originValue + 5); };
    /// public Result<string> ResultToString(int originValue){ return Result<string>.Ok($"Success: {originValue}"); };
    ///     
    /// // [...]
    ///     
    /// Result<int> result = SomeMethod(); // Here, result is a Result<int> object with a value of 10.
    ///     
    /// // Here, we first bind the function ResultToString to the result of 'SomeMethod'.
    /// Result<int> stringResult1 = result.Bind(ResultToString); // Here, stringResult1 is a Result<string> object with a value of "Success: 10".
    ///     
    /// // Then we bind the function ResultAddFive to the result of 'SomeMethod'.
    /// Result<int> result2 = result.Bind(ResultAddFive); // Here, result2 is a Result<int> object with a value of 15.
    ///  
    /// // Finally, we bind the function ResultToString to the result of 'SomeOtherMethod'.
    /// Result<string> result3 = result2.Bind(ResultToString); // Here, result3 is a Result<string> object with a value of "Success: 15".
    ///  
    /// // And if we have an error:
    /// Result<int> result4 = FailedMethod().Bind(ResultToString); // Here, result4 is a Result<string> object with a value of "Failed".
    /// ]]>
    /// </code>
    /// </summary>
    /// <param name="binder">The function to apply to the value of the <see cref="Result{T}"/> object.</param>
    /// <typeparam name="U">The type of the result of the function to apply.</typeparam>
    /// <returns>
    /// A new <see cref="Result{U}"/> object representing the result of applying the function to the value of the original <see cref="Result{T}"/> object.<br/>
    /// Or a failure with the same error message if the original <see cref="Result{T}"/> object was a failure.
    /// </returns> 
    public Result<U> Bind<U>(Func<T, Result<U>> binder)
    {
        return IsSuccess ? binder(Value) : Result<U>.Failure(Error);
    }

    public Result Bind(Func<T, Result> binder)
    {
        return IsSuccess ? binder(Value) : Result.Failure(Error);
    }
}