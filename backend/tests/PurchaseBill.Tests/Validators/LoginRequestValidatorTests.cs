using PurchaseBill.Application.Dtos.Auth;
using PurchaseBill.Application.Validators;
using Xunit;

namespace PurchaseBill.Tests.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void Validate_WithValidRequest_Passes()
    {
        var result = _validator.Validate(new LoginRequest("info@enhanzer.com", "Welcome#5"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", "Welcome#5")]
    [InlineData("not-an-email", "Welcome#5")]
    [InlineData("info@enhanzer.com", "")]
    public void Validate_WithInvalidInput_Fails(string email, string password)
    {
        var result = _validator.Validate(new LoginRequest(email, password));

        Assert.False(result.IsValid);
    }
}
