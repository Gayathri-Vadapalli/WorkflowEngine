// Program.cs
using Infonetica.WorkflowApi.Models;
using Infonetica.WorkflowApi.Services;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var workflowService = new WorkflowService();

app.MapPost("/workflow-definition", (WorkflowDefinition def) =>
{
    var result = workflowService.AddDefinition(def);
    return result.IsSuccess ? Results.Ok(result.Message) : Results.BadRequest(result.Message);
});

app.MapGet("/workflow-definition/{id}", (string id) =>
{
    var def = workflowService.GetDefinition(id);
    return def is not null ? Results.Ok(def) : Results.NotFound("Workflow definition not found.");
});

app.MapPost("/workflow-instance", (string definitionId) =>
{
    var instance = workflowService.StartInstance(definitionId);
    return instance is not null ? Results.Ok(instance) : Results.BadRequest("Cannot start instance.");
});

app.MapPost("/workflow-instance/{id}/action", (string id, string actionId) =>
{
    var result = workflowService.ExecuteAction(id, actionId);
    return result.IsSuccess ? Results.Ok(result.Message) : Results.BadRequest(result.Message);
});

app.MapGet("/workflow-instance/{id}", (string id) =>
{
    var instance = workflowService.GetInstance(id);
    return instance is not null ? Results.Ok(instance) : Results.NotFound("Instance not found.");
});

app.Run();