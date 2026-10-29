using System.ComponentModel.DataAnnotations;

namespace MyFirstApi.DTOs;

public class CreateUserDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(50, MinimumLength = 3)]
    public string Name { get; set; } = "";
}
