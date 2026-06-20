using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantopiaApi.Data;
using PlantopiaApi.Models;
using PlantopiaApi.Units;
using System.Text.RegularExpressions;

namespace PlantopiaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly PlantopiaDbContext _context;
        
        private static readonly Dictionary<string, UserSession> _farmerSessions = new();
        private static readonly Dictionary<string, UserSession> _expertSessions = new();

        public AuthController(PlantopiaDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var emailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (string.IsNullOrWhiteSpace(request.Email) || !Regex.IsMatch(request.Email.Trim(), emailRegex))
            {
                return BadRequest(new { message = "Некорректный формат email." });
            }

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6 || request.Password.Length > 128)
            {
                return BadRequest(new { message = "Пароль должен содержать от 6 до 128 символов." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email.Trim().ToLowerInvariant());

            if (user == null || user.Password != request.Password)
            {
                return Unauthorized(new { message = "Неверный email или пароль" });
            }

            return CreateSessionAndResponse(
                _farmerSessions, 
                user.Id, 
                user.Email, 
                user.FirstName, 
                user.LastName, 
                user.Phone, 
                user.SubscriptionStatus, 
                user.UserRole ?? "farmer", 
                user.ApiKey, 
                user.NDVIApiKey,
                null, null, null, null, null, null
            );
        }

        [HttpPost("expert-login")]
        public async Task<IActionResult> ExpertLogin([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var emailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (string.IsNullOrWhiteSpace(request.Email) || !Regex.IsMatch(request.Email.Trim(), emailRegex))
            {
                return BadRequest(new { message = "Некорректный формат email." });
            }

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6 || request.Password.Length > 128)
            {
                return BadRequest(new { message = "Пароль должен содержать от 6 до 128 символов." });
            }

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var expert = await _context.Experts
                .FirstOrDefaultAsync(e => e.Email == normalizedEmail && e.Password == request.Password);

            if (expert == null)
            {
                return Unauthorized(new { message = "Неверный email или пароль эксперта" });
            }

            return CreateSessionAndResponse(
                _expertSessions, 
                expert.Id, 
                expert.Email, 
                expert.FirstName, 
                expert.LastName, 
                expert.Phone, 
                true, 
                "expert", 
                null, 
                null,
                expert.Specialization,
                expert.ExperienceYears,
                expert.HourlyRate,
                expert.Country,
                expert.Region,
                expert.City
            );
        }

        private IActionResult CreateSessionAndResponse(
            Dictionary<string, UserSession> sessionStore,
            int id, string email, string? firstName, string? lastName, string? phone, 
            bool subscriptionStatus, string role, string? apiKey, string? ndviApiKey,
            string? specialization, int? experienceYears, decimal? hourlyRate,
            string? country, string? region, string? city)
        {
            var sessionId = Guid.NewGuid().ToString();

            var session = new UserSession
            {
                UserId = id,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Phone = phone,
                SubscriptionStatus = subscriptionStatus,
                UserRole = role,
                ApiKey = apiKey,
                CreatedAt = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow,
                NDVIApiKey = ndviApiKey,
                
                Specialization = specialization,
                ExperienceYears = experienceYears,
                HourlyRate = hourlyRate,
                Country = country,
                Region = region,
                City = city
            };

            lock (sessionStore)
            {
                sessionStore[sessionId] = session;
            }

            var response = new LoginResponse
            {
                SessionId = sessionId,
                UserId = id,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Phone = phone,
                SubscriptionStatus = subscriptionStatus,
                UserRole = role,
                ApiKey = apiKey,
                NDVIApiKey = ndviApiKey,
                
                Specialization = specialization,
                ExperienceYears = experienceYears,
                HourlyRate = hourlyRate,
                Country = country,
                Region = region,
                City = city
            };

            return Ok(response);
        }

        [HttpPost("logout")]
        public IActionResult Logout([FromBody] LogoutRequest request)
        {
            if (string.IsNullOrEmpty(request.SessionId))
                return BadRequest(new { message = "Session ID is required" });

            bool removed = false;
            
            lock (_farmerSessions)
            {
                if (_farmerSessions.ContainsKey(request.SessionId))
                {
                    _farmerSessions.Remove(request.SessionId);
                    removed = true;
                }
            }

            if (!removed)
            {
                lock (_expertSessions)
                {
                    if (_expertSessions.ContainsKey(request.SessionId))
                    {
                        _expertSessions.Remove(request.SessionId);
                        removed = true;
                    }
                }
            }

            return removed 
                ? Ok(new { message = "Successfully logged out" }) 
                : NotFound(new { message = "Session not found" });
        }

        [HttpGet("validate")]
        public IActionResult ValidateSession([FromQuery] string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return Unauthorized(new { message = "Session ID is required" });

            UserSession? session = null;

            lock (_farmerSessions)
            {
                _farmerSessions.TryGetValue(sessionId, out session);
            }

            if (session == null)
            {
                lock (_expertSessions)
                {
                    _expertSessions.TryGetValue(sessionId, out session);
                }
            }

            if (session == null)
            {
                return Unauthorized(new { message = "Invalid or expired session" });
            }

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
                apiKey = session.ApiKey,
                ndvdiApiKey = session.NDVIApiKey,
                
                specialization = session.Specialization,
                experienceYears = session.ExperienceYears,
                hourlyRate = session.HourlyRate,
                country = session.Country,
                region = session.Region,
                city = session.City
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var emailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (string.IsNullOrWhiteSpace(request.Email) || !Regex.IsMatch(request.Email.Trim(), emailRegex))
            {
                return BadRequest(new { message = "Некорректный формат email." });
            }

            var phoneRegex = @"^\+7\d{10}$";
            if (string.IsNullOrWhiteSpace(request.Phone) || !Regex.IsMatch(request.Phone.Trim(), phoneRegex))
            {
                return BadRequest(new { message = "Номер телефона должен быть в формате +7XXXXXXXXXX." });
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6 || request.NewPassword.Length > 128)
            {
                return BadRequest(new { message = "Пароль должен содержать от 6 до 128 символов." });
            }

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == normalizedEmail && u.Phone == request.Phone.Trim());

            if (user == null)
                return Unauthorized(new { message = "Пользователь с такими данными не найден" });

            user.Password = request.NewPassword;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Пароль успешно изменен" });
        }

        [HttpPost("expert-forgot-password")]
        public async Task<IActionResult> ExpertForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var emailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (string.IsNullOrWhiteSpace(request.Email) || !Regex.IsMatch(request.Email.Trim(), emailRegex))
            {
                return BadRequest(new { message = "Некорректный формат email." });
            }

            var phoneRegex = @"^\+7\d{10}$";
            if (string.IsNullOrWhiteSpace(request.Phone) || !Regex.IsMatch(request.Phone.Trim(), phoneRegex))
            {
                return BadRequest(new { message = "Номер телефона должен быть в формате +7XXXXXXXXXX." });
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6 || request.NewPassword.Length > 128)
            {
                return BadRequest(new { message = "Пароль должен содержать от 6 до 128 символов." });
            }

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var expert = await _context.Experts
                .FirstOrDefaultAsync(e => e.Email == normalizedEmail && e.Phone == request.Phone.Trim());

            if (expert == null)
                return Unauthorized(new { message = "Эксперт с такими данными не найден" });

            expert.Password = request.NewPassword;
           
            await _context.SaveChangesAsync();

            return Ok(new { message = "Пароль эксперта успешно изменен" });
        }
    }
}