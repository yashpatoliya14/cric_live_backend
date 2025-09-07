//using CricLive.Models;
//using Microsoft.AspNetCore.Mvc;
//using System.Data;
//using System.Data.SqlClient;

//namespace CricLive.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class CL_TournamentsController : ControllerBase
//    {
//        private readonly IConfiguration _configuration;

//        // Constructor to inject the configuration
//        public CL_TournamentsController(IConfiguration configuration)
//        {
//            _configuration = configuration;
//        }

//        /// <summary>
//        /// Gets all tournaments from the database.
//        /// </summary>
//        [HttpGet]
//        [Route("GetTournaments")]
//        public IActionResult GetTournaments()
//        {
//            try
//            {
//                // Use a dictionary to group scorers by tournament
//                var tournamentDictionary = new Dictionary<int, Tournament>();
//                string sqlDataSource = _configuration.GetConnectionString("CricLive");

//                using (SqlConnection conn = new SqlConnection(sqlDataSource))
//                {
//                    conn.Open();
//                    using (SqlCommand command = new SqlCommand("CL_GetTournaments", conn))
//                    {
//                        command.CommandType = CommandType.StoredProcedure;

//                        using (SqlDataReader reader = command.ExecuteReader())
//                        {
//                            while (reader.Read())
//                            {
//                                int tournamentId = Convert.ToInt32(reader["tournamentId"]);
//                                Tournament tournament;

//                                if (!tournamentDictionary.TryGetValue(tournamentId, out tournament))
//                                {
//                                    tournament = new Tournament
//                                    {
//                                        TournamentId = tournamentId,
//                                        Name = reader["name"].ToString(),
//                                        Location = reader["location"].ToString(),
//                                        StartDate = Convert.ToDateTime(reader["startDate"]),
//                                        EndDate = Convert.ToDateTime(reader["endDate"]),
//                                        Format = reader["format"].ToString(),
//                                        HostId = Convert.ToInt32(reader["hostId"]),
//                                        CreatedAt = Convert.ToDateTime(reader["createdAt"]),
//                                        Scorers = new List<Scorer>() // Initialize the scorer list
//                                    };
//                                    tournamentDictionary.Add(tournamentId, tournament);
//                                }

//                                if (reader["scorerId"] != DBNull.Value)
//                                {
//                                    var scorer = new Scorer
//                                    {
//                                        ScorerId = Convert.ToInt32(reader["scorerId"]),
//                                        Username = reader["username"].ToString()
//                                    };
//                                    tournament.Scorers.Add(scorer);
//                                }
//                            }
//                        }
//                    }
//                }

//                var tournaments = tournamentDictionary.Values.ToList();
//                return Ok(new { Tournaments = tournaments });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { Message = "An error occurred while fetching tournaments.", Details = ex.Message });
//            }
//        }

//        /// <summary>
//        /// Gets a specific tournament by its ID.
//        /// </summary>
//        [HttpGet]
//        [Route("GetTournamentById/{hostId}")]
//        public IActionResult GetTournamentById(int hostId)
//        {
//            try
//            {
//                Dictionary<int,Tournament> tDic = new Dictionary<int,Tournament>();
//                string sqlDataSource = _configuration.GetConnectionString("CricLive");

//                using (SqlConnection conn = new SqlConnection(sqlDataSource))
//                {
//                    conn.Open();
//                    using (SqlCommand command = new SqlCommand("CL_GetTournamentById", conn))
//                    {
//                        command.CommandType = CommandType.StoredProcedure;
//                        command.Parameters.AddWithValue("@hostId", hostId);
//                        Tournament tournament = null;

//                        using (SqlDataReader reader = command.ExecuteReader())
//                        {
//                            while (reader.Read())
//                            {
//                                int tournamentId = Convert.ToInt32(reader["tournamentId"]);
//                                if (!tDic.TryGetValue(tournamentId, out tournament)){
//                                    tournament = new Tournament
//                                    {
//                                        TournamentId = Convert.ToInt32(reader["tournamentId"]),
//                                        Name = reader["name"].ToString(),
//                                        Location = reader["location"].ToString(),
//                                        StartDate = Convert.ToDateTime(reader["startDate"]),
//                                        EndDate = Convert.ToDateTime(reader["endDate"]),
//                                        Format = reader["format"].ToString(),
//                                        HostId = Convert.ToInt32(reader["hostId"]),
//                                        CreatedAt = Convert.ToDateTime(reader["createdAt"]),
//                                        Scorers = new List<Scorer>()
//                                    };
//                                   tDic.Add(tournamentId, tournament);
//                                }

