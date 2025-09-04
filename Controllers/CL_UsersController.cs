using CricLive.Models;
using CricLive.Models.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;


namespace CricLive.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CL_UsersController : ControllerBase
    {
        public IConfiguration _configuration;

        public CL_UsersController(IConfiguration configuration )
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
                SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("CricLive"));
                conn.Open();
                SqlCommand command = conn.CreateCommand();
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = @"SELECT uid, email, password
                                        FROM CL_Users
                                        WHERE email = @email";
                command.Parameters.AddWithValue("@email", _request.email);
                SqlDataReader reader = command.ExecuteReader();

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
                            expires: DateTime.UtcNow.AddMinutes(30), // increase expiry
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
            catch (Exception e)
            {
                return BadRequest(new
                {
                    Message = e.Message,
                    innerException = e.InnerException
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

                SqlConnection conn =new SqlConnection(_configuration.GetConnectionString("CricLive"));
                conn.Open();
                SqlCommand command = conn.CreateCommand();
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
                conn.Close();
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
                return StatusCode(500,new
                {
                    Message = e.Message,
                    innerException = e.InnerException
                });
            }
        }

        [HttpPost]
        [Route("VerifyOtp")]
        public IActionResult VerifyOtp([FromBody] OtpModel otp)
        {
                SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("CricLive"));
            try
            {
                conn.Open();
                SqlCommand command = conn.CreateCommand();
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = @"select *
	                                    from CL_Users
	                                    where email = @email";

                command.Parameters.AddWithValue("@email", otp.email);

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    DateTime otpTime = DateTime.Parse(reader["otpTime"].ToString());
                    int dbOtp = Convert.ToInt32(reader["otp"]);
                    string uid = reader["uid"].ToString();   // ✅ store before closing
                    string emailFromDb = reader["email"].ToString();

                    reader.Close();

                    if ((DateTime.Now - otpTime).TotalMinutes > 10)
                    {
                        return BadRequest(new { Message = "OTP has expired. Please request a new one." });
                    }

                    if (dbOtp == otp.otp)
                    {
                        SqlCommand verifyCommand = conn.CreateCommand();
                        verifyCommand.CommandType = System.Data.CommandType.Text;
                        verifyCommand.CommandText = @"update CL_Users
                                      set isVerified = 1
                                      where email = @email";
                        verifyCommand.Parameters.AddWithValue("@email", otp.email);
                        verifyCommand.ExecuteNonQuery();

                        var claims = new[]
                        {
            new Claim(JwtRegisteredClaimNames.Sub, _configuration["JwtConfig:Subject"]),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("uid", uid),                // ✅ use stored values
            new Claim("email", emailFromDb)
        };

                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtConfig:Key"]));
                        var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                        var token = new JwtSecurityToken(
                            issuer: _configuration["JwtConfig:Issuer"],
                            audience: _configuration["JwtConfig:Audience"],
                            claims: claims,
                            expires: DateTime.UtcNow.AddMinutes(30),
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
                    reader.Close();
                    return BadRequest(new { Message = "Invalid Email Address." });
                }
            }
            catch (Exception e)
            {
                return StatusCode(500,new
                {
                    Message = e.Message,
                    innerException = e.InnerException?.Message 
                });
            }
            finally
            {
                conn.Close();
            }
        }

        [HttpGet]

        [HttpPost]
        [Route("CreateUser")]
        public IActionResult CreateUser(User user)
        {
            try
            {
                using (SqlConnection _con = new SqlConnection(_configuration.GetConnectionString("CricLive")))
                {
                    _con.Open();

                    using (SqlCommand commandForCheckUser = _con.CreateCommand())
                    {
                        commandForCheckUser.CommandType = System.Data.CommandType.Text;
                        commandForCheckUser.CommandText = @"SELECT uid, email, password FROM CL_Users  WHERE email = @email";
                        commandForCheckUser.Parameters.AddWithValue("@email", user.Email);

                        using (SqlDataReader reader = commandForCheckUser.ExecuteReader())
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

                    using (SqlCommand command = _con.CreateCommand())
                    {
                        command.CommandType = System.Data.CommandType.Text;
                        command.CommandText = @"Insert into CL_Users (fullName,gender,email, isVerified,profilePhoto,role,username,password) values
	                                            (@firstName + ' ' + @lastName,@gender,@email,@isVerified,@profilePhoto,@role,@username,@password)";
                        command.Parameters.AddWithValue("@firstName", user.FirstName);
                        command.Parameters.AddWithValue("@lastName", user.LastName);
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@email", user.Email);
                        command.Parameters.AddWithValue("@profilePhoto", user.ProfilePhoto ?? " ");
                        command.Parameters.AddWithValue("@gender", user.Gender);
                        command.Parameters.AddWithValue("@isVerified", 0);
                        command.Parameters.AddWithValue("@role", "user");
                        command.Parameters.AddWithValue("@password", user.Password);

                        var newUserId = command.ExecuteScalar();
                        return Ok(new { Message = "User created successfully", Uid = newUserId });
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"A database error occurred: {ex.Message}");
                return StatusCode(500, new { Message = "An error occurred while communicating with the database." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                return StatusCode(500, new { Message = "An unexpected error occurred." });
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
                if (password == null || email == null)
                {
                    return BadRequest(new
                    {
                        Message = "Credentials are missing"
                    });
                }
                    using (SqlConnection _con = new SqlConnection(_configuration.GetConnectionString("CricLive")))
                    {
                        _con.Open();

                        using (SqlCommand command = _con.CreateCommand())
                        {
                            command.CommandType = System.Data.CommandType.Text;
                            command.CommandText = @"select * from CL_Users where email = @email";
                            command.Parameters.AddWithValue("@email", email);
                            SqlDataReader reader =  command.ExecuteReader();
                            if (!reader.HasRows)
                            {
                                return NotFound(new { Message = "User Not Found" });
                            }
                        }
                    }
                


                var hasher = new PasswordHasher<string>();
                password = hasher.HashPassword(null, password);
                using (SqlConnection _con = new SqlConnection(_configuration.GetConnectionString("CricLive")))
                {
                    _con.Open();
                    using(SqlCommand command = _con.CreateCommand())
                    {
                        command.CommandType = System.Data.CommandType.Text;
                        command.CommandText = @"update CL_Users set password = @password where email = @email";
                        command.Parameters.AddWithValue("@email",email);
                        command.Parameters.AddWithValue("@password",password);
                        command.ExecuteNonQuery();

                        return Ok(new { Message = "Password Change Successful" });
                    }
                }
            }
            catch(Exception e)
            {
                return StatusCode(500, new
                {
                    Message = e.Message,
                    InnerException = e.InnerException
                    
                });
            }
        }

        [HttpGet]
        [Route("GetUserByEmail/{email}")]
        public IActionResult GetUserByEmail(string email)
        {
            try
            {

                SqlConnection _con = new SqlConnection(_configuration.GetConnectionString("CricLive"));
                _con.Open();
                SqlCommand command = _con.CreateCommand();
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = @"select * from CL_Users where email = @email";
                command.Parameters.AddWithValue("@email", email);
                SqlDataReader reader = command.ExecuteReader();

                User user = new User();
                if (reader.Read())
                {
                    user.FirstName = reader["fullName"].ToString().Split(" ")[0];
                    //user.LastName = reader["fullName"].ToString().Split(" ")[1];
                    user.Role = reader["role"].ToString();
                    user.IsVerified = Convert.ToInt32(reader["isVerified"]);
                    user.ProfilePhoto = reader["profilePhoto"].ToString();
                    user.Username = reader["username"].ToString();
                    user.Email = reader["email"].ToString();
                    user.Gender = reader["gender"].ToString();
                    user.Password = reader["password"].ToString();
                }
                else
                {
                    return NotFound(new
                    {
                        Message = "User not found",
                    });
                }
                _con.Close();
                return Ok(new { Message = "User created successfully", User = user });
            }
            catch (Exception e)
            {
                return BadRequest(new
                {
                    Message = e.Message,
                    innerException = e.InnerException
                });
            }
        }

        [HttpGet]
        [Authorize]
        [Route("GetUser")]
        public IActionResult GetUser(int uid)
        {
            try
            {

            SqlConnection _con = new SqlConnection(_configuration.GetConnectionString("CricLive"));
            _con.Open();
            SqlCommand command = _con.CreateCommand();
            command.CommandType = System.Data.CommandType.Text;
            command.CommandText = @"select * from CL_Users where uid = @uid";
            command.Parameters.AddWithValue("@uid", uid);
            SqlDataReader reader =  command.ExecuteReader();

                User user = new User();
            if (reader.Read())
            {
                user.FirstName = reader["fullName"].ToString().Split(" ")[0];
                //user.LastName = reader["fullName"].ToString().Split(" ")[1];
                    user.Role = reader["role"].ToString();
                user.IsVerified = Convert.ToInt32(reader["isVerified"]);
                user.ProfilePhoto = reader["profilePhoto"].ToString();
                user.Username = reader["username"].ToString();
                user.Email = reader["email"].ToString();
                user.Gender = reader["gender"].ToString();
                user.Password = reader["password"].ToString();
                }
                else
                {
                    return NotFound(new
                    {
                        Message = "User not found",
                    });
                }
                    _con.Close();
            return Ok(new { Message = "User created successfully", User = user});
            }
            catch (Exception e)
            {
                return BadRequest(new
                {
                    Message = e.Message,
                    innerException = e.InnerException
                });
            }
        }

        [HttpGet]
        [Route("DeleteUser")]
        public IActionResult DeleteUser(int uid)
        {
            try
            {

                SqlConnection _con = new SqlConnection(_configuration.GetConnectionString("CricLive"));
                _con.Open();
                SqlCommand command = _con.CreateCommand();
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = "delete from CL_Users where uid = @userId ";
                command.Parameters.AddWithValue("@uid", uid);
                command.ExecuteNonQuery();

                // Read returned UserId
                _con.Close();
                return Ok(new { Message = "User delete successfully"});
            }
            catch (Exception e)
            {
                return BadRequest(new
                {
                    Message = e.Message,
                    innerException = e.InnerException
                });
            }
        }

        [HttpPut]
        [Route("UpdateUser")]
        public IActionResult UpdateUser(User user)
        {
            try
            {

                SqlConnection _con = new SqlConnection(_configuration.GetConnectionString("CricLive"));
                _con.Open();
                SqlCommand command = _con.CreateCommand();
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = @"Update CL_Users 
	                                    set fullName =  @firstName + ' ' + @lastName,
		                                    gender = @gender,
		                                    email = @email,
		                                    isVerified = @isVerified,
		                                    profilePhoto= @profilePhoto,
		                                    role = @role,
		                                    username =  @username,
		                                    password = @password
	                                    where uid = @userId";
                command.Parameters.AddWithValue("@userId", user.Uid);
                command.Parameters.AddWithValue("@firstName", user.FirstName);
                command.Parameters.AddWithValue("@lastName", user.LastName);
                command.Parameters.AddWithValue("@username", user.Username);
                command.Parameters.AddWithValue("@email", user.Email);
                command.Parameters.AddWithValue("@profilePhoto", user.ProfilePhoto ?? " ");
                command.Parameters.AddWithValue("@gender", user.Gender);
                command.Parameters.AddWithValue("@isVerified", user.IsVerified);
                command.Parameters.AddWithValue("@role", user.Role);
                command.Parameters.AddWithValue("@password", user.Password);
                command.ExecuteNonQuery();

                _con.Close();
                return Ok(new { Message = "User update successfully" });
            }
            catch (Exception e)
            {
                return BadRequest(new
                {
                    Message = e.Message,
                    innerException = e.InnerException
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
                using (SqlConnection _con = new SqlConnection(_configuration.GetConnectionString("CricLive")))
                {
                    _con.Open();
                    SqlCommand command = _con.CreateCommand();
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandText = "select * from CL_Users";

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            User user = new User
                            {
                                Uid = Convert.ToInt32(reader["uid"]),
                                FirstName = reader["fullName"].ToString().Split()[0], 
                                LastName = reader["fullName"].ToString().Split().Length == 1 ? " " : reader["fullName"].ToString().Split()[1],
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
                // Log the exception details for debugging purposes
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
        public IActionResult SearchUser( string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Ok(new List<User>()); 
            }

            try
            {
                List<User> users = new List<User>();
                using (SqlConnection _con = new SqlConnection(_configuration.GetConnectionString("CricLive")))
                {
                    _con.Open();
                    SqlCommand command = _con.CreateCommand();
                    command.CommandType = System.Data.CommandType.Text;

                    command.CommandText = @"
                        SELECT uid, fullName, username, email, gender
                        FROM CL_Users
                        WHERE fullName LIKE @searchTerm
                           OR username LIKE @searchTerm
                           OR email LIKE @searchTerm";

                    command.Parameters.AddWithValue("@searchTerm", $"%{q}%");

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            User user = new User
                            {
                                Uid = Convert.ToInt32(reader["uid"]),
                                FirstName = reader["fullName"].ToString().Split(' ')[0],
                                // Safely handle names with only one part
                                LastName = reader["fullName"].ToString().Split(' ').Length > 1
                                           ? reader["fullName"].ToString().Split(' ')[1]
                                           : "",
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
                    Message = "Success to seach user",
                    Users = users
                });
            }
            catch (Exception e)
            {
                // Log the exception details for debugging
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
