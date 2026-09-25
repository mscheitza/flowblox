using Amazon.S3;
using Aspose.Words.Saving;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using FlowBlox.Core.Models.FlowBlocks.AWS;
using FlowBlox.Core.Models.Components.IO;
using FlowBlox.Core.Attributes;
using FlowBlox.Core.Models.FlowBlocks.SequenceFlow;
using FlowBlox.Core.Models.FlowBlocks.TextOperations;
using FlowBlox.Core.Models.Project;
using FlowBlox.Core.Provider.Project;
using FlowBlox.Core.Util.OpenXml;
using FlowBlox.Test.Runtime;
using WDocument = DocumentFormat.OpenXml.Wordprocessing.Document;

namespace FlowBloxTest.FlowBlocks.IO
{
    [TestClass]
    public class DocumentAndStorageFlowBlockTests : FlowBloxTestsBase
    {
        private FlowBloxProject _project;

        [TestInitialize]
        public void Initialize()
        {
            _project = new FlowBloxProject();
            FlowBloxProjectManager.Instance.ActiveProject = _project;
        }

        [TestMethod]
        public void OpenXmlTemplateProcessor_ReplacesSplitPlaceholdersInBodyHeaderAndFooter()
        {
            var result = OpenXmlTemplateProcessor.ReplacePlaceholders(
                CreateTemplate(),
                value => value.Replace("$Data::Name", "Ada").Replace("$Data::Code", "42"));

            using var stream = new MemoryStream(result);
            using var document = WordprocessingDocument.Open(stream, false);
            var main = document.MainDocumentPart!;
            var body = main.Document.Body!;
            Assert.AreEqual("Hello Ada!", body.InnerText);
            Assert.AreEqual("Header 42", main.HeaderParts.Single().Header.InnerText);
            Assert.AreEqual("Footer Ada", main.FooterParts.Single().Footer.InnerText);
            Assert.IsNotNull(body.Descendants<Run>().First().RunProperties?.Bold);
        }

        [TestMethod]
        public void AsposeWords_ConvertsGeneratedDocxToPdfA()
        {
            using var input = new MemoryStream(CreateTemplate());
            var document = new Aspose.Words.Document(input);
            using var output = new MemoryStream();
            document.Save(output, new PdfSaveOptions { Compliance = PdfCompliance.PdfA1b });

            CollectionAssert.AreEqual("%PDF"u8.ToArray(), output.ToArray()[..4]);
        }

        [TestMethod]
        public void TextBuilderStack_AppendsAndOutputsText()
        {
            var start = CreateFlowBlock<StartFlowBlock>();
            var builder = CreateFlowBlock<TextBuilderFlowBlock>(start);
            builder.InitialText = "Start:";
            var append = CreateFlowBlock<TextBuilderAppendFlowBlock>(builder);
            append.AssociatedTextBuilder = builder;
            append.Text = " one";
            var appendLine = CreateFlowBlock<TextBuilderAppendFlowBlock>(append);
            appendLine.AssociatedTextBuilder = builder;
            appendLine.Text = " two";
            appendLine.AppendLine = true;
            var output = CreateFlowBlock<TextBuilderOutputFlowBlock>(appendLine);
            output.AssociatedTextBuilder = builder;
            var runtime = new FlowBloxUnitTestRuntime(_project);

            Assert.IsTrue(builder.Execute(runtime, null));
            Assert.IsTrue(append.Execute(runtime, null));
            Assert.IsTrue(appendLine.Execute(runtime, null));
            Assert.IsTrue(output.Execute(runtime, null));
            Assert.AreEqual("Start: one two" + Environment.NewLine,
                output.GridElementResult.Results.Single().FieldValueMappings.Single().Value);
        }

        [TestMethod]
        public void S3Provider_CreatesMinIoCompatibleConfiguration()
        {
            var provider = new AmazonWebServicesProvider
            {
                ApiKey = "access", SecretKey = "secret", Region = "eu-central-1",
                S3ServiceUrl = "http://localhost:9000", S3ForcePathStyle = true
            };

            AmazonS3Config configuration = provider.CreateS3Configuration();

            Assert.AreEqual("http://localhost:9000/", configuration.ServiceURL);
            Assert.AreEqual("eu-central-1", configuration.AuthenticationRegion);
            Assert.IsTrue(configuration.ForcePathStyle);
        }

