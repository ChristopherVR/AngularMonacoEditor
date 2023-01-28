using IntellisenseServer.Interfaces;

namespace IntellisenseServer.Models;

public sealed record Signature(string Label, string? Documentation, Parameter[] Parameters);
public sealed record Parameter(string Label, string? Documentation = default);
public sealed record TabCompletionRequest(string Code, int Position, string[] Assemblies) : IRequest;
public sealed record HoverInfoRequest(string Code, int Position, string[] Assemblies) : IRequest;
public sealed record CodeCheckRequest(string Code, int Position, string[] Assemblies) : IRequest;
public sealed record SignatureRequest(string Code, int Position, string[] Assemblies) : IRequest;
