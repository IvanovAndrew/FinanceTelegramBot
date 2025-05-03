namespace Application;

public interface IMessage
{
    int? Id { get; }
    long ChatId { get; }
    DateTime Date { get; }
    string Text { get; }
    bool Edited { get; }
    MessageOptions? Options { get; }
    Table? Table { get; }
    IFileInfo? FileInfo { get; }
}

public class Message : IMessage
{
    public int? Id { get; init; }
    public long ChatId { get; init;}
    public DateTime Date { get; init;}
    public string Text { get; init; } = string.Empty;
    public bool Edited { get; init;}
    public MessageOptions? Options { get; init; }
    public Table? Table { get; init; }
    public IFileInfo? FileInfo { get; init; }
}

public class Option
{
    public string Code { get; }
    public string Text { get; }

    public Option(string text) : this(text, text)
    {
        
    }
    
    public Option(string code, string text)
    {
        Code = code;
        Text = text;
    }
}

public class MessageOptions
{
    private IReadOnlyCollection<Option> Options { get; }
    private int ChunkSize { get; } = 4;
    private Option? SingleLineOption { get; }

    private MessageOptions(IReadOnlyCollection<Option> options, int chunkSize = 4, Option? singleLineOption = null)
    {
        Options = options;
        ChunkSize = chunkSize;
        SingleLineOption = singleLineOption;
    }
    
    public static MessageOptions FromList(IReadOnlyCollection<Option> items)
    {
        return new MessageOptions(items);
    }
    
    public static MessageOptions FromList(IReadOnlyCollection<string> items)
    {
        return new MessageOptions(items.Select(item => new Option(item)).ToList());
    }
    
    public static MessageOptions FromListAndLastSingleLine(IReadOnlyCollection<string> items, string lastOption)
    {
        return new MessageOptions(items.Select(item => new Option(item)).ToList(), singleLineOption:new Option(lastOption));
    }

    public IEnumerable<IReadOnlyList<Option>> Chunks()
    {
        foreach (var chunks in Options.Chunk(ChunkSize))
        {
            yield return chunks;
        }

        if (SingleLineOption != null)
        {
            yield return new []{SingleLineOption};
        }
    }

    public IReadOnlyList<Option> AllOptions()
    {
        var list = new List<Option>();
        list.AddRange(Options);
        if (SingleLineOption != null)
        {
            list.Add(SingleLineOption);
        }

        return list;
    }
}