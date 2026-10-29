using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace ProductCatalogApi.Tests;

public class AuthServiceTest
{
    private static UserService BuildService(AppDbContext context)
    {
        var passwordHasher = new Argon2PasswordHasher(Options.Create(new PasswordHashOptions()));
        var jwtTokenService = new JwtTokenService(Options.Create(new JwtOptions
        {
            Key = "00000000000000000000000000000000",
            Issuer = "test-issuer",
            Audience = "test-audience"
        }));
        var refreshTokenService = new RefreshTokenService(context, NullLogger<RefreshTokenService>.Instance);

        return new UserService(
            context,
            passwordHasher,
            jwtTokenService,
            refreshTokenService,
            NullLogger<UserService>.Instance);
    }

    private static AppDbContext BuildContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task LoginAsync_WithCorrectPassword_Succeeds()
    {
        await using var context = BuildContext();
        var service = BuildService(context);

        var passwordHasher = new Argon2PasswordHasher(Options.Create(new PasswordHashOptions()));
        context.Users.Add(new User
        {
            Id = 1,
            Username = "test",
            PasswordHash = passwordHasher.Hash("12345678"),
            Role = "User"
        });
        await context.SaveChangesAsync();

        var result = await service.LoginAsync(new LoginRequestDto
        {
            Username = "test",
            Password = "12345678"
        });

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.False(string.IsNullOrWhiteSpace(result.Value!.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.Value!.RefreshToken));
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_Fails()
    {
        await using var context = BuildContext();
        var service = BuildService(context);

        var passwordHasher = new Argon2PasswordHasher(Options.Create(new PasswordHashOptions()));
        context.Users.Add(new User
        {
            Id = 1,
            Username = "test",
            PasswordHash = passwordHasher.Hash("12345678"),
            Role = "User"
        });
        await context.SaveChangesAsync();

        var result = await service.LoginAsync(new LoginRequestDto
        {
            Username = "test",
            Password = "wrong-password"
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.UnAuthorized, result.ErrorType);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentUsername_Fails()
    {
        await using var context = BuildContext();
        var service = BuildService(context);

        var result = await service.LoginAsync(new LoginRequestDto
        {
            Username = "does-not-exist",
            Password = "whatever123"
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.UnAuthorized, result.ErrorType);
    }
}