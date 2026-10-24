using System.Reflection;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.FlowBlocks.SequenceFlow;
using FlowBlox.Core.Models.FlowBlocks.Web;
using FlowBlox.Core.Models.FlowBlocks.Web.WebSearch;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Test.Runtime;

namespace FlowBloxTest.FlowBlocks.Web
{
    [TestClass]
    [TestCategory(FlowBloxTestCategories.UnitTest)]
    public sealed class WebSearchFlowBlockTests : FlowBloxTestsBase
    {
        [TestMethod]
        public void Execute_MapsEachProviderResultToFixedDestinationFields()
        {
            var project = new FlowBloxProject();
            FlowBloxProjectManager.Instance.ActiveProject = project;
            var start = CreateFlowBlock<StartFlowBlock>();
            var search = CreateFlowBlock<WebSearchFlowBlock>(start);
            var provider = new RecordingWebSearchProvider
            {
                Results =
                [
                    new WebSearchResult
                    {
                        Title = "API documentation",
                        Url = "https://example.test/api",
                        Description = "REST API reference"
                    },
                    new WebSearchResult
                    {
                        Title = "OpenAPI specification",
                        Url = "https://example.test/openapi.json",
                        Description = "Machine-readable schema"
                    }
                ]
            };
            search.Provider = provider;
            search.Query = "Example API documentation";
            search.MaxResults = 5;

            search.Execute(new FlowBloxUnitTestRuntime(project), null);

            Assert.AreEqual("Example API documentation", provider.LastRequest?.Query);
            Assert.AreEqual(5, provider.LastRequest?.MaxResults);
            Assert.AreEqual(2, search.GridElementResult.Results.Count);
            Assert.AreEqual("API documentation", GetResult(search, 0, WebSearchDestinations.Title));
            Assert.AreEqual("https://example.test/openapi.json", GetResult(search, 1, WebSearchDestinations.Url));
        }

        [TestMethod]
        public void Execute_PassesPortableSearchOptionsToProvider()
        {
            var project = new FlowBloxProject();
            FlowBloxProjectManager.Instance.ActiveProject = project;
            var start = CreateFlowBlock<StartFlowBlock>();
            var search = CreateFlowBlock<WebSearchFlowBlock>(start);
            var provider = new RecordingWebSearchProvider();
            search.Provider = provider;
            search.Query = "FlowBlox documentation";
            search.MaxResults = 99;
            search.Country = "DE";
            search.SearchLanguage = "de";
            search.SafeSearch = WebSearchSafeSearch.Strict;

            search.Execute(new FlowBloxUnitTestRuntime(project), null);

            Assert.AreEqual(20, provider.LastRequest?.MaxResults);
            Assert.AreEqual("DE", provider.LastRequest?.Country);
            Assert.AreEqual("de", provider.LastRequest?.SearchLanguage);
            Assert.AreEqual(WebSearchSafeSearch.Strict, provider.LastRequest?.SafeSearch);
        }

        [TestMethod]
        public void Execute_EmptyQuery_DoesNotInvokeProvider()
        {
            var project = new FlowBloxProject();
            FlowBloxProjectManager.Instance.ActiveProject = project;
            var start = CreateFlowBlock<StartFlowBlock>();
            var search = CreateFlowBlock<WebSearchFlowBlock>(start);
            var provider = new RecordingWebSearchProvider();
            search.Provider = provider;
            search.Query = "   ";

            search.Execute(new FlowBloxUnitTestRuntime(project), null);

            Assert.IsNull(provider.LastRequest);
        }

        [TestMethod]
        public void DeepSeekProvider_MapsSharedResultFields()
        {
            const string response = """
                {
                  "results": [
                    {
                      "title": "DeepSeek API documentation",
                      "url": "https://api-docs.deepseek.com/api/search/",
                      "snippet": "Search the web and return ranked structured results.",
                      "date": "2026-10-17T00:00:00+00:00"
                    }
                  ]
                }
                """;

            var result = DeepSeekWebSearchProvider.ParseResponse(response).Single();

            Assert.AreEqual("DeepSeek API documentation", result.Title);
            Assert.AreEqual("https://api-docs.deepseek.com/api/search/", result.Url);
            Assert.AreEqual("Search the web and return ranked structured results.", result.Description);
            Assert.AreEqual("2026-10-17T00:00:00+00:00", result.PageAge);
        }

        [TestMethod]
        public void DeepSeekProvider_AddsSupportedLanguageOperator()
        {
            Assert.AreEqual(
                "FlowBlox documentation lang:de,en",
                DeepSeekWebSearchProvider.AddLanguageFilter("FlowBlox documentation", " de,en "));
        }

        [TestMethod]
        public void ProviderCapabilities_ControlOptionalSearchProperties()
        {
            var search = new WebSearchFlowBlock { Provider = new DeepSeekWebSearchProvider() };

            Assert.IsFalse(IsPropertyActive(search, nameof(WebSearchFlowBlock.Country)));
            Assert.IsTrue(IsPropertyActive(search, nameof(WebSearchFlowBlock.SearchLanguage)));
            Assert.IsFalse(IsPropertyActive(search, nameof(WebSearchFlowBlock.SafeSearch)));

            search.Provider = new BraveWebSearchProvider();

            Assert.IsTrue(IsPropertyActive(search, nameof(WebSearchFlowBlock.Country)));
            Assert.IsTrue(IsPropertyActive(search, nameof(WebSearchFlowBlock.SearchLanguage)));
            Assert.IsTrue(IsPropertyActive(search, nameof(WebSearchFlowBlock.SafeSearch)));
        }

        private static bool IsPropertyActive(WebSearchFlowBlock flowBlock, string propertyName) =>
            typeof(WebSearchFlowBlock)
                .GetProperty(propertyName)!
                .GetCustomAttribute<ActivationConditionAttribute>()!
                .IsActive(flowBlock);

        private static string GetResult(
            WebSearchFlowBlock flowBlock,
            int row,
            WebSearchDestinations destination)
        {
            var field = flowBlock.ResultFields.Single(x => x.EnumValue == destination).ResultField;
            return flowBlock.GridElementResult.Results[row].FieldValueMappings
                .Single(x => ReferenceEquals(x.Field, field))
                .Value;
        }

        private sealed class RecordingWebSearchProvider : WebSearchProviderBase
        {
            public IReadOnlyList<WebSearchResult> Results { get; init; } = [];
            public WebSearchRequest? LastRequest { get; private set; }

            protected override Task<IReadOnlyList<WebSearchResult>> SearchCoreAsync(
                WebSearchRequest request,
                CancellationToken cancellationToken)
            {
                LastRequest = request;
                return Task.FromResult(Results);
            }
        }
    }
}
