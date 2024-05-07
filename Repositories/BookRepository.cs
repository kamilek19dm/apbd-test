using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using test.Models.DTOs;

namespace test.Repositories
{
    public class BookRepository
    {
        private readonly IConfiguration _configuration;
        public BookRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<string>> GetGenresForBook(int bookId)
        {
            List<string> genres = new List<string>();

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default")))
            {
                await connection.OpenAsync();
                string sqlQuery = @"
                    SELECT g.name
                    FROM genres g
                    JOIN books_genres bg ON g.PK = bg.FK_genre
                    WHERE bg.FK_book = @BookId";

                SqlCommand command = new SqlCommand(sqlQuery, connection);
                command.Parameters.AddWithValue("@BookId", bookId);

                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        string genreName = reader["name"].ToString();
                        genres.Add(genreName);
                    }
                }
            }
            return genres;
        }

        public async Task<ResponseDTO> AddBook(BookDTO bookDTO)
        {
            int newBookId;
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default")))
            {
                string insertBookQuery = @"
                    INSERT INTO books (title)
                    VALUES (@Title);
                    SELECT SCOPE_IDENTITY()";

                SqlCommand command = new SqlCommand(insertBookQuery, connection);
                command.Parameters.AddWithValue("@Title", bookDTO.Title);

                await connection.OpenAsync();

                newBookId = Convert.ToInt32(await command.ExecuteScalarAsync());
            }

            // Assign genres to the new book
            if (bookDTO.GenreIDs != null && bookDTO.GenreIDs.Count > 0)
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default")))
                {
                    string insertBookGenreQuery = @"
                        INSERT INTO books_genres (FK_book, FK_genre)
                        VALUES (@BookId, @GenreId);";

                    await connection.OpenAsync();

                    foreach (int genreId in bookDTO.GenreIDs)
                    {
                        SqlCommand command = new SqlCommand(insertBookGenreQuery, connection);
                        command.Parameters.AddWithValue("@BookId", newBookId);
                        command.Parameters.AddWithValue("@GenreId", genreId);

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }

            var genres = new List<string>();

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default")))
            {
                await connection.OpenAsync();
                string sqlQuery = @"
                    SELECT g.name
                    FROM genres g
                    JOIN books_genres bg ON g.PK = bg.FK_genre
                    WHERE bg.FK_book = @BookId";

                SqlCommand command = new SqlCommand(sqlQuery, connection);
                command.Parameters.AddWithValue("@BookId", newBookId);

                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        string genreName = reader["name"].ToString();
                        genres.Add(genreName);
                    }
                }
            }

            return new ResponseDTO
            {
                Id = newBookId,
                Title = bookDTO.Title,
                genres = genres
            };
        }
    }
}
