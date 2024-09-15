using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Infrastructure.Telegram;

public abstract class TelegramCommand
{
    public IExpenseInfoState Execute(IExpenseInfoState state, IStateFactory stateFactory)
    {
        Task.WaitAll(Cancel(state));

        return ToNextState(state, stateFactory);
    }

    private async Task Cancel(IExpenseInfoState state)
    {
        if (state is ILongTermOperation longTermOperation)
        {
            await longTermOperation.Cancel();
        }
    }

    protected abstract IExpenseInfoState ToNextState(IExpenseInfoState state, IStateFactory stateFactory);

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
        
        var type = assembly.GetTypes()
            .FirstOrDefault(t => t.IsSubclassOf(typeof(TelegramCommand)) && 
                                 t.GetCustomAttributes(typeof(CommandAttribute), true).Length > 0 && 
                                string.Equals(((CommandAttribute)t.GetCustomAttributes(typeof(CommandAttribute), true).First()).Command, text, StringComparison.InvariantCultureIgnoreCase));

        if (type == null)
        {
            telegramCommand = null;
            return false;
        }

        telegramCommand = (TelegramCommand) Activator.CreateInstance(type)!;
        return true;
    }
}