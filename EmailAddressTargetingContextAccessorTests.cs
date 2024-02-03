using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace dotnetcore.featureflags
{
    public class EmailAddressTargetingContextAccessorTests
    {
        readonly HostApplicationBuilder _hostApplicationBuilder = new HostApplicationBuilder();
        readonly IHost _host;
        readonly IFeatureManager _featureManager;
        public EmailAddressTargetingContextAccessorTests()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("appsettings.json");
            var configuration = configurationBuilder.Build();
            _hostApplicationBuilder
                .Services
                .AddFeatureManagement(configuration.GetSection("FeatureManagement"))
                .AddFeatureFilter<PercentageFilter>()
                .AddFeatureFilter<ContextualTargetingFilter>();

            _host = _hostApplicationBuilder.Build();
            _featureManager = _host.Services.GetService<IFeatureManager>();
        }
        [Theory]
        [InlineData("test.com", null)]
        [InlineData("a.test.com", null)]
        [InlineData(null, "testing-")]
        [InlineData(null, null)]
        public void TargetingContext_Ctor_Invalid(string? emailAddress, string? domain)
        {
            Assert.Throws<ArgumentException>(() => new EmailAddressTargetingContextAccessor(emailAddress, domain));
        }
        [Theory]
        [InlineData("test@test.com", null)]
        [InlineData("test@test.com", "test.com")]
        [InlineData(null, "test.com")]
        public void TargetingContext_Ctor_Valid(string? emailAddress, string? domain)
        {
            _ = new EmailAddressTargetingContextAccessor(emailAddress, domain);
        }
        [Theory]
        [InlineData("test@test.com", null)]
        [InlineData(null, "test.com")]
        public async Task FeatureWithTargetGroup_NoMatch(string? emailAddress, string? domain)
        {
            var targetingContext = new EmailAddressTargetingContextAccessor(emailAddress, domain);
            var isEnabled = await _featureManager.IsEnabledAsync(FeatureFlags.TargetedGroupFeature, targetingContext);
            isEnabled.Should().BeFalse();
        }
    }
}
