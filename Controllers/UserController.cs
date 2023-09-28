using Google.Authenticator; 
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Threading.Tasks;// Import the Google Authenticator namespace
using BCrypt.Net; // Import the BCrypt.Net namespace
using Models;
using System.Security.Cryptography;
using MailKit.Net.Smtp;
using MimeKit;

//using User;
//using UserLoginModel;


[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IMongoCollection<User> _usersCollection;

    public UserController(IMongoDatabase database)
    {
        _usersCollection = database.GetCollection<User>("users");
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser(User user)
    {
        // Validate user data
        if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Password) || string.IsNullOrWhiteSpace(user.Email))
        {
            return BadRequest("Username, password, and email are required fields.");
        }

        // Check if the username or email is already in use
        var existingUser = await _usersCollection.Find(u => u.Username == user.Username || u.Email == user.Email).FirstOrDefaultAsync();
        if (existingUser != null)
        {
            return Conflict("Username or email is already in use.");
        }

        // Generate a random security key
        var securityKey = GenerateSecurityKey();
        
        // Hash the user's password securely using BCrypt
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);
        user.Password = hashedPassword;
        user.TwoFactorSecretKey = securityKey;
        user.TwoFactorEnabled = true;

        // Save the user to the MongoDB collection
        await _usersCollection.InsertOneAsync(user);

           // Send a welcome email to the registered user
        try
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Zurisoft", "test@manqoba-zwane.co.za")); // Replace with your email and name
            emailMessage.To.Add(new MailboxAddress(user.Username, user.Email));
            emailMessage.Subject = "Welcome to Our Website!";
            emailMessage.Body = new TextPart("plain")
            {
                Text = "Dear " + user.Username + ",\n\nWelcome to our website! We are excited to have you on board."+ $"\nYour verification code is: {securityKey}"+"\n\nBest regards,\nThe Team"
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("mail.manqoba-zwane.co.za", 587, false); // Replace with your SMTP server and port
                await client.AuthenticateAsync("test@manqoba-zwane.co.za", "wBXPa2UpQ29E"); // Replace with your SMTP username and password
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }
        catch (Exception ex)
        {
            // Log the error or handle it accordingly
            return StatusCode(500, "Failed to send an email: " + ex.Message);
        }

        return Ok("User registered successfully!");
    }
    
     // Helper method to send an email
        // private void SendEmail(string toEmail, string subject, string body)
        // {
        //     using (var client = new SmtpClient("mail.manqoba-zwane.co.za", 465)) // Replace with your SMTP server details
        //     {
        //         client.EnableSsl = true; // Set to true if your SMTP server requires SSL
        //         client.UseDefaultCredentials = false;
        //         client.Credentials = new NetworkCredential("test@manqoba-zwane.co.za", "wBXPa2UpQ29E"); // Replace with your SMTP credentials

        //         MailMessage mailMessage = new MailMessage();
        //         mailMessage.From = new MailAddress("test@manqoba-zwane.co.za");
        //         mailMessage.To.Add(toEmail);
        //         mailMessage.Subject = subject;
        //         mailMessage.Body = body;

        //         client.Send(mailMessage);
        //     }
        // }

    // [HttpPost("login")]
    //  public IActionResult Login(UserLoginModel loginModel)
    //  {
    //     //Validate user login data
    //     if (string.IsNullOrWhiteSpace(loginModel.Username) || string.IsNullOrWhiteSpace(loginModel.Password))
    //     {
    //         return BadRequest("Username and password are required fields.");
    //     }

    //     // Find the user by their username or email
    //     var user = _usersCollection.Find(u => u.Username == loginModel.Username || u.Email == loginModel.Username).FirstOrDefault();
    //     if (user == null)
    //     {
    //         return NotFound("Invalid username or email.");
    //     }

    //     // Validate the password using BCrypt
    //     if (!BCrypt.Net.BCrypt.Verify(loginModel.Password, user.Password))
    //     {
    //         return Unauthorized("Invalid password.");
    //     }

    //     if (user.TwoFactorEnabled)
    //         {
    //             if (string.IsNullOrWhiteSpace(loginModel.TwoFactorCode))
    //             {
    //                 return BadRequest("Two-factor authentication code is required.");
    //             }

    //             if (!VerifyTwoFactorCode(loginModel.TwoFactorCode, user.TwoFactorSecretKey))
    //             {
    //                 return Unauthorized("Invalid two-factor authentication code.");
    //             }
    //         }


    //         // Continue with successful login handling
    //     var userIdentifier = user.Username; // Assuming you use the username as the identifier, you can adjust this based on your needs

    //     // Check if two-factor authentication is enabled for the user
    //     bool twoFactorEnabled = user.TwoFactorEnabled;
    //     string twoFactorSecretKey = user.TwoFactorSecretKey;

    //     return Ok(new { Identifier = userIdentifier, TwoFactorEnabled = twoFactorEnabled, TwoFactorSecretKey = twoFactorSecretKey });
    // }

    // [HttpPost("resendverificationcode")]
    // public async Task<IActionResult> ResendVerificationCode(ResendVerificationCodeModel resendModel)
    // {
   
    //         // Validate the input data
    //     if (string.IsNullOrWhiteSpace(resendModel.Username) || string.IsNullOrWhiteSpace(resendModel.Email))
    //     {
    //         return BadRequest("Username and email are required fields.");
    //     }
    //       // Find the user by their username or email
    //     var user = _usersCollection.Find(u => u.Username == resendModel.Username || u.Email == resendModel.Email).FirstOrDefault();
    //     if (user == null)
    //     {
    //         return NotFound("User not found. Invalid username or email.");
    //     }

    //         // Generate a new verification code
    //     string newVerificationCode = GenerateSecurityKey(); // Implement this method to generate the code
    //     user.TwoFactorSecretKey = newVerificationCode;

    //     // Save the updated user data back to the MongoDB collection
    //     _usersCollection.ReplaceOne(u => u.Id == user.Id, user);

    //     // Send the new verification code to the user via email
    //     bool emailSent = await SendVerificationCodeEmail(user.Email, user.Username, newVerificationCode);

    //     if (!emailSent)
    //     {
    //         return StatusCode(500, "Failed to resend the verification code via email. Please try again later.");
    //     }


    //     return Ok("Verification code email resent successfully!");
    // }
