using CricLive.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql; // Changed from System.Data.SqlClient
using System.Data;
using System.Dynamic;
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
                using (NpgsqlCommand command = new NpgsqlCommand(@"
                                SELECT *,t1.teamName as team1Name,t2.teamName as team2Name FROM CL_Matches as m
                                inner join CL_Teams as t1
                                on t1.teamId = m.team1
                                inner join CL_Teams as t2
                                on t2.teamId = m.team2
                                 WHERE status = 'live'", conn))
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
                                StrikerBatsmanId = reader["strikerBatsmanId"] != DBNull.Value ? Convert.ToInt32(reader["strikerBatsmanId"]) : null,
                                NonStrikerBatsmanId = reader["nonStrikerBatsmanId"] != DBNull.Value ? Convert.ToInt32(reader["nonStrikerBatsmanId"]) : null,
                                BowlerId = reader["bowlerId"] != DBNull.Value ? Convert.ToInt32(reader["bowlerId"]) : null,
                                CurrentBattingTeamId = reader["currentBattingTeamId"] != DBNull.Value ? Convert.ToInt32(reader["currentBattingTeamId"]) : null,
                                Team1Name = reader["team1Name"].ToString(),
                                Team2Name = reader["team2Name"].ToString(),
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
                using (NpgsqlCommand command = new NpgsqlCommand(@"SELECT *,t1.teamName as team1Name,t2.teamName as team2Name FROM CL_Matches as m
                                inner join CL_Teams as t1
                                on t1.teamId = m.team1
                                inner join CL_Teams as t2
                                on t2.teamId = m.team2
                                WHERE uid = @uid AND (tournamentId IS NULL)", conn))
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
                                StrikerBatsmanId = reader["strikerBatsmanId"] != DBNull.Value ? Convert.ToInt32(reader["strikerBatsmanId"]) : null,
                                NonStrikerBatsmanId = reader["nonStrikerBatsmanId"] != DBNull.Value ? Convert.ToInt32(reader["nonStrikerBatsmanId"]) : null,
                                BowlerId = reader["bowlerId"] != DBNull.Value ? Convert.ToInt32(reader["bowlerId"]) : null,
                                CurrentBattingTeamId = reader["currentBattingTeamId"] != DBNull.Value ? Convert.ToInt32(reader["currentBattingTeamId"]) : null,
                                Team1Name = reader["team1Name"].ToString(),
                                Team2Name = reader["team2Name"].ToString(),
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
                using (NpgsqlCommand command = new NpgsqlCommand(@"INSERT INTO CL_Matches
           (inningNo
           ,team1
           ,team2
           ,matchDate
           ,overs
           ,status
           ,tournamentId
           ,wideRun
           ,noBallRun
           ,uid
           ,matchState)
     VALUES
           (@inningNo,
            @team1,
            @team2,
            @matchDate,
            @overs,
            @status,
            @tournamentId,
            @wideRun,
            @noBallRun,
            @uid,
            @matchState) RETURNING id;", conn))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@inningNo", 1);
                    command.Parameters.AddWithValue("@status", "scheduled");
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
                            Match match = new Match
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
                                StrikerBatsmanId = reader["strikerBatsmanId"] != DBNull.Value ? Convert.ToInt32(reader["strikerBatsmanId"]) : null,
                                NonStrikerBatsmanId = reader["nonStrikerBatsmanId"] != DBNull.Value ? Convert.ToInt32(reader["nonStrikerBatsmanId"]) : null,
                                BowlerId = reader["bowlerId"] != DBNull.Value ? Convert.ToInt32(reader["bowlerId"]) : null,
                                CurrentBattingTeamId = reader["currentBattingTeamId"] != DBNull.Value ? Convert.ToInt32(reader["currentBattingTeamId"]) : null
                            };
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


    [HttpDelete]
    [Route("DeleteMatch/{id}")]
    public IActionResult DeleteMatch(int id)
    {
        try
        {
            string pgDataSource = _configuration.GetConnectionString("CricLive");
            using (NpgsqlConnection conn = new NpgsqlConnection(pgDataSource))
            {
                conn.Open();
                using (NpgsqlCommand command = new NpgsqlCommand("delete from CL_Matches where id = @id", conn))
                {
                    command.CommandType = CommandType.Text;
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
                using (NpgsqlCommand command = new NpgsqlCommand(@"SELECT
	*,
    home_team.teamName AS team1Name,  
    away_team.teamName AS team2Name 
   
FROM
    CL_Matches AS m
INNER JOIN
    CL_Teams AS home_team ON m.team1 = home_team.teamId 
INNER JOIN
    CL_Teams AS away_team ON m.team2 = away_team.teamId 
WHERE
    m.tournamentId = @tournamentId;", conn))
                {
                    command.CommandType = CommandType.Text;
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


    [HttpGet]
    [Route("Search/{text}")]
    public IActionResult Search(string text)
    {
        try
        {
            string pgDataSource = _configuration.GetConnectionString("CricLive");
            List<dynamic> result = new List<dynamic>();
            using (NpgsqlConnection conn = new NpgsqlConnection(pgDataSource))
            {
                conn.Open();
                using (NpgsqlCommand command = new NpgsqlCommand(@"SELECT * from CL_Matches where CAST(id AS TEXT) ILIKE @text", conn))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@text", $"%{text}%");
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
                                StrikerBatsmanId = reader["strikerBatsmanId"] != DBNull.Value ? Convert.ToInt32(reader["strikerBatsmanId"]) : null,
                                NonStrikerBatsmanId = reader["nonStrikerBatsmanId"] != DBNull.Value ? Convert.ToInt32(reader["nonStrikerBatsmanId"]) : null,
                                BowlerId = reader["bowlerId"] != DBNull.Value ? Convert.ToInt32(reader["bowlerId"]) : null,
                                CurrentBattingTeamId = reader["currentBattingTeamId"] != DBNull.Value ? Convert.ToInt32(reader["currentBattingTeamId"]) : null
                            };
                                result.Add(match);
                        }
                    }
                }

                using (NpgsqlCommand command = new NpgsqlCommand(@"SELECT 
                            t.tournamentId, t.name, t.location, t.startDate, 
                            t.endDate, t.format, t.hostId, t.createdAt,
                            u.uid as scorerId, u.username
                        FROM 
                            CL_Tournaments as t
                        LEFT JOIN 
                            CL_TournamentScorers as ts ON t.tournamentId = ts.tournamentId
                        LEFT JOIN 
                            CL_Users as u ON ts.uid = u.uid
                        where (t.name like @text) or (t.location like @text)
                        ORDER BY
                            t.tournamentId", conn))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@text", $"%{text}%");
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int tournamentId = Convert.ToInt32(reader["tournamentId"]);
                            
                                var tournament = new Tournament
                                {
                                    TournamentId = tournamentId,
                                    Name = reader["name"].ToString(),
                                    Location = reader["location"].ToString(),
                                    StartDate = Convert.ToDateTime(reader["startDate"]),
                                    EndDate = Convert.ToDateTime(reader["endDate"]),
                                    Format = reader["format"].ToString(),
                                    HostId = Convert.ToInt32(reader["hostId"]),
                                    CreatedAt = Convert.ToDateTime(reader["createdAt"]),
                                    Scorers = new List<Scorer>()
                                };
                                result.Add(tournament);
                            

                            if (reader.GetValue("scorerId")!=null && reader["scorerId"] != DBNull.Value)
                            {
                                tournament.Scorers.Add(new Scorer
                                {
                                    ScorerId = Convert.ToInt32(reader["scorerId"]),
                                    Username = reader["username"].ToString()
                                });
                            }
                        }
                    }
                }

            }
            return Ok(new { Message= "Success to fetch",Result = result });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { Message = e.Message });
        }
    }
}