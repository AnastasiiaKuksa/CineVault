namespace CineVault.API.Responses.Omdb;

// Відповідь при пошуку одного фільму (By Title/ID)
public class OmdbMovieResponse
{
    public string Title { get; set; } = null!;
    public string Year { get; set; } = null!;
    public string Runtime { get; set; } = null!;
    public string Rated { get; set; } = null!;
    public string Released { get; set; } = null!;
    public string Genre { get; set; } = null!;
    public string Director { get; set; } = null!;
    public string Writer { get; set; } = null!;
    public string Actors { get; set; } = null!;
    public string Plot { get; set; } = null!;
    public string Awards { get; set; } = null!;
    public string imdbRating { get; set; } = null!;
    // Ratings — масив, наприклад: [{Source: "Rotten Tomatoes", Value: "87%"}]
    public List<OmdbRating> Ratings { get; set; } = [];
}

public class OmdbRating
{
    public string Source { get; set; } = null!;
    public string Value { get; set; } = null!;
}

// Відповідь при пошуку списку фільмів (By Search)
public class OmdbSearchResponse
{
    public List<OmdbSearchItem> Search { get; set; } = [];
    public int totalResults { get; set; }
}

public class OmdbSearchItem
{
    public string Title { get; set; } = null!;
    public string Year { get; set; } = null!;
    public string imdbID { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Poster { get; set; } = null!;
}