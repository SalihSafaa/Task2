using Microsoft.AspNetCore.Mvc;

namespace ProductCatalogApi;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserResponseDto>> Register(RegisterRequestDto request)
    {
        var result = await _userService.RegisterAsync(request);
        if (!result.IsSuccess)
            return Conflict(result.ErrorMessage);

        return Created("api/auth/register", result.Value);
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto request)
    {
        var result = await _userService.LoginAsync(request);
        if (!result.IsSuccess)
            return Unauthorized(result.ErrorMessage);

        return Ok(result.Value);
    }
}