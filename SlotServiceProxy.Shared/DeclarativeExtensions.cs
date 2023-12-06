namespace SlotServiceProxy.Shared;

public static class DeclarativeExtensions
{
    /// <summary>
    /// Forward pipe operator (`|>` in F#)
    /// </summary> 
    public static R To<T, R>(this T x, Func<T, R> map) => map(x);

    /// <summary>
    /// Forward pipe operator (`|>` in F#) with the additional state A for two arguments function
    /// </summary> 
    public static R To<T, S, R>(this T x, S state, Func<T, S, R> map) => map(x, state);

    /// <summary>
    /// Cast to the R type with the forward pipe operator (`|>` in F#)
    /// </summary> 
    public static R To<R>(this object x) => (R)x;

    /// <summary>
    /// Forward pipe operator (`|>` in F#) but with side effect propagating the original `x` value
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
    /// Forward pipe operator (`|>` in F#) but with side effect propagating the original `x` value and the state object
    /// </summary> 
    public static T Do<T, S>(this T x, S state, Action<T, S> effect)
    {
        effect(x, state);
        return x;
    }
    
    /// <summary>Calls predicate on each item in <paramref name="source"/> array until predicate returns true,
    /// then method will return this item index, or if predicate returns false for each item, method will return -1.</summary>
    /// <typeparam name="T">Type of array items.</typeparam>
    /// <param name="source">Source array: if null or empty, then method will return -1.</param>
    /// <param name="predicate">Delegate to evaluate on each array item until delegate returns true.</param>
    /// <returns>Index of item for which predicate returns true, or -1 otherwise.</returns>
    public static int FindIndex<T>(this T[] source, Predicate<T> predicate) => Array.FindIndex(source, predicate);

    /// <summary>Looks up for item in source array equal to provided value, and returns its index, or -1 if not found.</summary>
    public static int IndexOf<T>(this T[] source, T value) => Array.IndexOf(source, value);

    /// <summary>Wraps item in array.</summary>
    public static T[] AsArray<T>(this T one) => new[] { one };
    
    public static void ForEach<T>(this T[] source, Action<T> action)
    {
        if (source is not { Length: > 0, })
            return;

        foreach (var item in source)
            action(item);
    }
}
