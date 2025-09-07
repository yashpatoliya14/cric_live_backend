using CricLive.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql; // Changed from System.Data.SqlClient
using System.Data;

namespace CricLive.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CL_TeamsController : ControllerBase
    {
        public IConfiguration _configuration;
        public CL_TeamsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("GetTeams")]
        public IActionResult GetTeams()
        {
            try
            {
                List<Team> teams = new List<Team>();
                string pgDataSource = _configuration.GetConnectionString("CricLive");
                using (NpgsqlConnection con = new NpgsqlConnection(pgDataSource))
                {
                    con.Open();
                    // Removed the [dbo]. schema prefix for better portability
                    using (NpgsqlCommand command = new NpgsqlCommand(@"SELECT * FROM CL_Teams", con))
                    {
                        command.CommandType = CommandType.Text;
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Team team = new Team
                                {
                                    teamId = Convert.ToInt32(reader["teamId"]),
                                    teamName = reader["teamName"].ToString(),
                                    tournamentId = reader["tournamentId"] != DBNull.Value ? Convert.ToInt32(reader["tournamentId"]) : null
                                };
                                teams.Add(team);
                            }
                        }
                    }
                }
                return Ok(teams);
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [HttpGet]
        [Route("GetTeamsByUid/{uid}")]
        public IActionResult GetTeamByUid(int uid)
        {
            try
            {
                List<Team> teams = new List<Team>();
                string pgDataSource = _configuration.GetConnectionString("CricLive");
                using (NpgsqlConnection con = new NpgsqlConnection(pgDataSource))
                {
                    con.Open();
                    using (NpgsqlCommand command = new NpgsqlCommand(@"SELECT teamId, teamName, logo, tournamentId
                        FROM CL_Teams
                        WHERE createdBy = @uid;", con))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@uid", uid);
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Team team = new Team
                                {
                                    teamId = Convert.ToInt32(reader["teamId"]),
                                    teamName = reader["teamName"].ToString(),
                                    logo = reader["logo"].ToString(),
                                    tournamentId = reader["tournamentId"] != DBNull.Value ? Convert.ToInt32(reader["tournamentId"]) : null
                                };
                                teams.Add(team);
                            }
                        }
                    }
                }
                return Ok(new
                {
                    Message = "Success to fetch teams",
                    Data = teams
                });
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [HttpGet]
        [Route("GetTeamsById/{teamId}")]
        public IActionResult GetTeamById(int teamId)
        {
            try
            {
                Team team = null; // Initialize as null
                string pgDataSource = _configuration.GetConnectionString("CricLive");
                using (NpgsqlConnection con = new NpgsqlConnection(pgDataSource))
                {
                    con.Open();
                    using (NpgsqlCommand command = new NpgsqlCommand(@"SELECT teamId, teamName, logo, tournamentId
                        FROM CL_Teams
                        WHERE teamId = @teamId;", con))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@teamId", teamId);
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                team = new Team
                                {
                                    teamId = Convert.ToInt32(reader["teamId"]),
                                    teamName = reader["teamName"].ToString(),
                                    logo = reader["logo"].ToString(),
                                    tournamentId = reader["tournamentId"] != DBNull.Value ? Convert.ToInt32(reader["tournamentId"]) : null
                                };
                            }
                        }
                    }
                }
                return Ok(new
                {
                    Message = "Success to fetch team",
                    Data = team
                });
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [HttpPost]
        [Route("CreateTeam")]
        public IActionResult CreateTeam([FromBody] Team team)
        {
            try
            {
                string pgDataSource = _configuration.GetConnectionString("CricLive");
                using (NpgsqlConnection con = new NpgsqlConnection(pgDataSource))
                {
                    con.Open();
                    // Use RETURNING clause for PostgreSQL to get the new ID
                    using (NpgsqlCommand command = new NpgsqlCommand(@"INSERT INTO CL_Teams (teamName, tournamentId, createdBy)
                        VALUES (@teamName, @tournamentId, @uid) RETURNING teamId;", con))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@teamName", team.teamName);
                        command.Parameters.AddWithValue("@uid", team.Uid);

                        if (team.tournamentId.HasValue)
                        {
                            command.Parameters.AddWithValue("@tournamentId", team.tournamentId.Value);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@tournamentId", DBNull.Value);
                        }

                        var newTeamId = command.ExecuteScalar();
                        return Ok(new { message = "Team created successfully.", teamId = newTeamId });
                    }
                }
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message, innerException = e.InnerException?.Message });
            }
        }

        [HttpPut]
        [Route("UpdateTeam/{teamId}")]
        public IActionResult UpdateTeam(int teamId, [FromBody] Team team)
        {
            try
            {
                string pgDataSource = _configuration.GetConnectionString("CricLive");
                using (NpgsqlConnection con = new NpgsqlConnection(pgDataSource))
                {
                    con.Open();
                    using (NpgsqlCommand command = new NpgsqlCommand(@"UPDATE CL_Teams
                        SET
                            teamName = @teamName,
                            tournamentId = @tournamentId
                        WHERE
                            teamId = @teamId;", con))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@teamId", teamId);
                        command.Parameters.AddWithValue("@teamName", team.teamName);

                        if (team.tournamentId.HasValue)
                        {
                            command.Parameters.AddWithValue("@tournamentId", team.tournamentId.Value);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@tournamentId", DBNull.Value);
                        }

                        command.ExecuteNonQuery();
                        return Ok(new { message = "Team updated successfully." });
                    }
                }
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [HttpDelete]
        [Route("DeleteTeam/{teamId}")]
        public IActionResult DeleteTeam(int teamId)
        {
            try
            {
                string pgDataSource = _configuration.GetConnectionString("CricLive");
                using (NpgsqlConnection con = new NpgsqlConnection(pgDataSource))
                {
                    con.Open();

                    // Note: For atomicity, these two operations should ideally be in a transaction.
                    // This conversion maintains the original logic.

                    // First, delete related players
                    using (NpgsqlCommand command = new NpgsqlCommand(@"DELETE FROM CL_TeamPlayers
                        WHERE teamId = @teamId;", con))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@teamId", teamId);
                        command.ExecuteNonQuery();
                    }

                    // Then, delete the team
                    using (NpgsqlCommand command = new NpgsqlCommand(@"DELETE FROM CL_Teams
                        WHERE teamId = @teamId;", con))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@teamId", teamId);
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            return NotFound(new { message = "Team not found or already deleted." });
                        }

                        return Ok(new { message = "Team deleted successfully." });
                    }
                }
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }
    }
}