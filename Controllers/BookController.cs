using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Web.Http;
using System.Web.Http.Results;
using test.Models.DTOs;
using test.Repositories;
namespace test.Controllers
{
    [ApiController]
    [Microsoft.AspNetCore.Mvc.Route("api/books")]
    public class BookController : Controller
    {

        private readonly IConfiguration _configuration;
        private readonly BookRepository _bookRepository;
        public BookController(IConfiguration configuration)
        {
            _configuration = configuration;
            _bookRepository = new(configuration);
        }


        [Microsoft.AspNetCore.Mvc.HttpGet("{id}/genres")]
        public async Task<ActionResult> GetBookGenres(int id)
        {
            if (id <= 0) return BadRequest("Invalid ID");

            var genres = await _bookRepository.GetGenresForBook(id);

            //http 500 rzuci się sam jak się wysypie

            if (genres.Count == 0)
            {
                return NotFound();
            }

            return Ok(genres);
        }


        [Microsoft.AspNetCore.Mvc.HttpPost()]
        
        public async Task<ActionResult> PostBook(BookDTO bookDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _bookRepository.AddBook(bookDTO);

            return CreatedAtAction(nameof(PostBook), new { id = response.Id }, response);
        }
    }
}