//                                if (reader["scorerId"] != DBNull.Value )
//                                { 
//                                    Scorer scorer = new Scorer
//                                    {
//                                        ScorerId = Convert.ToInt32(reader["scorerId"]),
//                                        Username = reader["username"].ToString()
//                                    };
//                                    Console.Write(scorer.ScorerId);
//                                    tournament.Scorers.Add(scorer);
//                                }
//                            }
//                        }
//                    }
//                }

//                var tournaments = tDic.Values.ToList();
//                return Ok(new { Tournaments = tournaments });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { Message = "An error occurred while fetching the tournament.", Details = ex.Message });
//            }
//        }

//        /// <summary>
//        /// Creates a new tournament.
//        /// </summary>
//        [HttpPost]
//        [Route("CreateTournament")]
//        public IActionResult CreateTournament([FromBody] Tournament tournamentDto)
//        {
//            try
//            {
//                string sqlDataSource = _configuration.GetConnectionString("CricLive");
//                using (SqlConnection conn = new SqlConnection(sqlDataSource))
//                {
//                    conn.Open();
//                    using (SqlCommand command = new SqlCommand("CL_CreateTournament", conn))
//                    {
//                        command.CommandType = CommandType.StoredProcedure;
//                        command.Parameters.AddWithValue("@name", tournamentDto.Name);
//                        command.Parameters.AddWithValue("@location", tournamentDto.Location);
//                        command.Parameters.AddWithValue("@startDate", tournamentDto.StartDate);
//                        command.Parameters.AddWithValue("@endDate", tournamentDto.EndDate);
//                        command.Parameters.AddWithValue("@format", tournamentDto.Format);
//                        command.Parameters.AddWithValue("@hostId", tournamentDto.HostId);


//                        // Assuming the stored procedure returns the new ID
//                        var newTournamentId = command.ExecuteScalar();
//                        if (newTournamentId != null)
//                        {
//                            for (int i = 0; i < tournamentDto.Scorers.Count; i++)
//                            {
//                                Scorer scorer = tournamentDto.Scorers[i];
//                                int scorerId = scorer.ScorerId;
//                                int tournamentId = Convert.ToInt32(newTournamentId);
//                                using (SqlCommand scorerCmd = new SqlCommand("PR_CL_CreateScorer", conn))
//                                {
//                                    scorerCmd.CommandType = CommandType.StoredProcedure;
//                                    scorerCmd.Parameters.AddWithValue("@scorerId", scorerId);
//                                    scorerCmd.Parameters.AddWithValue("@tournamentId", tournamentId);
//                                    scorerCmd.ExecuteNonQuery();
//                                }
//                            }
//                        }


//                        return Ok(new { Message = "Tournament created successfully.", TournamentId = newTournamentId });
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { Message = "An error occurred while creating the tournament.", Details = ex.Message });
//            }
//        }

//        /// <summary>
//        /// Updates an existing tournament.
//        /// </summary>
//        [HttpPut]
//        [Route("UpdateTournament/{id}")]
//        public IActionResult UpdateTournament(int id, [FromBody] Tournament tournamentDto)
//        {
//            try
//            {
//                string sqlDataSource = _configuration.GetConnectionString("CricLive");
//                using (SqlConnection conn = new SqlConnection(sqlDataSource))
//                {
//                    conn.Open();
//                    using (SqlCommand command = new SqlCommand("CL_UpdateTournament", conn))
//                    {
//                        command.CommandType = CommandType.StoredProcedure;
//                        command.Parameters.AddWithValue("@tournamentId", id);
//                        command.Parameters.AddWithValue("@name", tournamentDto.Name);
//                        command.Parameters.AddWithValue("@location", tournamentDto.Location);
//                        command.Parameters.AddWithValue("@startDate", tournamentDto.StartDate);
//                        command.Parameters.AddWithValue("@endDate", tournamentDto.EndDate);
//                        command.Parameters.AddWithValue("@format", tournamentDto.Format);


