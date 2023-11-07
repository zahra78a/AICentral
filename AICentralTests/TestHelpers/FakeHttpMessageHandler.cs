﻿namespace AICentralTests.TestHelpers;

public class FakeHttpMessageHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.RequestUri!.AbsoluteUri.Equals("https://api.openai.com/v1/chat/completions"))
        {
            return Task.FromResult(AICentralFakeResponses.FakeChatCompletionsResponse());
        }

        if (request.RequestUri!.AbsoluteUri.Equals(
                $"https://{AICentralFakeResponses.Endpoint404}/openai/deployments/Model1/chat/completions?api-version=2023-05-15"))
        {
            return Task.FromResult(AICentralFakeResponses.NotFoundResponse());
        }

        if (request.RequestUri!.AbsoluteUri.Equals(
                $"https://{AICentralFakeResponses.Endpoint500}/openai/deployments/Model1/chat/completions?api-version=2023-05-15"))
        {
            return Task.FromResult(AICentralFakeResponses.InternalServerErrorResponse());
        }

        if (request.RequestUri!.AbsoluteUri.Equals(
                $"https://{AICentralFakeResponses.Endpoint200}/openai/deployments/Model1/chat/completions?api-version=2023-05-15"))
        {
            return Task.FromResult(AICentralFakeResponses.FakeChatCompletionsResponse());
        }

        if (request.RequestUri!.AbsoluteUri.Equals(
                $"https://{AICentralFakeResponses.Endpoint200}/openai/deployments/Model1/completions?api-version=2023-05-15"))
        {
            return Task.FromResult(AICentralFakeResponses.FakeCompletionsResponse());
        }


        if (request.RequestUri!.AbsoluteUri.Equals(
                $"https://{AICentralFakeResponses.Endpoint200Number2}/openai/deployments/Model1/chat/completions?api-version=2023-05-15"))
        {
            return Task.FromResult(AICentralFakeResponses.FakeChatCompletionsResponse());
        }

        if (request.RequestUri!.AbsoluteUri.Equals(
                $"https://{AICentralFakeResponses.Endpoint200}/openai/images/generations:submit?api-version=2023-09-01-preview"))
        {
            return Task.FromResult(AICentralFakeResponses.FakeAzureOpenAIImageResponse());
        }

        throw new NotSupportedException($"No fake response registered for {request.RequestUri.AbsoluteUri}");
    }
}