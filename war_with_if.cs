public class AlertStreamController : Controller
{
    private readonly IAlertsStreamsService _alertsStreamsService;

    public AlertStreamController(IAlertsStreamsService alertsStreamsService)
    {
        _alertsStreamsService = alertsStreamsService;
    }

    public HttpResponseMessage AlertStream(HttpRequestMessage request, string teamName, DateTime start)
    {
        return SensorStream(new OnAlertStreamAvailableFrom(_alertsStreamsService, teamName, start), teamName);
    }

    public HttpResponseMessage AlertStream(HttpRequestMessage request, string teamName)
    {
        return SensorStream(new OnAlertStreamAvailable(_alertsStreamsService), teamName);
    }

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
}

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

public interface IOnAlertStreamAvailable
{
    void Execute(TeamEventClient client);
}