//                        command.ExecuteNonQuery();

//                        using (SqlCommand scorerCmd = new SqlCommand("PR_CL_DeleteScorer", conn))
//                        {
//                            scorerCmd.Parameters.AddWithValue("@tournamentId", id);
//                        }

//                        for (int i = 0; i < tournamentDto.Scorers.Count; i++)
//                            {
//                            Scorer scorer = tournamentDto.Scorers[i];
//                                int scorerId = scorer.ScorerId;
//                                int tournamentId = Convert.ToInt32(id);
//                                using (SqlCommand scorerCmd = new SqlCommand("PR_CL_CreateScorer", conn))
//                                {
//                                    scorerCmd.CommandType = CommandType.StoredProcedure; 
//                                    scorerCmd.Parameters.AddWithValue("@scorerId", scorerId);
//                                    scorerCmd.Parameters.AddWithValue("@tournamentId", tournamentId);
//                                }
//                            }

//                        return Ok(new { Message = "Tournament updated successfully." });
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { Message = "An error occurred while updating the tournament.", Details = ex.Message });
//            }
//        }

//        /// <summary>
//        /// Deletes a tournament by its ID.
//        /// </summary>
//        [HttpDelete]
//        [Route("DeleteTournament/{id}")]
//        public IActionResult DeleteTournament(int id)
//        {
//            try
//            {
//                string sqlDataSource = _configuration.GetConnectionString("CricLive");
//                using (SqlConnection conn = new SqlConnection(sqlDataSource))
//                {
//                    conn.Open();
//                    using (SqlCommand command = new SqlCommand("CL_DeleteTournament", conn))
//                    {
//                        command.CommandType = CommandType.StoredProcedure;
//                        command.Parameters.AddWithValue("@tournamentId", id);

//                        int rowsAffected = command.ExecuteNonQuery();

//                        if (rowsAffected == 0)
//                        {
//                            return NotFound(new { Message = "Tournament not found or already deleted." });
//                        }

//                        return Ok(new { Message = "Tournament deleted successfully." });
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { Message = "An error occurred while deleting the tournament.", Details = ex.Message });
//            }
//        }
//    }
//}


using CricLive.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql; // Changed from System.Data.SqlClient
using System.Data;

