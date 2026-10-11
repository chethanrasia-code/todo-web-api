using TodoApi.Application.DTOs;
using TodoApi.Application.Interfaces;
using TodoApi.Domain.Entities;
using TodoApi.Domain.Exceptions;

namespace TodoApi.Application.Services
{
    public class TodoService : ITodoService
    {
        private readonly ITodoRepository _todoRepository;

        public TodoService(ITodoRepository todoRepository)
        {
            _todoRepository = todoRepository;
        }

        public async Task<List<TodoDto>> GetAllTodosAsync()
        {
            var todoItems = await _todoRepository.GetAllAsync();
            return todoItems.Select(MapToDto).ToList();
        }

        public Task<List<TodoDto>> GetAllAsync()
        {
            return GetAllTodosAsync();
        }

        public async Task<TodoDto> GetTodoByIdAsync(int id)
        {
            var todoItem = await _todoRepository.GetByIdAsync(id);
            if (todoItem is null)
            {
                throw new NotFoundException($"Todo item with id {id} not found.");
            }

            return MapToDto(todoItem);
        }

        public Task<TodoDto> GetByIdAsync(int id)
        {
            return GetTodoByIdAsync(id);
        }

        public async Task<TodoDto> CreateTodoAsync(TodoDto todoDto)
        {
            var todoItem = new TodoItem
            {
                Title = todoDto.Title,
                Description = todoDto.Description,
                IsCompleted = todoDto.IsCompleted,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdTodo = await _todoRepository.AddAsync(todoItem);
            return MapToDto(createdTodo ?? todoItem);
        }

        public Task<TodoDto> CreateAsync(TodoDto todoDto)
        {
            return CreateTodoAsync(todoDto);
        }

        public async Task<TodoDto> UpdateTodoAsync(int id, TodoDto todoDto)
        {
            var todoItem = await _todoRepository.GetByIdAsync(id);
            if (todoItem is null)
            {
                todoItem = new TodoItem
                {
                    Id = id,
                    Title = todoDto.Title,
                    Description = todoDto.Description,
                    IsCompleted = todoDto.IsCompleted,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _todoRepository.AddAsync(todoItem);
                return MapToDto(todoItem);
            }

            todoItem.Title = todoDto.Title;
            todoItem.Description = todoDto.Description;
            todoItem.IsCompleted = todoDto.IsCompleted;
            todoItem.UpdatedAt = DateTime.UtcNow;

            await _todoRepository.UpdateAsync(todoItem);
            return MapToDto(todoItem);
        }

        public Task<TodoDto> UpdateAsync(int id, TodoDto todoDto)
        {
            return UpdateTodoAsync(id, todoDto);
        }

        public async Task DeleteTodoAsync(int id)
        {
            var todoItem = await _todoRepository.GetByIdAsync(id);
            if (todoItem is null)
            {
                throw new NotFoundException($"Todo item with id {id} not found.");
            }

            await _todoRepository.DeleteAsync(todoItem);
        }

        public Task DeleteAsync(int id)
        {
            return DeleteTodoAsync(id);
        }

        private static TodoDto MapToDto(TodoItem todoItem)
        {
            return new TodoDto
            {
                Id = todoItem.Id,
                Title = todoItem.Title,
                Description = todoItem.Description,
                IsCompleted = todoItem.IsCompleted,
                CreatedAt = todoItem.CreatedAt,
                UpdatedAt = todoItem.UpdatedAt
            };
        }
    }
}