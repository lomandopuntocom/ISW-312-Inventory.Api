using System;
using System.Collections.Concurrent;
using System.Threading.Channels;
using Inventory.Api.Domain.Entities;

namespace Inventory.Api.Infrastructure;

public sealed class RestockEventBroadcaster
{
    private readonly ConcurrentDictionary<Guid, (string CompanyCen, Channel<RestockEvent> Channel)> _subscribers = new();

    public void Broadcast(RestockEvent @event)
    {
        foreach (var sub in _subscribers.Values)
        {
            if (string.Equals(sub.CompanyCen, @event.CompanyCen, StringComparison.OrdinalIgnoreCase))
            {
                sub.Channel.Writer.TryWrite(@event);
            }
        }
    }

    public Guid Subscribe(string companyCen, Channel<RestockEvent> channel)
    {
        var id = Guid.NewGuid();
        _subscribers.TryAdd(id, (companyCen, channel));
        return id;
    }

    public void Unsubscribe(Guid id)
    {
        _subscribers.TryRemove(id, out _);
    }
}
