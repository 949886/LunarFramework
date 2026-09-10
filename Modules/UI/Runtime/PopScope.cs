using System;
using UnityEngine;

/// <summary>Local back-policy gate used by Navigator.MaybePop.</summary>
/// <remarks>
/// PopScope affects back-policy only; structural Navigator.Pop stays local and
/// does not consult scopes. Scope collection stops at descendant Navigator
/// boundaries so nested flows guard their own back requests.
/// </remarks>
public sealed class PopScope : MonoBehaviour
{
    [SerializeField] private bool _canPop = true;

    /// <summary>Whether this scope allows the current MaybePop request.</summary>
    public bool CanPop
    {
        get { return _canPop; }
        set { _canPop = value; }
    }

    /// <summary>
    /// Raised after MaybePop is evaluated in this scope's Navigator policy domain.
    /// </summary>
    /// <remarks>
    /// didPop is true when a route was accepted for pop anywhere in the delegated
    /// parent chain; result is the MaybePop result payload.
    /// </remarks>
    public event Action<bool, object> PopInvoked;

    internal void Notify(bool didPop, object result)
    {
        Action<bool, object> handler = PopInvoked;
        if (handler != null)
            handler(didPop, result);
    }
}
