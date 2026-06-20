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
    public class TasksController : ControllerBase
    {
        private readonly PlantopiaDbContext _context;

        public TasksController(PlantopiaDbContext context)
        {
            _context = context;
        }

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

            return Ok(tasks);
        }

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

            return Ok(tasks);
        }

        [HttpPost("create-task")]
        public async Task<IActionResult> CreateTask([FromBody] CreateUserTaskRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId);
                if (!userExists)
                {
                    return BadRequest(new { message = "Пользователь не найден." });
                }

                var nameRegex = @"^[а-яА-ЯёЁa-zA-Z\s]+$";
                if (!string.IsNullOrWhiteSpace(request.Title))
                {
                    if (!Regex.IsMatch(request.Title.Trim(), nameRegex))
                    {
                        return BadRequest(new { message = "Название задачи должно содержать только буквы." });
                    }
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

        [HttpPatch("{id:int}/toggle-complete")]
        public async Task<IActionResult> ToggleTaskCompletion(int id)
        {
            var task = await _context.UserTasks.FindAsync(id);
            if (task == null)
            {
                return NotFound(new { message = "Задача не найдена." });
            }

            task.Completed = !task.Completed;
            task.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                
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

            return NoContent();
        }

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