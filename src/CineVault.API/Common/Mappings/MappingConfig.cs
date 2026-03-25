using CineVault.API.Controllers.Requests;
using CineVault.API.Controllers.Responses;
using CineVault.API.Data.Entities;
using Mapster;

namespace CineVault.API.Common.Mappings;

public class MappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Movie entity -> MovieResponse
        config.NewConfig<Movie, MovieResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.ReleaseDate, src => src.ReleaseDate)
            .Map(dest => dest.Genre, src => src.Genre)
            .Map(dest => dest.Director, src => src.Director)
            .Map(dest => dest.AverageRating, src => src.Reviews.Count != 0
                ? src.Reviews.Average(r => r.Rating)
                : 0)
            .Map(dest => dest.ReviewCount, src => src.Reviews.Count);

        // MovieRequest -> Movie entity
        config.NewConfig<MovieRequest, Movie>()
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.ReleaseDate, src => src.ReleaseDate)
            .Map(dest => dest.Genre, src => src.Genre)
            .Map(dest => dest.Director, src => src.Director)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Reviews);

        // Review entity -> ReviewResponse
        config.NewConfig<Review, ReviewResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.MovieId, src => src.MovieId)
            .Map(dest => dest.MovieTitle, src => src.Movie!.Title)
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.Username, src => src.User!.Username)
            .Map(dest => dest.Rating, src => src.Rating)
            .Map(dest => dest.Comment, src => src.Comment)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);

        // ReviewRequest -> Review entity
        config.NewConfig<ReviewRequest, Review>()
            .Map(dest => dest.MovieId, src => src.MovieId)
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.Rating, src => src.Rating)
            .Map(dest => dest.Comment, src => src.Comment)
            .Map(dest => dest.CreatedAt, src => DateTime.UtcNow)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Movie)
            .Ignore(dest => dest.User);

        // User entity -> UserResponse
        config.NewConfig<User, UserResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Username, src => src.Username)
            .Map(dest => dest.Email, src => src.Email);

        // UserRequest -> User entity
        config.NewConfig<UserRequest, User>()
            .Map(dest => dest.Username, src => src.Username)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.Password, src => src.Password)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Reviews);
    }
}