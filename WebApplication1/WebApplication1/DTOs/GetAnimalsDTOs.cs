namespace WebApplication1.DTOs;

public record GetAnimalsResponse(int IdAnimals, string Name, string? Description, string Category, string Area);