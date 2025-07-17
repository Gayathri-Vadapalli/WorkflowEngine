using Infonetica.WorkflowApi.Models;

namespace Infonetica.WorkflowApi.Services;

public class WorkflowService
{
    private readonly Dictionary<string, WorkflowDefinition> _definitions = new();
    private readonly Dictionary<string, WorkflowInstance> _instances = new();

    public (bool IsSuccess, string Message) AddDefinition(WorkflowDefinition def)
    {
        if (_definitions.ContainsKey(def.Id))
            return (false, "Duplicate workflow ID.");

        if (!def.States.Any(s => s.IsInitial))
            return (false, "Workflow must have one initial state.");

        if (def.States.Count(s => s.IsInitial) > 1)
            return (false, "Workflow must have only one initial state.");

        var stateIds = def.States.Select(s => s.Id).ToHashSet();

        foreach (var action in def.Actions)
        {
            if (!action.Enabled)
                continue;

            if (!stateIds.Contains(action.ToState))
                return (false, $"Action {action.Id} targets unknown state {action.ToState}.");

            if (!action.FromStates.All(fs => stateIds.Contains(fs)))
                return (false, $"Action {action.Id} has unknown fromStates.");
        }

        _definitions[def.Id] = def;
        return (true, "Workflow definition added.");
    }

    public WorkflowDefinition? GetDefinition(string id) =>
        _definitions.TryGetValue(id, out var def) ? def : null;

    public WorkflowInstance? StartInstance(string definitionId)
    {
        if (!_definitions.TryGetValue(definitionId, out var def))
            return null;

        var initial = def.States.FirstOrDefault(s => s.IsInitial && s.Enabled);
        if (initial == null) return null;

        var instance = new WorkflowInstance
        {
            DefinitionId = definitionId,
            CurrentState = initial.Id
        };

        _instances[instance.Id] = instance;
        return instance;
    }

    public (bool IsSuccess, string Message) ExecuteAction(string instanceId, string actionId)
    {
        if (!_instances.TryGetValue(instanceId, out var instance))
            return (false, "Instance not found.");

        if (!_definitions.TryGetValue(instance.DefinitionId, out var def))
            return (false, "Definition not found.");

        var currentState = instance.CurrentState;
        var action = def.Actions.FirstOrDefault(a => a.Id == actionId);

        if (action == null || !action.Enabled)
            return (false, "Action not found or disabled.");

        if (!action.FromStates.Contains(currentState))
            return (false, "Action not valid from current state.");

        var currentStateObj = def.States.FirstOrDefault(s => s.Id == currentState);
        if (currentStateObj?.IsFinal == true)
            return (false, "Cannot act on final state.");

        instance.CurrentState = action.ToState;
        instance.History.Add((action.Id, DateTime.UtcNow));
        return (true, $"Moved to state {action.ToState}.");
    }

    public WorkflowInstance? GetInstance(string id) =>
        _instances.TryGetValue(id, out var inst) ? inst : null;
}