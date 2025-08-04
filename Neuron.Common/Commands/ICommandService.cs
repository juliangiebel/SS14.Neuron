using Neuron.Common.Commands;

namespace Neuron.Common.Commands;

public interface ICommandService
{
    Task<ICommand<T>?> Run<T>(ICommand<T> command)  where T : class;
}