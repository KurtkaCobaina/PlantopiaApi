using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantopiaApi.Data;
using PlantopiaApi.Models;
using PlantopiaApi.Units;

namespace PlantopiaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly PlantopiaDbContext _context;
        
        // Отдельные хранилища для сессий фермеров и экспертов
        private static readonly Dictionary<string, UserSession> _farmerSessions = new();
        private static readonly Dictionary<string, UserSession> _expertSessions = new();

        public AuthController(PlantopiaDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Авторизация Фермера
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || user.Password != request.Password)
            {
                return Unauthorized(new { message = "Неверный email или пароль" });
            }

            // Создаем сессию фермера (доп. поля будут null)
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
                null, null, null, null, null, null // Специализация, Опыт, Ставка, Страна, Регион, Город
            );
        }

        /// <summary>
        /// Авторизация Эксперта
        /// </summary>
        [HttpPost("expert-login")]
        public async Task<IActionResult> ExpertLogin([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var expert = await _context.Experts
                .FirstOrDefaultAsync(e => e.Email == request.Email && e.Password == request.Password);

            if (expert == null)
            {
                return Unauthorized(new { message = "Неверный email или пароль эксперта" });
            }

            // Создаем сессию эксперта с передачей всех дополнительных данных
            return CreateSessionAndResponse(
                _expertSessions, 
                expert.Id, 
                expert.Email, 
                expert.FirstName, 
                expert.LastName, 
                expert.Phone, 
                true, // SubscriptionStatus активен по умолчанию для экспертов
                "expert", 
                null, // ApiKey у экспертов обычно нет
                null, // NDVIApiKey у экспертов обычно нет
                
                // --- ЗАПОЛНЯЕМ СПЕЦИФИЧНЫЕ ДАННЫЕ ЭКСПЕРТА ---
                expert.Specialization,
                expert.ExperienceYears,
                expert.HourlyRate,
                expert.Country,
                expert.Region,
                expert.City
            );
        }

        /// <summary>
        /// Универсальный метод создания сессии
        /// </summary>
        private IActionResult CreateSessionAndResponse(
            Dictionary<string, UserSession> sessionStore,
            int id, string email, string? firstName, string? lastName, string? phone, 
            bool subscriptionStatus, string role, string? apiKey, string? ndviApiKey,
            
            // Параметры для эксперта
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
                
                // Заполняем новые поля
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
                
                // Заполняем новые поля ответа
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
                
                // Возвращаем данные эксперта при валидации сессии тоже
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

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.Phone == request.Phone);

            if (user == null)
                return Unauthorized(new { message = "Пользователь с такими данными не найден" });

            user.Password = request.NewPassword;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Пароль успешно изменен" });
        }
        
        public class ForgotPasswordRequest
        {
            public string Email { get; set; }
            public string Phone { get; set; }
            public string NewPassword { get; set; }
        }
    }
}