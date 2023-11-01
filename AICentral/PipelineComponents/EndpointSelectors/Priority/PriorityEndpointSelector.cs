﻿using AICentral.PipelineComponents.Endpoints;
using AICentral.PipelineComponents.EndpointSelectors.Random;

namespace AICentral.PipelineComponents.EndpointSelectors.Priority;

public class PriorityEndpointSelector : EndpointSelectorBase
{
    private readonly System.Random _rnd = new(Environment.TickCount);
    private readonly IAICentralEndpointDispatcher[] _prioritisedOpenAiEndpoints;
    private readonly IAICentralEndpointDispatcher[] _fallbackOpenAiEndpoints;

    public PriorityEndpointSelector(
        IAICentralEndpointDispatcher[] prioritisedOpenAiEndpoints,
        IAICentralEndpointDispatcher[] fallbackOpenAiEndpoints)
    {
        _prioritisedOpenAiEndpoints = prioritisedOpenAiEndpoints;
        _fallbackOpenAiEndpoints = fallbackOpenAiEndpoints;
    }

    public override async Task<AICentralResponse> Handle(HttpContext context, AICentralPipelineExecutor pipeline,
        CancellationToken cancellationToken)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<RandomEndpointSelectorBuilder>>();
        try
        {
            logger.LogDebug("Prioritised Endpoint selector handling request");
            return await Handle(context, pipeline, cancellationToken, _prioritisedOpenAiEndpoints, false);
        }
        catch (Exception)
        {
            try
            {
                logger.LogWarning("e, Prioritised Endpoint selector failed with primary. Trying fallback servers");
                return await Handle(context, pipeline, cancellationToken, _fallbackOpenAiEndpoints, true);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to handle request. Exhausted endpoints");
                throw new InvalidOperationException("No available Open AI hosts", e);
            }
        }
    }

    private async Task<AICentralResponse> Handle(
        HttpContext context,
        AICentralPipelineExecutor pipeline,
        CancellationToken cancellationToken,
        IAICentralEndpointDispatcher[] endpoints,
        bool isFallbackCollection)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<RandomEndpointSelectorBuilder>>();
        var toTry = endpoints.ToList();
        do
        {
            var chosen = toTry.ElementAt(_rnd.Next(0, toTry.Count));
            toTry.Remove(chosen);
            try
            {
                var responseMessage =
                    await chosen.Handle(context, pipeline,
                        cancellationToken); //awaiting to unwrap any Aggregate Exceptions
                return await HandleResponse(
                    logger,
                    context,
                    responseMessage.Item1,
                    responseMessage.Item2,
                    isFallbackCollection && !toTry.Any(),
                    cancellationToken);
            }
            catch (Exception e)
            {
                if (!toTry.Any())
                {
                    logger.LogError(e, "Failed to handle request. Exhausted endpoints");
                    throw new InvalidOperationException("No available Open AI hosts", e);
                }

                logger.LogWarning(e, "Failed to handle request. Trying another endpoint");
            }
        } while (toTry.Count > 0);

        throw new InvalidOperationException("Failed to satisfy request");
    }

    public override object WriteDebug()
    {
        return new
        {
            Type = "Priority Router",
            PrioritisedEndpoints = _prioritisedOpenAiEndpoints.Select(x => x.WriteDebug()),
            FallbackEndpoints = _fallbackOpenAiEndpoints.Select(x => x.WriteDebug()),
        };
    }
}