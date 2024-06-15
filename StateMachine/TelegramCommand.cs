using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Infrastructure;

namespace StateMachine;

public class CommandAttribute : Attribute
{
    public string Text { get; init; } = "";
    public string Command { get; init; } = "";
    public int Order { get; init; }
}

public abstract class TelegramCommand
{
    public abstract Task<IExpenseInfoState> Execute(IExpenseInfoState state, StateFactory stateFactory);

    public static List<CommandAttribute> GetAllCommands()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        
        return assembly.GetTypes()
            .Where(t => t.GetCustomAttributes(typeof(CommandAttribute), true).Length > 0)
            .Select(t => (CommandAttribute)t.GetCustomAttributes(typeof(CommandAttribute), true).First())
            .ToList();
    }
    
    public static bool TryGetCommand(string text, [NotNullWhen(true)] out TelegramCommand? telegramCommand)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        Dictionary<string, Type> commandToTypeMap = new Dictionary<string, Type>();
        
        var type = assembly.GetTypes()
            .FirstOrDefault(t => t.IsInstanceOfType(typeof(TelegramCommand)) && 
                                 t.GetCustomAttributes(typeof(CommandAttribute), true).Length > 0 && 
                                ((CommandAttribute)t.GetCustomAttributes(typeof(CommandAttribute), true).First()).Command == text);

        if (type == null)
        {
            telegramCommand = null;
            return false;
        }

        telegramCommand = (TelegramCommand) Activator.CreateInstance(type)!;
        return true;
    }
}

[Command(Text = "Cancel", Command = "/cancel", Order = 2)]
public class CancelCommand : TelegramCommand
{
    public override Task<IExpenseInfoState> Execute(IExpenseInfoState state, StateFactory stateFactory)
    {
        if (state is ILongTermOperation longTermOperation)
        {
            longTermOperation.Cancel();
        }

        return Task.FromResult(stateFactory.CreateGreetingState());
    }

}

[Command(Text = "Start", Command = "/start", Order = 0)]
public class StartCommand : TelegramCommand
{
    public override Task<IExpenseInfoState> Execute(IExpenseInfoState state, StateFactory stateFactory)
    {
        return Task.FromResult(stateFactory.CreateGreetingState());
    }
}

[Command(Text = "Back", Command = "/back", Order = 1)]
public class BackCommand : TelegramCommand
{
    public override Task<IExpenseInfoState> Execute(IExpenseInfoState state, StateFactory stateFactory)
    {
        return Task.FromResult(state.PreviousState);
    }
}