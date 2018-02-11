# War with ifs

Recently I was asked to review following code:


```csharp
public HttpResponseMessage AlertStream(HttpRequestMessage request, string team, DateTime? start = null)
{
    var mediaType = "text/event-stream";
    var response = new HttpResponseMessage(HttpStatusCode.OK)
    {
        Content = new PushStreamContent((a, b, c) =>
        {
            OnAlertStreamAvailable(a, b, c, team, start);
        }, mediaType)
    };
    response.Content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);

    return response;
}

private void OnAlertStreamAvailable(Stream stream, HttpContent content, TransportContext context, string teamName, DateTime? start = null)
{
    var client = new TeamEventClient(teamName, stream);

    DateTime? startDateTime = null;

    if (start != null)
    {
        startDateTime = start.Value.ToUniversalTime();
    }

    if (startDateTime != null)
    {
        var cachedItems = alertsStreamsService.GetCachedEvents(teamName, startDateTime.Value);
        client.SendCachedData(cachedItems).ContinueWith(x => { alertsStreamsService.TryAddClient(client); });
    }
    else
    {
        alertsStreamsService.TryAddClient(client);
    }
}
```

What is wrong with this code. Actually nothing particularly wrong beside unnecessary if statements which we can get rid of in manner of few clicks. Let us see how this can be done first so next we could talk about why we should do that.

First thing we should do is to replace ```AlertStream``` method with two overloaded methods:

```csharp
public HttpResponseMessage AlertStream(HttpRequestMessage request, string team, DateTime? start)
```

```csharp
public HttpResponseMessage AlertStream(HttpRequestMessage request, string team)
```

Now we have clear two paths for alert streams that do take start DateTime into account and other one that doesn't. We also have to do the same with ```OnAlertStreamAvailable``` method.

Complete solution would look like this:

```csharp

```
