using CricLive.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace CricLive.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CL_TournamentTeamsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CL_TournamentTeamsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Gets all teams associated with a specific tournament.
        /// </summary>
        [HttpGet]
        [Route("GetTournamentTeams")]
        public IActionResult GetTournamentTeams()
        {
            try
            { 
                List<TournamentTeam> tournamentTeams = new List<TournamentTeam>();
                string sqlDataSource = _configuration.GetConnectionString("CricLive");

                using (SqlConnection conn = new SqlConnection(sqlDataSource))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand("PR_CL_GetTournamentTeams", conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tournamentTeams.Add(new TournamentTeam
                                {
                                    TournamentTeamId = Convert.ToInt32(reader["tournamentTeamId"]),
                                    TournamentId = Convert.ToInt32(reader["tournamentId"]),
                                    TeamId = Convert.ToInt32(reader["teamId"]),
                                    TeamName = reader["teamName"].ToString(),
                                    Logo = reader["logo"].ToString()
                                });
                            }
                        }
                    }
                }
                return Ok(new { 
                    Message="Success to fetch teams",
                    Data = tournamentTeams });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while fetching tournament teams.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Gets a specific tournament-team mapping by its unique ID.
        /// </summary>
        [HttpGet]
        [Route("GetTournamentTeamById/{id}")]
        public IActionResult GetTournamentTeamById(int id)
        {
            try
            {
                string sqlDataSource = _configuration.GetConnectionString("CricLive");
                List<TournamentTeam> teams = new List<TournamentTeam>();
                using (SqlConnection conn = new SqlConnection(sqlDataSource))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand("PR_CL_GetTournamentTeamById", conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@tournamentTeamId", id);

                        using (SqlDataReader reader = command.ExecuteReader())
                        { 
                            while (reader.Read())
                            {
                                TournamentTeam tournamentTeam = null;
                                tournamentTeam = new TournamentTeam
                                {
                                    TournamentTeamId = Convert.ToInt32(reader["tournamentTeamId"]),
                                    TournamentId = Convert.ToInt32(reader["tournamentId"]),
                                    TeamId = Convert.ToInt32(reader["teamId"]),
                                    TeamName = reader["teamName"].ToString(),
                                    Logo = reader["logo"].ToString()
                                };
                                teams.Add(tournamentTeam);
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
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while fetching the tournament team.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Creates a new mapping between a tournament and a team.
        /// </summary>
        [HttpPost]
        [Route("CreateTournamentTeam")]
        public IActionResult CreateTournamentTeam([FromBody] TournamentTeam tournamentTeamDto)
        {
            try
            {
                string sqlDataSource = _configuration.GetConnectionString("CricLive");
                using (SqlConnection conn = new SqlConnection(sqlDataSource))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand("PR_CL_CreateTournamentTeam", conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@tournamentId", tournamentTeamDto.TournamentId);
                        command.Parameters.AddWithValue("@teamId", tournamentTeamDto.TeamId);

                        var newId = command.ExecuteScalar();
                        return Ok(new { Message = "Tournament team created successfully.", TournamentTeamId = newId });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while creating the tournament team.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing tournament-team mapping.
        /// </summary>
        [HttpPut]
        [Route("UpdateTournamentTeam/{id}")]
        public IActionResult UpdateTournamentTeam(int id, [FromBody] TournamentTeam tournamentTeamDto)
        {
            try
            {
                string sqlDataSource = _configuration.GetConnectionString("CricLive");
                using (SqlConnection conn = new SqlConnection(sqlDataSource))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand("PR_CL_UpdateTournamentTeam", conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@tournamentTeamId", id);
                        command.Parameters.AddWithValue("@tournamentId", tournamentTeamDto.TournamentId);
                        command.Parameters.AddWithValue("@teamId", tournamentTeamDto.TeamId);

                        command.ExecuteNonQuery();
                        return Ok(new { Message = "Tournament team updated successfully." });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while updating the tournament team.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a tournament-team mapping by its ID.
        /// </summary>
        [HttpDelete]
        [Route("DeleteTournamentTeam/{id}")]
        public IActionResult DeleteTournamentTeam(int id)
        {
            try
            {
                string sqlDataSource = _configuration.GetConnectionString("CricLive");
                using (SqlConnection conn = new SqlConnection(sqlDataSource))
                {
                    conn.Open();
                    using (SqlCommand command = new SqlCommand("PR_CL_DeleteTournamentTeam", conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@tournamentTeamId", id);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            return NotFound(new { Message = "Tournament team not found or already deleted." });
                        }

                        return Ok(new { Message = "Tournament team deleted successfully." });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while deleting the tournament team.", Details = ex.Message });
            }
        }
    }
}
