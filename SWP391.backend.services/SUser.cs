using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Tls;
using SWP391.backend.repository;
using SWP391.backend.repository.DTO;
using SWP391.backend.repository.DTO.Account;
using SWP391.backend.repository.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace SWP391.backend.services
{
    public class SUser : IUser
    {
        private readonly IConfiguration _configuration;

        private readonly swpContext context;

        public SUser(swpContext Context, IConfiguration configuration)
        {
            context = Context;
            _configuration = configuration;
        }

        public async Task<List<User>> GetAll(GetAllDTO request)
        {
            try
            {
                var query = context.Users.AsQueryable();

                // Filtering
                if (!string.IsNullOrEmpty(request.FilterOn) && !string.IsNullOrEmpty(request.FilterQuery))
                {
                    switch (request.FilterOn.ToLower())
                    {
                        case "email":
                            query = query.Where(a => a.Email != null && a.Email.Contains(request.FilterQuery));
                            break;
                        case "fullname":
                            query = query.Where(a => a.Fullname != null && a.Fullname.Contains(request.FilterQuery));
                            break;
                        case "role":
                            query = query.Where(a => a.Role != null && a.Role.Contains(request.FilterQuery));
                            break;
                        case "lastlogin":
                            if (DateTime.TryParse(request.FilterQuery, out var lastLogin))
                                query = query.Where(a => a.LastLogin.HasValue && a.LastLogin.Value.Date == lastLogin.Date);
                            break;
                        case "createdat":
                            if (DateTime.TryParse(request.FilterQuery, out var createdAt))
                                query = query.Where(a => a.CreatedAt.HasValue && a.CreatedAt.Value.Date == createdAt.Date);
                            break;
                        case "updatedat":
                            if (DateTime.TryParse(request.FilterQuery, out var updatedAt))
                                query = query.Where(a => a.UpdatedAt.HasValue && a.UpdatedAt.Value.Date == updatedAt.Date);
                            break;
                        default:
                            throw new InvalidOperationException($"Invalid filter field: {request.FilterOn}");
                    }
                }

                // Sorting
                if (!string.IsNullOrEmpty(request.SortBy))
                {
                    bool isAscending = request.IsAscending ?? true;
                    query = request.SortBy.ToLower() switch
                    {
                        "email" => isAscending ? query.OrderBy(a => a.Email) : query.OrderByDescending(a => a.Email),
                        "fullname" => isAscending ? query.OrderBy(a => a.Fullname) : query.OrderByDescending(a => a.Fullname),
                        "role" => isAscending ? query.OrderBy(a => a.Role) : query.OrderByDescending(a => a.Role),
                        "lastlogin" => isAscending ? query.OrderBy(a => a.LastLogin) : query.OrderByDescending(a => a.LastLogin),
                        "createdat" => isAscending ? query.OrderBy(a => a.CreatedAt) : query.OrderByDescending(a => a.CreatedAt),
                        "updatedat" => isAscending ? query.OrderBy(a => a.UpdatedAt) : query.OrderByDescending(a => a.UpdatedAt),
                        _ => isAscending ? query.OrderBy(a => a.Fullname) : query.OrderByDescending(a => a.Fullname) // Default sort
                    };
                }

                // Paging
                int pageNumber = request.PageNumber ?? 1;
                int pageSize = request.PageSize ?? 10;

                var totalRecords = await query.CountAsync();
                var accounts = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

                return accounts;
            }
            catch (Exception ex)
            {
                // More detailed error message to help with debugging.
                throw new Exception($"Error fetching accounts: {ex.Message}", ex);
            }
        }

        public async Task<User> CreateUser(CreateUserDTO request)
        {
            try
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request), "Request cannot be null.");

                if (string.IsNullOrWhiteSpace(request.Email))
                    throw new ArgumentException("Email cannot be null or empty.");

                // Normalize email (trim + lowercase) to prevent duplicate emails due to case differences
                request.Email = request.Email.Trim().ToLower();

                // Validate email format
                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
                if (!emailRegex.IsMatch(request.Email))
                    throw new ArgumentException("Invalid email format.");

                // Check if the email already exists in the database
                var existingAccount = await context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Email.ToLower() == request.Email);
                if (existingAccount != null)
                    throw new InvalidOperationException("Email has already been registered!");

                // Create new account
                var newAccount = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    Role = "user",
                    Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    CreatedAt = DateTime.UtcNow
                };

                await context.Users.AddAsync(newAccount);
                await context.SaveChangesAsync();

                // Send confirmation email
                await SendConfirmationEmail(newAccount.Email);

                return newAccount;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                throw new Exception($"Account creation failed: {ex.Message}", ex);
            }
        }

        public async Task<User> CreateStaff(CreateStaffDTO request)
        {
            try
            {
                if (request == null)
                    throw new ArgumentException("Request data cannot be null.");

                // Validate email format
                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
                if (!emailRegex.IsMatch(request.Email))
                    throw new ArgumentException("Invalid email format.");

                // Check if email already exists
                var existingAccount = (await context.Users.ToListAsync())
                .FirstOrDefault(x => x.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase));

                if (existingAccount != null)
                    throw new InvalidOperationException("Email has already been registered.");

                // Create new account
                var newAccount = new User
                {
                    Fullname = request.Fullname,
                    Username =request.Username,
                    Email = request.Email,
                    Role = "staff",
                    Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    CreatedAt = DateTime.UtcNow
                };

                await context.Users.AddAsync(newAccount);
                await context.SaveChangesAsync();

                return newAccount;
            }
            catch (Exception ex)
            {
                throw new Exception($"Account creation failed: {ex.Message}", ex);
            }
        }

        public async Task<User> CreateDoctor(CreateStaffDTO request)
        {
            try
            {
                if (request == null)
                    throw new ArgumentException("Request data cannot be null.");

                // Validate email format
                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
                if (!emailRegex.IsMatch(request.Email))
                    throw new ArgumentException("Invalid email format.");

                // Check if email already exists
                var existingAccount = (await context.Users.ToListAsync())
                .FirstOrDefault(x => x.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase));

                if (existingAccount != null)
                    throw new InvalidOperationException("Email has already been registered.");

                // Create new account
                var newAccount = new User
                {
                    Fullname = request.Username,
                    Username = request.Username,
                    Email = request.Email,
                    Role = "doctor",
                    Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    CreatedAt = DateTime.UtcNow
                };

                await context.Users.AddAsync(newAccount);
                await context.SaveChangesAsync();

                return newAccount;
            }
            catch (Exception ex)
            {
                throw new Exception($"Account creation failed: {ex.Message}", ex);
            }
        }

        private async Task SendConfirmationEmail(string email)
        {
            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587, // ✅ Fixed Port
                    Credentials = new NetworkCredential("lhh547801@gmail.com", "ujxu vapq mxjb adjb"), // Use an App Password
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("moduleshopfuniture@gmail.com"),
                    Subject = "Account Created Successfully",
                    Body = @"
                    <html>
                    <head>
                        <style>
                            body { text-align: center; font-family: Arial, sans-serif; }
                            .bold { font-weight: bold; }
                        </style>
                    </head>
                    <body>
                        <p>Hello <b>" + email + @"</b>,</p>
                        <p>Your account has been created in <span class='bold'>Module Shop Furniture</span> successfully.</p>
                        <br>
                        <p><b>Best Regards,</b></p>
                        <p><b>Team Module Shop Furniture</b></p>
                    </body>
                    </html>",
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(email);

                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to send confirmation email: {ex.Message}");
            }
        }

        public async Task ForgotPassword(string email)
        {
            try
            {
                var user = await context.Users.FirstOrDefaultAsync(x => x.Email == email);
                if (user == null)
                    throw new Exception("No account found with this email.");

                // Generate Reset Token
                user.ResetToken = new Random().Next(100000, 999999).ToString(); // Generates a 6-digit number
                user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1); // Token valid for 1 hour
                await context.SaveChangesAsync();

                // Send Email with Reset Link
                string resettoken = user.ResetToken;
                await SendPasswordResetEmail(user.Email, resettoken);
            }
            catch (Exception ex)
            {
                throw new Exception($"Forgot password failed: {ex.Message}");
            }
        }

        private async Task SendPasswordResetEmail(string email, string resettoken)
        {
            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("lhh547801@gmail.com", "ujxu vapq mxjb adjb"), // Use App Password
                    EnableSsl = true,
                };
                var mailMessage = new MailMessage
                {
                    From = new MailAddress("moduleshopfuniture@gmail.com"),
                    Subject = "Reset Your Password",
                    Body = $@"
                    <html>
                    <head>
                        <style>
                            body {{ text-align: center; font-family: Arial, sans-serif; }}
                            .token-box {{ 
                                display: inline-block; 
                                background-color: #f0f0f0; 
                                padding: 10px 20px; 
                                border-radius: 5px; 
                                font-weight: bold; 
                                font-size: 16px; 
                            }}
                        </style>
                    </head>
                    <body>
                        <p>Hello,</p>
                        <p>Please copy an otp to reset your password:</p>
                        <p class='token-box'>{resettoken}</p>
                        <p>This link is valid for <b>1 hour</b>.</p>
                        <br>
                        <p><b>Best Regards,</b></p>
                        <p><b>Team Vaccinecare</b></p>
                    </body>
                    </html>",
                    IsBodyHtml = true,
                };


                mailMessage.To.Add(email);
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to send password reset email: {ex.Message}");
            }
        }

        public async Task ResetPassword(string token, string newPassword, string confirmPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
                    throw new ArgumentException("Password fields cannot be empty.");

                if (newPassword != confirmPassword)
                    throw new ArgumentException("Passwords do not match.");

                var user = await context.Users.FirstOrDefaultAsync(x => x.ResetToken == token);
                if (user == null || user.ResetTokenExpiry == null || user.ResetTokenExpiry < DateTime.UtcNow)
                    throw new Exception("Invalid or expired reset token.");

                // Hash new password
                user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);

                // Clear reset token after successful reset
                user.ResetToken = null;
                user.ResetTokenExpiry = null;

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Password reset failed: {ex.Message}");
            }
        }

        public async Task<User> Update(int id, UpdateUserDTO user)
        {
            try
            {
                var existingUser = await context.Users.FirstOrDefaultAsync(x => x.Id == id);
                if (existingUser == null)
                {
                    throw new KeyNotFoundException($"User with ID {id} not found.");
                }


                // Update other user properties
                existingUser.Fullname = user.Fullname ?? existingUser.Fullname;
                existingUser.UpdatedAt = DateTime.Now;


                // Save changes
                context.Users.Update(existingUser); // Update the Account entity
                await context.SaveChangesAsync();

                return existingUser;
            }
            catch (ValidationException ex)
            {
                throw new ValidationException($"Validation failed: {ex.Message}");
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating user: {ex.Message}", ex);
            }
        }

        public async Task<User> GetByID(int id)
        {
            try
            {
                // Fetch user by ID
                var user = await context.Users
                                        .FirstOrDefaultAsync(x => x.Id == id);

                // Check if user exists
                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID {id} not found.");
                }

                return user; // Return found user
            }
            catch (KeyNotFoundException ex)
            {
                // Specific exception when user is not found
                throw new KeyNotFoundException(ex.Message);
            }
            catch (Exception ex)
            {
                // General exception handling
                throw new Exception($"Error fetching user: {ex.Message}", ex);
            }
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                // Find the user by UserID
                var user = await context.Users
                    .Where(x => x.Id == id)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    throw new Exception("User not found");
                }

                // Remove the user
                context.Users.Remove(user);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Capture the error message and include inner exception if available
                var errorMessage = $"{ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" Inner Exception: {ex.InnerException.Message}";
                }

                throw new Exception(errorMessage);
            }
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim("Id", user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("Email", user.Email),
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: creds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }

        public async Task<string> Login(LoginDTO request)
        {
            try
            {
                // Retrieve the user based on the  email
                var user = await this.context.Users
                    .Where(x => x.Email.Equals(request.Email))
                    .FirstOrDefaultAsync();

                if (user == null)
                    throw new Exception("USER IS NOT FOUND");

                // Check if the password is correct
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                    throw new Exception("INVALID PASSWORD");

                this.context.Users.Update(user);
                await this.context.SaveChangesAsync();

                // Create and return token (or any other mechanism, since JWT is not being used)
                var token = CreateToken(user); // Update this as per your token generation
                return token;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }

        public async Task<bool> Logout(string email)
        {
            var user = await  context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return false;
            }
            user.LastLogin = DateTime.UtcNow;
            this.context.Users.Update(user);
            await this.context.SaveChangesAsync();
            return true;
        }
    }
}
