namespace SlotServiceProxy.Shared;

/// <summary>
/// Very simple extensions for providing some declarative syntax and techniques in C#.
/// Mostly inspired by F# and Elixir :).
/// </summary>
public static class DeclarativeExtensions
{
    /// <summary>
    /// Forward pipe operator (`|>` in F#).
    /// </summary> 
    public static R To<T, R>(this T x, Func<T, R> map) => map(x);

    /// <summary>
    /// Forward pipe operator (`|>` in F#) with the additional state A for two arguments function.
    /// </summary> 
    public static R To<T, S, R>(this T x, S state, Func<T, S, R> map) => map(x, state);

    /// <summary>
    /// Cast to the R type with the forward pipe operator (`|>` in F#).
    /// </summary> 
    public static R To<R>(this object x) => (R)x;

    /// <summary>
    /// Forward pipe operator (`|>` in F#) but with side effect propagating the original `x` value.
    /// </summary> 
    public static T Do<T>(this T x, Action<T> effect)
    {
        effect(x);
        return x;
    }
    
    public static async Task DoAsync<T>(this T x, Func<T,Task> effect)
    {
        await effect(x);
    }

    /// <summary>
    /// Forward pipe operator (`|>` in F#) but with side effect propagating the original `x` value and the state object.
    /// </summary> 
    public static T Do<T, S>(this T x, S state, Action<T, S> effect)
    {
        effect(x, state);
        return x;
    }

    /// <summary>Wraps single item in array.</summary>
    public static T[] AsArray<T>(this T one) => new[] { one };
}
