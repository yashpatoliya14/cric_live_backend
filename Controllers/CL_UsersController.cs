using CricLive.Models;
using CricLive.Models.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Net.Mail;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Npgsql; // Changed from System.Data.SqlClient

namespace CricLive.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CL_UsersController : ControllerBase
    {
        public IConfiguration _configuration;

        public CL_UsersController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        [Route("Login")]
        public IActionResult Login(LoginRequest _request)
        {
            try
            {
                _request.email = _request.email?.Trim();
                _request.password = _request.password?.Trim();

                if (string.IsNullOrEmpty(_request.email) || string.IsNullOrEmpty(_request.password))
                {
                    return BadRequest(new { Message = "Email and Password are required" });
                }

                using (NpgsqlConnection conn = new NpgsqlConnection(_configuration.GetConnectionString("CricLive")))
                {
                    conn.Open();
                    NpgsqlCommand command = new NpgsqlCommand();
                    command.Connection = conn;
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandText = @"SELECT uid, email, password
                                            FROM CL_Users
                                            WHERE email = @email";
                    command.Parameters.AddWithValue("@email", _request.email);

                    // Corrected the type from 'Np' to 'NpgsqlDataReader'
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string hashedPasswordFromDb = reader["password"].ToString();
                            var hasher = new PasswordHasher<string>();
                            var result = hasher.VerifyHashedPassword(null, hashedPasswordFromDb, _request.password);

                            if (result == PasswordVerificationResult.Success)
                            {
                                var claims = new[]
                                {
                                    new Claim(JwtRegisteredClaimNames.Sub, _configuration["JwtConfig:Subject"]),
                                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                                    new Claim("uid", reader["uid"].ToString()),
                                    new Claim("email", reader["email"].ToString())
                                };

                                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtConfig:Key"]));
                                var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                                var token = new JwtSecurityToken(
                                    issuer: _configuration["JwtConfig:Issuer"],
                                    audience: _configuration["JwtConfig:Audience"],
                                    claims: claims,
                                    expires: DateTime.UtcNow.AddDays(30),
                                    signingCredentials: signIn
                                );

                                return Ok(new
                                {
                                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                                    Message = "Welcome back !"
                                });
                            }
                            else
                            {
                                return BadRequest(new { Message = "Invalid password" });
                            }
                        }
                        else
                        {
                            return BadRequest(new { Message = "User not found" });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return BadRequest(new
                {
                    Message = e.Message,
                    innerException = e.InnerException?.Message
                });
            }
        }

        [HttpPost]
        [Route("SendOtp")]
        public IActionResult SendOtp([FromBody] string email)
        {
            try
            {
                string fromMail = "yashpatoliya05@gmail.com";
                string fromPassword = "kjcdzcjktnhlndbu";
                Random random = new Random();
                int otp = random.Next(100000, 999999);

                MailMessage message = new MailMessage();
                message.From = new MailAddress(fromMail);
                message.Subject = "CricLive Verification";
                message.To.Add(new MailAddress(email));
                message.Body = $"<html><body> <h1>Welcome To CricLive</h1><h2>Your Otp is {otp}</h2> </body></html>";
                message.IsBodyHtml = true;

                using (NpgsqlConnection conn = new NpgsqlConnection(_configuration.GetConnectionString("CricLive")))
                {
                    conn.Open();
                    NpgsqlCommand command = conn.CreateCommand();
                    DateTime datetime = DateTime.Now;
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandText = @"update CL_Users
                                            set otp = @otp,
                                            otpTime = @time
                                            where email = @email";
                    command.Parameters.AddWithValue("@otp", otp);
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@time", datetime);
                    command.ExecuteNonQuery();
                }

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromMail, fromPassword),
                    EnableSsl = true,
                };

                smtpClient.Send(message);

                return Ok(new { Message = "Email sent successfully" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new
                {
                    Message = e.Message,
                    innerException = e.InnerException?.Message
                });
            }
        }

        [HttpPost]
        [Route("VerifyOtp")]
        public IActionResult VerifyOtp([FromBody] OtpModel otp)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(_configuration.GetConnectionString("CricLive")))
            {
                try
                {
                    conn.Open();
                    NpgsqlCommand command = conn.CreateCommand();
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandText = @"select * from CL_Users where email = @email";
                    command.Parameters.AddWithValue("@email", otp.email);

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            DateTime otpTime = DateTime.Parse(reader["otpTime"].ToString());
                            int dbOtp = Convert.ToInt32(reader["otp"]);
                            string uid = reader["uid"].ToString();
                            string emailFromDb = reader["email"].ToString();

                            reader.Close(); // Close reader before executing another command

                            if ((DateTime.Now - otpTime).TotalMinutes > 10)
                            {
                                return BadRequest(new { Message = "OTP has expired. Please request a new one." });
                            }

                            if (dbOtp == otp.otp)
                            {
                                using (NpgsqlCommand verifyCommand = conn.CreateCommand())
                                {
                                    verifyCommand.CommandType = System.Data.CommandType.Text;
                                    verifyCommand.CommandText = @"update CL_Users
                                                                  set isVerified = 1
                                                                  where email = @email";
                                    verifyCommand.Parameters.AddWithValue("@email", otp.email);
                                    verifyCommand.ExecuteNonQuery();
                                }

                                var claims = new[]
                                {
                                    new Claim(JwtRegisteredClaimNames.Sub, _configuration["JwtConfig:Subject"]),
                                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                                    new Claim("uid", uid),
                                    new Claim("email", emailFromDb)
                                };

                                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtConfig:Key"]));
                                var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                                var token = new JwtSecurityToken(
                                    issuer: _configuration["JwtConfig:Issuer"],
                                    audience: _configuration["JwtConfig:Audience"],
                                    claims: claims,
                                    expires: DateTime.UtcNow.AddDays(30),
                                    signingCredentials: signIn
                                );

                                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                                return Ok(new { Message = "Email Verification Success", Token = tokenString });
                            }
                            else
                            {
                                return BadRequest(new { Message = "Invalid OTP! Email Verification Failed." });
                            }
                        }
                        else
                        {
                            return BadRequest(new { Message = "Invalid Email Address." });
                        }
                    }
                }
                catch (Exception e)
                {
                    return StatusCode(500, new
                    {
                        Message = e.Message,
                        innerException = e.InnerException?.Message
                    });
                }
            }
        }

        [HttpPost]
        [Route("CreateUser")]
        public IActionResult CreateUser(User user)
        {
            try
            {
                using (NpgsqlConnection _con = new NpgsqlConnection(_configuration.GetConnectionString("CricLive")))
                {
                    _con.Open();

                    using (NpgsqlCommand commandForCheckUser = _con.CreateCommand())
                    {
                        commandForCheckUser.CommandType = System.Data.CommandType.Text;
                        commandForCheckUser.CommandText = @"SELECT uid FROM CL_Users WHERE email = @email";
                        commandForCheckUser.Parameters.AddWithValue("@email", user.Email);

                        using (NpgsqlDataReader reader = commandForCheckUser.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                return BadRequest(new { Message = "User with this email already exists" });
                            }
                        }
                    }

                    var hasher = new PasswordHasher<string>();
                    user.Password = hasher.HashPassword(null, user.Password);
                    string username = user.Email.Split('@')[0];

                    using (NpgsqlCommand command = _con.CreateCommand())
                    {
                        command.CommandType = System.Data.CommandType.Text;
                        // Added 'RETURNING uid' to get the ID of the new row
                        command.CommandText = @"INSERT INTO CL_Users (fullName, gender, email, isVerified, profilePhoto, role, username, password) 
                                                VALUES (@fullName, @gender, @email, @isVerified, @profilePhoto, @role, @username, @password)
                                                RETURNING uid";

                        command.Parameters.AddWithValue("@fullName", $"{user.FirstName} {user.LastName}");
                        command.Parameters.AddWithValue("@gender", user.Gender);
                        command.Parameters.AddWithValue("@email", user.Email);
                        command.Parameters.AddWithValue("@isVerified", 0);
                        command.Parameters.AddWithValue("@profilePhoto", user.ProfilePhoto ?? " ");
                        command.Parameters.AddWithValue("@role", "user");
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", user.Password);

                        var newUserId = command.ExecuteScalar();
                        return Ok(new { Message = "User created successfully", Uid = newUserId });
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"A database error occurred: {ex.Message}");
                return StatusCode(500, new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                return StatusCode(500, new { Message = ex.Message ,InnerException=ex.InnerException});
            }
        }

        [HttpPost]
        [Route("ForgotPassword")]
        public IActionResult ForgotPassword([FromBody] ForgotPasswordRequest body)
        {
            try
            {
                string password = body.Password;
                string email = body.Email;
                if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(email))
                {
                    return BadRequest(new { Message = "Credentials are missing" });
                }

                using (NpgsqlConnection _con = new NpgsqlConnection(_configuration.GetConnectionString("CricLive")))
                {
                    _con.Open();

                    using (NpgsqlCommand checkCommand = _con.CreateCommand())
                    {
                        checkCommand.CommandType = System.Data.CommandType.Text;
                        checkCommand.CommandText = @"select 1 from CL_Users where email = @email";
                        checkCommand.Parameters.AddWithValue("@email", email);
                        var userExists = checkCommand.ExecuteScalar();
                        if (userExists == null)
                        {
                            return NotFound(new { Message = "User Not Found" });
                        }
                    }

                    var hasher = new PasswordHasher<string>();
                    password = hasher.HashPassword(null, password);

                    using (NpgsqlCommand updateCommand = _con.CreateCommand())
                    {
                        updateCommand.CommandType = System.Data.CommandType.Text;
                        updateCommand.CommandText = @"update CL_Users set password = @password where email = @email";
                        updateCommand.Parameters.AddWithValue("@email", email);
                        updateCommand.Parameters.AddWithValue("@password", password);
                        updateCommand.ExecuteNonQuery();

                        return Ok(new { Message = "Password Change Successful" });
                    }
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, new
                {
                    Message = e.Message,
                    InnerException = e.InnerException?.Message
                });
            }
        }

        [HttpGet]
        [Route("GetUserByEmail/{email}")]
        public IActionResult GetUserByEmail(string email)
        {
            try
            {
                User user = null;
                using (NpgsqlConnection _con = new NpgsqlConnection(_configuration.GetConnectionString("CricLive")))
                {
                    _con.Open();
                    NpgsqlCommand command = _con.CreateCommand();
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandText = @"select * from CL_Users where email = @email";
                    command.Parameters.AddWithValue("@email", email);

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                Uid = Convert.ToInt32(reader["uid"]),
                                FirstName = reader["fullName"].ToString().Split(' ')[0],
                                LastName = reader["fullName"].ToString().Split(' ').Length > 1 ? reader["fullName"].ToString().Split(' ')[1] : "",
                                Role = reader["role"].ToString(),
                                IsVerified = Convert.ToInt32(reader["isVerified"]),
                                ProfilePhoto = reader["profilePhoto"].ToString(),
                                Username = reader["username"].ToString(),
                                Email = reader["email"].ToString(),
                                Gender = reader["gender"].ToString(),
                                Password = "" // Never send the hash back
                            };
                        }
                    }
                }

                if (user != null)
                {
                    return Ok(new { Message = "User found successfully", User = user });
                }
                else
                {
                    return NotFound(new { Message = "User not found" });
                }
            }
            catch (Exception e)
            {
                return BadRequest(new
                {
                    Message = e.Message,
                    innerException = e.InnerException?.Message
                });
            }
        }

        [HttpGet]
        [Authorize]
        [Route("GetUser/{uid}")]
        public IActionResult GetUser(int uid)
        {
            try
            {
                User user = null;
                using (NpgsqlConnection _con = new NpgsqlConnection(_configuration.GetConnectionString("CricLive")))
                {
                    _con.Open();
                    NpgsqlCommand command = _con.CreateCommand();
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandText = @"select * from CL_Users where uid = @uid";
                    command.Parameters.AddWithValue("@uid", uid);

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                Uid = Convert.ToInt32(reader["uid"]),
                                FirstName = reader["fullName"].ToString().Split(' ')[0],
                                LastName = reader["fullName"].ToString().Split(' ').Length > 1 ? reader["fullName"].ToString().Split(' ')[1] : "",
                                Role = reader["role"].ToString(),
                                IsVerified = Convert.ToInt32(reader["isVerified"]),
                                ProfilePhoto = reader["profilePhoto"].ToString(),
                                Username = reader["username"].ToString(),
                                Email = reader["email"].ToString(),
                                Gender = reader["gender"].ToString(),
                                Password = "" // Never send the hash back
                            };
                        }
                    }
                }

                if (user != null)
                {
                    return Ok(new { Message = "User found successfully", User = user });
                }
                else
                {
                    return NotFound(new { Message = "User not found" });
                }
            }
            catch (Exception e)
            {
                return BadRequest(new
                {
                    Message = e.Message,
                    innerException = e.InnerException?.Message
                });
            }
        }

        [HttpDelete] // Changed to HttpDelete for RESTful standards
        [Route("DeleteUser/{uid}")]
        public IActionResult DeleteUser(int uid)
        {
            try
            {
                using (NpgsqlConnection _con = new NpgsqlConnection(_configuration.GetConnectionString("CricLive")))
                {
                    _con.Open();
                    NpgsqlCommand command = _con.CreateCommand();
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandText = "delete from CL_Users where uid = @uid";
                    command.Parameters.AddWithValue("@uid", uid);
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return Ok(new { Message = "User deleted successfully" });
                    }
                    else
                    {
                        return NotFound(new { Message = "User not found" });
                    }
                }
            }
            catch (Exception e)
            {
                return BadRequest(new
                {
                    Message = e.Message,
                    innerException = e.InnerException?.Message
                });
            }
        }

        [HttpPut]
        [Route("UpdateUser")]
        public IActionResult UpdateUser(User user)
        {
            try
            {
                using (NpgsqlConnection _con = new NpgsqlConnection(_configuration.GetConnectionString("CricLive")))
                {
                    _con.Open();
                    NpgsqlCommand command = _con.CreateCommand();
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandText = @"UPDATE CL_Users SET
                                                fullName = @fullName,
                                                gender = @gender,
                                                email = @email,
                                                isVerified = @isVerified,
                                                profilePhoto = @profilePhoto,
                                                role = @role,
                                                username = @username
                                            WHERE uid = @uid";

                    command.Parameters.AddWithValue("@uid", user.Uid);
                    command.Parameters.AddWithValue("@fullName", $"{user.FirstName} {user.LastName}");
                    command.Parameters.AddWithValue("@username", user.Username);
                    command.Parameters.AddWithValue("@email", user.Email);
                    command.Parameters.AddWithValue("@profilePhoto", user.ProfilePhoto ?? " ");
                    command.Parameters.AddWithValue("@gender", user.Gender);
                    command.Parameters.AddWithValue("@isVerified", user.IsVerified);
                    command.Parameters.AddWithValue("@role", user.Role);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return Ok(new { Message = "User updated successfully" });
                    }
                    else
                    {
                        return NotFound(new { Message = "User not found" });
                    }
                }
            }
            catch (Exception e)
            {
                return BadRequest(new
                {
                    Message = e.Message,
                    innerException = e.InnerException?.Message
                });
            }
        }

        [HttpGet]
        [Route("GetAllUsers")]
        public IActionResult GetAllUsers()
        {
            try
            {
                List<User> users = new List<User>();
                using (NpgsqlConnection _con = new NpgsqlConnection(_configuration.GetConnectionString("CricLive")))
                {
                    _con.Open();
                    NpgsqlCommand command = _con.CreateCommand();
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandText = "select * from CL_Users";

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            User user = new User
                            {
                                Uid = Convert.ToInt32(reader["uid"]),
                                FirstName = reader["fullName"].ToString().Split(' ')[0],
                                LastName = reader["fullName"].ToString().Split(' ').Length > 1 ? reader["fullName"].ToString().Split(' ')[1] : "",
                                Username = reader["username"].ToString(),
                                Email = reader["email"].ToString(),
                                ProfilePhoto = reader["profilePhoto"].ToString(),
                                Gender = reader["gender"].ToString(),
                                IsVerified = Convert.ToInt32(reader["isVerified"]),
                                Role = reader["role"].ToString(),
                                Password = "" // IMPORTANT: Do not send password hashes to the client.
                            };
                            users.Add(user);
                        }
                    }
                }
                return Ok(users);
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred in GetAllUsers: {e.Message}");
                return StatusCode(500, new
                {
                    Message = "An internal server error occurred.",
                    error = e.Message
                });
            }
        }

        [HttpGet]
        [Route("SearchUser/{q}")]
        public IActionResult SearchUser(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Ok(new List<User>());
            }

            try
            {
                List<User> users = new List<User>();
                using (NpgsqlConnection _con = new NpgsqlConnection(_configuration.GetConnectionString("CricLive")))
                {
                    _con.Open();
                    NpgsqlCommand command = _con.CreateCommand();
                    command.CommandType = System.Data.CommandType.Text;

                    // In PostgreSQL, LIKE is case-sensitive. ILIKE is case-insensitive.
                    // Using ILIKE is generally better for user search.
                    command.CommandText = @"
                        SELECT uid, fullName, username, email, gender
                        FROM CL_Users
                        WHERE fullName ILIKE @searchTerm
                           OR username ILIKE @searchTerm
                           OR email ILIKE @searchTerm";

                    command.Parameters.AddWithValue("@searchTerm", $"%{q}%");

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            User user = new User
                            {
                                Uid = Convert.ToInt32(reader["uid"]),
                                FirstName = reader["fullName"].ToString().Split(' ')[0],
                                LastName = reader["fullName"].ToString().Split(' ').Length > 1 ? reader["fullName"].ToString().Split(' ')[1] : "",
                                Username = reader["username"].ToString(),
                                Email = reader["email"].ToString(),
                                Gender = reader["gender"].ToString(),
                            };
                            users.Add(user);
                        }
                    }
                }
                return Ok(new
                {
                    Message = "Success in searching for user",
                    Users = users
                });
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred in SearchUser: {e.Message}");
                return StatusCode(500, new
                {
                    Message = "An internal server error occurred.",
                    Error = e.Message
                });
            }
        }
    }
}