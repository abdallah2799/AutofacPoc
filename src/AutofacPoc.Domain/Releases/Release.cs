namespace AutofacPoc.Domain.Releases;

/// <summary>
/// A music release (a single or an album) that we deliver to DSPs such as Spotify or Anghami.
/// </summary>
/// <remarks>
/// Domain layer: plain business objects. This project references nothing else,
/// and nothing here knows about databases, APIs or dependency injection.
/// </remarks>
public class Release
{
    /// <summary>Unique number of the release, e.g. <c>1</c>.</summary>
    public int Id { get; }

    /// <summary>Name of the release, e.g. "Lost in Cairo".</summary>
    public string Title { get; }

    /// <summary>Name of the main artist.</summary>
    public string Artist { get; }

    /// <summary>Creates a release.</summary>
    /// <param name="id">Unique number of the release.</param>
    /// <param name="title">Name of the release.</param>
    /// <param name="artist">Name of the main artist.</param>
    public Release(int id, string title, string artist)
    {
        Id = id;
        Title = title;
        Artist = artist;
    }
}
