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

What is wrong with this code? Actually nothing particularly wrong beside unnecessary if statements which we can get rid of in manner of few clicks. Let us see how this can be done so we could talk about why we should do that.

First thing we should do is to replace ```AlertStream``` method with two overloaded methods:

```csharp
public HttpResponseMessage AlertStream(HttpRequestMessage request, string team, DateTime? start)
```

```csharp
public HttpResponseMessage AlertStream(HttpRequestMessage request, string team)
```

Now we have two clear paths for ```AlertStream``` that does take start DateTime into account and other one that doesn't. We also have to do the same with ```OnAlertStreamAvailable``` method.

Complete solution would look like this:

```csharp
public HttpResponseMessage AlertStream(HttpRequestMessage request, string teamName, DateTime start)
{
    var mediaType = "text/event-stream";
    var response = new HttpResponseMessage(HttpStatusCode.OK)
    {
        Content = new PushStreamContent((stream, content, context) =>
        {
            var client = new TeamEventClient(teamName, stream);

            OnAlertStreamAvailable(client, teamName, start);
        }, mediaType)
    };
    response.Content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);

    return response;
}

public HttpResponseMessage AlertStream(HttpRequestMessage request, string teamName)
{
    var mediaType = "text/event-stream";
    var response = new HttpResponseMessage(HttpStatusCode.OK)
    {
        Content = new PushStreamContent((stream, content, context) =>
        {
            var client = new TeamEventClient(teamName, stream);
            OnAlertStreamAvailable(client);
        }, mediaType)
    };
    response.Content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);

    return response;
}

private void OnAlertStreamAvailable(TeamEventClient client, string teamName, DateTime start)
{
  var cachedItems = alertsStreamsService.GetCachedEvents(teamName, start.ToUniversalTime());

  client.SendCachedData(cachedItems).ContinueWith(x => { alertsStreamsService.TryAddClient(client); });
}

private void OnAlertStreamAvailable(TeamEventClient client)
{
    alertsStreamsService.TryAddClient(client);
}
```

We have done quite a lot here to make our code better. We renamed team to ```teamName``` to be it more concise. We removed unused parameters and we organized our code in two distinct path one for ```AlertStream``` that produces alerts form given ```DateTime``` and one that starts from now. But we can make this code better, we can get rid of unnecessary repetitions in public methods. In order to do that we have to create template method that can be used in ```AlertStream``` methods:

```csharp
private HttpResponseMessage SensorStream(IOnAlertStreamAvailable onAlertStreamAvailable, string teamName)
{
    var mediaType = "text/event-stream";
    var response = new HttpResponseMessage(HttpStatusCode.OK)
    {
        Content = new PushStreamContent((stream, content, context) =>
        {
            var client = new TeamEventClient(teamName, stream);
            onAlertStreamAvailable.Execute(client);
        }, mediaType)
    };
    response.Content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);

    return response;
}
```

As you can see we moved code used in two places to one method which executes implementation of IOnAlertStreamAvailable interface. We have two separate implementation in our example:

```csharp
public class OnAlertStreamAvailableFrom : IOnAlertStreamAvailable
{
    private readonly IAlertsStreamsService _alertStreamService;
    private readonly string _teamName;
    private readonly DateTime _start;

    public OnAlertStreamAvailableFrom(IAlertsStreamsService alertStreamService, string teamName, DateTime start)
    {
        _alertStreamService = alertStreamService;
        _teamName = teamName;
        _start = start;
    }

    public void Execute(TeamEventClient client)
    {
        var cachedItems = _alertStreamService.GetCachedEvents(_teamName, _start.ToUniversalTime());
        client.SendCachedData(cachedItems).ContinueWith(x => { _alertStreamService.TryAddClient(client); });
    }
}

public class OnAlertStreamAvailable : IOnAlertStreamAvailable
{
    private readonly IAlertsStreamsService _alertStreamService;

    public OnAlertStreamAvailable(IAlertsStreamsService alertStreamService)
    {
        _alertStreamService = alertStreamService;
    }

    public void Execute(TeamEventClient client)
    {
        _alertStreamService.TryAddClient(client);
    }
}
```

Now we can rewrite our public methods to use our new code as follows:

```csharp
public HttpResponseMessage AlertStream(HttpRequestMessage request, string teamName, DateTime start)
{
    return SensorStream(new OnAlertStreamAvailableFrom(alertsStreamsService, teamName, start), teamName);
}

public HttpResponseMessage AlertStream(HttpRequestMessage request, string teamName)
{
    return SensorStream(new OnAlertStreamAvailable(alertsStreamsService), teamName);
}
```

Using this simple technique you can avoid if statements so that your code could be more readable and easier to maintain. Hope this small example can help you on your war with ifs.

You can find full code sample here: [code](war_with_if.cs).

11.02.2018
