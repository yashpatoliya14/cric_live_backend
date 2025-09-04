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
                    using (SqlCommand command = new SqlCommand("PR_CL_GetAllTeams", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Team team = new Team
                                {
                                    teamId = Convert.ToInt32(reader["teamId"]),
                                    teamName = reader["teamName"].ToString(),
                                    logo = reader["logo"].ToString(),
                                    // CORRECT: This properly handles DBNull and assigns null to your int? property.
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
        [Route("GetTeamById/{teamId}")]
        public IActionResult GetTeamById(int teamId)
        {
            try
            {
                Team team = null;
                string sqlDataSource = _configuration.GetConnectionString("CricLive");
                using (SqlConnection con = new SqlConnection(sqlDataSource))
                {
                    con.Open();
                    using (SqlCommand command = new SqlCommand("PR_CL_GetTeamById", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
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
                                    // CORRECT: Also handles DBNull correctly for a single record.
                                    tournamentId = reader["tournamentId"] != DBNull.Value ? Convert.ToInt32(reader["tournamentId"]) : null
                                };
                            }
                        }
                    }
                }
                if (team == null)
                {
                    return NotFound(new { message = "Team not found." });
                }
                return Ok(team);
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        // fetch by user id 
        [HttpGet]
        [Route("GetTeamByUid/{uid}")]
        public IActionResult GetTeamByUid(int uid)
        {
            try
            {
                List<Team> teams = new List<Team>();
                string sqlDataSource = _configuration.GetConnectionString("CricLive");
                using (SqlConnection con = new SqlConnection(sqlDataSource))
                {
                    con.Open();
                    using (SqlCommand command = new SqlCommand("PR_CL_GetTeamByUid", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
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
                    using (SqlCommand command = new SqlCommand("PR_CL_CreateTeam", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@teamName", team.teamName);
                        command.Parameters.AddWithValue("@logo", team.logo);
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
                    using (SqlCommand command = new SqlCommand("PR_CL_UpdateTeam", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@teamId", teamId);
                        command.Parameters.AddWithValue("@teamName", team.teamName);
                        command.Parameters.AddWithValue("@logo", team.logo);

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
                    using (SqlCommand command = new SqlCommand("PR_CL_DeleteTeam", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
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