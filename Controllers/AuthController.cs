using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Google.Authenticator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using MimeKit;
using MailKit.Net.Smtp;
using WebApi.utils;
using Models;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class AuthController : ControllerBase
    {
        private readonly IMongoCollection<User> _usersCollection;
        private readonly IConfiguration _configuration;

        public AuthController(IMongoDatabase database, IConfiguration configuration)
        {
            _usersCollection = database.GetCollection<User>("users");
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login(UserLoginModel loginModel)
        {
            if (string.IsNullOrWhiteSpace(loginModel.Username) || string.IsNullOrWhiteSpace(loginModel.Password))
            {
                return BadRequest("Username and password are required fields.");
            }

            var user = _usersCollection.Find(u => u.Username == loginModel.Username || u.Email == loginModel.Username).FirstOrDefault();
            if (user == null)
            {
                return NotFound("Invalid username or email.");
            }

            if (!BCrypt.Net.BCrypt.Verify(loginModel.Password, user.Password))
            {
                return Unauthorized("Invalid password.");
            }


            // if (user.TwoFactorEnabled)
            // {

            //     twoFactorEnabled = user.TwoFactorEnabled;
            // }

            // Generate a new secret key using the SecretKeyGenerator class
            //var secretKey = SecretKeyGenerator.GenerateSecretKey();

            // Generate a JWT token 
            var token = GenerateJwtToken(user);

            // Continue with successful login handling
            var userIdentifier = user.Username;
            bool twoFactorEnabled = user.TwoFactorEnabled;
            var twoFactorSecretKey = user.TwoFactorSecretKey;

            return Ok(new
            {
                Identifier = userIdentifier,
                TwoFactorEnabled = twoFactorEnabled,
                TwoFactorSecretKey = twoFactorSecretKey,
                Token = token
            });
        }
        [HttpPost("verify-2fa")]
        public async Task<IActionResult> VerifyTwoFactorCode(VerifyTwoFactorModel model)
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                Console.WriteLine(jwtToken);

               

                var tokenUsername = GetUserFromToken(jwtToken);

                Console.WriteLine(tokenUsername);

                // Fetch the user's information from the database based on the unique identifier
                var user = _usersCollection.Find(u => u.Username == tokenUsername).FirstOrDefault();
                Console.WriteLine(user.TwoFactorSecretKey);
                Console.WriteLine(model.TwoFactorCode);
                if (user == null)
                {
                    return BadRequest("Invalid user");
                }
                Console.WriteLine(VerifyTwoFactorCode(model.TwoFactorCode, user.TwoFactorSecretKey));

                // Perform the two-factor verification using the user's data
                if (VerifyTwoFactorCode(model.TwoFactorCode, user.TwoFactorSecretKey))
                {
                    // Disable two-factor authentication for the user
                    user.TwoFactorEnabled = false;
                    user.TwoFactorSecretKey = null;

                    // Update the user document in the database
                    var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
                    var update = Builders<User>.Update
                        .Set(u => u.TwoFactorEnabled, user.TwoFactorEnabled)
                        .Set(u => u.TwoFactorSecretKey, user.TwoFactorSecretKey);

                    var result = await _usersCollection.UpdateOneAsync(filter, update);

                    if (result.ModifiedCount > 0)
                    {
                        return Ok("Two-Factor Authentication Successful");
                    }
                    else
                    {
                        return BadRequest("Error updating user data.");
                    }
                }
                else
                {
                    return BadRequest("Invalid two-factor authentication code.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occurred: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return StatusCode(500, "Internal server error");
            }
        }


        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("12345@manqobazwane897@nqo@nqo@nqo"); // Get secret from configuration
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim("Email", user.Email)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        private bool VerifyTwoFactorCode(string enteredCode, string storedCode)
        {
            // Compare the entered code with the stored code in the database
            return enteredCode == storedCode;
        }
        private string GetUserFromToken(string jwtToken)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("12345@manqobazwane897@nqo@nqo@nqo")),
                    ValidateIssuer = false, // You might need to adjust this based on your setup
                    ValidateAudience = false // You might need to adjust this based on your setup
                };

                var principal = tokenHandler.ValidateToken(jwtToken, validationParameters, out SecurityToken validatedToken);

                var usernameClaim = principal.FindFirst(ClaimTypes.Name);
                return usernameClaim?.Value;
                
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error validating token: {ex.Message}");
                return null; // Return null if the token is invalid or any error occurs
            }
        }

    }
}


