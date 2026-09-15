using Microsoft.EntityFrameworkCore;

namespace ProductCatalogApi;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        AppDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        ILogger<UserService> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<Result<UserResponseDto>> RegisterAsync(RegisterRequestDto request)
    {
        var username = request.Username.Trim();
        var usernameExists = await _context.Users
            .AnyAsync(user => user.Username.ToUpper() == username.ToUpper());

        if (usernameExists)
        {
            return Result<UserResponseDto>.Failure(ErrorType.Conflict, $"Username '{username}' is already in use. Please choose a different username.");
        }

        var user = new User
        {
            Username = username,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = "User"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User registered with ID: {UserId}", user.Id);

        return Result<UserResponseDto>.Success(ToResponse(user));
    }

    public async Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request)
    {
        var username = request.Username.Trim();
        var user = await _context.Users
            .SingleOrDefaultAsync(candidate => candidate.Username.ToUpper() == username.ToUpper());

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for username: {Username}", username);
            return Result<LoginResponseDto>.Failure(ErrorType.UnAuthorized, "Invalid username or password. Please check your credentials and try again.");
        }

        return Result<LoginResponseDto>.Success(new LoginResponseDto
        {
            AccessToken = _jwtTokenService.CreateToken(user)
        });
    }

    private static UserResponseDto ToResponse(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Role = user.Role
    };
}