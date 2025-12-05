namespace UserService.Application.DTOs.Requests
{
    public class CreateUserRequest
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; } // Handles initial password or delegating to Keycloak
    }
}
