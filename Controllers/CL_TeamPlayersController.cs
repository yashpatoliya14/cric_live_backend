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

        // GET: api/CL_TeamPlayers/GetTeamPlayers?teamId=1
        [HttpGet]
        [Route("GetTeamPlayers")]
        public IActionResult GetTeamPlayers(int teamId)
        {
            try
            {
                List<TeamPlayer> players = new List<TeamPlayer>();
                string sqlDataSource = _configuration.GetConnectionString("CricLive");
                using (SqlConnection con = new SqlConnection(sqlDataSource))
                {
                    con.Open();
                    using (SqlCommand command = new SqlCommand("PR_CL_GetPlayersByTeamId", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@teamId", teamId);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                TeamPlayer player = new TeamPlayer
                                {
                                    teamPlayerId = Convert.ToInt32(reader["teamPlayerId"]),
                                    teamId = Convert.ToInt32(reader["teamId"]),
                                    playerId = Convert.ToInt32(reader["playerId"]),
                                    playerName = reader["playerName"].ToString(),
                                    uid = Convert.ToInt32(reader["playerId"]) // Assuming uid is the playerId
                                };
                                players.Add(player);
                            }
                        }
                    }
                }
                return Ok(players);
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        // GET: api/CL_TeamPlayers/GetTeamPlayerById/5
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
                    using (SqlCommand command = new SqlCommand("PR_CL_GetPlayersByTeamId", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@teamId", teamId);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                TeamPlayer player = null;
                                player = new TeamPlayer
                                {
                                    teamPlayerId = Convert.ToInt32(reader["teamPlayerId"]),
                                    teamId = Convert.ToInt32(reader["teamId"]),
                                    playerId = Convert.ToInt32(reader["playerId"]),
                                    playerName = reader["playerName"].ToString(),
                                    uid = Convert.ToInt32(reader["playerId"])
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
                    using (SqlCommand command = new SqlCommand("PR_CL_CreateTeamPlayer", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@teamId", teamPlayer.teamId);
                        command.Parameters.AddWithValue("@uid", teamPlayer.uid); // uid is the player's main ID

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

        // PUT: api/CL_TeamPlayers/UpdateTeamPlayer/5
        [HttpPut]
        [Route("UpdateTeamPlayer/{teamPlayerId}")]
        public IActionResult UpdateTeamPlayer(int teamPlayerId, [FromBody] TeamPlayer teamPlayer)
        {
            try
            {
                string sqlDataSource = _configuration.GetConnectionString("CricLive");
                using (SqlConnection con = new SqlConnection(sqlDataSource))
                {
                    con.Open();
                    using (SqlCommand command = new SqlCommand("PR_CL_UpdateTeamPlayer", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@teamPlayerId", teamPlayerId);
                        command.Parameters.AddWithValue("@teamId", teamPlayer.teamId);
                        command.Parameters.AddWithValue("@uid", teamPlayer.uid);

                        command.ExecuteNonQuery();
                        return Ok(new { message = "Team player updated successfully." });
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
                    using (SqlCommand command = new SqlCommand("PR_CL_DeleteTeamPlayer", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
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