// [HttpGet("qrCodeUrl")]
// public Task<IActionResult> GetQRCodeUrl(string userId)
// {
//     // Find the user by their ID
//     var user = _usersCollection.Find(u => u.Id == userId).FirstOrDefault();
//     if (user == null)
//     {
//         return NotFound();
//     }

//     if (!user.TwoFactorEnabled || string.IsNullOrWhiteSpace(user.TwoFactorSecretKey))
//     {
//         return BadRequest("Two-factor authentication is not enabled for this user.");
//     }

//     // Convert the Base32-encoded secret key to a byte array
//     byte[] secretKeyBytes = Base32Encoding.ToBytes(user.TwoFactorSecretKey);

//     // Generate the QR code URL based on the user's secret key and other details
//     var authenticator = new TwoFactorAuthenticator();
//     string qrCodeUrl = authenticator.GenerateSetupCode("Kounta", user.Email, secretKeyBytes);

//     return Ok(new { QRCodeUrl = qrCodeUrl });
// }

// [HttpPost("enableTwoFactorWithCode")]
// public async Task<IActionResult> EnableTwoFactorWithCode(string userId, string verificationCode)
// {
//    // Find the user by their ID
//     var user = _usersCollection.Find(u => u.Id == userId).FirstOrDefault();
//     if (user == null)
//     {
//         return NotFound();
//     }

//     if (!user.TwoFactorEnabled || string.IsNullOrWhiteSpace(user.TwoFactorSecretKey))
//     {
//         return BadRequest("Two-factor authentication is not enabled for this user.");
//     }

//     // Convert the Base32-encoded secret key to a byte array
//     byte[] secretKeyBytes = Base32Encoding.ToBytes(user.TwoFactorSecretKey);

//     // Generate the QR code URL based on the user's secret key and other details
//     var authenticator = new TwoFactorAuthenticator();
//     SetupCode setupInfo = authenticator.GenerateSetupCode("Kounta", user.Email, secretKeyBytes);
//     string qrCodeUrl = setupInfo.QrCodeSetupImageUrl;

//     return Ok(new { QRCodeUrl = qrCodeUrl });
// }



 // Helper method to generate a random security key
    private string GenerateSecurityKey()
    {
        // You can use any logic to generate the security key. For example, you can use Guid.NewGuid().ToString().
        // For the sake of simplicity, let's just generate a random 6-digit number.
        Random random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    private bool VerifyTwoFactorCode(string code, string secretKey)
    {
        var authenticator = new TwoFactorAuthenticator();
        return authenticator.ValidateTwoFactorPIN(secretKey, code);
    }
    public async Task<bool> SendVerificationCodeEmail(string toEmail, string username, string verificationCode)
{
    try
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress("Zurisoft", "test@manqoba-zwane.co.za")); // Replace with your email and name
        emailMessage.To.Add(new MailboxAddress(username, toEmail));
        emailMessage.Subject = "Welcome to Our Website!";
        emailMessage.Body = new TextPart("plain")
        {
            Text = "Dear " + username + ",\n\nWelcome to our website! We are excited to have you on board." + $"\nYour verification code is: {verificationCode}" + "\n\nBest regards,\nThe Team"
        };

        using (var client = new SmtpClient())
        {
            await client.ConnectAsync("mail.manqoba-zwane.co.za", 587, false); // Replace with your SMTP server and port
            await client.AuthenticateAsync("test@manqoba-zwane.co.za", "wBXPa2UpQ29E"); // Replace with your SMTP username and password
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }

        return true; // Email sent successfully
    }
    catch (Exception ex)
    {
        // Return false to indicate that the email sending failed
        return false;
    }
}

// [HttpPost("forgot-password")]
// public IActionResult ForgotPassword(ForgotPasswordModel forgotPasswordModel)
// {
//     // Check if the email exists in the database
//     var user = _usersCollection.Find(u => u.Email == forgotPasswordModel.Email).FirstOrDefault();
//     if (user == null)
//     {
//         return NotFound("User not found.");
//     }

//     // Generate a password reset token and store it in the user's record
//     var passwordResetToken = GeneratePasswordResetToken();
//     user.PasswordResetToken = passwordResetToken;
//     user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1); // Token valid for 1 hour

//     // Save the user with the updated token in the database
//     _usersCollection.ReplaceOne(u => u.Id == user.Id, user);

//     // Send an email with the password reset instructions
//     SendPasswordResetEmail(user.Email, user.Username, passwordResetToken);

//     return Ok("Password reset instructions sent to the email address.");
// }

// Helper method to generate a password reset token (you can implement your own logic here)
private string GeneratePasswordResetToken()
{
    // Generate a unique and secure token (e.g., using GUID)
    return Guid.NewGuid().ToString("N");
}

// Helper method to send the password reset email (similar to what you did earlier)
private void SendPasswordResetEmail(string userEmail, string username, string resetToken)
{
    // Implementation to send the email
}

}


