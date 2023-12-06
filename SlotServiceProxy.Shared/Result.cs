namespace SlotServiceProxy.Shared;

public class VerificationResult<TError>
{
    protected VerificationResult()
    {
    }

    private VerificationResult(TError error)
        => InternalError = error;

    public static VerificationResult<TError> Ok() => new();

    public static VerificationResult<TError> Failure(TError error) => new(error);

    protected TError InternalError { get; set; }

    public TError Problem => InternalError;

    public bool IsSuccess => IsError(Problem);

    public bool IsFailure => !IsSuccess;

    private static bool IsError(TError data) => EqualityComparer<TError>.Default.Equals(data, default);
}

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
    /// project values into new types within Result context
    /// </summary>
    /// <param name="data">data projection</param>
    /// <param name="error">error projection</param>
    /// <typeparam name="TR">new type of data</typeparam>
    /// <typeparam name="TL">new type of error</typeparam>
    /// <returns>updated Result</returns>
    public Result<TR, TL> BiMap<TR, TL>(Func<TData, TR> data, Func<TError, TL> error) => IsSuccess
        ? Result<TR, TL>.Ok(data(Data))
        : Result<TR, TL>.Failure(error(Problem));

    /// <summary>
    /// project values into new types within Result context
    /// </summary>
    /// <param name="data">data projection</param>
    /// <typeparam name="TR">new type of data</typeparam>
    /// <returns>updated Result</returns>
    public Result<TR, TError> Map<TR>(Func<TData, TR> map) => BiMap(map, e => e);

    /// <summary>
    /// perform action with side-effects in Result context
    /// </summary>
    /// <param name="data">action to perform for Result in Data state</param>
    /// <param name="error">action to perform for Result in Error state</param>
    public void Do(Action<TData> data, Action<TError> error)
    {
        if (IsSuccess) data(Data);
        else error(Problem);
    }

    /// <summary>
    /// simplify instantiation of context in Data state
    /// </summary>
    /// <param name="data">value to wrap</param>
    /// <returns></returns>
    public static implicit operator Result<TData, TError>(TData data) => Ok(data);

    /// <summary>
    /// simplify instantiation of context in Error state
    /// </summary>
    /// <param name="error">error to wrap</param>
    /// <returns></returns>
    public static implicit operator Result<TData, TError>(TError error) => Failure(error);

    /// <summary>
    /// project Data state to new type within Result context
    /// </summary>
    /// <param name="binder">projection returning context</param>
    /// <typeparam name="TR">new Data type</typeparam>
    /// <returns>Result context with updated Data type</returns>
    public Result<TR, TError> Bind<TR>(Func<TData, Result<TR, TError>> binder) =>
        IsSuccess ? binder(Data) : Result<TR, TError>.Failure(Problem);

    /// <summary>
    /// project Data state to new type within Result context
    /// </summary>
    /// <param name="binder">projection returning context</param>
    /// <typeparam name="TR">new Data type</typeparam>
    /// <typeparam name="TState">additional data which needed to perform binding</typeparam>
    /// <returns>Result context with updated Data type</returns>
    public Result<TR, TError> Bind<TR, TState>(Func<TData, TState, Result<TR, TError>> binder, TState state) =>
        IsSuccess ? binder(Data, state) : Result<TR, TError>.Failure(Problem);

    public async Task<Result<TR, TError>> BindAsync<TR>(Func<TData, Task<Result<TR, TError>>> binder) =>
        await (IsSuccess ? binder(Data) : Task.FromResult(Result<TR, TError>.Failure(Problem)));

    public async Task<Result<TR, TError>> BindAsync<TR, TState>(Func<TData, TState, Task<Result<TR, TError>>> binder, TState state) =>
        await (IsSuccess ? binder(Data, state) : Task.FromResult(Result<TR, TError>.Failure(Problem)));
}

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
