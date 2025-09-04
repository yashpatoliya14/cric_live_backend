using CricLive.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

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
                string sqlDataSource = _configuration.GetConnectionString("CricLive");
                using (SqlConnection con = new SqlConnection(sqlDataSource))
                {
                    con.Open();
                    using (SqlCommand command = new SqlCommand(@"SELECT 
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
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                TeamPlayer player = null;
                                player = new TeamPlayer
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
                return Ok(new { 
                    Message="Succes to fetch",
                    Data= players});
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
                string sqlDataSource = _configuration.GetConnectionString("CricLive");
                using (SqlConnection con = new SqlConnection(sqlDataSource))
                {
                    con.Open();
                    using (SqlCommand command = new SqlCommand(@"INSERT INTO CL_TeamPlayers (playerName,teamId)
    VALUES (@playerName,@teamId)", con))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@playerName", teamPlayer.PlayerName); // uid is the player's main ID
                        command.Parameters.AddWithValue("@teamId", teamPlayer.TeamId); // uid is the player's main ID

                        var newId = command.ExecuteScalar();
                        return Ok(new { message = "Team player added successfully.", teamPlayerId = newId });

                    }
                }
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        

        // DELETE: api/CL_TeamPlayers/DeleteTeamPlayer/5
        [HttpDelete]
        [Route("DeleteTeamPlayer/{teamPlayerId}")]
        public IActionResult DeleteTeamPlayer(int teamPlayerId)
        {
            try
            {
                string sqlDataSource = _configuration.GetConnectionString("CricLive");
                using (SqlConnection con = new SqlConnection(sqlDataSource))
                {
                    con.Open();
                    using (SqlCommand command = new SqlCommand(@"DELETE FROM CL_TeamPlayers
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