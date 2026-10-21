namespace Matchday.Api.Dtos;

// Azerbaijani messages for the validation attributes on the DTOs.
// {1} and {2} are filled in by the attribute (for example the max length, or the range limits).
public static class ValidationMessages
{
    public const string Required = "Bu xana mütləqdir.";
    public const string Email = "Düzgün e-poçt daxil edin.";
    public const string MaxLength = "Maksimum {1} simvol ola bilər.";
    public const string MinLength = "Ən azı {1} simvol olmalıdır.";
    public const string Range = "{1} ilə {2} arasında olmalıdır.";
    public const string Lineup = "Heyətdə dəqiq 8 oyunçu olmalıdır.";
}
