using CricLive.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class CL_MatchesController : ControllerBase
{
    IConfiguration _configuration;


    // Constructor to get the connection string
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

        
        string sqlDataSource = _configuration.GetConnectionString("CricLive");
        List<Match> matches = new List<Match>();
        using (SqlConnection conn = new SqlConnection(sqlDataSource))
        {
            conn.Open();
            using (SqlCommand command = new SqlCommand("select * from CL_Matches where status = 'live'", conn))
            {
                command.CommandType = CommandType.Text;

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader!=null) // Check if a record was found
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
                            TossWon = Convert.ToInt32(reader["tossWon"]),
                            Decision = reader["decision"].ToString(),
                            TournamentId = reader["tournamentId"] != DBNull.Value ? Convert.ToInt32(reader["tournamentId"] ):null,
                            Uid = Convert.ToInt32(reader["uid"]),
                            MatchState = reader["matchState"]?.ToString(),
                            CurrentOvers = Convert.ToDecimal(reader["currentOvers"]),
                            // Handle possible NULL values from the database
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
        }
             return Ok(new { Matches = matches });
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());

            // This creates a more helpful error message to send back to the client.
            var errorResponse = new
            { 
                Message = "An unexpected server error occurred while creating the match.",
                // The InnerException almost always contains the specific SQL Server error.
                Error = e.InnerException?.Message ?? e.Message
            };
            return StatusCode(500,new
            {
                ErrorResponse = errorResponse,
                Message = "Server error from fetch live matches"
            });
        }
    }

    [HttpGet]
    [Route("GetMatchesByUser/{uid}")]
    public IActionResult GetAllMatchesByUser(int uid)
    {
        try
        {

        var matches = new List<Match>();
        string sqlDataSource = _configuration.GetConnectionString("CricLive");
        using (SqlConnection conn = new SqlConnection(sqlDataSource))
        {
            conn.Open();
            using (SqlCommand command = new SqlCommand("select * from CL_Matches where uid = @uid", conn))
            {
                command.CommandType = CommandType.Text;
                command.Parameters.AddWithValue("@uid", uid);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read()) // Loop through all records
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
                            TossWon = Convert.ToInt32(reader["tossWon"]),
                            Decision = reader["decision"].ToString(),
                            TournamentId = reader["tournamentId"] != DBNull.Value ? Convert.ToInt32(reader["tournamentId"]) : null,
                            Uid = Convert.ToInt32(reader["uid"]),
                            MatchState = reader["matchState"]?.ToString(),
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
        }catch(Exception e)
        {
            return StatusCode(500,new
            {
                Message=e.Message
            });
        }
    }

    [HttpPost]
    [Route("CreateMatch")]
    public IActionResult CreateMatch([FromBody] CreateMatchDto matchDto)
    {
        try
        {

        string sqlDataSource = _configuration.GetConnectionString("CricLive");
        using (SqlConnection conn = new SqlConnection(sqlDataSource))
        {
            conn.Open();
            using (SqlCommand command = new SqlCommand("CL_CreateMatch", conn))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@team1", matchDto.Team1);
                command.Parameters.AddWithValue("@team2", matchDto.Team2);
                command.Parameters.AddWithValue("@matchDate", matchDto.MatchDate);
                command.Parameters.AddWithValue("@overs", matchDto.Overs);
                //command.Parameters.AddWithValue("@tossWon", matchDto.TossWon);
                //command.Parameters.AddWithValue("@decision", matchDto.Decision);
                //command.Parameters.AddWithValue("@strikerBatsmanId", matchDto.StrikerBatsmanId);
                //command.Parameters.AddWithValue("@nonStrikerBatsmanId", matchDto.NonStrikerBatsmanId);
                //command.Parameters.AddWithValue("@currentBattingTeamId", matchDto.CurrentBattingTeamId);
                command.Parameters.AddWithValue("@wideRun", matchDto.WideRun);
                command.Parameters.AddWithValue("@noBallRun", matchDto.NoBallRun);
                //command.Parameters.AddWithValue("@bowlerId", matchDto.BowlerId);
                command.Parameters.AddWithValue("@tournamentId", matchDto.TournamentId);
                command.Parameters.AddWithValue("@uid", matchDto.Uid);
                command.Parameters.AddWithValue("@matchState", (object)matchDto.MatchState ?? DBNull.Value);

                // Execute the command and get the new ID
                var newMatchId = command.ExecuteScalar();
                return Ok(new { MatchId = newMatchId });
            }
        }
        }catch(Exception e)
        {
            Console.WriteLine(e.ToString());

            // This creates a more helpful error message to send back to the client.
            var errorResponse = new
            {
                Message = "An unexpected server error occurred while creating the match.",
                // The InnerException almost always contains the specific SQL Server error.
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
            string sqlDataSource = _configuration.GetConnectionString("CricLive");
            using (SqlConnection conn = new SqlConnection(sqlDataSource))
            {
                conn.Open();
                // The SQL query is written directly here
                string query = "SELECT * FROM CL_Matches WHERE id = @matchId";
                using (SqlCommand command = new SqlCommand(query, conn))
                {
                    // Use parameters to prevent SQL injection
                    command.Parameters.AddWithValue("@matchId", matchId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Create a dictionary to hold the match data
                            var match = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                var value = reader.GetValue(i);
                                match.Add(reader.GetName(i), value == DBNull.Value ? null : value);
                            }
                            return Ok(new { Message = "Success to fetch",Match = match});
                        }
                        else
                        {
                            // If no rows are found, return a 404 Not Found
                            return NotFound(new { Message = "Match not found" });
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            // Log the exception e.ToString() for better debugging
            return StatusCode(500, new { Message = "Server error while fetching match" });
        }
    }

    // PUT: api/CL_Match/UpdateMatch?matchId={id}
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
            string sqlDataSource = _configuration.GetConnectionString("CricLive");
            using (SqlConnection conn = new SqlConnection(sqlDataSource))
            {
                conn.Open();

                var queryBuilder = new StringBuilder("UPDATE CL_Matches SET ");
                var parameters = new List<SqlParameter>();

                // --- Dynamically add fields to the query ---

                if (matchDto.Status != null)
                {
                    queryBuilder.Append("status = @status, ");
                    parameters.Add(new SqlParameter("@status", matchDto.Status));
                }
                if (matchDto.InningNo.HasValue)
                {
                    queryBuilder.Append("inningNo = @inningNo, ");
                    parameters.Add(new SqlParameter("@inningNo", matchDto.InningNo.Value));
                }
                if (matchDto.MatchState != null)
                {
                    queryBuilder.Append("matchState = @matchState, ");
                    parameters.Add(new SqlParameter("@matchState", matchDto.MatchState));
                }
                if (matchDto.CurrentOvers.HasValue)
                {
                    queryBuilder.Append("currentOvers = @currentOvers, ");
                    parameters.Add(new SqlParameter("@currentOvers", matchDto.CurrentOvers.Value));
                }
                if (matchDto.MatchDate.HasValue)
                {
                    queryBuilder.Append("matchDate = @matchDate, ");
                    parameters.Add(new SqlParameter("@matchDate", matchDto.MatchDate.Value));
                }
                if (matchDto.TossWon.HasValue)
                {
                    queryBuilder.Append("tossWon = @tossWon, ");
                    parameters.Add(new SqlParameter("@tossWon", matchDto.TossWon.Value));
                }
                if (matchDto.Decision != null)
                {
                    queryBuilder.Append("decision = @decision, ");
                    parameters.Add(new SqlParameter("@decision", matchDto.Decision));
                }

                // --- KEY CHANGE: Added validation to ignore invalid IDs (like 0) ---
                if (matchDto.StrikerBatsmanId.HasValue && matchDto.StrikerBatsmanId.Value > 0)
                {
                    queryBuilder.Append("strikerBatsmanId = @strikerBatsmanId, ");
                    parameters.Add(new SqlParameter("@strikerBatsmanId", matchDto.StrikerBatsmanId.Value));
                }
                if (matchDto.NonStrikerBatsmanId.HasValue && matchDto.NonStrikerBatsmanId.Value > 0)
                {
                    queryBuilder.Append("nonStrikerBatsmanId = @nonStrikerBatsmanId, ");
                    parameters.Add(new SqlParameter("@nonStrikerBatsmanId", matchDto.NonStrikerBatsmanId.Value));
                }
                if (matchDto.BowlerId.HasValue && matchDto.BowlerId.Value > 0)
                {
                    queryBuilder.Append("bowlerId = @bowlerId, ");
                    parameters.Add(new SqlParameter("@bowlerId", matchDto.BowlerId.Value));
                }
                if (matchDto.CurrentBattingTeamId.HasValue && matchDto.CurrentBattingTeamId.Value > 0)
                {
                    queryBuilder.Append("currentBattingTeamId = @currentBattingTeamId, ");
                    parameters.Add(new SqlParameter("@currentBattingTeamId", matchDto.CurrentBattingTeamId.Value));
                }
                // --- End of change ---

                if (matchDto.WideRun.HasValue)
                {
                    queryBuilder.Append("wideRun = @wideRun, ");
                    parameters.Add(new SqlParameter("@wideRun", matchDto.WideRun.Value));
                }
                if (matchDto.NoBallRun.HasValue)
                {
                    queryBuilder.Append("noBallRun = @noBallRun, ");
                    parameters.Add(new SqlParameter("@noBallRun", matchDto.NoBallRun.Value));
                }

                if (parameters.Count == 0)
                {
                    return BadRequest(new { Message = "No fields provided to update." });
                }

                queryBuilder.Length -= 2; // Remove the final trailing comma and space
                queryBuilder.Append(" WHERE id = @id");
                parameters.Add(new SqlParameter("@id", matchId));

                using (SqlCommand command = new SqlCommand(queryBuilder.ToString(), conn))
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
            Console.WriteLine(e.ToString());
            var errorResponse = new
            {
                Message = "An unexpected server error occurred.",
                Error = e.InnerException?.Message ?? e.Message
            };
            return StatusCode(500, errorResponse);
        }
    }

    // PUT: api/CL_Match/{matchId}/state
    [HttpPut]
    [Route("UpdateMatchState")]

    public IActionResult UpdateMatchState( [FromBody] UpdateMatchStateDto stateDto)
    {
        try
        {

        string sqlDataSource = _configuration.GetConnectionString("CricLive");
        using (SqlConnection conn = new SqlConnection(sqlDataSource))
        {
            conn.Open(); 
            using (SqlCommand command = new SqlCommand("CL_UpdateMatchState", conn))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@matchId", stateDto.MatchId);
                command.Parameters.AddWithValue("@matchState", stateDto.MatchState);
                command.ExecuteNonQuery();
                return Ok(new { 
                Message = "Updated"}); 
            }
        }
        }
        catch (Exception e)
        {
            return StatusCode(500, new { Message = e.ToString() });
        }
    }

    // GET: api/CL_Match/{matchId}/state
    [HttpGet]
    [Route("GetMatchState")]
    public IActionResult GetMatchState(int matchId)
    {
        string sqlDataSource = _configuration.GetConnectionString("CricLive");
        using (SqlConnection conn = new SqlConnection(sqlDataSource))
        {
            conn.Open();
            using (SqlCommand command = new SqlCommand("CL_GetMatchState", conn))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@matchId", matchId);
                var matchState = command.ExecuteScalar();

                if (matchState == null || matchState == DBNull.Value)
                {
                    return BadRequest();
                }
                // Return the raw JSON string
                return Ok(new { 
                    Message="Data Get Successful",
                    Data = matchState.ToString() });
            }
        }
    }

    // DELETE: api/CL_Match/{id}
    [HttpDelete]
    [Route("DeleteMatch")]
    public IActionResult DeleteMatch(int id)
    {
        string sqlDataSource = _configuration.GetConnectionString("CricLive");
        using (SqlConnection conn = new SqlConnection(sqlDataSource))
        {
            conn.Open();
            using (SqlCommand command = new SqlCommand("CL_DeleteMatch", conn))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
                return Ok("Match deleted successfully.");
            }
        }
    }

    [HttpGet]
    [Route("GetMatchesByTournamentId/{id}")]
    public IActionResult GetMatchesByTournamentId(int id)
    {
        try
        {

        string sqlDataSource = _configuration.GetConnectionString("CricLive");
        using (SqlConnection conn = new SqlConnection(sqlDataSource))
        {
            
            conn.Open();
            List<Match> matches = new List<Match>();
            using (SqlCommand command = new SqlCommand("CL_GetMatchesByTournamentId", conn))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@tournamentId", id);
                SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        matches.Add(new Match
                        {
                            // Non-nullable columns can be converted directly
                            Id = Convert.ToInt32(reader["id"]),
                            Team1 = Convert.ToInt32(reader["team1"]),
                            Team2 = Convert.ToInt32(reader["team2"]),
                            MatchDate = Convert.ToDateTime(reader["matchDate"]),
                            Overs = Convert.ToInt32(reader["overs"]),
                            Uid = Convert.ToInt32(reader["uid"]),

                            // Use the helper functions for all nullable columns
                            InningNo = GetInt(reader["inningNo"]),
                            Team1Name = GetString(reader["team1Name"]),
                            Team2Name = GetString(reader["team2Name"]),
                            Status = GetString(reader["status"]),
                            TossWon = GetInt(reader["tossWon"]),
                            Decision = GetString(reader["decision"]),
                            MatchState = GetString(reader["matchState"]),

                            // --- All your nullable columns are now safe ---
                            TournamentId = GetInt(reader["tournamentId"]),
                            WideRun = GetInt(reader["wideRun"]),
                            NoBallRun = GetInt(reader["noBallRun"]),
                            CurrentOvers = reader["currentOvers"] != DBNull.Value ? Convert.ToDecimal(reader["currentOvers"]) : 0,

                            // Your existing checks for nullable foreign keys are also good
                            StrikerBatsmanId = reader["strikerBatsmanId"] != DBNull.Value ? Convert.ToInt32(reader["strikerBatsmanId"]) : (int?)null,
                            NonStrikerBatsmanId = reader["nonStrikerBatsmanId"] != DBNull.Value ? Convert.ToInt32(reader["nonStrikerBatsmanId"]) : (int?)null,
                            BowlerId = reader["bowlerId"] != DBNull.Value ? Convert.ToInt32(reader["bowlerId"]) : (int?)null,
                            CurrentBattingTeamId = reader["currentBattingTeamId"] != DBNull.Value ? Convert.ToInt32(reader["currentBattingTeamId"]) : (int?)null,
                        });
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
    /// <summary>
    /// Safely converts a database object to an integer.
    /// Returns 0 if the object is DBNull.
    /// </summary>
    private int GetInt(object dbObject)
    {
        if (dbObject == DBNull.Value)
        {
            return 0; // Or another default value you prefer
        }
        return Convert.ToInt32(dbObject);
    }

    /// <summary>
    /// Safely converts a database object to a string.
    /// Returns an empty string if the object is DBNull.
    /// </summary>
    private string GetString(object dbObject)
    {
        return dbObject?.ToString() ?? "";
    }
}