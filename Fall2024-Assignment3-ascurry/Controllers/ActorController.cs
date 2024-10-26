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
    public class ActorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public ActorController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<IActionResult> GetActorPhoto(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor.FindAsync(id);
            if (actor == null || actor.Photo == null)
            {
                return NotFound();
            }

            var data = actor.Photo;
            return File(data, "image/jpg");
        }

        // GET: Actor
        public async Task<IActionResult> Index()
        {
              return _context.Actor != null ? 
                          View(await _context.Actor.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Actor'  is null.");
        }

        // GET: Actor/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Actor == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            var movies = await _context.MovieActor
            .Include(ma => ma.Movie)
            .Where(ma => ma.ActorId == actor.Id)
            .Select(ma => ma.Movie)
            .ToListAsync();

            ////////////////////////////////////////////////////
            var ApiKey = _config["OpenAi:Key"] ?? throw new Exception("OpenAI:Key does not exist in the current Configuration"); ;
            var ApiEndpoint = _config["OpenAi:Endpoint"] ?? throw new Exception("OpenAI:Endpoint does not exist in the current Configuration");
            var AiDeployment = "gpt-35-turbo-16k";
            ApiKeyCredential ApiCredential = new(ApiKey);

            var ActorName = actor.Name;

            ChatClient chatClient = new AzureOpenAIClient(new Uri(ApiEndpoint), ApiCredential).GetChatClient(AiDeployment);
            var analyzer = new SentimentIntensityAnalyzer();
            double sentimentTotal = 0;
            var tweets_and_sentiments = new List<Object[]>();

            var messages = new ChatMessage[]
            {
            new SystemChatMessage($"You represent the Twitter social media platform. Generate an answer with a valid JSON formatted array of objects containing the tweet and username. The response should start with [. Tweets have a 50 character limit."),
            new UserChatMessage($"Generate 20 tweets about the actor {ActorName}.")
            };
            var chatCompletionOptions = new ChatCompletionOptions
            {
                MaxOutputTokenCount = 1000,
            };

            ClientResult<ChatCompletion> result = await chatClient.CompleteChatAsync(messages, chatCompletionOptions);
            string tweetsJsonString = result.Value.Content.FirstOrDefault()?.Text ?? "[]";
            JsonArray json = JsonNode.Parse(tweetsJsonString)!.AsArray();

            var tweets = json.Select(t => new { Username = t!["username"]?.ToString() ?? "", Text = t!["tweet"]?.ToString() ?? "" }).ToArray();
            foreach (var tweet in tweets)
            {
                SentimentAnalysisResults sentiment = analyzer.PolarityScores(tweet.Text);
                sentimentTotal += sentiment.Compound;
                tweets_and_sentiments.Add(new Object[] { tweet.Username, tweet.Text, sentiment.Compound });
            }

            double sentimentAverage = sentimentTotal / tweets_and_sentiments.Count;

            var vm = new ActorDetailsViewModel(actor, movies, tweets_and_sentiments, sentimentAverage);

            return View(vm);
        }

        // GET: Actor/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Actor/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Gender,Age,IMDBLink,Photo")] Actor actor, IFormFile? photo)
        {
            if (ModelState.IsValid)
            {
                if (photo != null && photo.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    photo.CopyTo(memoryStream);
                    actor.Photo = memoryStream.ToArray();
                }

                _context.Add(actor);
                await _context.SaveChangesAsync();





                return RedirectToAction(nameof(Index));
            }
            return View(actor);
        }

        // GET: Actor/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Actor == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor.FindAsync(id);
            if (actor == null)
            {
                return NotFound();
            }
            return View(actor);
        }

        // POST: Actor/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Gender,Age,IMDBLink,Photo")] Actor actor, IFormFile? photo)
        {
            if (id != actor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (photo != null && photo.Length > 0)
                    {
                        using var memoryStream = new MemoryStream();
                        photo.CopyTo(memoryStream);
                        actor.Photo = memoryStream.ToArray();
                    }

                    _context.Update(actor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActorExists(actor.Id))
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
            return View(actor);
        }

        // GET: Actor/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Actor == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            return View(actor);
        }

        // POST: Actor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Actor == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Actor'  is null.");
            }
            var actor = await _context.Actor.FindAsync(id);
            if (actor != null)
            {
                _context.Actor.Remove(actor);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ActorExists(int id)
        {
          return (_context.Actor?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
