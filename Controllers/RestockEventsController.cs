using System;
using System.Text.Json;
using System.Threading.Channels;
using Inventory.Api.Domain.Entities;
using Inventory.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("api/inventory/companies/{companyCen}/restock-events")]
public sealed class RestockEventsController(RestockEventBroadcaster broadcaster) : ControllerBase
{
    [HttpGet]
    public async Task Stream(string companyCen, CancellationToken ct)
    {
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");

        var clientChannel = Channel.CreateUnbounded<RestockEvent>();
        var subscriptionId = broadcaster.Subscribe(companyCen, clientChannel);

        try
        {
            while (!ct.IsCancellationRequested)
            {
                using var delayTokenSource = CancellationTokenSource.CreateLinkedTokenSource(ct);
                var delayTask = Task.Delay(15000, delayTokenSource.Token);
                var readTask = clientChannel.Reader.WaitToReadAsync(ct).AsTask();

                var completedTask = await Task.WhenAny(readTask, delayTask);
                if (completedTask == readTask)
                {
                    delayTokenSource.Cancel();
                    if (await readTask)
                    {
                        while (clientChannel.Reader.TryRead(out var evento))
                        {
                            var json = JsonSerializer.Serialize(evento, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                            await Response.WriteAsync($"data: {json}\n\n", ct);
                            await Response.Body.FlushAsync(ct);
                        }
                    }
                }
                else
                {
                    await Response.WriteAsync($": keep-alive\n\n", ct);
                    await Response.Body.FlushAsync(ct);
                }
            }
        }
        finally
        {
            broadcaster.Unsubscribe(subscriptionId);
        }
    }
}
