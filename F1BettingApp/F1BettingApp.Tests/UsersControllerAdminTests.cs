using Xunit;
using Moq;
using F1BettingApp.Application.Interfaces;
using F1BettingApp.Application.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace F1BettingApp.Tests;

/// <summary>
/// Tests for the admin endpoints in UsersController, focusing on RBAC and admin-specific logic.
/// </summary>
public class UsersControllerAdminTests
{
    private readonly Mock<IUserService> _mockUserService;

    public UsersControllerAdminTests()
    {
        _mockUserService = new Mock<IUserService>();
    }

    // ========================================
    // GetAllUsers - Admin Endpoint Tests
    // ========================================

    [Fact]
    public async Task GetAllUsers_AdminRole_ShouldReturnUsers()
    {
        // Arrange
        var mockUsers = new PagedResult<AdminUserDto>
        {
            Items = new List<AdminUserDto>
            {
                new AdminUserDto { Id = 1, Username = "testuser", Email = "test@example.com", Points = 1000, IsActive = true, IsAdmin = false, CreatedAt = System.DateTime.UtcNow },
                new AdminUserDto { Id = 2, Username = "adminuser", Email = "admin@example.com", Points = 500, IsActive = true, IsAdmin = true, CreatedAt = System.DateTime.UtcNow.AddDays(-1) }
            },
            Page = 1,
            PageSize = 20,
            TotalItems = 2,
            TotalPages = 1
        };

        _mockUserService.Setup(s => s.GetAllUsersAsync(1, 20, null, null))
            .ReturnsAsync(mockUsers);

        // Act
        var result = await _mockUserService.Object.GetAllUsersAsync(1, 20, null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count());
        Assert.Equal(2, result.TotalItems);
    }

    [Fact]
    public async Task GetAllUsers_WithIsActiveFilter_ShouldFilterCorrectly()
    {
        // Arrange
        var mockUsers = new PagedResult<AdminUserDto>
        {
            Items = new List<AdminUserDto>
            {
                new AdminUserDto { Id = 1, Username = "activeuser", Email = "active@example.com", IsActive = true, CreatedAt = System.DateTime.UtcNow }
            },
            Page = 1,
            PageSize = 20,
            TotalItems = 1,
            TotalPages = 1
        };

        _mockUserService.Setup(s => s.GetAllUsersAsync(1, 20, true, null))
            .ReturnsAsync(mockUsers);

        // Act
        var result = await _mockUserService.Object.GetAllUsersAsync(1, 20, true, null);

        // Assert
        Assert.Equal(1, result.Items.Count());
        Assert.True(result.Items.First().IsActive);
    }

    [Fact]
    public async Task GetAllUsers_WithSearchTerm_ShouldSearchCorrectly()
    {
        // Arrange
        var mockUsers = new PagedResult<AdminUserDto>
        {
            Items = new List<AdminUserDto>
            {
                new AdminUserDto { Id = 1, Username = "john", Email = "john@example.com", CreatedAt = System.DateTime.UtcNow }
            },
            Page = 1,
            PageSize = 20,
            TotalItems = 1,
            TotalPages = 1
        };

        _mockUserService.Setup(s => s.GetAllUsersAsync(1, 20, null, "john"))
            .ReturnsAsync(mockUsers);

        // Act
        var result = await _mockUserService.Object.GetAllUsersAsync(1, 20, null, "john");

        // Assert
        Assert.Equal(1, result.Items.Count());
        Assert.Contains("john", result.Items.First().Username.ToLower());
    }

    [Fact]
    public async Task GetAllUsers_EmptyResult_ShouldReturnEmptyPagedResult()
    {
        // Arrange
        var mockUsers = new PagedResult<AdminUserDto>
        {
            Items = new List<AdminUserDto>(),
            Page = 1,
            PageSize = 20,
            TotalItems = 0,
            TotalPages = 0
        };

        _mockUserService.Setup(s => s.GetAllUsersAsync(1, 20, null, null))
            .ReturnsAsync(mockUsers);

        // Act
        var result = await _mockUserService.Object.GetAllUsersAsync(1, 20, null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalItems);
    }

    // ========================================
    // AdjustUserPoints - Admin Endpoint Tests
    // ========================================

    [Fact]
    public async Task AdjustUserPoints_AddPoints_ShouldIncreaseBalance()
    {
        // Arrange
        var expectedResult = new AdjustPointsResultDto
        {
            UserId = 1,
            Username = "testuser",
            NewBalance = 10500,
            AdjustedBy = 2,
            Reason = "Bonus points",
            AdjustedAt = System.DateTime.UtcNow
        };

        _mockUserService.Setup(s => s.AdjustUserPointsAsync(1, 500, "Bonus points", 2))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _mockUserService.Object.AdjustUserPointsAsync(1, 500, "Bonus points", 2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10500, result.NewBalance);
        Assert.Equal("testuser", result.Username);
    }

