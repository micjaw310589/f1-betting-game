using F1BettingApp.Domain.Enums;

namespace F1BettingApp.Application.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public int Points { get; set; }
    }
}