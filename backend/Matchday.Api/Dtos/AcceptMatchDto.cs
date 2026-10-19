using System.ComponentModel.DataAnnotations;

namespace Matchday.Api.Dtos;

public record AcceptMatchDto(
    [Required, Length(8, 8)] List<int> PlayerIds
);