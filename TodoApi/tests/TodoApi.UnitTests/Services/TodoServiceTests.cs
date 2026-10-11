using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using TodoApi.Application.DTOs;
using TodoApi.Application.Interfaces;
using TodoApi.Application.Services;
using TodoApi.Domain.Entities;
using TodoApi.Domain.Exceptions;

namespace TodoApi.UnitTests.Services
{
    public class TodoServiceTests
    {
        private readonly Mock<ITodoRepository> _mockTodoRepository;
        private readonly TodoService _todoService;

        public TodoServiceTests()
        {
            _mockTodoRepository = new Mock<ITodoRepository>();
            _todoService = new TodoService(_mockTodoRepository.Object);
        }

        [Fact]
        public async Task GetAllTodos_ReturnsListOfTodos()
        {
            var todos = new List<TodoItem>
            {
                new TodoItem { Id = 1, Title = "Test Todo 1", IsCompleted = false },
                new TodoItem { Id = 2, Title = "Test Todo 2", IsCompleted = true }
            };

            _mockTodoRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(todos);

            var result = await _todoService.GetAllTodosAsync();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetTodoById_ExistingId_ReturnsTodo()
        {
            var todo = new TodoItem { Id = 1, Title = "Test Todo", IsCompleted = false };

            _mockTodoRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(todo);

            var result = await _todoService.GetTodoByIdAsync(1);

            Assert.Equal("Test Todo", result.Title);
        }

        [Fact]
        public async Task GetTodoById_NonExistingId_ThrowsNotFoundException()
        {
            _mockTodoRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((TodoItem)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _todoService.GetTodoByIdAsync(1));
        }

        [Fact]
        public async Task CreateTodo_ValidTodo_ReturnsCreatedTodo()
        {
            var todoDto = new TodoDto { Title = "New Todo", IsCompleted = false };
            var todoItem = new TodoItem { Id = 1, Title = "New Todo", IsCompleted = false };

            _mockTodoRepository.Setup(repo => repo.AddAsync(It.IsAny<TodoItem>())).ReturnsAsync(todoItem);

            var result = await _todoService.CreateTodoAsync(todoDto);

            Assert.Equal("New Todo", result.Title);
        }

        [Fact]
        public async Task UpdateTodo_ExistingId_UpdatesTodo()
        {
            var todoDto = new TodoDto { Title = "Updated Todo", IsCompleted = true };
            var todoItem = new TodoItem { Id = 1, Title = "Old Todo", IsCompleted = false };

            _mockTodoRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(todoItem);
            _mockTodoRepository.Setup(repo => repo.UpdateAsync(todoItem)).Returns(Task.CompletedTask);

            await _todoService.UpdateTodoAsync(1, todoDto);

            Assert.Equal("Updated Todo", todoItem.Title);
            Assert.True(todoItem.IsCompleted);
        }

        [Fact]
        public async Task DeleteTodo_ExistingId_DeletesTodo()
        {
            var todoItem = new TodoItem { Id = 1, Title = "Todo to Delete", IsCompleted = false };

            _mockTodoRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(todoItem);
            _mockTodoRepository.Setup(repo => repo.DeleteAsync(todoItem)).Returns(Task.CompletedTask);

            await _todoService.DeleteTodoAsync(1);

            _mockTodoRepository.Verify(repo => repo.DeleteAsync(todoItem), Times.Once);
        }
    }
}