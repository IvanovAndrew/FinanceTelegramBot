namespace Infrastructure;

public interface IMessage
{
    int Id { get; }
    DateTime Date { get; }
    string Text { get; }
}