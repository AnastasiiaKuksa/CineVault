using System.ComponentModel.DataAnnotations;

namespace CineVault.API.Requests;

public class OmdbSearchRequest
{
    // Обов'язковий, мінімум 3 символи
    [Required]
    [MinLength(3, ErrorMessage = "Search filter must be at least 3 characters")]
    public string SearchFilter { get; set; } = null!;

    // Необов'язковий, від 1900 до 2026
    [Range(1900, 2026, ErrorMessage = "Year must be between 1900 and 2026")]
    public int? YearOfRelease { get; set; }
}