namespace CricLive.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CL_TournamentsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CL_TournamentsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Gets all tournaments from the database.
        /// </summary>
        [HttpGet]
        [Route("GetTournaments")]
        public IActionResult GetTournaments()
        {
            try
            {
                var tournamentDictionary = new Dictionary<int, Tournament>();
                string pgDataSource = _configuration.GetConnectionString("CricLive");

                using (NpgsqlConnection conn = new NpgsqlConnection(pgDataSource))
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            t.tournamentId, t.name, t.location, t.startDate, 
                            t.endDate, t.format, t.hostId, t.createdAt,
                            u.uid as scorerId, u.username
                        FROM 
                            CL_Tournaments as t
                        LEFT JOIN 
                            CL_TournamentScorers as ts ON t.tournamentId = ts.tournamentId
                        LEFT JOIN 
                            CL_Users as u ON ts.uid = u.uid
                        ORDER BY
                            t.tournamentId;";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, conn))
                    {
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int tournamentId = Convert.ToInt32(reader["tournamentId"]);
                                if (!tournamentDictionary.TryGetValue(tournamentId, out var tournament))
                                {
                                    tournament = new Tournament
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
                                    tournamentDictionary.Add(tournamentId, tournament);
                                }

                                if (reader["scorerId"] != DBNull.Value)
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
                return Ok(new { Tournaments = tournamentDictionary.Values.ToList() });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while fetching tournaments.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Gets a specific tournament by its host ID.
        /// </summary>
        [HttpGet]
        [Route("GetTournamentById/{hostId}")]
        public IActionResult GetTournamentById(int hostId)
        {
            try
            {
                var tournamentDictionary = new Dictionary<int, Tournament>();
                string pgDataSource = _configuration.GetConnectionString("CricLive");

                using (NpgsqlConnection conn = new NpgsqlConnection(pgDataSource))
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            t.tournamentId, t.name, t.location, t.startDate, 
                            t.endDate, t.format, t.hostId, t.createdAt,
                            u.uid as scorerId, u.username
                        FROM 
                            CL_Tournaments as t
                        LEFT JOIN 
                            CL_TournamentScorers as ts ON t.tournamentId = ts.tournamentId
                        LEFT JOIN 
                            CL_Users as u ON ts.uid = u.uid
                        WHERE 
                            t.hostId = @hostId;";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@hostId", hostId);
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int tournamentId = Convert.ToInt32(reader["tournamentId"]);
                                if (!tournamentDictionary.TryGetValue(tournamentId, out var tournament))
                                {
                                    tournament = new Tournament
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
                                    tournamentDictionary.Add(tournamentId, tournament);
                                }

                                if (reader["scorerId"] != DBNull.Value)
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
                return Ok(new { Tournaments = tournamentDictionary.Values.ToList() });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while fetching the tournament.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Creates a new tournament.
        /// </summary>
        [HttpPost]
        [Route("CreateTournament")]
        public IActionResult CreateTournament([FromBody] Tournament tournamentDto)
        {
            string pgDataSource = _configuration.GetConnectionString("CricLive");
            using (NpgsqlConnection conn = new NpgsqlConnection(pgDataSource))
            {
                conn.Open();
                NpgsqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    // Step 1: Insert the tournament and get the new ID using RETURNING
                    string tournamentQuery = @"
                        INSERT INTO CL_Tournaments (name, location, startDate, endDate, format, hostId)
                        VALUES (@name, @location, @startDate, @endDate, @format, @hostId)
                        RETURNING tournamentId;";

                    int newTournamentId;
                    using (NpgsqlCommand command = new NpgsqlCommand(tournamentQuery, conn, transaction))
                    {
                        command.Parameters.AddWithValue("@name", tournamentDto.Name);
                        command.Parameters.AddWithValue("@location", tournamentDto.Location);
                        command.Parameters.AddWithValue("@startDate", tournamentDto.StartDate);
                        command.Parameters.AddWithValue("@endDate", tournamentDto.EndDate);
                        command.Parameters.AddWithValue("@format", tournamentDto.Format);
                        command.Parameters.AddWithValue("@hostId", tournamentDto.HostId);
                        newTournamentId = Convert.ToInt32(command.ExecuteScalar());
                    }

                    // Step 2: Insert the associated scorers
                    if (tournamentDto.Scorers != null && tournamentDto.Scorers.Any())
                    {
                        string scorerQuery = "INSERT INTO CL_TournamentScorers (tournamentId, uid) VALUES (@tournamentId, @scorerId);";
                        foreach (var scorer in tournamentDto.Scorers)
                        {
                            using (NpgsqlCommand scorerCmd = new NpgsqlCommand(scorerQuery, conn, transaction))
                            {
                                scorerCmd.Parameters.AddWithValue("@tournamentId", newTournamentId);
                                scorerCmd.Parameters.AddWithValue("@scorerId", scorer.ScorerId);
                                scorerCmd.ExecuteNonQuery();
                            }
                        }
                    }

                    transaction.Commit();
                    return Ok(new { Message = "Tournament created successfully.", TournamentId = newTournamentId });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return StatusCode(500, new { Message = "An error occurred while creating the tournament.", Details = ex.Message });
                }
            }
        }

        /// <summary>
        /// Updates an existing tournament.
        /// </summary>
        [HttpPut]
        [Route("UpdateTournament/{id}")]
        public IActionResult UpdateTournament(int id, [FromBody] Tournament tournamentDto)
        {
            string pgDataSource = _configuration.GetConnectionString("CricLive");
            using (NpgsqlConnection conn = new NpgsqlConnection(pgDataSource))
            {
                conn.Open();
                NpgsqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    // Step 1: Update the tournament details
                    string tournamentQuery = @"
                        UPDATE CL_Tournaments
                        SET name = @name, location = @location, startDate = @startDate,
                            endDate = @endDate, format = @format
                        WHERE tournamentId = @tournamentId;";
                    using (NpgsqlCommand command = new NpgsqlCommand(tournamentQuery, conn, transaction))
                    {
                        command.Parameters.AddWithValue("@tournamentId", id);
                        command.Parameters.AddWithValue("@name", tournamentDto.Name);
                        command.Parameters.AddWithValue("@location", tournamentDto.Location);
                        command.Parameters.AddWithValue("@startDate", tournamentDto.StartDate);
                        command.Parameters.AddWithValue("@endDate", tournamentDto.EndDate);
                        command.Parameters.AddWithValue("@format", tournamentDto.Format);
                        command.ExecuteNonQuery();
                    }

                    // Step 2: Delete existing scorers for this tournament
                    string deleteScorersQuery = "DELETE FROM CL_TournamentScorers WHERE tournamentId = @tournamentId;";
                    using (NpgsqlCommand deleteCmd = new NpgsqlCommand(deleteScorersQuery, conn, transaction))
                    {
                        deleteCmd.Parameters.AddWithValue("@tournamentId", id);
                        deleteCmd.ExecuteNonQuery();
                    }

                    // Step 3: Insert the new list of associated scorers
                    if (tournamentDto.Scorers != null && tournamentDto.Scorers.Any())
                    {
                        string insertScorerQuery = "INSERT INTO CL_TournamentScorers (tournamentId, uid) VALUES (@tournamentId, @scorerId);";
                        foreach (var scorer in tournamentDto.Scorers)
                        {
                            using (NpgsqlCommand insertCmd = new NpgsqlCommand(insertScorerQuery, conn, transaction))
                            {
                                insertCmd.Parameters.AddWithValue("@tournamentId", id);
                                insertCmd.Parameters.AddWithValue("@scorerId", scorer.ScorerId);
                                insertCmd.ExecuteNonQuery();
                            }
                        }
                    }

                    transaction.Commit();
                    return Ok(new { Message = "Tournament updated successfully." });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return StatusCode(500, new { Message = "An error occurred while updating the tournament.", Details = ex.Message });
                }
            }
        }

        /// <summary>
        /// Deletes a tournament by its ID.
        /// </summary>
        [HttpDelete]
        [Route("DeleteTournament/{id}")]
        public IActionResult DeleteTournament(int id)
        {
            string pgDataSource = _configuration.GetConnectionString("CricLive");
            using (NpgsqlConnection conn = new NpgsqlConnection(pgDataSource))
            {
                conn.Open();
                NpgsqlTransaction transaction = conn.BeginTransaction();
                try
                {
                    // Foreign key constraints might require deleting from child tables first.
                    // This is handled by the application logic here.

                    // Step 1: Delete associated scorers
                    string deleteScorersQuery = "DELETE FROM CL_TournamentScorers WHERE tournamentId = @tournamentId;";
                    using (NpgsqlCommand deleteScorersCmd = new NpgsqlCommand(deleteScorersQuery, conn, transaction))
                    {
                        deleteScorersCmd.Parameters.AddWithValue("@tournamentId", id);
                        deleteScorersCmd.ExecuteNonQuery();
                    }

                    // Step 2: Delete the tournament itself
                    string deleteTournamentQuery = "DELETE FROM CL_Tournaments WHERE tournamentId = @tournamentId;";
                    int rowsAffected;
                    using (NpgsqlCommand command = new NpgsqlCommand(deleteTournamentQuery, conn, transaction))
                    {
                        command.Parameters.AddWithValue("@tournamentId", id);
                        rowsAffected = command.ExecuteNonQuery();
                    }

                    transaction.Commit();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { Message = "Tournament not found or already deleted." });
                    }

                    return Ok(new { Message = "Tournament deleted successfully." });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return StatusCode(500, new { Message = "An error occurred while deleting the tournament.", Details = ex.Message });
                }
            }
        }
    }
}