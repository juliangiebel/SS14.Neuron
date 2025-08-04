using Neuron.Common.Commands;

namespace Neuron.Core.Commands;

public sealed class CommandService : ICommandService
{
    private readonly Dictionary<string, ICommandHandler> _commandHandlers = new();

    public CommandService(IEnumerable<ICommandHandler> commandHandlers)
    {
        foreach (var handler in commandHandlers)
        {
            _commandHandlers.Add(handler.CommandName, handler);
        }
    }


    public async Task<ICommand<T>?> Run<T>(ICommand<T> command) where T : class
    {
        if (!_commandHandlers.TryGetValue(command.Name, out var handler))
            return null;

        return await handler.Handle(command);
    }
}