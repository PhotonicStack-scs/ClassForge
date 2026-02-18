using System.Threading.Channels;
using ClassForge.Application.Interfaces;

namespace ClassForge.Infrastructure.Services;

public class TimetableGenerationQueue : ITimetableGenerationQueue
{
    private readonly Channel<TimetableGenerationRequest> _channel =
        Channel.CreateUnbounded<TimetableGenerationRequest>(new UnboundedChannelOptions { SingleReader = true });

    public ChannelReader<TimetableGenerationRequest> Reader => _channel.Reader;

    public async ValueTask EnqueueAsync(TimetableGenerationRequest request, CancellationToken cancellationToken = default)
    {
        await _channel.Writer.WriteAsync(request, cancellationToken);
    }
}
