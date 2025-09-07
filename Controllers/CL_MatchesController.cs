using CricLive.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql; // Changed from System.Data.SqlClient
using System.Data;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class CL_MatchesController : ControllerBase
{
    IConfiguration _configuration;

    public CL_MatchesController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    [Route("GetLiveMatch")]
    public IActionResult GetLiveMatches()
    {
        try
        {
            string pgDataSource = _configuration.GetConnectionString("CricLive");
            List<Match> matches = new List<Match>();
            using (NpgsqlConnection conn = new NpgsqlConnection(pgDataSource))
            {
                conn.Open();
                using (NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM CL_Matches WHERE status = 'live'", conn))
                {
                    command.CommandType = CommandType.Text;
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var match = new Match
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                InningNo = Convert.ToInt32(reader["inningNo"]),
                                Team1 = Convert.ToInt32(reader["team1"]),
                                Team2 = Convert.ToInt32(reader["team2"]),
                                MatchDate = Convert.ToDateTime(reader["matchDate"]),
                                Overs = Convert.ToInt32(reader["overs"]),
                                Status = reader["status"].ToString(),
                                TossWon = reader["tossWon"] != DBNull.Value ? Convert.ToInt32(reader["tossWon"]) : null,
                                Decision = reader["decision"].ToString(),
                                TournamentId = reader["tournamentId"] != DBNull.Value ? Convert.ToInt32(reader["tournamentId"]) : null,
                                Uid = Convert.ToInt32(reader["uid"]),
                                MatchState = reader["matchState"]?.ToString(),
                                CurrentOvers = Convert.ToDecimal(reader["currentOvers"]),
                                StrikerBatsmanId = reader["strikerBatsmanId"] != DBNull.Value ? Convert.ToInt32(reader["strikerBatsmanId"]) : null,
                                NonStrikerBatsmanId = reader["nonStrikerBatsmanId"] != DBNull.Value ? Convert.ToInt32(reader["nonStrikerBatsmanId"]) : null,
                                BowlerId = reader["bowlerId"] != DBNull.Value ? Convert.ToInt32(reader["bowlerId"]) : null,
                                CurrentBattingTeamId = reader["currentBattingTeamId"] != DBNull.Value ? Convert.ToInt32(reader["currentBattingTeamId"]) : null
                            };
                            matches.Add(match);
                        }
                    }
                }
            }
            return Ok(new { Matches = matches });
        }
        catch (Exception e)
        {
            var errorResponse = new
            {
                Message = "An unexpected server error occurred while fetching live matches.",
                Error = e.InnerException?.Message ?? e.Message
            };
            return StatusCode(500, errorResponse);
        }
    }

    [HttpGet]
    [Route("GetMatchesByUser/{uid}")]
    public IActionResult GetAllMatchesByUser(int uid)
    {
        try
        {
            var matches = new List<Match>();
            string pgDataSource = _configuration.GetConnectionString("CricLive");
            using (NpgsqlConnection conn = new NpgsqlConnection(pgDataSource))
            {
                conn.Open();
                using (NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM CL_Matches WHERE uid = @uid AND tournamentId IS NULL", conn))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@uid", uid);
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            matches.Add(new Match
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                InningNo = Convert.ToInt32(reader["inningNo"]),
                                Team1 = Convert.ToInt32(reader["team1"]),
                                Team2 = Convert.ToInt32(reader["team2"]),
                                MatchDate = Convert.ToDateTime(reader["matchDate"]),
                                Overs = Convert.ToInt32(reader["overs"]),
                                Status = reader["status"].ToString(),
                                TossWon = reader["tossWon"] != DBNull.Value ? Convert.ToInt32(reader["tossWon"]) : null,
                                Decision = reader["decision"].ToString(),
                                TournamentId = reader["tournamentId"] != DBNull.Value ? Convert.ToInt32(reader["tournamentId"]) : null,
                                Uid = Convert.ToInt32(reader["uid"]),
                                MatchState = reader["matchState"] != DBNull.Value ? reader["matchState"]?.ToString() : null,
                                CurrentOvers = Convert.ToDecimal(reader["currentOvers"]),
                                StrikerBatsmanId = reader["strikerBatsmanId"] != DBNull.Value ? Convert.ToInt32(reader["strikerBatsmanId"]) : null,
                                NonStrikerBatsmanId = reader["nonStrikerBatsmanId"] != DBNull.Value ? Convert.ToInt32(reader["nonStrikerBatsmanId"]) : null,
                                BowlerId = reader["bowlerId"] != DBNull.Value ? Convert.ToInt32(reader["bowlerId"]) : null,
                                CurrentBattingTeamId = reader["currentBattingTeamId"] != DBNull.Value ? Convert.ToInt32(reader["currentBattingTeamId"]) : null
                            });
                        }
                    }
                }
            }
            return Ok(new { Matches = matches });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { Message = e.Message });
        }
    }

    [HttpPost]
    [Route("CreateMatch")]
    public IActionResult CreateMatch([FromBody] CreateMatchDto matchDto)
    {
        try
        {
            string pgDataSource = _configuration.GetConnectionString("CricLive");
            using (NpgsqlConnection conn = new NpgsqlConnection(pgDataSource))
            {
                conn.Open();
                using (NpgsqlCommand command = new NpgsqlCommand("CL_CreateMatch", conn))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@team1", matchDto.Team1);
                    command.Parameters.AddWithValue("@team2", matchDto.Team2);
                    command.Parameters.AddWithValue("@matchDate", matchDto.MatchDate);
                    command.Parameters.AddWithValue("@overs", matchDto.Overs);
                    command.Parameters.AddWithValue("@wideRun", matchDto.WideRun);
                    command.Parameters.AddWithValue("@noBallRun", matchDto.NoBallRun);
                    command.Parameters.AddWithValue("@tournamentId", (object)matchDto.TournamentId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@uid", matchDto.Uid);
                    command.Parameters.AddWithValue("@matchState", (object)matchDto.MatchState ?? DBNull.Value);

                    var newMatchId = command.ExecuteScalar();
                    return Ok(new { MatchId = newMatchId });
                }
            }
        }
        catch (Exception e)
        {
            var errorResponse = new
            {
                Message = "An unexpected server error occurred while creating the match.",
                Error = e.InnerException?.Message ?? e.Message
            };
            return StatusCode(500, errorResponse);
        }
    }

    [HttpGet]
    [Route("GetMatchById/{matchId}")]
    public IActionResult GetMatchById(int matchId)
    {
        try
        {
            string pgDataSource = _configuration.GetConnectionString("CricLive");
            using (NpgsqlConnection conn = new NpgsqlConnection(pgDataSource))
            {
                conn.Open();
                string query = "SELECT * FROM CL_Matches WHERE id = @matchId";
                using (NpgsqlCommand command = new NpgsqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("@matchId", matchId);
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var match = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                var value = reader.GetValue(i);
                                match.Add(reader.GetName(i), value == DBNull.Value ? null : value);
                            }
                            return Ok(new { Message = "Success to fetch", Match = match });
                        }
                        else
                        {
                            return NotFound(new { Message = "Match not found" });
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            return StatusCode(500, new { Message = "Server error while fetching match", Error = e.Message });
        }
    }

    [HttpPut]
    [Route("UpdateMatch/{matchId}")]
    public IActionResult UpdateMatch(int matchId, [FromBody] UpdateMatchDto matchDto)
    {
        if (matchDto == null || matchId <= 0)
        {
            return BadRequest(new { Message = "Invalid request data or Match ID." });
        }

        try
        {
            string pgDataSource = _configuration.GetConnectionString("CricLive");
            using (NpgsqlConnection conn = new NpgsqlConnection(pgDataSource))
            {
                conn.Open();
                var queryBuilder = new StringBuilder("UPDATE CL_Matches SET ");
                var parameters = new List<NpgsqlParameter>();

                if (matchDto.Status != null)
                {
                    queryBuilder.Append("status = @status, ");
                    parameters.Add(new NpgsqlParameter("@status", matchDto.Status));
                }
                if (matchDto.InningNo.HasValue)
                {
                    queryBuilder.Append("inningNo = @inningNo, ");
                    parameters.Add(new NpgsqlParameter("@inningNo", matchDto.InningNo.Value));
                }
                if (matchDto.MatchState != null)
                {
                    queryBuilder.Append("matchState = @matchState, ");
                    parameters.Add(new NpgsqlParameter("@matchState", matchDto.MatchState));
                }
                // ... (add other fields similarly)

                if (parameters.Count == 0)
                {
                    return BadRequest(new { Message = "No fields provided to update." });
                }

                queryBuilder.Length -= 2; // Remove the final trailing comma and space
                queryBuilder.Append(" WHERE id = @id");
                parameters.Add(new NpgsqlParameter("@id", matchId));

                using (NpgsqlCommand command = new NpgsqlCommand(queryBuilder.ToString(), conn))
                {
                    command.Parameters.AddRange(parameters.ToArray());
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return Ok(new { Message = "Match updated successfully." });
                    }
                    else
                    {
                        return NotFound(new { Message = "Match not found or no changes were made." });
                    }
                }
            }
        }
        catch (Exception e)
        {
            var errorResponse = new
            {
                Message = "An unexpected server error occurred.",
                Error = e.InnerException?.Message ?? e.Message
            };
            return StatusCode(500, errorResponse);
        }
    }

    [HttpPut]
    [Route("UpdateMatchState")]
    public IActionResult UpdateMatchState([FromBody] UpdateMatchStateDto stateDto)
    {
        try
        {
            string pgDataSource = _configuration.GetConnectionString("CricLive");
            using (NpgsqlConnection conn = new NpgsqlConnection(pgDataSource))
            {
                conn.Open();
                using (NpgsqlCommand command = new NpgsqlCommand("CL_UpdateMatchState", conn))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@matchId", stateDto.MatchId);
                    command.Parameters.AddWithValue("@matchState", stateDto.MatchState);
                    command.ExecuteNonQuery();
                    return Ok(new { Message = "Updated" });
                }
            }
        }
        catch (Exception e)
        {
            return StatusCode(500, new { Message = e.ToString() });
        }
    }

    [HttpGet]
    [Route("GetMatchState")]
    public IActionResult GetMatchState(int matchId)
    {
        try
        {
            string pgDataSource = _configuration.GetConnectionString("CricLive");
            using (NpgsqlConnection conn = new NpgsqlConnection(pgDataSource))
            {
                conn.Open();
                using (NpgsqlCommand command = new NpgsqlCommand("CL_GetMatchState", conn))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@matchId", matchId);
                    var matchState = command.ExecuteScalar();

                    if (matchState == null || matchState == DBNull.Value)
                    {
                        return NotFound(new { Message = "Match state not found." });
                    }
                    return Ok(new
                    {
                        Message = "Data Get Successful",
                        Data = matchState.ToString()
                    });
                }
            }
        }
        catch (Exception e)
        {
            return StatusCode(500, new { Message = "Server error while fetching match state.", Error = e.Message });
        }
    }

    [HttpDelete]
    [Route("DeleteMatch")]
    public IActionResult DeleteMatch(int id)
    {
        try
        {
            string pgDataSource = _configuration.GetConnectionString("CricLive");
            using (NpgsqlConnection conn = new NpgsqlConnection(pgDataSource))
            {
                conn.Open();
                using (NpgsqlCommand command = new NpgsqlCommand("CL_DeleteMatch", conn))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@id", id);
                    command.ExecuteNonQuery();
                    return Ok("Match deleted successfully.");
                }
            }
        }
        catch (Exception e)
        {
            return StatusCode(500, new { Message = "Server error while deleting match.", Error = e.Message });
        }
    }

    [HttpGet]
    [Route("GetMatchesByTournamentId/{id}")]
    public IActionResult GetMatchesByTournamentId(int id)
    {
        try
        {
            string pgDataSource = _configuration.GetConnectionString("CricLive");
            using (NpgsqlConnection conn = new NpgsqlConnection(pgDataSource))
            {
                conn.Open();
                List<Match> matches = new List<Match>();
                using (NpgsqlCommand command = new NpgsqlCommand("CL_GetMatchesByTournamentId", conn))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@tournamentId", id);
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            matches.Add(new Match
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Team1 = Convert.ToInt32(reader["team1"]),
                                Team2 = Convert.ToInt32(reader["team2"]),
                                MatchDate = Convert.ToDateTime(reader["matchDate"]),
                                Overs = Convert.ToInt32(reader["overs"]),
                                Uid = Convert.ToInt32(reader["uid"]),
                                InningNo = reader["inningNo"] != DBNull.Value ? Convert.ToInt32(reader["inningNo"]) : 0,
                                Team1Name = reader["team1Name"]?.ToString(),
                                Team2Name = reader["team2Name"]?.ToString(),
                                Status = reader["status"]?.ToString(),
                                TossWon = reader["tossWon"] != DBNull.Value ? Convert.ToInt32(reader["tossWon"]) : null,
                                Decision = reader["decision"]?.ToString(),
                                MatchState = reader["matchState"]?.ToString(),
                                TournamentId = reader["tournamentId"] != DBNull.Value ? Convert.ToInt32(reader["tournamentId"]) : null,
                                WideRun = reader["wideRun"] != DBNull.Value ? Convert.ToInt32(reader["wideRun"]) : 0,
                                NoBallRun = reader["noBallRun"] != DBNull.Value ? Convert.ToInt32(reader["noBallRun"]) : 0,
                                CurrentOvers = reader["currentOvers"] != DBNull.Value ? Convert.ToDecimal(reader["currentOvers"]) : 0,
                                StrikerBatsmanId = reader["strikerBatsmanId"] != DBNull.Value ? Convert.ToInt32(reader["strikerBatsmanId"]) : (int?)null,
                                NonStrikerBatsmanId = reader["nonStrikerBatsmanId"] != DBNull.Value ? Convert.ToInt32(reader["nonStrikerBatsmanId"]) : (int?)null,
                                BowlerId = reader["bowlerId"] != DBNull.Value ? Convert.ToInt32(reader["bowlerId"]) : (int?)null,
                                CurrentBattingTeamId = reader["currentBattingTeamId"] != DBNull.Value ? Convert.ToInt32(reader["currentBattingTeamId"]) : (int?)null,
                            });
                        }
                    }
                    return Ok(new
                    {
                        Message = "Data Get Successful",
                        Matches = matches
                    });
                }
            }
        }
        catch (Exception e)
        {
            return StatusCode(500, new
            {
                Message = e.Message
            });
        }
    }
}