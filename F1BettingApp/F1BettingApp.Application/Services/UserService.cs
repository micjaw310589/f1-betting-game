using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace F1BettingApp.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserDto> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Points = user.Points
            };
        }

        public async Task<UserDto> GetUserByUsernameAsync(string username)
        {
            var user = _userRepository.GetAll().FirstOrDefault(u => u.Username == username);
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Points = user.Points
            };
        }

        public async Task RegisterUserAsync(string username, string email, string password)
        {
            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = password, // In real app, hash it
                Points = 0
            };
            await _userRepository.AddAsync(user);
        }

        public async Task<bool> ValidateUserAsync(string username, string password)
        {
            var user = _userRepository.GetAll().FirstOrDefault(u => u.Username == username);
            return user != null && user.PasswordHash == password; // In real app, verify hash
        }
    }
}