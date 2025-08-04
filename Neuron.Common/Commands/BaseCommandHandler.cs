namespace Neuron.Common.Commands;

public abstract class BaseCommandHandler<TCommand, TType> : ICommandHandler where TCommand : ICommand<TType> where TType : class
{
    public abstract string CommandName { get; }

    protected abstract Task<TCommand> Handle(TCommand command);
    
    public async Task<ICommand<T>?> Handle<T>(ICommand<T> genericCommand) where T : class
    {
        if (genericCommand is not TCommand command)
            return null;
        
        var result = await Handle(command);

        return (ICommand<T>)result;
    }
}