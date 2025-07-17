namespace Infonetica.WorkflowApi.Models;

public class ActionTransition
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public List<string> FromStates { get; set; } = new();
    public string ToState { get; set; } = default!;
    public bool Enabled { get; set; } = true;
}