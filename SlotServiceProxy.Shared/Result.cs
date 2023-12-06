namespace SlotServiceProxy.Shared;

/// <summary>
/// Simple model which represents result of operation without any additional data.
/// </summary>
/// <typeparam name="TError">Type which expected when unsuccessful flow occured. Usually must contain detail info
/// about unsuccessful flow.</typeparam>
public class VerificationResult<TError>
{
    protected VerificationResult()
    {
    }

    private VerificationResult(TError error)
        => InternalError = error;

    public static VerificationResult<TError> Ok() => new();

    public static VerificationResult<TError> Failure(TError error) => new(error);

    protected TError InternalError { get; set; } = default!;

    public TError Problem => InternalError;

    public bool IsSuccess => IsError(Problem);

    public bool IsFailure => !IsSuccess;

    private static bool IsError(TError data) => EqualityComparer<TError>.Default.Equals(data, default);
}

/// <summary>
/// Represent result of operation with additional data, in case of successful flow.
/// </summary>
/// <typeparam name="TData">Result of operation which can be used by initiator of flow (and usually expected).</typeparam>
/// <typeparam name="TError">Type which expected when unsuccessful flow occured. Usually must contain detail info
/// about unsuccessful flow.</typeparam>
public class Result<TData, TError> : VerificationResult<TError>
{
    public Result(TData data, TError error)
    {
        Data = data;
        InternalError = error;
    }

    public static Result<TData, TError> Ok(TData data) => new(data, default);

    public static Result<TData, TError> Failure(TError error) => new(default, error);
    
    public TData Data { get; }

    /// <summary>
    /// Project values into new types within Result context.
    /// </summary>
    /// <param name="data">Data projection.</param>
    /// <param name="error">Error projection.</param>
    /// <typeparam name="TR">New type of data.</typeparam>
    /// <typeparam name="TL">New type of error.</typeparam>
    public Result<TR, TL> BiMap<TR, TL>(Func<TData, TR> data, Func<TError, TL> error) => IsSuccess
        ? Result<TR, TL>.Ok(data(Data))
        : Result<TR, TL>.Failure(error(Problem));

    /// <summary>
    /// Project values into new types within Result context.
    /// </summary>
    /// <param name="data">Data projection.</param>
    /// <typeparam name="TR">New type of data.</typeparam>
    public Result<TR, TError> Map<TR>(Func<TData, TR> map) => BiMap(map, e => e);

    /// <summary>
    /// Perform action with side-effects in Result context, without returning any value.
    /// </summary>
    /// <param name="data">Action to perform for Result in Data state.</param>
    /// <param name="error">Action to perform for Result in Error state.</param>
    public void Do(Action<TData> data, Action<TError> error)
    {
        if (IsSuccess) data(Data);
        else error(Problem);
    }

    /// <summary>
    /// Simplify instantiation of context in Data state.
    /// </summary>
    /// <param name="data">Value to wrap.</param>
    public static implicit operator Result<TData, TError>(TData data) => Ok(data);

    /// <summary>
    /// Simplify instantiation of context in Error state.
    /// </summary>
    /// <param name="error">Error to wrap</param>
    public static implicit operator Result<TData, TError>(TError error) => Failure(error);

    /// <summary>
    /// Project Data state to new type within Result context.
    /// </summary>
    /// <param name="binder">Projection returning context.</param>
    /// <typeparam name="TR">New Data type.</typeparam>
    /// <returns>Result context with updated Data type.</returns>
    public Result<TR, TError> Bind<TR>(Func<TData, Result<TR, TError>> binder) =>
        IsSuccess ? binder(Data) : Result<TR, TError>.Failure(Problem);

    /// <summary>
    /// Project Data state to new type within Result context
    /// </summary>
    /// <param name="binder">Projection returning context.</param>
    /// <typeparam name="TR">New Data type.</typeparam>
    /// <typeparam name="TState">Additional data which needed to perform binding (very primitive currying)</typeparam>
    /// <returns>Result context with updated Data type</returns>
    public Result<TR, TError> Bind<TR, TState>(Func<TData, TState, Result<TR, TError>> binder, TState state) =>
        IsSuccess ? binder(Data, state) : Result<TR, TError>.Failure(Problem);

    public async Task<Result<TR, TError>> BindAsync<TR>(Func<TData, Task<Result<TR, TError>>> binder) =>
        await (IsSuccess ? binder(Data) : Task.FromResult(Result<TR, TError>.Failure(Problem)));

    public async Task<Result<TR, TError>> BindAsync<TR, TState>(Func<TData, TState, Task<Result<TR, TError>>> binder, TState state) =>
        await (IsSuccess ? binder(Data, state) : Task.FromResult(Result<TR, TError>.Failure(Problem)));
}

/// <summary>
/// Useful extensions for Result type.
/// </summary>
public static class Result
{
    public static Result<TData, TError> OnSuccess<TData, TError>(this Result<TData, TError> source, Action<TData> action)
    {
        if (source.IsSuccess) action(source.Data);
        return source;
    }

    public static Result<TData, TError> OnSuccess<TData, TState, TError>(this Result<TData, TError> source, TState state, Action<TData, TState> action)
    {
        if (source.IsSuccess) action(source.Data, state);
        return source;
    }

    public static Result<TData, TError> OnError<TData, TError>(this Result<TData, TError> source, Action<TError> action)
    {
        if (source.IsFailure) action(source.Problem);
        return source;
    }

    public static Result<TData, TError> OnError<TData, TState, TError>(this Result<TData, TError> source, TState state, Action<TError, TState> action)
    {
        if (source.IsFailure) action(source.Problem, state);
        return source;
    }

    public static async Task<Result<TData, TError>> OnSuccessAsync<TData, TError>(this Result<TData, TError> source, Func<TData, Task> action)
    {
        if (source.IsSuccess) await action(source.Data);
        return source;
    }

    public static async Task<Result<TData, TError>> OnSuccessAsync<TData, TState, TError>(this Result<TData, TError> source, TState state, Func<TData, TState, Task> action)
    {
        if (source.IsSuccess) await action(source.Data, state);
        return source;
    }

    public static async Task<Result<TData, TError>> FinallyAsync<TData, TError>(this Result<TData, TError> source, Task task)
    {
        await task;
        return source;
    }

    public static async Task<Result<TData, TError>> OnErrorAsync<TData, TError>(this Result<TData, TError> source, Func<Task> action)
    {
        if (source.IsFailure) await action();
        return source;
    }
}
