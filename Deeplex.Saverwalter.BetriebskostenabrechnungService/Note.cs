// Copyright (c) 2023-2026 Kai Lawrence
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

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

        public override int GetHashCode() => Message.GetHashCode() ^ Severity.GetHashCode();

        public override bool Equals(object? obj) => obj is Note other && Equals(other);

        public bool Equals(Note? note) =>
            note is not null && Message == note.Message && Severity == note.Severity;
    }

    public enum Severity
    {
        Info,
        Warning,
        Error,
    }

    public static class NoteExtensions
    {
        public static void Add(this List<Note> notes, string message, Severity severity)
            => notes.Add(new Note(message, severity));
    }
}
