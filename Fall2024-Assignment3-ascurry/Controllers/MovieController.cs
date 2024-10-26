using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Nodes;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using VaderSharp2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Fall2024_Assignment3_ascurry.Data;
using Fall2024_Assignment3_ascurry.Models;

namespace Fall2024_Assignment3_ascurry.Controllers
{
    public class MovieController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public MovieController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }


        public async Task<IActionResult> GetMoviePoster(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie.FindAsync(id);
            if (movie == null || movie.Poster == null)
            {
                return NotFound();
            }

            var data = movie.Poster;
            return File(data, "image/jpg");
        }

     

        // GET: Movie
        public async Task<IActionResult> Index()
        {
              return _context.Movie != null ? 
                          View(await _context.Movie.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Movie'  is null.");
        }

        // GET: Movie/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Movie == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            var actors = await _context.MovieActor
           .Include(cs => cs.Actor)
                    .Where(cs => cs.MovieId == movie.Id)
           .Select(cs => cs.Actor)
           .ToListAsync();

            ////////////////////////////////////////////////////
            
            var ApiKey = _config["OpenAi:Key"] ?? throw new Exception("OpenAI:Key does not exist in the current Configuration"); ;
            var ApiEndpoint = _config["OpenAi:Endpoint"] ?? throw new Exception("OpenAI:Endpoint does not exist in the current Configuration");
            var AiDeployment = "gpt-35-turbo-16k";
            ApiKeyCredential ApiCredential = new(ApiKey);

            var MovieYear = movie.ReleaseYear;
            var MovieName = movie.Title;

            ChatClient chatClient = new AzureOpenAIClient(new Uri(ApiEndpoint), ApiCredential).GetChatClient(AiDeployment);
            var analyzer = new SentimentIntensityAnalyzer();
            double sentimentTotal = 0;

            var reviews_and_sentiments = new List<Object[]>();
            var messages = new ChatMessage[]
            {
                new SystemChatMessage($"You are a film reviewer and film critic. You are either harsh, a lover of comedies, a general lover of movies, or impartial. Generate an answer with a valid JSON formatted array of objects containing the review. The response should start with [."),
                new UserChatMessage($"Generate 10 movie reviews, each based on one of your possible personas and less than 50 words long. Rate the movie {MovieName} ({MovieYear}) out of 10 and make the review structure varied.")
            };

            var chatCompletionOptions = new ChatCompletionOptions
            {
                MaxOutputTokenCount = 1000,
            };
            ClientResult<ChatCompletion> result = await chatClient.CompleteChatAsync(messages, chatCompletionOptions);
            string reviewJsonString = result.Value.Content.FirstOrDefault()?.Text ?? "[]";
            JsonArray json = JsonNode.Parse(reviewJsonString)!.AsArray();
            var reviews = json.Select(t => new { Text = t!["review"]?.ToString() ?? "" }).ToArray();
            foreach (var review in reviews)
            {
                SentimentAnalysisResults sentiment = analyzer.PolarityScores(review.Text);
                sentimentTotal += sentiment.Compound;
                reviews_and_sentiments.Add(new Object[] { review.Text, sentiment.Compound });
            }

            //Thread.Sleep(TimeSpan.FromSeconds(10)); // Request throttle due to rate limit


            double sentimentAverage = sentimentTotal / reviews_and_sentiments.Count;

            var vm = new MovieDetailsViewModel(movie, actors, reviews_and_sentiments, sentimentAverage);
           
            return View(vm);
        }

        // GET: Movie/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movie/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,IMDBLink,Genre,ReleaseYear,Poster")] Movie movie, IFormFile? poster)
        {
            if (ModelState.IsValid)
            {
                if (poster != null && poster.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    poster.CopyTo(memoryStream);
                    movie.Poster = memoryStream.ToArray();
                }
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(movie);
        }

        // GET: Movie/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Movie == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: Movie/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,IMDBLink,Genre,ReleaseYear,Poster")] Movie movie, IFormFile? poster)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (poster != null && poster.Length > 0)
                    {
                        using var memoryStream = new MemoryStream();
                        poster.CopyTo(memoryStream);
                        movie.Poster = memoryStream.ToArray();
                    }
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Movie/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Movie == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movie/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Movie == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Movie'  is null.");
            }
            var movie = await _context.Movie.FindAsync(id);
            if (movie != null)
            {
                _context.Movie.Remove(movie);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
          return (_context.Movie?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
