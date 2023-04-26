namespace Chatify.Helpers;

public interface IOidGenerator
{
    Task<string> GenerateOidAsync();
}