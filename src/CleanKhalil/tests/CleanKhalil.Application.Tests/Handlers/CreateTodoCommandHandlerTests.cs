using AutoMapper;
using CleanKhalil.Application.Commands;
using CleanKhalil.Application.DTOs;
using CleanKhalil.Application.Handlers;
using CleanKhalil.Application.Mappings;
using CleanKhalil.Domain.Entities;
using CleanKhalil.Domain.Enums;
using CleanKhalil.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace CleanKhalil.Application.Tests.Handlers;

public class CreateTodoCommandHandlerTests
{
    private readonly Mock<ITodoRepository> _mockRepository;
    private readonly IMapper _mapper;
    private readonly CreateTodoCommandHandler _handler;

    public CreateTodoCommandHandlerTests()
    {
        _mockRepository = new Mock<ITodoRepository>();
        
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<TodoMappingProfile>();
        });
        _mapper = configuration.CreateMapper();
        
        _handler = new CreateTodoCommandHandler(_mockRepository.Object, _mapper);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateTodoAndReturnDto()
    {
        // Arrange
        var createDto = new CreateTodoItemDto
        {
            Title = "Test Todo",
            Description = "Test Description",
            Priority = TodoPriority.High,
            Tags = "test,important"
        };
        
        var command = new CreateTodoCommand(createDto);
        
        var createdTodo = new TodoItem
        {
            Id = 1,
            Title = createDto.Title,
            Description = createDto.Description,
            Priority = createDto.Priority,
            Tags = createDto.Tags,
            CreatedAt = DateTime.UtcNow
        };

        _mockRepository
            .Setup(x => x.CreateAsync(It.IsAny<TodoItem>()))
            .ReturnsAsync(createdTodo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Title.Should().Be("Test Todo");
        result.Description.Should().Be("Test Description");
        result.Priority.Should().Be(TodoPriority.High);
        result.Tags.Should().Be("test,important");
        result.IsCompleted.Should().BeFalse();
        
        _mockRepository.Verify(x => x.CreateAsync(It.Is<TodoItem>(t => 
            t.Title == createDto.Title &&
            t.Description == createDto.Description &&
            t.Priority == createDto.Priority &&
            t.Tags == createDto.Tags &&
            !t.IsCompleted
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDueDate_ShouldSetDueDate()
    {
        // Arrange
        var dueDate = DateTime.UtcNow.AddDays(7);
        var createDto = new CreateTodoItemDto
        {
            Title = "Todo with due date",
            DueDate = dueDate,
            Priority = TodoPriority.Medium
        };
        
        var command = new CreateTodoCommand(createDto);
        
        var createdTodo = new TodoItem
        {
            Id = 1,
            Title = createDto.Title,
            DueDate = dueDate,
            Priority = createDto.Priority,
            CreatedAt = DateTime.UtcNow
        };

        _mockRepository
            .Setup(x => x.CreateAsync(It.IsAny<TodoItem>()))
            .ReturnsAsync(createdTodo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.DueDate.Should().Be(dueDate);
        
        _mockRepository.Verify(x => x.CreateAsync(It.Is<TodoItem>(t => 
            t.DueDate == dueDate
        )), Times.Once);
    }

    [Theory]
    [InlineData(TodoPriority.Low)]
    [InlineData(TodoPriority.Medium)]
    [InlineData(TodoPriority.High)]
    [InlineData(TodoPriority.Urgent)]
    public async Task Handle_WithDifferentPriorities_ShouldSetPriorityCorrectly(TodoPriority priority)
    {
        // Arrange
        var createDto = new CreateTodoItemDto
        {
            Title = $"Todo with {priority} priority",
            Priority = priority
        };
        
        var command = new CreateTodoCommand(createDto);
        
        var createdTodo = new TodoItem
        {
            Id = 1,
            Title = createDto.Title,
            Priority = priority,
            CreatedAt = DateTime.UtcNow
        };

        _mockRepository
            .Setup(x => x.CreateAsync(It.IsAny<TodoItem>()))
            .ReturnsAsync(createdTodo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Priority.Should().Be(priority);
        
        _mockRepository.Verify(x => x.CreateAsync(It.Is<TodoItem>(t => 
            t.Priority == priority
        )), Times.Once);
    }
} 