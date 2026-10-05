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
