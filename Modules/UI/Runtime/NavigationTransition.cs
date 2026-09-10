using System.Threading.Tasks;
using UnityEngine;

/// <summary>Base ScriptableObject for Cherry route transitions.</summary>
/// <remarks>
/// Transition Tasks run inside Navigator's serialized operation transaction.
/// No later stack mutation begins until the current transition Task completes.
/// </remarks>
public abstract class NavigationTransition : ScriptableObject
{
    /// <summary>Runs after incoming activation but before incoming route commit.</summary>
    /// <param name="incoming">Prepared incoming page.</param>
    /// <param name="outgoing">Previously committed current page, or null initially.</param>
    public virtual Task PushAsync(
        NavigationPage incoming,
        NavigationPage outgoing)
    {
        return Task.CompletedTask;
    }

    /// <summary>Runs while outgoing is still mounted/committed.</summary>
    /// <param name="outgoing">Page leaving Navigator.</param>
    /// <param name="incoming">Underlying page to reveal, or null.</param>
    /// <remarks>Outgoing is removed only after this Task completes.</remarks>
    public virtual Task PopAsync(
        NavigationPage outgoing,
        NavigationPage incoming)
    {
        return Task.CompletedTask;
    }
}
