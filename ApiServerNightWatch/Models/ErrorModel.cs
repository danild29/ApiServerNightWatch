
using System.Text.Json;

namespace ApiServerNightWatch.Models;


public class ErrorModel
{
    public string Message { get; set; } = null!;
    public List<string> Errors { get; set; }
    public ErrorModel(string m)
    {
        Message = m;
        Errors = new List<string>();
    }

}