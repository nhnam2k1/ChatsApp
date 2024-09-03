using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

public class Auth0UserIdProvider : IUserIdProvider
{
    public string GetUserId(HubConnectionContext connection)
    {
        // Extract the 'sub' claim from the user's claims
        var value = connection.User
                        .FindFirst(ClaimTypes.NameIdentifier)
                        ?.Value 
                        ?? String.Empty;
        return value;
    }
}
