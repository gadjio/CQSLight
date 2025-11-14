using System.Net;
using Newtonsoft.Json;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using PGMS.ScenarioTesting.Exceptions;
using RestSharp;

namespace PGMS.ScenarioTesting;

public class RequiredApiVersionBaseAttribute : Attribute, ITestAction
{
    protected string BaseUrl { get; }
    protected string GetVersionPath { get; }
    protected string InMemoryIndicator { get; }

    private readonly string requiredVersion;

    
    public RequiredApiVersionBaseAttribute(string version, string baseUrl, string getVersionPath = "api/v1/common/GetVersion", string inMemoryIndicator = "(InMemory)")
    {
        BaseUrl = baseUrl;
        GetVersionPath = getVersionPath;
        InMemoryIndicator = inMemoryIndicator;
        this.requiredVersion = version;
    }

    public void BeforeTest(ITest test)
    {
        if (test.FullName.Contains(InMemoryIndicator))
        { // Test is in memory - no call to API - IntegratedAndInMemoryDualFixture
            return;
        }

        var version = GetVersion();
        if (version.Version == "1.0.0.0")
        {   //Local env
            Console.WriteLine($"Run on local server : {BaseUrl}");
            return;
        }

        if (string.IsNullOrEmpty(requiredVersion))
        {
            return;
        }

        var requestedVersion = new Version(requiredVersion);
        var apiVersion = new Version(version.Version);

        if (requestedVersion.CompareTo(apiVersion) == 1)
        {
            Assert.Ignore($"API version ({version.Version}) doesn't meet the requirements. Actual version  {version.Version} - Required version : {requiredVersion}");
        }
    }

    public void AfterTest(ITest test)
    {

    }

    public ActionTargets Targets { get; }

    public VersionInfoModel GetVersion()
    {
        var url = BaseUrl + GetVersionPath;
        var client = new RestClient(new RestClientOptions { MaxTimeout = -1 });

        var request = new RestRequest(url, Method.Get);
        request.AddHeader("Content-Type", "application/json");

        RestResponse response = client.Execute(request);

        if (response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == 0)
        {
            Assert.Ignore($"Unable to connect to API {BaseUrl}");
        }

        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new ApiHelperException(url, response.Content);
        }

        if (string.IsNullOrEmpty(response.Content))
        {
            Assert.Ignore($"Unable to connect to API {BaseUrl}");
        }

        return JsonConvert.DeserializeObject<VersionInfoModel>(response.Content);
    }
}

public class VersionInfoModel
{
    public string Version { get; set; }
}