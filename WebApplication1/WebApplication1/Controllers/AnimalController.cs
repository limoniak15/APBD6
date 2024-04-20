using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using WebApplication1.DTOs;
using WebApplication1.Modules;

namespace WebApplication1.Controllers;

[Route("controller-animal")]
[ApiController]
public class AnimalsController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AnimalsController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult GetAnimals(string orderBy = "name")
    {
        if (!IsValidOrderBy(orderBy))
        {
            return BadRequest("Invalid orderBy parameter. Available values: name, description, category, area.");
        }

        List<Animal> animals = new List<Animal>();

        using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default")))
        {
            string query = $"SELECT * FROM Animals ORDER BY {orderBy} ASC";
            SqlCommand command = new SqlCommand(query, connection);

            try
            {
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    animals.Add(new Animal
                    {
                        IdAnimal = Convert.ToInt32(reader["IdAnimal"]),
                        Name = reader["Name"].ToString(),
                        Description = reader["Description"].ToString(),
                        Category = reader["Category"].ToString(),
                        Area = reader["Area"].ToString()
                    });
                }

                reader.Close();
                return Ok(animals);
            }
            catch (Exception ex)
            {
                return BadRequest("Error retrieving data: " + ex.Message);
            }
        }
    }

    // Metoda do sprawdzania poprawności parametru orderBy
    private bool IsValidOrderBy(string orderBy)
    {
        string[] validOrderBys = { "name", "description", "category", "area" };
        return Array.IndexOf(validOrderBys, orderBy.ToLower()) != -1;
    }


    [HttpGet("{id}")]
    public IActionResult GetAnimal(int id)
    {
        using var sqlConnection = new SqlConnection(_configuration.GetConnectionString("Default"));
        var sqlCommand = new SqlCommand("SELECT * FROM Animals WHERE IdAnimal = @IdAnimal", sqlConnection);
        sqlCommand.Parameters.AddWithValue("@IdAnimal", id);
        sqlCommand.Connection.Open();

        var reader = sqlCommand.ExecuteReader();
        if (!reader.Read()) return NotFound();

        return Ok(new Animal
        {
            IdAnimal = reader.GetInt32(0),
            Name = reader.GetString(1),
            Description = reader.IsDBNull(2) ? null : reader.GetString(2),
            Category = reader.GetString(3),
            Area = reader.GetString(4)
        });
    }

    [HttpPost]
    public IActionResult CreateAnimal(CreateAnimalRequest request)
    {
        // Sprawdź, czy dane wejściowe są poprawne
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Dodaj nowe zwierzę do bazy danych
        using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default")))
        {
            string query =
                "INSERT INTO Animals (Name, Description, Category, Area) VALUES (@Name, @Description, @Category, @Area); SELECT SCOPE_IDENTITY();";
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Name", request.Name);
            command.Parameters.AddWithValue("@Description", request.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Category", request.Category);
            command.Parameters.AddWithValue("@Area", request.Area);

            connection.Open();
            int id = Convert.ToInt32(command.ExecuteScalar());
            return Created($"/api/animals/{id}", new CreateAnimalResponse(id, request));
        }
    }

    [HttpPut("{id}")]
    public IActionResult UpdateAnimal(int id, [FromBody] UpdateAnimalRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default")))
        {
            string query =
                "UPDATE Animals SET Name = @Name, Description = @Description, Category = @Category, Area = @Area WHERE IdAnimal = @IdAnimal";
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Name", request.Name);
            command.Parameters.AddWithValue("@Description", request.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Category", request.Category);
            command.Parameters.AddWithValue("@Area", request.Area);
            command.Parameters.AddWithValue("@IdAnimal", id);

            try
            {
                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    return NotFound("Animal not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest("Error updating animal: " + ex.Message);
            }
        }
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteAnimal(int id)
    {
        using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default")))
        {
            string query = "DELETE FROM Animals WHERE IdAnimal = @IdAnimal";
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@IdAnimal", id);

            try
            {
                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    return NotFound("Animal not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest("Error deleting animal: " + ex.Message);
            }
        }
    }
}