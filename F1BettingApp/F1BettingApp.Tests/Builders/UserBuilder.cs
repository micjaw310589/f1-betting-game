using F1BettingApp.Domain.Entities;

namespace F1BettingApp.Tests.Builders
{
    /// <summary>
    /// Builder for creating test User entities
    /// </summary>
    public class UserBuilder
    {
        private int _id = 1;
        private string _username = "testuser";
        private string _email = "test@example.com";
        private string _passwordHash = "hashedpassword";
        private int _points = 1000;
        private DateTime _createdAt = DateTime.UtcNow;
        private string? _profileImageUrl = null;
        private DateTime? _lastLogin = null;
        private bool _isActive = true;
        private bool _isAdmin = false;

        /// <summary>
        /// Sets the user ID
        /// </summary>
        /// <param name="id">The user ID</param>
        /// <returns>The builder instance</returns>
        public UserBuilder WithId(int id)
        {
            _id = id;
            return this;
        }

        /// <summary>
        /// Sets the username
        /// </summary>
        /// <param name="username">The username</param>
        /// <returns>The builder instance</returns>
        public UserBuilder WithUsername(string username)
        {
            _username = username;
            return this;
        }

        /// <summary>
        /// Sets the email
        /// </summary>
        /// <param name="email">The email</param>
        /// <returns>The builder instance</returns>
        public UserBuilder WithEmail(string email)
        {
            _email = email;
            return this;
        }

        /// <summary>
        /// Sets the password hash
        /// </summary>
        /// <param name="passwordHash">The password hash</param>
        /// <returns>The builder instance</returns>
        public UserBuilder WithPasswordHash(string passwordHash)
        {
            _passwordHash = passwordHash;
            return this;
        }

        /// <summary>
        /// Sets the points
        /// </summary>
        /// <param name="points">The points</param>
        /// <returns>The builder instance</returns>
        public UserBuilder WithPoints(int points)
        {
            _points = points;
            return this;
        }

        /// <summary>
        /// Sets the user as admin
        /// </summary>
        /// <returns>The builder instance</returns>
        public UserBuilder AsAdmin()
        {
            _isAdmin = true;
            return this;
        }

        /// <summary>
        /// Sets the user as inactive
        /// </summary>
        /// <returns>The builder instance</returns>
        public UserBuilder AsInactive()
        {
            _isActive = false;
            return this;
        }

        /// <summary>
        /// Sets the profile image URL
        /// </summary>
        /// <param name="profileImageUrl">The profile image URL</param>
        /// <returns>The builder instance</returns>
        public UserBuilder WithProfileImageUrl(string profileImageUrl)
        {
            _profileImageUrl = profileImageUrl;
            return this;
        }

        /// <summary>
        /// Sets the last login date
        /// </summary>
        /// <param name="lastLogin">The last login date</param>
        /// <returns>The builder instance</returns>
        public UserBuilder WithLastLogin(DateTime lastLogin)
        {
            _lastLogin = lastLogin;
            return this;
        }

        /// <summary>
        /// Builds the User entity
        /// </summary>
        /// <returns>The constructed User entity</returns>
        public User Build()
        {
            // Use the User constructor for validation
            var user = new User(_username, _email, _passwordHash, _isActive, _isAdmin)
            {
                Id = _id,
                Points = _points,
                CreatedAt = _createdAt,
                ProfileImageUrl = _profileImageUrl,
                LastLogin = _lastLogin
            };

            return user;
        }

        /// <summary>
        /// Builds a list of users with sequential IDs
        /// </summary>
        /// <param name="count">The number of users to create</param>
        /// <returns>List of User entities</returns>
        public List<User> BuildList(int count)
        {
            var users = new List<User>();
            for (int i = 0; i < count; i++)
            {
                users.Add(WithId(i + 1).Build());
            }
            return users;
        }
    }
}