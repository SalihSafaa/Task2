namespace ProductCatalogApi;

public interface IJwtTokenService
{
    string CreateToken(User user);
}