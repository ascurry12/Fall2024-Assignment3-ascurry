namespace Fall2024_Assignment3_ascurry.Models;


public class MovieDetailsViewModel
{
    public Movie Movie { get; set; }
    public IEnumerable<Actor> Actors { get; set; }
    public List<Object[]> ReviewsAndSentiments { get; set; }
    public double AvgSentiment { get; set; }

    public MovieDetailsViewModel(Movie movie, IEnumerable<Actor> actors,
                                List<Object[]> reviews_and_sentiments, double average)
    {
        Movie = movie;
        Actors = actors;
        ReviewsAndSentiments = reviews_and_sentiments;
        AvgSentiment = average;
    }

}
