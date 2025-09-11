using Kont.backend.Services;

namespace Kont.backend.tests.Services;

[TestFixture]
public class PasswordServiceTests
{
    private IPasswordService _passwordService;

    [SetUp]
    public void Setup()
    {
        _passwordService = new PasswordService();
    }

    [Test]
    public void HashPassword_ShouldReturnDifferentHashForSamePassword()
    {
        // Arrange
        var password = "testPassword123";

        // Act
        var hash1 = _passwordService.HashPassword(password);
        var hash2 = _passwordService.HashPassword(password);

        // Assert
        Assert.That(hash1, Is.Not.Null);
        Assert.That(hash2, Is.Not.Null);
        Assert.That(hash1, Is.Not.EqualTo(hash2), "Each hash should be unique due to different salt");
    }

    [Test]
    public void HashPassword_ShouldReturnValidBase64String()
    {
        // Arrange
        var password = "testPassword123";

        // Act
        var hash = _passwordService.HashPassword(password);

        // Assert
        Assert.That(hash, Is.Not.Null);
        Assert.That(hash.Length, Is.GreaterThan(0));
        
        // Verify it's a valid base64 string
        try
        {
            var bytes = Convert.FromBase64String(hash);
            Assert.That(bytes.Length, Is.GreaterThan(0));
        }
        catch (FormatException)
        {
            Assert.Fail("Hash should be a valid base64 string");
        }
    }

    [Test]
    public void VerifyPassword_ShouldReturnTrueForCorrectPassword()
    {
        // Arrange
        var password = "testPassword123";
        var hash = _passwordService.HashPassword(password);

        // Act
        var result = _passwordService.VerifyPassword(password, hash);

        // Assert
        Assert.That(result, Is.True, "Password verification should succeed for correct password");
    }

    [Test]
    public void VerifyPassword_ShouldReturnFalseForIncorrectPassword()
    {
        // Arrange
        var correctPassword = "testPassword123";
        var incorrectPassword = "wrongPassword123";
        var hash = _passwordService.HashPassword(correctPassword);

        // Act
        var result = _passwordService.VerifyPassword(incorrectPassword, hash);

        // Assert
        Assert.That(result, Is.False, "Password verification should fail for incorrect password");
    }

    [Test]
    public void VerifyPassword_ShouldReturnFalseForEmptyPassword()
    {
        // Arrange
        var password = "testPassword123";
        var hash = _passwordService.HashPassword(password);

        // Act
        var result = _passwordService.VerifyPassword("", hash);

        // Assert
        Assert.That(result, Is.False, "Password verification should fail for empty password");
    }

    [Test]
    public void VerifyPassword_ShouldReturnFalseForNullPassword()
    {
        // Arrange
        var password = "testPassword123";
        var hash = _passwordService.HashPassword(password);

        // Act
        var result = _passwordService.VerifyPassword(null!, hash);

        // Assert
        Assert.That(result, Is.False, "Password verification should fail for null password");
    }

    [Test]
    public void HashPassword_ShouldHandleSpecialCharacters()
    {
        // Arrange
        var password = "test@Password#123$%^&*()";

        // Act
        var hash = _passwordService.HashPassword(password);
        var result = _passwordService.VerifyPassword(password, hash);

        // Assert
        Assert.That(result, Is.True, "Password with special characters should be handled correctly");
    }

    [Test]
    public void HashPassword_ShouldHandleUnicodeCharacters()
    {
        // Arrange
        var password = "testPässwörd123";

        // Act
        var hash = _passwordService.HashPassword(password);
        var result = _passwordService.VerifyPassword(password, hash);

        // Assert
        Assert.That(result, Is.True, "Password with unicode characters should be handled correctly");
    }

    [Test]
    public void HashPassword_ShouldHandleLongPassword()
    {
        // Arrange
        var password = new string('a', 1000);

        // Act
        var hash = _passwordService.HashPassword(password);
        var result = _passwordService.VerifyPassword(password, hash);

        // Assert
        Assert.That(result, Is.True, "Long password should be handled correctly");
    }

    [Test]
    public void HashPassword_ShouldHandleShortPassword()
    {
        // Arrange
        var password = "123";

        // Act
        var hash = _passwordService.HashPassword(password);
        var result = _passwordService.VerifyPassword(password, hash);

        // Assert
        Assert.That(result, Is.True, "Short password should be handled correctly");
    }

    [Test]
    public void MultiplePasswordVerifications_ShouldWorkCorrectly()
    {
        // Arrange
        var passwords = new[] { "password1", "password2", "password3" };
        var hashes = passwords.Select(p => _passwordService.HashPassword(p)).ToArray();

        // Act & Assert
        for (int i = 0; i < passwords.Length; i++)
        {
            var correctResult = _passwordService.VerifyPassword(passwords[i], hashes[i]);
            Assert.That(correctResult, Is.True, $"Password {i + 1} should verify correctly");

            // Verify wrong password fails
            var wrongIndex = (i + 1) % passwords.Length;
            var wrongResult = _passwordService.VerifyPassword(passwords[wrongIndex], hashes[i]);
            Assert.That(wrongResult, Is.False, $"Wrong password {wrongIndex + 1} should not verify with hash {i + 1}");
        }
    }
}
