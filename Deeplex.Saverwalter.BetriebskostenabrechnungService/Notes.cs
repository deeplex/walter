namespace Deeplex.Saverwalter.BetriebskostenabrechnungService
{
    public sealed class Note : IEquatable<Note>
    {
        public string Message { get; }
        public Severity Severity { get; }

        public Note(string message, Severity severity)
        {
            Message = message;
            Severity = severity;
        }

        public override int GetHashCode()
        {
            return Message.GetHashCode() ^ Severity.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                return false;
            }

            return Equals((Note)obj);
        }

        public bool Equals(Note? note)
        {
            if (note == null)
            {
                return false;
            }

            return Message == note.Message && Severity == note.Severity;
        }
    }

    public enum Severity
    {
        Info,
        Warning,
        Error,
    }
}
