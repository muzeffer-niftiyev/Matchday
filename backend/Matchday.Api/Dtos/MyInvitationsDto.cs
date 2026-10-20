namespace Matchday.Api.Dtos;

public record MyInvitationsDto(List<InvitationDto> Received, List<InvitationDto> Sent);