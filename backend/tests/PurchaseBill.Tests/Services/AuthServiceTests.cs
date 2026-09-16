using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PurchaseBill.Application.Dtos.Auth;
using PurchaseBill.Application.Dtos.Enhanzer;
using PurchaseBill.Application.Entities;
using PurchaseBill.Application.Exceptions;
using PurchaseBill.Application.Interfaces;
using PurchaseBill.Application.Services;
using PurchaseBill.Tests.TestUtils;
using Xunit;

namespace PurchaseBill.Tests.Services;

public class AuthServiceTests
{
    private static readonly LoginRequest ValidRequest = new("info@enhanzer.com", "Welcome#5");

    private static EnhanzerApiEnvelope SuccessEnvelope(params (string Code, string Name)[] locations) => new()
    {
        StatusCode = 200,
        Message = "GetLoginData POS API Executed Successfully.",
        ResponseBody =
        [
            new EnhanzerLoginResult
            {
                Email = "info@enhanzer.com",
                CompanyCode = "EZCMP-1",
                UserLocations = locations.Select(l => new EnhanzerUserLocation { LocationCode = l.Code, LocationName = l.Name }).ToList()
            }
        ]
    };

    private static (AuthService Service, IApplicationDbContext Db, Mock<IEnhanzerAuthClient> EnhanzerClient) BuildService()
    {
        var db = TestDbContextFactory.Create();
        var enhanzerClient = new Mock<IEnhanzerAuthClient>();
        var tokenGenerator = new Mock<IJwtTokenGenerator>();
        tokenGenerator
            .Setup(t => t.GenerateToken(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(("fake-jwt-token", DateTime.UtcNow.AddHours(1)));

        var service = new AuthService(enhanzerClient.Object, db, tokenGenerator.Object, NullLogger<AuthService>.Instance);
        return (service, db, enhanzerClient);
    }

    [Fact]
    public async Task LoginAsync_OnSuccess_SavesLocationsAndReturnsToken()
    {
        var (service, db, enhanzerClient) = BuildService();
        enhanzerClient
            .Setup(c => c.GetLoginDataAsync(ValidRequest.Email, ValidRequest.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessEnvelope(("LOC-1", "Head Office"), ("LOC-2", "Warehouse")));

        var response = await service.LoginAsync(ValidRequest);

        Assert.Equal("fake-jwt-token", response.Token);
        Assert.Equal(2, response.Locations.Count);
        Assert.Equal(2, db.LocationDetails.Count());
    }

    [Fact]
    public async Task LoginAsync_OnSecondLogin_UpsertsRatherThanDuplicatingLocations()
    {
        var (service, db, enhanzerClient) = BuildService();
        enhanzerClient
            .Setup(c => c.GetLoginDataAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessEnvelope(("LOC-1", "Head Office")));

        await service.LoginAsync(ValidRequest);

        enhanzerClient
            .Setup(c => c.GetLoginDataAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessEnvelope(("LOC-1", "Head Office - Renamed")));
        await service.LoginAsync(ValidRequest);

        Assert.Single(db.LocationDetails);
        Assert.Equal("Head Office - Renamed", db.LocationDetails.Single().LocationName);
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIsWrong_ThrowsWithDocMsg()
    {
        var (service, _, enhanzerClient) = BuildService();
        enhanzerClient
            .Setup(c => c.GetLoginDataAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnhanzerApiEnvelope
            {
                StatusCode = 200,
                ResponseBody = [new EnhanzerLoginResult { Email = ValidRequest.Email, DocMsg = "Invalid Login Details" }]
            });

        var ex = await Assert.ThrowsAsync<AuthenticationFailedException>(() => service.LoginAsync(ValidRequest));
        Assert.Equal("Invalid Login Details", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_WhenCompanyIsUnknown_ThrowsWithTopLevelMessage()
    {
        var (service, _, enhanzerClient) = BuildService();
        enhanzerClient
            .Setup(c => c.GetLoginDataAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnhanzerApiEnvelope { StatusCode = 401, Message = "Un-Authorize POS API Request.", ResponseBody = null });

        var ex = await Assert.ThrowsAsync<AuthenticationFailedException>(() => service.LoginAsync(ValidRequest));
        Assert.Equal("Un-Authorize POS API Request.", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_WhenEnhanzerReturnsAnEmptyEnvelope_ThrowsAFriendlyRetryMessage()
    {
        // Reproduces a real staging quirk: back-to-back calls to the shared demo account
        // sometimes get Status_Code 0 / Message null / Response_Body null instead of a real error.
        var (service, _, enhanzerClient) = BuildService();
        enhanzerClient
            .Setup(c => c.GetLoginDataAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnhanzerApiEnvelope { StatusCode = 0, Message = null, ResponseBody = null });

        var ex = await Assert.ThrowsAsync<AuthenticationFailedException>(() => service.LoginAsync(ValidRequest));
        Assert.Contains("try again", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoginAsync_WhenNoLocationsAreReturned_Throws()
    {
        var (service, _, enhanzerClient) = BuildService();
        enhanzerClient
            .Setup(c => c.GetLoginDataAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SuccessEnvelope());

        await Assert.ThrowsAsync<AuthenticationFailedException>(() => service.LoginAsync(ValidRequest));
    }
}
