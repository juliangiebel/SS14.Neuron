namespace Neuron.Core.ModuleLoader;

public class ModuleLoaderConfiguration
{
    public string ModuleDirectoryPath { get; set; } = string.Empty;
    
    public List<string> Modules { get; set; } = [];
}