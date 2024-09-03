using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using Microsoft.AspNetCore.Mvc;

public class UserService : ControllerBase
{
    private readonly IManagementApiClient _managementApiClient;
    private readonly ILogger<UserService> _logger;

    public UserService(IManagementApiClient managementApiClient, ILogger<UserService> logger)
    {
        _managementApiClient = managementApiClient;
        _logger = logger;
    }

    public async Task<IResult> GetUsers(HttpRequest request)
    {
        try
        {
            var currentUserId = request.Query["current_user_id"].ToString();

            if (string.IsNullOrEmpty(currentUserId))
            {
                var error = new { error = "current_user_id parameter is required" };
                return Results.BadRequest(error);
            }

            var users = await _managementApiClient.Users.GetAllAsync(new GetUsersRequest());

            var filteredUsers = users
                .Where(user => user.UserId != currentUserId)
                .Select(user => new
                {
                    id = user.UserId,
                    name = user.NickName,
                    picture = user.Picture
                });

            return Results.Ok(filteredUsers);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Results.StatusCode(500);
        }
    }
}