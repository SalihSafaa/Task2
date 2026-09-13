namespace ProductCatalogApi;

public interface IUserService
{
    Task<Result<UserResponseDto>> RegisterAsync(RegisterRequestDto request);
    Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request);
}