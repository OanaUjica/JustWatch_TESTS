using AutoMapper;
using Lab1_.NET.Data;
using Lab1_.NET.Mapping;
using Lab1_.NET.Models;
using Lab1_.NET.Services;
using Lab1_.NET.ViewModels;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tests_JustWatch
{
    public class Tests_MoviesService
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        private MoviesService _moviesService;

        [SetUp]
        public void Setup()
        {
            Console.WriteLine("In setup.");
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDB")
                .Options;
            _context = new ApplicationDbContext(options, new OperationalStoreOptionsForTests());

            var dateAdded = DateTime.Now;
            var reviews = new List<Review>();
            reviews.Add(new Review
            {
                Id = 1,
                Text = "review 1",
                Important = true,
                DateTime = dateAdded
            });
            reviews.Add(new Review
            {
                Id = 2,
                Text = "review 2",
                Important = false,
                DateTime = dateAdded
            });

            _context.Movies.Add(new Movie
            {
                Id = 1,
                Title = "Movie1 test",
                Description = "Test",
                Genre = "Action",
                DurationInMinutes = 128,
                YearOfRelease = 2020,
                Director = "Test",
                DateAdded = dateAdded,
                Rating = 9,
                Watched = true,
                Reviews = reviews
            });
            _context.Movies.Add(new Movie
            {
                Id = 2,
                Title = "Movie2 test",
                Description = "Test",
                Genre = "Action",
                DurationInMinutes = 145,
                YearOfRelease = 2021,
                Director = "Test",
                DateAdded = dateAdded,
                Rating = 9,
                Watched = true,
                Reviews = reviews
            });
            _context.SaveChanges();


            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MappingProfile());
            });
            _mapper = config.CreateMapper();

            _moviesService = new MoviesService(_context, _mapper);
        }

        [TearDown]
        public void Teardown()
        {
            Console.WriteLine("In teardown");

            foreach (var movie in _context.Movies)
            {
                _context.Remove(movie);
            }
            _context.SaveChanges();
        }

        [Test]
        public async Task TestGetMovies()
        {

            // Act
            var moviesService = new MoviesService(_context, _mapper);
            var getAllMoviesResult = await moviesService.GetMovies();

            // Assert
            Assert.AreEqual(2, getAllMoviesResult.ResponseOk.TotalEntities);
        }

        [Test]
        public async Task TestGetMovie()
        {
            // Act
            var movie = await _context.Movies.FirstOrDefaultAsync();
            var movieId = movie.Id;
            var response = await _moviesService.GetMovie(movieId);

            // Assert
            Assert.AreEqual(movieId, response.ResponseOk.Id);
        }

        [Test]
        public async Task TestPostMovie()
        {
            // Act
            var movies = await _moviesService.GetMovies();
            Assert.AreEqual(2, movies.ResponseOk.TotalEntities);

            var requestMovie = new MovieViewModel
            {
                Title = "Test title",
                Description = "Test description",
                Genre = MovieGenre.Action,
                DurationInMinutes = 126,
                YearOfRelease = 2020,
                Director = "Test director",
                DateAdded = DateTime.Now,
                Rating = 9.2f,
                Watched = false
            };
            await _moviesService.PostMovie(requestMovie);
            var moviesAfterAddingNewOne = await _moviesService.GetMovies();

            // Assert
            Assert.AreEqual(3, moviesAfterAddingNewOne.ResponseOk.TotalEntities);
        }

        [Test]
        public async Task TestDeleteMovie()
        {
            // Act
            var movie = await _context.Movies.FirstOrDefaultAsync();
            await _moviesService.DeleteMovie(movie.Id);

            // Assert
            Assert.False(await _context.Movies.AnyAsync(m => m.Id == movie.Id));
        }

        [Test]
        public async Task TestGetReviewsForMovie()
        {
            // Act
            var movie = await _context.Movies
                .Include(s => s.Reviews)
                .FirstOrDefaultAsync();
            var result = await _moviesService.GetReviewsForMovie(movie.Id);

            // Assert
            Assert.AreEqual(2, result.ResponseOk.TotalEntities);
        }

        [Test]
        public async Task TestPostReviewToMovie()
        {
            // Act
            var movie = await _context.Movies
                .FirstOrDefaultAsync();

            var review = new ReviewViewModel
            {
                Text = "review 3",
                Important = true,
                DateTime = DateTime.Now
            };
            await _moviesService.PostReviewForMovie(movie.Id, review);
            var movieWithNewReviewAdded = await _context.Movies.FindAsync(movie.Id);

            // Assert
            Assert.AreEqual(3, movieWithNewReviewAdded.Reviews.Count);
        }

        [Test]
        public async Task TestDeleteReviewFromMovie()
        {
            // Act
            var movie = await _context.Movies
                .FirstOrDefaultAsync();
            await _moviesService.DeleteReviewFromMovie(2);
            var movieWithRemovedReview = await _context.Movies.FindAsync(movie.Id);

            // Assert
            Assert.AreEqual(1, movieWithRemovedReview.Reviews.Count);
        }
    }
}