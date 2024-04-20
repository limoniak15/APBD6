namespace WebApplication1.DTOs;

public record GetAnimalDetailsResponse(int Id, string FirstName, string LastName, string Phone, DateTime Birthdate);