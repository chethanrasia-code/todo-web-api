using TodoApi.Application.DTOs;

namespace TodoApi.Application.Interfaces
{
    public interface ITodoService
    {
        Task<List<TodoDto>> GetAllTodosAsync();
        Task<List<TodoDto>> GetAllAsync();
        Task<TodoDto> GetTodoByIdAsync(int id);
        Task<TodoDto> GetByIdAsync(int id);
        Task<TodoDto> CreateTodoAsync(TodoDto todoDto);
        Task<TodoDto> CreateAsync(TodoDto todoDto);
        Task<TodoDto> UpdateTodoAsync(int id, TodoDto todoDto);
        Task<TodoDto> UpdateAsync(int id, TodoDto todoDto);
        Task DeleteTodoAsync(int id);
        Task DeleteAsync(int id);
    }
}