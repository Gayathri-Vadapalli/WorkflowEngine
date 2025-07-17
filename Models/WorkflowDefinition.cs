namespace Infonetica.WorkflowApi.Models;

public class WorkflowDefinition
{
    public string Id { get; set; } = default!;
    public List<State> States { get; set; } = new();
    public List<ActionTransition> Actions { get; set; } = new();
}