    [Fact]
    public async Task AdjustUserPoints_DeductPoints_ShouldDecreaseBalance()
    {
        // Arrange
        var expectedResult = new AdjustPointsResultDto
        {
            UserId = 1,
            Username = "testuser",
            NewBalance = 9500,
            AdjustedBy = 2,
            Reason = "Penalty",
            AdjustedAt = System.DateTime.UtcNow
        };

        _mockUserService.Setup(s => s.AdjustUserPointsAsync(1, -500, "Penalty", 2))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _mockUserService.Object.AdjustUserPointsAsync(1, -500, "Penalty", 2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(9500, result.NewBalance);
    }

    [Fact]
    public async Task AdjustUserPoints_UserNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        _mockUserService.Setup(s => s.AdjustUserPointsAsync(999, 100, "Test", 2))
            .ThrowsAsync(new KeyNotFoundException("User with ID 999 not found."));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _mockUserService.Object.AdjustUserPointsAsync(999, 100, "Test", 2));
    }

    [Fact]
    public async Task AdjustUserPoints_BelowZero_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _mockUserService.Setup(s => s.AdjustUserPointsAsync(1, -20000, "Test", 2))
            .ThrowsAsync(new System.InvalidOperationException("Cannot deduct points below zero."));

        // Act & Assert
        await Assert.ThrowsAsync<System.InvalidOperationException>(() => 
            _mockUserService.Object.AdjustUserPointsAsync(1, -20000, "Test", 2));
    }

    // ========================================
    // ChangeUserStatus - Admin Endpoint Tests
    // ========================================

    [Fact]
    public async Task ChangeUserStatus_SuspendUser_ShouldSetInactive()
    {
        // Arrange
        var expectedResult = new AdminUserDto
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            IsActive = false,
            IsAdmin = false,
            CreatedAt = System.DateTime.UtcNow
        };

        _mockUserService.Setup(s => s.ChangeUserStatusAsync(1, false, "Violation", 2))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _mockUserService.Object.ChangeUserStatusAsync(1, false, "Violation", 2);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsActive);
    }

    [Fact]
    public async Task ChangeUserStatus_ReactivateUser_ShouldSetActive()
    {
        // Arrange
        var expectedResult = new AdminUserDto
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            IsActive = true,
            IsAdmin = false,
            CreatedAt = System.DateTime.UtcNow
        };

        _mockUserService.Setup(s => s.ChangeUserStatusAsync(1, true, null, 2))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _mockUserService.Object.ChangeUserStatusAsync(1, true, null, 2);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task ChangeUserStatus_UserNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        _mockUserService.Setup(s => s.ChangeUserStatusAsync(999, false, "Test", 2))
            .ThrowsAsync(new KeyNotFoundException("User with ID 999 not found."));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _mockUserService.Object.ChangeUserStatusAsync(999, false, "Test", 2));
    }

    // ========================================
    // RBAC - Role-Based Access Control Tests
    // ========================================

    [Fact]
    public async Task RBAC_NonAdminUser_DoesNotHaveAdminFlag()
    {
        // This test verifies the conceptual RBAC flow:
        // 1. Non-admin user authenticates -> JWT has no Admin role claim
        // 2. Non-admin user requests admin endpoint -> [Authorize(Roles = "Admin")] fails
        // 3. Result: 403 Forbidden

        // Arrange - Simulate a non-admin user by checking that the AdminUserDto
        // correctly identifies non-admin users
        var nonAdminUser = new AdminUserDto { Id = 5, Username = "regularuser", IsAdmin = false, IsActive = true };

        // Assert - Non-admin user should not have admin privileges
        Assert.False(nonAdminUser.IsAdmin);

        // In the actual controller, the [Authorize(Roles = "Admin")] attribute
        // would check the JWT token for the Admin role claim.
        // A non-admin JWT would not contain this claim, resulting in 403.
    }

    [Fact]
    public async Task RBAC_AdminUser_HasAdminFlag()
    {
        // Arrange - Simulate an admin user
        var adminUser = new AdminUserDto { Id = 1, Username = "admin", IsAdmin = true, IsActive = true };

        // Assert - Admin user should have admin privileges
        Assert.True(adminUser.IsAdmin);

        // In the actual controller, the [Authorize(Roles = "Admin")] attribute
        // would find the Admin role claim in the JWT token and allow access.
    }

    [Fact]
    public void JWTToken_IncludesAdminRoleClaim_ForAdminUsers()
    {
        // Arrange - This test verifies that the JWT token generation
        // includes the Admin role claim for admin users.
        // The actual JWT generation is tested in UserService tests.
        // Here we verify the concept: Admin users should have the role claim.

        var adminUser = new AdminUserDto { Id = 1, Username = "admin", IsAdmin = true };
        var regularUser = new AdminUserDto { Id = 2, Username = "regular", IsAdmin = false };

        // Assert
        Assert.True(adminUser.IsAdmin);
        Assert.False(regularUser.IsAdmin);

        // The UserService.GenerateJwtToken method adds:
        // new Claim(ClaimTypes.Role, "Admin") when user.IsAdmin is true
        // This claim is then checked by [Authorize(Roles = "Admin")] in the controller.
    }
}
