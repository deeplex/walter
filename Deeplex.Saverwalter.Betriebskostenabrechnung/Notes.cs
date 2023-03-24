namespace Deeplex.Saverwalter.Model
{
    public sealed class Note
    {
        public string Message { get; }
        public Severity Severity { get; }

        public Note(string message, Severity severity)
        {
            Message = message;
            Severity = severity;
        }
    }

    public enum Severity
    {
        Info,
        Warning,
        Error,
    }
}
