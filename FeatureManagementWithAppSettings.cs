using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;

namespace dotnetcore.featureflags;

public static class FeatureFlags
{
    public const string EnabledFeature = "EnabledFeature";
    public const string PercentFeature = "PercentFeature";
    public const string Undefined = "Undefined";
}
public class FeatureManagementWithAppSettings
{
    readonly HostApplicationBuilder _hostApplicationBuilder = new HostApplicationBuilder();
    readonly IHost _host;
    readonly IFeatureManager _featureManager;
    public FeatureManagementWithAppSettings()
    {
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddJsonFile("appsettings.json");
        var configuration = configurationBuilder.Build();
        _hostApplicationBuilder
            .Services
            .AddFeatureManagement(configuration.GetSection("FeatureManagement"))
            .AddFeatureFilter<PercentageFilter>();

        _host = _hostApplicationBuilder.Build();
        _featureManager = _host.Services.GetService<IFeatureManager>();
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
        percentTrue.Should().BeApproximately(50, 10);
    }
}