        [TestMethod]
        public void SftpProvider_CreatesPasswordAuthenticatedClient()
        {
            var provider = new SftpConnectionProvider
            {
                Host = "sftp.example.test", Port = 2222, UserName = "user", Password = "secret",
                PrivateKey = "This inactive value must be ignored",
                HostKeyFingerprint = "SHA256:known-host-key"
            };

            using var client = provider.CreateClient();

            Assert.AreEqual("sftp.example.test", client.ConnectionInfo.Host);
            Assert.AreEqual(2222, client.ConnectionInfo.Port);
            Assert.AreEqual("user", client.ConnectionInfo.Username);
            Assert.AreEqual(1, client.ConnectionInfo.AuthenticationMethods.Count());
        }

        [TestMethod]
        public void SftpProvider_ActivatesOnlyCredentialsForSelectedAuthenticationMethod()
        {
            var provider = new SftpConnectionProvider();
            var passwordCondition = typeof(SftpConnectionProvider).GetProperty(nameof(SftpConnectionProvider.Password))!
                .GetCustomAttributes(typeof(ActivationConditionAttribute), true)
                .Cast<ActivationConditionAttribute>().Single();
            var privateKeyCondition = typeof(SftpConnectionProvider).GetProperty(nameof(SftpConnectionProvider.PrivateKey))!
                .GetCustomAttributes(typeof(ActivationConditionAttribute), true)
                .Cast<ActivationConditionAttribute>().Single();

            provider.AuthenticationMethod = SftpAuthenticationMethod.Password;
            Assert.IsTrue(passwordCondition.IsActive(provider));
            Assert.IsFalse(privateKeyCondition.IsActive(provider));

            provider.AuthenticationMethod = SftpAuthenticationMethod.PrivateKey;
            Assert.IsFalse(passwordCondition.IsActive(provider));
            Assert.IsTrue(privateKeyCondition.IsActive(provider));

            provider.AuthenticationMethod = SftpAuthenticationMethod.PasswordAndPrivateKey;
            Assert.IsTrue(passwordCondition.IsActive(provider));
            Assert.IsTrue(privateKeyCondition.IsActive(provider));
        }

        [TestMethod]
        public void SftpProvider_RequiresPrivateKeyForPrivateKeyAuthentication()
        {
            var provider = new SftpConnectionProvider
            {
                Host = "sftp.example.test", UserName = "user", Password = "inactive-password",
                AuthenticationMethod = SftpAuthenticationMethod.PrivateKey
            };

            var exception = Assert.ThrowsExactly<System.ComponentModel.DataAnnotations.ValidationException>(provider.CreateClient);

            StringAssert.Contains(exception.Message, "private key");
        }

        private static byte[] CreateTemplate()
        {
            using var stream = new MemoryStream();
            using (var package = WordprocessingDocument.Create(stream, DocumentFormat.OpenXml.WordprocessingDocumentType.Document, true))
            {
                var main = package.AddMainDocumentPart();
                main.Document = new WDocument(new Body(new Paragraph(
                    new Run(new RunProperties(new Bold()), new Text("Hello $Data::")),
                    new Run(new RunProperties(new Italic()), new Text("Name!")))));
                var header = main.AddNewPart<HeaderPart>();
                header.Header = new Header(new Paragraph(new Run(new Text("Header $Data::Code"))));
                var footer = main.AddNewPart<FooterPart>();
                footer.Footer = new Footer(new Paragraph(new Run(new Text("Footer $Data::Name"))));
                var section = main.Document.Body!.AppendChild(new SectionProperties());
                section.Append(new HeaderReference { Id = main.GetIdOfPart(header) });
                section.Append(new FooterReference { Id = main.GetIdOfPart(footer) });
                main.Document.Save();
            }
            return stream.ToArray();
        }
    }
}
