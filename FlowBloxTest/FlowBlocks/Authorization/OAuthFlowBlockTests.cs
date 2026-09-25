using FlowBlox.Core.Models.FlowBlocks.Authorization;
using FlowBlox.Core.Models.FlowBlocks.SequenceFlow;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Test.Runtime;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FlowBloxTest.FlowBlocks.Authorization
{
    [TestClass]
    public class OAuthFlowBlockTests : FlowBloxTestsBase
    {
        private FlowBloxProject _project;

        [TestInitialize]
        public void Initialize()
        {
            _project = new FlowBloxProject();
            FlowBloxProjectManager.Instance.ActiveProject = _project;
        }

        [TestMethod]
        public async Task ClientCredentialsGrant_SendsClientCredentialsWithoutUserCredentials()
        {
            var (endpoint, requestBodyTask) = StartTokenServer();
            var block = CreateFlowBlock<OAuthFlowBlock>(CreateFlowBlock<StartFlowBlock>());
            block.TokenEndpoint = endpoint;
            block.ClientId = "test-client";
            block.ClientSecret = "client-secret";
            block.Scope = "api.read";

            Assert.IsTrue(block.Execute(new FlowBloxUnitTestRuntime(_project), null));
            var form = ParseForm(await requestBodyTask);

            Assert.AreEqual("client_credentials", form["grant_type"]);
            Assert.AreEqual("test-client", form["client_id"]);
            Assert.AreEqual("client-secret", form["client_secret"]);
            Assert.AreEqual("api.read", form["scope"]);
            Assert.IsFalse(form.ContainsKey("username"));
            Assert.IsFalse(form.ContainsKey("password"));
            Assert.AreEqual("test-access-token", block.GridElementResult.Results.Single().FieldValueMappings.Single().Value);
        }

        [TestMethod]
        public async Task PasswordGrant_SendsClientAndUserCredentials()
        {
            var (endpoint, requestBodyTask) = StartTokenServer();
            var block = CreateFlowBlock<OAuthFlowBlock>(CreateFlowBlock<StartFlowBlock>());
            block.TokenEndpoint = endpoint;
            block.ClientId = "test-client";
            block.ClientSecret = "client-secret";
            block.GrantType = OAuthGrantType.Password;
            block.UserName = "alice@example.test";
            block.Password = "user-password";

            Assert.IsTrue(block.Execute(new FlowBloxUnitTestRuntime(_project), null));
            var form = ParseForm(await requestBodyTask);

            Assert.AreEqual("password", form["grant_type"]);
            Assert.AreEqual("test-client", form["client_id"]);
            Assert.AreEqual("client-secret", form["client_secret"]);
            Assert.AreEqual("alice@example.test", form["username"]);
            Assert.AreEqual("user-password", form["password"]);
        }

        private static (string Endpoint, Task<string> RequestBody) StartTokenServer()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            return ($"http://127.0.0.1:{port}/token", HandleRequest());

            async Task<string> HandleRequest()
            {
                try
                {
                    using var client = await listener.AcceptTcpClientAsync();
                    using var stream = client.GetStream();
                    using var reader = new StreamReader(stream, Encoding.ASCII, false, 1024, leaveOpen: true);
                    var contentLength = 0;
                    string line;
                    while (!string.IsNullOrEmpty(line = await reader.ReadLineAsync()))
                    {
                        if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                            contentLength = int.Parse(line["Content-Length:".Length..].Trim());
                    }

                    var buffer = new char[contentLength];
                    var read = 0;
                    while (read < buffer.Length)
                        read += await reader.ReadAsync(buffer.AsMemory(read, buffer.Length - read));

                    const string responseBody = "{\"access_token\":\"test-access-token\",\"token_type\":\"Bearer\"}";
                    var response = "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n" +
                        $"Content-Length: {Encoding.UTF8.GetByteCount(responseBody)}\r\nConnection: close\r\n\r\n{responseBody}";
                    var responseBytes = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseBytes);
                    return new string(buffer);
                }
                finally
                {
                    listener.Stop();
                }
            }
        }

        private static Dictionary<string, string> ParseForm(string body)
            => body.Split('&', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Split('=', 2))
                .ToDictionary(
                    x => Uri.UnescapeDataString(x[0].Replace('+', ' ')),
                    x => Uri.UnescapeDataString((x.Length > 1 ? x[1] : string.Empty).Replace('+', ' ')));
    }
}
