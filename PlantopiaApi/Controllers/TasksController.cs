using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantopiaApi.Data;
using PlantopiaApi.Models;
using PlantopiaApi.Units;

namespace PlantopiaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly PlantopiaDbContext _context;

        public TasksController(PlantopiaDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Возвращает задачи пользователя по его ID
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        [HttpGet("{userId:int}")]
        public async Task<IActionResult> GetUserTasks(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest(new { message = "Неверный ID пользователя." });
            }

            var tasks = await _context.UserTasks
                .Where(t => t.UserId == userId)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.DueDate,
                    t.Completed,
                    t.Category,
                    t.CreatedAt,
                    t.UpdatedAt
                })
                .ToListAsync();

            if (tasks.Count == 0)
            {
                return NotFound(new { message = $"Задачи для пользователя с ID {userId} не найдены." });
            }

            return Ok(tasks);
        }

        /// <summary>
        /// Альтернативный метод: принимает userId в теле POST-запроса
        /// </summary>
        [HttpPost("by-user-id")]
        public async Task<IActionResult> GetUserTasksByPost([FromBody] UserIdRequest request)
        {
            if (request?.UserId <= 0)
            {
                return BadRequest(new { message = "Неверный ID пользователя." });
            }

            var tasks = await _context.UserTasks
                .Where(t => t.UserId == request.UserId)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.DueDate,
                    t.Completed,
                    t.Category,
                    t.CreatedAt,
                    t.UpdatedAt
                })
                .ToListAsync();

            if (tasks.Count == 0)
            {
                return NotFound(new { message = $"Задачи для пользователя с ID {request.UserId} не найдены." });
            }

            return Ok(tasks);
        }

        /// <summary>
        /// Создаёт новую задачу для пользователя
        /// </summary>
        [HttpPost("create-task")]
        public async Task<IActionResult> CreateTask([FromBody] CreateUserTaskRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Проверяем, существует ли пользователь
                var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId);
                if (!userExists)
                {
                    return BadRequest(new { message = "Пользователь не найден." });
                }

                var task = new UserTask
                {
                    UserId = request.UserId,
                    Title = request.Title,
                    Description = request.Description,
                    DueDate = DateTime.SpecifyKind(request.DueDate, DateTimeKind.Utc),
                    Category = request.Category,
                    Completed = request.Completed ?? false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.UserTasks.Add(task);
                await _context.SaveChangesAsync();

                var response = new
                {
                    task.Id,
                    task.Title,
                    task.Description,
                    task.DueDate,
                    task.Completed,
                    task.Category,
                    task.CreatedAt,
                    task.UpdatedAt
                };

                return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании задачи: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { message = "Ошибка сервера при создании задачи", details = ex.Message });
            }
        }

        /// <summary>
        /// Обновляет статус выполнения задачи (completed)
        /// </summary>
        [HttpPatch("{id:int}/toggle-complete")]
        public async Task<IActionResult> ToggleTaskCompletion(int id)
        {
            var task = await _context.UserTasks.FindAsync(id);
            if (task == null)
            {
                return NotFound(new { message = "Задача не найдена." });
            }

            task.Completed = !task.Completed; // инвертируем статус
            task.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                
                // Возвращаем обновлённый статус
                var response = new
                {
                    task.Id,
                    task.Completed
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении статуса задачи: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { message = "Ошибка сервера при обновлении задачи", details = ex.Message });
            }
        }

        /// <summary>
        /// Удаляет задачу по ID
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.UserTasks.FindAsync(id);
            if (task == null)
            {
                return NotFound(new { message = "Задача не найдена." });
            }

            _context.UserTasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent(); // 204
        }

        /// <summary>
        /// Возвращает задачу по ID
        /// </summary>
        /// <param name="id">ID задачи</param>
        [HttpGet("detail/{id:int}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            var task = await _context.UserTasks
                .Where(t => t.Id == id)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.DueDate,
                    t.Completed,
                    t.Category,
                    t.CreatedAt,
                    t.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (task == null)
            {
                return NotFound(new { message = "Задача не найдена." });
            }

            return Ok(task);
        }
    }
}