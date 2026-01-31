
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantopiaApi.Data;


namespace PlantopiaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly PlantopiaDbContext _context;
        private static readonly Dictionary<string, UserSession> _activeSessions = new();

        public AuthController(PlantopiaDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Проверка пароля (предполагается, что пароль хранится в открытом виде)
            if (user.Password != request.Password)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Генерация уникального ID сессии
            var sessionId = Guid.NewGuid().ToString();
            
            // Создание сессии
            var session = new UserSession
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
                SubscriptionStatus = user.SubscriptionStatus,
                UserRole = user.UserRole,
                ApiKey = user.ApiKey,
                CreatedAt = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow
            };

            // Сохранение сессии в памяти
            lock (_activeSessions)
            {
                _activeSessions[sessionId] = session;
            }

            var response = new LoginResponse
            {
                SessionId = sessionId,
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
                SubscriptionStatus = user.SubscriptionStatus,
                UserRole = user.UserRole,
                ApiKey = user.ApiKey
            };

            return Ok(response);
        }

        [HttpPost("logout")]
        public IActionResult Logout([FromBody] LogoutRequest request)
        {
            if (string.IsNullOrEmpty(request.SessionId))
            {
                return BadRequest(new { message = "Session ID is required" });
            }

            lock (_activeSessions)
            {
                if (_activeSessions.ContainsKey(request.SessionId))
                {
                    _activeSessions.Remove(request.SessionId);
                }
            }

            return Ok(new { message = "Successfully logged out" });
        }

        [HttpGet("validate")]
        public IActionResult ValidateSession([FromQuery] string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                return Unauthorized(new { message = "Session ID is required" });
            }

            UserSession session;
            lock (_activeSessions)
            {
                if (!_activeSessions.TryGetValue(sessionId, out session))
                {
                    return Unauthorized(new { message = "Invalid or expired session" });
                }
            }

            // Обновляем время последней активности
            session.LastActivity = DateTime.UtcNow;

            return Ok(new 
            { 
                isValid = true,
                userId = session.UserId,
                email = session.Email,
                firstName = session.FirstName,
                lastName = session.LastName,
                phone = session.Phone,
                subscriptionStatus = session.SubscriptionStatus,
                userRole = session.UserRole,
                apiKey = session.ApiKey
            });
        }
    }

    

    

    

    
}