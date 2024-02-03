using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;
using System.Numerics;

namespace dotnetcore.featureflags;

public static class FeatureFlags
{
    public const string EnabledFeature = "EnabledFeature";
    public const string PercentFeature = "PercentFeature";
    public const string TargetedGroupFeature = "TargetedGroupFeature";
    public const string TargetedUserFeature = "TargetedUserFeature";
    public const string TargetedUserAndGroupFeature = "TargetedUserAndGroupFeature";
    public const string Undefined = "Undefined";
}
public class FeatureManagementWithAppSettingsTests
{
    readonly HostApplicationBuilder _hostApplicationBuilder = new HostApplicationBuilder();
    readonly IHost _host;
    readonly IFeatureManager _featureManager;
    public FeatureManagementWithAppSettingsTests()
    {
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddJsonFile("appsettings.json");
        var configuration = configurationBuilder.Build();
        _hostApplicationBuilder
            .Services
            .AddFeatureManagement(configuration.GetSection("FeatureManagement"))
            .AddFeatureFilter<PercentageFilter>()
            .AddFeatureFilter<ContextualTargetingFilter>();
            ///using ContextualTargetingFilter instead of .WithTargeting<> since we're creating a
            ///context manually duringn execution and not using DI



        _host = _hostApplicationBuilder.Build();
        _featureManager = _host.Services.GetService<IFeatureManager>()!;
    }
    [Fact]
    public void FeatureManagerIsRegistered()
    {
        _featureManager.Should().NotBeNull();

    }
    [Fact]
    public async Task FeatureAIsEnabled()
    {

        (await _featureManager.IsEnabledAsync(FeatureFlags.EnabledFeature)).Should().BeTrue();

    }
    [Fact]
    public async Task UndefinedFeatureShouldBeFalse()
    {
        (await _featureManager.IsEnabledAsync(FeatureFlags.Undefined)).Should().BeFalse();

    }
    [Fact]
    public async Task FeatureBIs50Percent()
    {
        int attempts = 100;
        double numberTrue = 0;
        for (var count = 0; count < attempts; count++)
        {
            if (await _featureManager.IsEnabledAsync(FeatureFlags.PercentFeature)) numberTrue++;
        }
        numberTrue.Should().NotBe(0);
        var percentTrue = numberTrue / attempts * 100;
        percentTrue.Should().BeApproximately(50, 20);
    }
    [Theory]
    [InlineData(null, "targetedgroup.com", true)]
    [InlineData(null, "targeted.com", false)]
    [InlineData("test.targetedgroup.com", null, false)]
    public async Task FeatureWithTargetGroup(string? emailAddress, string? domain, bool result)
    {
        var groupList = domain == null ? Enumerable.Empty<string>() : new List<string> { domain };

        var targetingContext = new EmailAddressTargetingContext{
            UserId = emailAddress!, 
            Groups = groupList
        };
        var isEnabled = await _featureManager.IsEnabledAsync(FeatureFlags.TargetedGroupFeature, targetingContext);
        isEnabled.Should().Be(result);
    }

    [Theory]
    [InlineData("test@targeteduser.com", null, true)]
    [InlineData("test@targeted.com", null, false)]
    [InlineData(null, "targeted.com", false)]
    public async Task FeatureWithTargetUser(string? emailAddress, string? domain, bool result)
    {
        var groupList = domain == null ? Enumerable.Empty<string>() : new List<string> { domain };

        var targetingContext = new EmailAddressTargetingContext
        {
            UserId = emailAddress!,
            Groups = groupList
        };
        var isEnabled = await _featureManager.IsEnabledAsync(FeatureFlags.TargetedUserFeature, targetingContext);
        isEnabled.Should().Be(result);
    }
    [Theory]
    [InlineData("test@targeteduser.com", null, true)]
    [InlineData("test@targeted.com", null, false)]
    [InlineData(null, "targeted.com", false)]
    [InlineData(null, "targetedgroup.com", true)]
    public async Task FeatureWithTargetUserAndGroup(string? emailAddress, string? domain, bool result)
    {
        var groupList = domain == null ? Enumerable.Empty<string>() : new List<string> { domain };

        var targetingContext = new EmailAddressTargetingContext
        {
            UserId = emailAddress!,
            Groups = groupList
        };
        var isEnabled = await _featureManager.IsEnabledAsync(FeatureFlags.TargetedUserAndGroupFeature, targetingContext);
        isEnabled.Should().Be(result);
    }
}