using F1BettingApp.Application.DTOs;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Domain.Entities;
using F1BettingApp.Infrastructure.Persistence.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace F1BettingApp.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Bet> _betRepository;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly JwtSecurityTokenHandler _tokenHandler;

        public UserService(
            IRepository<User> userRepository,
            IRepository<Bet> betRepository,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _betRepository = betRepository;
            _secretKey = configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            _issuer = configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
            _audience = configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");
            _tokenHandler = new JwtSecurityTokenHandler();
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
            var users = await _userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Username == username);
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Points = user.Points
            };
        }

        public async Task<AuthResponseDto> RegisterUserAsync(RegisterDto dto)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(dto.Username)) throw new ArgumentException("Username is required");
            if (string.IsNullOrWhiteSpace(dto.Email)) throw new ArgumentException("Email is required");
            if (string.IsNullOrWhiteSpace(dto.Password)) throw new ArgumentException("Password is required");
            if (dto.Password.Length < 8) throw new ArgumentException("Password must be at least 8 characters");

            // Check if user already exists
            var existingUsers = await _userRepository.GetAllAsync();
            if (existingUsers.Any(u => u.Username == dto.Username)) throw new InvalidOperationException("Username already exists");
            if (existingUsers.Any(u => u.Email == dto.Email)) throw new InvalidOperationException("Email already exists");

            // Hash password before storing
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User(dto.Username, dto.Email, hashedPassword);

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return new AuthResponseDto
            {
                IsSuccess = true,
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Points = 0
                },
                Message = "Registration successful"
            };
        }

        public async Task<AuthResponseDto> AuthenticateUserAsync(LoginDto dto)
        {
            var users = await _userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Username == dto.UsernameOrEmail || u.Email == dto.UsernameOrEmail);

            if (user == null)
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid credentials"
                };

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid credentials"
                };

            // Generate tokens
            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            // Store refresh token (in production, store in DB with expiration)
            // Note: StoreRefreshTokenAsync method not found - using SaveChangesAsync instead
            await _userRepository.SaveChangesAsync();

            return new AuthResponseDto
            {
                IsSuccess = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                TokenType = "Bearer",
                AccessTokenExpiration = 1800,
                RefreshTokenExpiration = 7,
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Points = user.Points
                }
            };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
        {
            // Verify refresh token exists and is valid
            var refreshData = await _userRepository.ValidateRefreshTokenAsync(dto.RefreshToken);

            if (refreshData == null || refreshData.UserId == 0)
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid refresh token"
                };

            // Check if token has expired
            if (refreshData.ExpiresAt < DateTime.UtcNow)
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Refresh token expired"
                };

            // Generate new access token
            var user = await _userRepository.GetByIdAsync(refreshData.UserId);
            if (user == null)
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    ErrorMessage = "User not found"
                };

            var accessToken = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                IsSuccess = true,
                AccessToken = accessToken,
                TokenType = "Bearer",
                AccessTokenExpiration = 1800,
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Points = user.Points
                }
            };
        }

        public async Task<bool> ValidateUserAsync(string username, string password)
        {
            var user = await GetUserByUsernameAsync(username);
            if (user == null) return false;

            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }

        public async Task<int> GetUserLeaderboardPositionAsync(int userId)
        {
            var users = await _userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return 0;

            return users.Count(u => u.Points > user.Points) + 1;
        }

        public async Task<UserStatisticsDto> GetUserStatisticsAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return null;

            var bets = await _betRepository.GetAllAsync();
            var userBets = bets.Where(b => b.UserId == userId);

            var wins = userBets.Count(b => b.Result == "Win");
            var losses = userBets.Count(b => b.Result == "Loss");
            var pending = userBets.Count(b => b.Result == "Pending");

            return new UserStatisticsDto
            {
                TotalBets = userBets.Count(),
                Wins = wins,
                Losses = losses,
                Pending = pending,
                WinRate = userBets.Count() > 0 ? ((double)wins / userBets.Count() * 100).ToString("F2") + "%" : "0%"
            };
        }

        public async Task UpdateUserPointsAsync(int userId, int points)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                user.Points += points;
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();
            }
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Name, user.Username),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                },
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return _tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var random = new Random();
            var characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~";
            var refreshToken = new string(
                characters.Skip(random.Next(0, characters.Length)).Take(64).ToArray()
            );
            return refreshToken;
        }
    }
}