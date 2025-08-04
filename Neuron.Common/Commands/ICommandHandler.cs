namespace Neuron.Common.Commands;

public interface ICommandHandler
{
    string CommandName { get; }
    Task<ICommand<T>?> Handle<T>(ICommand<T> command)  where T : class;   
}