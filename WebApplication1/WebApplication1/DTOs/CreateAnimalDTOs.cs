using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOs
{
    public record CreateAnimalRequest(
        [Required] [MaxLength(200)] string Name,
        [MaxLength(200)] string Description,
        [Required] [MaxLength(200)] string Category,
        [Required] [MaxLength(200)] string Area
    );
   

    public record CreateAnimalResponse(int IdAnimal, string Name, string Description, string Category, string Area)
    {
        public CreateAnimalResponse(int idAnimal, CreateAnimalRequest request) : this(idAnimal, request.Name, request.Description, request.Category, request.Area) { }
    };
}