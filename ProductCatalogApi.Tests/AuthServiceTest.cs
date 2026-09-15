using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace ProductCatalogApi.Tests;

public class AuthServiceTest
{
    [Fact]
    public async Task LoginAsync_WithCorrectPassword_Succeeds()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);
        var passwordHasher = new Argon2PasswordHasher(Options.Create(new PasswordHashOptions()));
        var user = new User
        {
            Id = 1,
            Username = "test",
            PasswordHash = passwordHasher.Hash("12345678"),
            Role = "User"
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var jwtTokenService = new JwtTokenService(Options.Create(new JwtOptions
        {
            Key = "00000000000000000000000000000000",
            Issuer = "test-issuer",
            Audience = "test-audience"
        }));
        var service = new UserService(
            context,
            passwordHasher,
            jwtTokenService,
            NullLogger<UserService>.Instance);
        var request = new LoginRequestDto
        {
            Username = "test",
            Password = "12345678"
        };

        var result = await service.LoginAsync(request);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.False(string.IsNullOrWhiteSpace(result.Value!.AccessToken));
    }
}