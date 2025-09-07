using CricLive.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql; // Changed from System.Data.SqlClient
using System.Data;

namespace CricLive.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CL_TeamPlayersController : ControllerBase
    {
        public IConfiguration _configuration;
        public CL_TeamPlayersController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("GetTeamPlayersById/{teamId}")]
        public IActionResult GetTeamPlayersById(int teamId)
        {
            try
            {
                List<TeamPlayer> players = new List<TeamPlayer>();
                string pgDataSource = _configuration.GetConnectionString("CricLive");
                using (NpgsqlConnection con = new NpgsqlConnection(pgDataSource))
                {
                    con.Open();
                    using (NpgsqlCommand command = new NpgsqlCommand(@"SELECT 
                            teamPlayerId,
                            teamId,
                            playerName
                        FROM 
                            CL_TeamPlayers 
                        WHERE 
                            teamId = @teamId;", con))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@teamId", teamId);
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                TeamPlayer player = new TeamPlayer
                                {
                                    TeamPlayerId = Convert.ToInt32(reader["teamPlayerId"]),
                                    TeamId = Convert.ToInt32(reader["teamId"]),
                                    PlayerName = reader["playerName"].ToString(),
                                };
                                players.Add(player);
                            }
                        }
                    }
                }
                return Ok(new
                {
                    Message = "Success to fetch players",
                    Data = players
                });
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        // POST: api/CL_TeamPlayers/CreateTeamPlayer
        [HttpPost]
        [Route("CreateTeamPlayer")]
        public IActionResult CreateTeamPlayer([FromBody] TeamPlayer teamPlayer)
        {
            try
            {
                string pgDataSource = _configuration.GetConnectionString("CricLive");
                using (NpgsqlConnection con = new NpgsqlConnection(pgDataSource))
                {
                    con.Open();
                    // Use the RETURNING clause for PostgreSQL to get the new ID
                    using (NpgsqlCommand command = new NpgsqlCommand(@"INSERT INTO CL_TeamPlayers (playerName, teamId)
                        VALUES (@playerName, @teamId) RETURNING teamPlayerId;", con))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@playerName", teamPlayer.PlayerName);
                        command.Parameters.AddWithValue("@teamId", teamPlayer.TeamId);

                        var newId = command.ExecuteScalar();
                        return Ok(new { message = "Team player added successfully.", teamPlayerId = newId });
                    }
                }
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message, innerException = e.InnerException?.Message });
            }
        }

        // DELETE: api/CL_TeamPlayers/DeleteTeamPlayer/5
        [HttpDelete]
        [Route("DeleteTeamPlayer/{teamPlayerId}")]
        public IActionResult DeleteTeamPlayer(int teamPlayerId)
        {
            try
            {
                string pgDataSource = _configuration.GetConnectionString("CricLive");
                using (NpgsqlConnection con = new NpgsqlConnection(pgDataSource))
                {
                    con.Open();
                    using (NpgsqlCommand command = new NpgsqlCommand(@"DELETE FROM CL_TeamPlayers
                        WHERE teamPlayerId = @teamPlayerId;", con))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@teamPlayerId", teamPlayerId);
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            return NotFound(new { message = "Team player not found or already deleted." });
                        }
                        return Ok(new { message = "Team player deleted successfully." });
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