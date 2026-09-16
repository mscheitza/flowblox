using FlowBlox.Core.Enums;
using FlowBlox.Core.Models.Notifications;
using FlowBlox.Core.Services.Notifications;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FlowBloxTest.Notifications
{
    [TestClass]
    public class RuntimeNotificationTests
    {
        [TestMethod]
        public void Configuration_ContainsEveryRuntimeEventDisabledByDefault()
        {
            var configuration = new RuntimeNotificationConfiguration();

            CollectionAssert.AreEquivalent(
                Enum.GetValues<RuntimeNotificationType>(),
                configuration.Notifications.Select(x => x.Type).ToArray());
            Assert.IsTrue(configuration.Notifications.All(x => !x.Enabled));
            Assert.IsTrue(configuration.DoNotSendWhileDebugging);
            Assert.IsTrue(configuration.AttachRuntimeLog);
        }

        [TestMethod]
        public void ConfigurationJson_ReplacesDefaultRulesInsteadOfAppendingDuplicates()
        {
            var configuration = new RuntimeNotificationConfiguration();
            configuration.Notifications.Single(x => x.Type == RuntimeNotificationType.RuntimeAborted).Enabled = true;

            var json = JsonConvert.SerializeObject(configuration);
            var deserialized = JsonConvert.DeserializeObject<RuntimeNotificationConfiguration>(json)
                ?? throw new AssertFailedException("The runtime notification configuration was not deserialized.");
            deserialized.EnsureDefaultRules();

            Assert.AreEqual(Enum.GetValues<RuntimeNotificationType>().Length, deserialized.Notifications.Count);
            Assert.AreEqual(deserialized.Notifications.Count, deserialized.Notifications.Select(x => x.Type).Distinct().Count());
            Assert.IsTrue(deserialized.Notifications.Single(x => x.Type == RuntimeNotificationType.RuntimeAborted).Enabled);
        }

        [TestMethod]
        public void Configuration_UsesDataAnnotationsForRequiredSmtpSettings()
        {
            var configuration = new RuntimeNotificationConfiguration();
            var errors = Validate(configuration);

            Assert.IsTrue(errors.Any(x => x.MemberNames.Contains(nameof(configuration.Host))));
            Assert.IsTrue(errors.Any(x => x.MemberNames.Contains(nameof(configuration.FromAddress))));
            Assert.IsTrue(errors.Any(x => x.MemberNames.Contains(nameof(configuration.ToAddresses))));
        }

        [TestMethod]
        public void Configuration_RequiresCredentialsOnlyWhenAuthenticationIsEnabled()
        {
            var configuration = new RuntimeNotificationConfiguration();

            var authenticationDisabledErrors = Validate(configuration);
            Assert.IsFalse(authenticationDisabledErrors.Any(x => x.MemberNames.Contains(nameof(configuration.UserName))));
            Assert.IsFalse(authenticationDisabledErrors.Any(x => x.MemberNames.Contains(nameof(configuration.Password))));

            configuration.UseAuthentication = true;
            var authenticationEnabledErrors = Validate(configuration);
            Assert.IsTrue(authenticationEnabledErrors.Any(x => x.MemberNames.Contains(nameof(configuration.UserName))));
            Assert.IsTrue(authenticationEnabledErrors.Any(x => x.MemberNames.Contains(nameof(configuration.Password))));
        }

        [TestMethod]
        public void MailFormatter_IncludesAbortCauseAndHtmlEncodesDetails()
        {
            var context = new RuntimeNotificationContext
            {
                Type = RuntimeNotificationType.RuntimeAborted,
                ProjectName = "Project <A>",
                Message = "Cancelled & stopped",
                TriggeringErrorMessage = "Flow block <failed>",
                FlowBlockName = "Reader & Parser",
                Exception = new InvalidOperationException("Bad <value>"),
                ProcessId = 42,
                ProcessName = "FlowBlox",
                HostName = "node-1"
            };

            var html = RuntimeNotificationMailFormatter.CreateHtmlBody(context);
            var subject = RuntimeNotificationMailFormatter.CreateSubject(context);

            StringAssert.Contains(subject, "Runtime aborted");
            StringAssert.Contains(html, "Event");
            StringAssert.Contains(html, "Runtime aborted");
            StringAssert.Contains(html, "Project &lt;A&gt;");
            StringAssert.Contains(html, "Flow block &lt;failed&gt;");
            StringAssert.Contains(html, "Bad &lt;value&gt;");
            Assert.IsFalse(html.Contains("Project <A>", StringComparison.Ordinal));
        }

        [TestMethod]
        public void MailFormatter_UsesCopyrightFromApplicationMetadata()
        {
            var context = new RuntimeNotificationContext
            {
                Type = RuntimeNotificationType.RuntimeCompletedSuccessfully
            };
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            var copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;

            var html = RuntimeNotificationMailFormatter.CreateHtmlBody(context);

            Assert.IsFalse(string.IsNullOrWhiteSpace(copyright));
            StringAssert.Contains(html, System.Net.WebUtility.HtmlEncode(copyright));
        }

        [TestMethod]
        public void ConfigurationJson_DoesNotContainSmtpPassword()
        {
            var configuration = new RuntimeNotificationConfiguration { Password = "secret-value" };

            var json = JsonConvert.SerializeObject(configuration);

            Assert.IsFalse(json.Contains("secret-value", StringComparison.Ordinal));
            Assert.IsFalse(json.Contains(nameof(configuration.Password), StringComparison.Ordinal));
        }

        private static List<ValidationResult> Validate(RuntimeNotificationConfiguration configuration)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(configuration, new ValidationContext(configuration), results, true);
            return results;
        }
    }
}
