using AutoMapper;
using CleanKhalil.Application.DTOs;
using CleanKhalil.Domain.Entities;

namespace CleanKhalil.Application.Mappings;

public class TodoMappingProfile : Profile
{
    public TodoMappingProfile()
    {
        CreateMap<TodoItem, TodoItemDto>()
            .ReverseMap();
            
        CreateMap<CreateTodoItemDto, TodoItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.CompletedAt, opt => opt.Ignore());
            
        CreateMap<UpdateTodoItemDto, TodoItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.CompletedAt, opt => opt.MapFrom((src, dest) => 
                src.IsCompleted && !dest.IsCompleted ? DateTime.UtcNow : 
                !src.IsCompleted && dest.IsCompleted ? null : dest.CompletedAt));
    }
} 