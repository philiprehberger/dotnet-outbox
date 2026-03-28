namespace Philiprehberger.Outbox;

/// <summary>
/// Defines priority levels for outbox messages.
/// Higher-priority messages are dispatched before lower-priority ones.
/// </summary>
public enum MessagePriority
{
    /// <summary>Low priority. Dispatched after all higher-priority messages.</summary>
    Low = 0,

    /// <summary>Normal priority. The default for new messages.</summary>
    Normal = 1,

    /// <summary>High priority. Dispatched before Normal and Low messages.</summary>
    High = 2,

    /// <summary>Critical priority. Dispatched before all other messages.</summary>
    Critical = 3
}
