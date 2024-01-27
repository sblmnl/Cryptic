namespace WebAPI.Features.Notes.CreateNote;

public static class HttpErrors
{
    public static readonly IResult DeleteAfterAlreadyPassed = Results.BadRequest(
        "The provided date and time to delete the note has already passed!");
}

public record Request(
    string Content,
    DateTimeOffset? DeleteAt,
    bool DoNotWarn);

public record Response(
    Guid Id,
    DateTimeOffset? DeleteAt,
    bool DoNotWarn);

public static class HttpHandler
{
    public static readonly Delegate Handler = async (
        Request req,
        Storage.NoteRepository noteRepository,
        HttpContext ctx) =>
    {
        var deleteAfter = Domain.DeleteAfter.From(req.DeleteAt, req.DoNotWarn);

        if (deleteAfter is Domain.DeleteAfter.Time deleteAfterTime
            && deleteAfterTime.DeleteAt <= DateTimeOffset.UtcNow)
        {
            return HttpErrors.DeleteAfterAlreadyPassed;
        }

        var note = new Domain.Note(
            Guid.NewGuid(),
            req.Content,
            deleteAfter);

        await noteRepository.AddNoteAsync(note, ctx.RequestAborted);

        return Results.Created(
            new Uri(note.Id.ToString(), UriKind.Relative), 
            new Response(note.Id, req.DeleteAt, req.DoNotWarn));
    };
}
