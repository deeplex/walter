// Copyright (c) 2023-2024 Kai Lawrence
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

namespace Deeplex.Saverwalter.Model
{
    public sealed class ErhaltungsaufwendungListeEntry
    {
        public Erhaltungsaufwendung Entity { get; }
        public SaverwalterContext db { get; }
        public Wohnung Wohnung => Entity.Wohnung;
        public Kontakt Aussteller => Entity.Aussteller;
        public string Bezeichnung => Entity.Bezeichnung;
        public DateOnly Datum => Entity.Datum;
        public double Betrag => Entity.Betrag;
        public bool active = true;
        //public string color => active ? "

        public ErhaltungsaufwendungListeEntry(Erhaltungsaufwendung e, SaverwalterContext _db)
        {
            Entity = e;
            db = _db;
        }
    }
}
