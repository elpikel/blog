public HttpResponseMessage AlertStream(HttpRequestMessage request, string team, DateTime start)
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

public HttpResponseMessage AlertStream(HttpRequestMessage request, string team)
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

private void OnAlertStreamAvailable(Stream stream, HttpContent content, TransportContext context, string teamName, DateTime start)
{
  var client = new TeamEventClient(teamName, stream);

  DateTime? startDateTime = start.Value.ToUniversalTime();

  var cachedItems = alertsStreamsService.GetCachedEvents(teamName, startDateTime.Value);
  client.SendCachedData(cachedItems).ContinueWith(x => { alertsStreamsService.TryAddClient(client); });
}

private void OnAlertStreamAvailable(Stream stream, HttpContent content, TransportContext context)
{
  var client = new TeamEventClient(teamName, stream);
  alertsStreamsService.TryAddClient(client);
}
