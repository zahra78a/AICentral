﻿using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace AICentral.PipelineComponents.Endpoints.OpenAILike.OpenAI;

public class OpenAIEndpointDispatcher : OpenAILikeEndpointDispatcher
{
    const string OpenAIV1 = "https://api.openai.com/v1";
    private readonly string? _organization;
    private readonly string _apiKey;

    public OpenAIEndpointDispatcher(string id,
        Dictionary<string, string> modelMappings,
        string apiKey, 
        string? organization): base(id, modelMappings)
    {
        _organization = organization;
        _apiKey = apiKey;
    }

    protected override HttpRequestMessage BuildRequest(
        HttpContext context, 
        AICallInformation aiCallInformation,
        string mappedModelName)
    {
        var newRequest = aiCallInformation.RequestContent.DeepClone();
        newRequest["model"] = mappedModelName;
        
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{OpenAIV1}/{aiCallInformation.RemainingUrl}"
        )
        {
            Content = new StringContent(newRequest.ToString(Formatting.None), Encoding.UTF8, "application/json"),
            Headers =
            {
                Authorization = new AuthenticationHeaderValue("Bearer", _apiKey)
            }
        };
        if (!string.IsNullOrWhiteSpace(_organization))
        {
            request.Headers.Add("OpenAI-Organization", _organization);
        }
        
        return request;
    }

    public override object WriteDebug()
    {
        return new
        {
            Type = "OpenAI",
            Url = OpenAIV1,
            Common = base.WriteDebug()
        };
    }

    protected override string HostUriBase => OpenAIV1;
}