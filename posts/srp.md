# Single responsibility principle

Recently I was asked to review following code:


```csharp
    internal class ProvisioningService : IProvisioningService
    {
        private readonly string _apiUrl;
        private readonly string _authorityUrl;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public ProvisioningService(string apiUrl, string authorityUrl, string clientId, string clientSecret)
        {
            _apiUrl = apiUrl;
            _authorityUrl = authorityUrl;
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        public async Task UpdateProvisionStepAsync(StepProcessedMessage message)
        {
            var accessToken = await GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new Exception("Access token is empty.");
            }

            var httpClient = new HttpClient();
            httpClient.SetBearerToken(accessToken);

            var endpointUrl = $"{_apiUrl}/api/teams/{message.TeamName}/services/{message.ServiceCode}/provision-steps";
            var response = await httpClient.PutAsJsonAsync(endpointUrl, message.ProvisionStep);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Provision step update failed. StatusCode: {response.StatusCode}. Error: {await response.Content.ReadAsStringAsync()}");
            }
        }

        private async Task<string> GetAccessTokenAsync()
        {
            // discover endpoints from metadata
            var discoveryClient = await DiscoveryClient.GetAsync(_authorityUrl);
            if (discoveryClient.IsError)
            {
                throw new Exception($"Couldn't fetch discovery endpoint in {_authorityUrl} for clientId {_clientId}.");
            }

            // request token
            var tokenClient = new TokenClient(discoveryClient.TokenEndpoint, _clientId, _clientSecret);
            var tokenResponse = await tokenClient.RequestClientCredentialsAsync();

            if (tokenResponse.IsError)
            {
                throw new Exception($"Error from token endpoint in for clientId {_clientId}. Error: {tokenResponse.ErrorDescription}.");
            }

            return tokenResponse.AccessToken;
        }
    }
  ```

  How we can improve this peace of code? First thing we should do is to notice that ProvisioningService class is doing too much. It is responsible of creating request, requesting token, calling endpoint and handling response. It is also hard to test because there is no easy way to stub out api call. The easiest way to improve this code is to refactor out code responsible for api call. Let see how it should look like:

  ```csharp
     public class WebApiClient : IWebApiClient
     {
         private readonly string _apiUrl;
         private readonly string _authorityUrl;
         private readonly string _clientId;
         private readonly string _clientSecret;

         public WebApiClient(string apiUrl, string authorityUrl, string clientId, string clientSecret)
         {
             _apiUrl = apiUrl;
             _authorityUrl = authorityUrl;
             _clientId = clientId;
             _clientSecret = clientSecret;
         }

         public async Task<HttpResponseMessage> PutAsync<T>(string resource, T item)
         {
             var accessToken = await GetAccessTokenAsync();

             using (var httpClient = new HttpClient())
             {
                 httpClient.SetBearerToken(accessToken);

                 var endpointUrl = $"{_apiUrl}/{resource}";
                 return await httpClient.PutAsJsonAsync(endpointUrl, item);
             }
         }

         private async Task<string> GetAccessTokenAsync()
         {
             // discover endpoints from metadata
             var discoveryClient = await DiscoveryClient.GetAsync(_authorityUrl);
             if (discoveryClient.IsError)
             {
                 throw new Exception($"Couldn't fetch discovery endpoint in {_authorityUrl} for clientId {_clientId}.");
             }

             // request token
             var tokenClient = new TokenClient(discoveryClient.TokenEndpoint, _clientId, _clientSecret);
             var tokenResponse = await tokenClient.RequestClientCredentialsAsync();

             if (tokenResponse.IsError)
             {
                 throw new Exception($"Error from token endpoint in for clientId {_clientId}. Error: {tokenResponse.ErrorDescription}.");
             }

             if (string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
             {
                 throw new Exception("Access token is empty.");
             }

             return tokenResponse.AccessToken;
         }
     }
  ```

And now we can use it in in our ProvisioningService which is now responsible only for creating request and handling response:

```csharp
    public class ProvisioningService : IProvisioningService
    {
        private readonly IWebApiClient _webApiClient;

        public ProvisioningService(IWebApiClient webApiClient)
        {
            _webApiClient = webApiClient;
        }

        public async Task UpdateProvisionStepAsync(StepProcessedMessage message)
        {
            var resource = $"api/teams/{message.TeamName}/services/{message.ServiceCode}/provision-steps";
            var response = await _webApiClient.PutAsync(resource, message.ProvisionStep);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Provision step update failed. StatusCode: {response.StatusCode}. Error: {await response.Content.ReadAsStringAsync()}");
            }
        }
    }
```

Now ProvisioningService looks perfect but we can improve WebApiClient a little bit more. We should get rid of comments and replace them with methods so code would be self explanatory:

```csharp
    public class WebApiClient : IWebApiClient
    {
        private async Task<string> GetAccessTokenAsync()
        {
            var tokenEndpoint = await DiscoverTokenEndpointAsync();

            return await RequestAccessTokenAsync(tokenEndpoint);
        }

        private async Task<string> DiscoverTokenEndpointAsync()
        {
            var discoveryClient = await DiscoveryClient.GetAsync(_authorityUrl);
            if (discoveryClient.IsError)
            {
                throw new Exception($"Couldn't fetch discovery endpoint in {_authorityUrl} for clientId {_clientId}.");
            }

            return discoveryClient.TokenEndpoint;
        }

        private async Task<string> RequestAccessTokenAsync(string tokenEndpoint)
        {
            var tokenClient = new TokenClient(tokenEndpoint, _clientId, _clientSecret);
            var tokenResponse = await tokenClient.RequestClientCredentialsAsync();

            if (tokenResponse.IsError)
            {
                throw new Exception($"Error from token endpoint in for clientId {_clientId}. Error: {tokenResponse.ErrorDescription}.");
            }

            if (string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            {
                throw new Exception("Access token is empty.");
            }

            return tokenResponse.AccessToken;
        }
    }
```

That way we have class that can change because of one reason which are easy to test and easy to reuse.
