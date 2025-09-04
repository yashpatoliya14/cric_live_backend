using CricLive.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

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
                string sqlDataSource = _configuration.GetConnectionString("CricLive");
                using (SqlConnection con = new SqlConnection(sqlDataSource))
                {
                    con.Open();
                    using (SqlCommand command = new SqlCommand(@"Select * from [dbo].CL_Teams", con))
                    {
                        command.CommandType = CommandType.Text;
                        using (SqlDataReader reader = command.ExecuteReader())
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

        // fetch by user id 
        [HttpGet]
        [Route("GetTeamsByUid/{uid}")]
        public IActionResult GetTeamByUid(int uid)
        {
            try
            {
                List<Team> teams = new List<Team>();
                string sqlDataSource = _configuration.GetConnectionString("CricLive");
                using (SqlConnection con = new SqlConnection(sqlDataSource))
                {
                    con.Open();
                    using (SqlCommand command = new SqlCommand(@"SELECT teamId, teamName, logo, tournamentId
    FROM CL_Teams
    WHERE createdBy = @uid;", con))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@uid", uid);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {

                            while (reader.Read())
                            {
                                Team team;
                                team = new Team
                                {
                                    teamId = Convert.ToInt32(reader["teamId"]),
                                    teamName = reader["teamName"].ToString(),
                                    logo = reader["logo"].ToString(),
                                    // CORRECT: Also handles DBNull correctly for a single record.
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

        // fetch by user id 
        [HttpGet]
        [Route("GetTeamsById/{teamId}")]
        public IActionResult GetTeamById(int teamId)
        {
            try
            {
                Team team = new Team();

                string sqlDataSource = _configuration.GetConnectionString("CricLive");
                using (SqlConnection con = new SqlConnection(sqlDataSource))
                {
                    con.Open();
                    using (SqlCommand command = new SqlCommand(@"SELECT teamId, teamName, logo, tournamentId
    FROM CL_Teams
    WHERE teamId = @teamId;", con))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@teamId", teamId);
                        using (SqlDataReader reader = command.ExecuteReader())
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
                    Message = "Success to fetch teams",
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
                string sqlDataSource = _configuration.GetConnectionString("CricLive");
                using (SqlConnection con = new SqlConnection(sqlDataSource))
                {
                    con.Open();
                    using (SqlCommand command = new SqlCommand(@" INSERT INTO CL_Teams (teamName, tournamentId,createdBy)
    VALUES (@teamName, @tournamentId,@uid);  SELECT SCOPE_IDENTITY();", con))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@teamName", team.teamName);
                        command.Parameters.AddWithValue("@uid", team.Uid);

                        // CORRECT: This logic works because team.tournamentId is now a nullable int (int?).
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
                return BadRequest(new { message = e.Message });
            }
        }

        [HttpPut]
        [Route("UpdateTeam/{teamId}")]
        public IActionResult UpdateTeam(int teamId, [FromBody] Team team)
        {
            try
            {
                string sqlDataSource = _configuration.GetConnectionString("CricLive");
                using (SqlConnection con = new SqlConnection(sqlDataSource))
                {
                    con.Open();
                    using (SqlCommand command = new SqlCommand(@"UPDATE CL_Teams
    SET
        teamName = @teamName,
        tournamentId = @tournamentId
    WHERE
        teamId = @teamId;", con))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@teamId", teamId);
                        command.Parameters.AddWithValue("@teamName", team.teamName);

                        // CORRECT: This logic also works perfectly with the updated model.
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
                string sqlDataSource = _configuration.GetConnectionString("CricLive");
                using (SqlConnection con = new SqlConnection(sqlDataSource))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand(@"DELETE FROM CL_TeamPlayers
    WHERE teamId = @teamId;", con))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@teamId", teamId);
                        int rowsAffected = command.ExecuteNonQuery();
                        
                    }
                    using (SqlCommand command = new SqlCommand(@"DELETE FROM CL_Teams
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