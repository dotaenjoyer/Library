using Microsoft.AspNetCore.Mvc;
using Library.ModelsORM;
using Library.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Text;

namespace Library.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly APIContext _context;
        public BookController(APIContext context)
        {
            _context = context;
        }
        /// <summary>
        /// Gets all books 
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public List<Book> GetAllBooks()
        {
            var books = _context.Books.ToList();

            return books;
        }
        /// <summary>
        /// Get csv file    
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]"), Authorize(Roles = "Admin")]
        public IActionResult GetsCsv()
        {
            List<object> books = (from book in _context.Books.Include(s => s.Authors)
                .Include(k => k.Genres).ToList().Take(10000)
                                  select new[] { book.Book_Id.ToString(),
                                                            book.Book_Name,
                                      String.Join("|", book.Authors.Select(p => p.Author_Name)), String.Join("|", book.Genres.Select(p => p.Genre_Name))}).ToList<object>();

            books.Insert(0, new string[4] { "Book Id", "Book Name", "Author Name", "Genre Name" });

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < books.Count; i++)
            {
                string[] book = (string[])books[i];
                for (int j = 0; j < book.Length; j++)
                {
                    sb.Append(book[j] + ',');
                }
                sb.Append("\r\n");

            }
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "Books.csv");
        }
        /// <summary>
        /// Gets info about a book
        /// </summary>
        /// <param name="bookname"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("{bookname}")]
        public async Task<ActionResult<List<Book>>> GetInfo(string bookname)
        {
            var bookinfo = _context.Books
                .Where(c => c.Book_Name == bookname)
                .Include(s => s.Authors)
                .Include(k => k.Genres).Select(x => new
                {
                    BookName = x.Book_Name,
                    GenreName = x.Genres.Select(a => a.Genre_Name),
                    AuthorName = x.Authors.Select(b => b.Author_Name)
                   .ToList()
                });
            return Ok(bookinfo);
}/// <summary>
/// Adds a book with an author and a genre
/// </summary>
/// <param name="bookname"></param>
/// <param name="authorname"></param>
/// <param name="genrename"></param>
/// <returns></returns>
        [HttpPost, Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<Book>>> AddBook(string bookname, string authorname, string genrename)
        {
                if (bookname == null || authorname == null || genrename == null)
                {
                    return BadRequest();
                }
            if (!_context.Books.Any(info => info.Book_Name == bookname))
            {
                Book b = new Book()
                {
                    Book_Id = _context.Books.Max(p => p.Book_Id) + 1,
                    Book_Name = bookname
                };
                await _context.Books.AddAsync(b);
                await _context.SaveChangesAsync();
            }
            if (!_context.Authors.Any(info => info.Author_Name == authorname))
                {
                    var auth = new Author()
                    {
                        Author_Id = _context.Authors.Max(p => p.Author_Id) + 1,
                        Author_Name = authorname
                    };
                await _context.Authors.AddAsync(auth);
                await _context.SaveChangesAsync();
            }
                if (!_context.Genres.Any(info => info.Genre_Name == genrename))
            {
                    var gen = new Genre()
                    {
                        Genre_Id = _context.Genres.Max(p => p.Genre_Id) + 1,
                        Genre_Name = genrename
                    };
                await _context.Genres.AddAsync(gen);
                await _context.SaveChangesAsync();
            }
                var book = await _context.Books.Where(c => c.Book_Name == bookname)
                .Include(c => c.Authors).Include(c => c.Genres).FirstOrDefaultAsync();
            if (book == null)
            {
                return NotFound();
            }
            var author = await _context.Authors.Where(c => c.Author_Name == authorname)
            .FirstOrDefaultAsync();
            if (author == null)
            {
                return NotFound();
            }
            var genre = await _context.Genres.Where(c => c.Genre_Name == genrename)
                .FirstOrDefaultAsync();
            if (genre == null)
            {
                return NotFound();
            }
            try
            {
            book.Authors.Add(author);
            book.Genres.Add(genre);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status200OK,
                    "bad");
            }
            await _context.SaveChangesAsync();  
                return await GetInfo(book.Book_Name);
        }
        /// <summary>
        /// Deletes book
        /// </summary>
        /// <param name="bookname"></param>
        /// <returns></returns>
        [HttpDelete, Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<Book>>> DeleteBook(string bookname)
        {
            var book = await _context.Books.Where(c => c.Book_Name == bookname).FirstOrDefaultAsync();
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return await GetInfo(bookname);
        }
        /// <summary>
        /// Adds a book to favourites
        /// </summary>
        /// <param name="bookname"></param>
        /// <returns></returns>
        
        [HttpPost("{bookname}"), Authorize(Roles = "Client")]
        public async Task<ActionResult<List<Book>>> AddToFavourites(string bookname)
        {
            var book = await _context.Books.Where(c => c.Book_Name == bookname)
                .Include(c => c.Users).FirstOrDefaultAsync();
            if (book == null)
            {
                return BadRequest();
            }
            var curuser = await _context.Users.Where(c => c.User_Email == Globals.CurUser)
        .FirstOrDefaultAsync();
            if (curuser == null)
            {
                return BadRequest();
            }
            book.Users.Add(curuser);
            await _context.SaveChangesAsync();
            return await GetInfo(book.Book_Name);
        }
        /// <summary>
        /// Deletes a book from favourites
        /// </summary>
        /// <param name="bookname"></param>
        /// <returns></returns>
        [HttpDelete("{bookname}"), Authorize(Roles = "Client")]
        public async Task<ActionResult<List<Book>>> DeleteFromFavourites(string bookname)
        {
            var book = await _context.Books.Where(c => c.Book_Name == bookname)
                .Include(c => c.Users).FirstOrDefaultAsync();
            if (book == null)
            {
                return BadRequest();
            }
            var curuser = await _context.Users.Where(c => c.User_Email == Globals.CurUser)
        .FirstOrDefaultAsync();
            if (curuser == null)
            {
                return BadRequest();
            }
            book.Users.Remove(curuser);
            await _context.SaveChangesAsync();
            return await GetInfo(book.Book_Name);
        }
    }
}
