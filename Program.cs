using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
namespace SuncoastMovies
{
    class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string PrimaryDirector { get; set; }
        public int YearReleased { get; set; }
        public string Genre { get; set; }
        public int RatingId { get; set; }
        //     data type (rating class)
        //     |      name of property
        //     |      | 
        public Rating Rating { get; set; }
        public List<Role> Roles { get; set; }
    }
    class Rating
    {
        public int Id { get; set; }
        public string Description { get; set; }
    }
    class Actor
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public DateTime Birthday { get; set; }
    }
    class Role
    {
        public int Id { get; set; }
        public string CharacterName { get; set; }
        public int MovieId { get; set; }
        public Movie Movie { get; set; }
        public int ActorId { get; set; }
        public Actor Actor { get; set; }
    }

    class SuncoastMoviesContext : DbContext
    {
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Movie> Movies { get; set; } // Acts like a list and establishes connection between table and class
        public DbSet<Actor> Actors { get; set; }
        public DbSet<Role> Roles { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            optionsBuilder.UseLoggerFactory(loggerFactory);
            optionsBuilder.UseNpgsql("server=localhost;database=SuncoastMovies"); // Connects to Db
        }
    }

    class Program
    {

        static void Main(string[] args)
        {

            var context = new SuncoastMoviesContext();
            var movies = context.Movies;
            var movieCount = movies.Count(); // Using Linq on DbSet retrieved from database
            Console.WriteLine($"There are {movieCount} movies!");
            var moviesWithRatings = context.Movies // makes a new collection of movies but each movie knows the associated Rating object
            .Include(movie => movie.Rating). // from our movie, please include the associated rating
            Include(movie => movie.Roles). // from our movie, please include the associated roles list
            ThenInclude(role => role.Actor); // THEN for each of the roles, please include associated actor object...join stmts in C# with includes, now we have access to all the data tables
            foreach (var movie in movies)
            {
                if (movie.Rating == null)
                {
                    Console.WriteLine($"There is an unrated movie named {movie.Title}");
                }
                else
                {
                    Console.WriteLine($"Movie {movie.Title} - movie.Rating.Description");
                }
            }
            foreach (var movie in movies)
            {
                if (movie.Rating == null)
                {
                    Console.WriteLine($"{movie.Title} - not rated");
                }
                else
                {
                    Console.WriteLine($"{movie.Title} - {movie.Rating.Description}");
                }
                foreach (var role in movie.Roles)
                {
                    Console.WriteLine($" - {role.CharacterName} played by {role.Actor.FullName}");
                }
            }
            var newMovie = new Movie
            {
                Title = "SpaceBalls",
                PrimaryDirector = "Mel Brooks",
                Genre = "Comedy",
                YearReleased = 1987,
                RatingId = 2
            };
            context.Movies.Add(newMovie); // DbSet can be treated like a list

            context.SaveChanges(); // Saves to database, must do this because it's hosted on a different computer, but add to the context (if not done, it will seriously slow down the program because it would have to connect with the database each time)
            // SaveChanges imparts atomicity
            var existingMovie = context.Movies.FirstOrDefault(movie => movie.Title == "SpaceBalls");
            if (existingMovie != null)
            {
                existingMovie.Title = "SpaceBalls - the best movie ever";
                context.SaveChanges();
            }
            var existingMovie2 = context.Movies.FirstOrDefault(movie => movie.Title == "Cujo");
            if (existingMovie2 != null)
            {
                context.Movies.Remove(existingMovie2);
                context.SaveChanges();
            }
        }
    }
}
