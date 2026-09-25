using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.FlowBlocks.Web;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;

namespace FlowBloxTest.FlowBlocks.Web
{
    [TestClass]
    public class OpenApiRestFlowBlockTests : FlowBloxTestsBase
    {
        [TestInitialize]
        public void Initialize()
        {
            FlowBloxProjectManager.Instance.ActiveProject = new FlowBloxProject();
        }

        [TestMethod]
        public void DirectOpenApiDefinition_GeneratesGroupedEndpointsParametersAndPayload()
        {
            var block = CreateFlowBlock<OpenApiRestFlowBlock>();

            block.OpenApiSource = Definition;

            CollectionAssert.AreEqual(
                new[]
                {
                    "Participant | POST /participants - Create participant",
                    "Participant | GET /participants/{id} - Read participant"
                },
                block.EndpointSuggestions.ToArray());

            block.Endpoint = block.EndpointSuggestions[0];

            Assert.AreEqual("https://api.example.test/v1", block.BaseUrl);
            Assert.AreEqual("Create participant", block.EndpointDescription);
            Assert.AreEqual("application/json", block.ContentType);
            Assert.AreEqual("X-Tenant", block.HeaderParameters.Single().Name);
            Assert.AreEqual("Tenant identifier", block.HeaderParameters.Single().Description);
            Assert.AreEqual("demo", block.HeaderParameters.Single().Value);
            Assert.IsTrue(block.Payload.Contains("\"name\"", StringComparison.Ordinal));
            Assert.IsTrue(block.Payload.Contains("\"age\"", StringComparison.Ordinal));
        }

        [TestMethod]
        public void ChangingEndpoint_ReplacesGeneratedConfiguration()
        {
            var block = CreateFlowBlock<OpenApiRestFlowBlock>();
            block.OpenApiSource = Definition;
            block.Endpoint = block.EndpointSuggestions[0];
            block.Payload = "custom";
            block.HeaderParameters.Single().Value = "changed";

            block.Endpoint = block.EndpointSuggestions[1];

            Assert.AreEqual(string.Empty, block.Payload);
            Assert.AreEqual(0, block.HeaderParameters.Count);
            Assert.AreEqual("id", block.RequestParameters.Single().Name);
            Assert.AreEqual("Path", block.RequestParameters.Single().Location);
        }

        [TestMethod]
        public void NewBlock_HasOnlyPayloadAndStatusCodeResultsByDefault()
        {
            var block = CreateFlowBlock<OpenApiRestFlowBlock>();

            CollectionAssert.AreEqual(
                new[] { OpenApiRestDestinations.Payload, OpenApiRestDestinations.StatusCode },
                block.ResultFields.Select(x => x.EnumValue!.Value).ToArray());
            Assert.AreEqual(FieldTypes.Integer, block.ResultFields[1].ResultField.FieldType.FieldType);
        }

        private const string Definition = """
            {
              "openapi": "3.0.1",
              "info": { "title": "Participants", "version": "1.0" },
              "servers": [{ "url": "https://api.example.test/v1" }],
              "paths": {
                "/participants": {
                  "post": {
                    "tags": ["Participant"],
                    "summary": "Create participant",
                    "parameters": [{
                      "name": "X-Tenant",
                      "in": "header",
                      "required": true,
                      "description": "Tenant identifier",
                      "schema": { "type": "string", "default": "demo" }
                    }],
                    "requestBody": {
                      "content": {
                        "application/json": {
                          "schema": {
                            "type": "object",
                            "properties": {
                              "name": { "type": "string" },
                              "age": { "type": "integer" }
                            }
                          }
                        }
                      }
                    },
                    "responses": { "200": { "description": "OK" } }
                  }
                },
                "/participants/{id}": {
                  "get": {
                    "tags": ["Participant"],
                    "summary": "Read participant",
                    "parameters": [{
                      "name": "id",
                      "in": "path",
                      "required": true,
                      "description": "Participant identifier",
                      "schema": { "type": "string" }
                    }],
                    "responses": { "200": { "description": "OK" } }
                  }
                }
              }
            }
            """;
    }
}
