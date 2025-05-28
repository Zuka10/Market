using Market.Domain.Entities.Market;
using Market.Infrastructure.Data;
using NetArchTest.Rules;

namespace Market.Test;

[TestFixture]
public class ArchitectureTests
{
    private const string ApplicationNamespace = "Market.Application";
    private const string InfrastructureNamespace = "Market.Infrastructure";
    private const string ApiNamespace = "Market.API";

    [Test]
    public void Domain_Should_Not_HaveDependencyOn_Application_Infrastructure_Api()
    {
        var result = Types.InAssembly(typeof(Product).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace, ApiNamespace)
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True, "Domain should not depend on Application, Infrastructure, or API.");
    }

    [Test]
    public void Application_Should_OnlyDependOn_Domain()
    {
        var result = Types.InAssembly(typeof(Market.Application.DependecyInjection).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNamespace, ApiNamespace)
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True, "Application should not depend on Infrastructure or API.");
    }

    [Test]
    public void Infrastructure_CanDependOn_Application_And_Domain_ButNotApi()
    {
        var result = Types.InAssembly(typeof(Market.Infrastructure.DependencyInjection).Assembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True, "Infrastructure should not depend on API.");
    }

    [Test]
    public void Api_CanDependOn_AllOtherLayers()
    {
        var result = Types.InAssembly(typeof(Market.API.Controllers.ProductController).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Market.Test")
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True, "API should not depend on test project.");
    }
}