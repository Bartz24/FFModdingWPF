using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF;
public class RandoException : Exception
{
    public string ErrorMessage { get; set; }
    public string Title { get; set; }
    public RandoException(string message, string title) : base($"{title}\n {message}")
    {
        ErrorMessage = message;
        Title = title;
    }
    public RandoException(string message, string title, Exception innerException) : base($"{title}\n {message}", innerException)
    {
        ErrorMessage = message;
        Title = title;
    }
}
