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

namespace Deeplex.Saverwalter.Model
{
    public class WohnungEigentuemer
    {
        public int WohnungEigentuemerId { get; set; }
        public DateOnly Von { get; set; }
        public DateOnly? Bis { get; set; }
        public decimal? Anteil { get; set; }
        public virtual Wohnung Wohnung { get; set; } = null!;
        public virtual Kontakt Kontakt { get; set; } = null!;

        public DateTime CreatedAt { get; private set; }
        public DateTime LastModified { get; set; }

        public WohnungEigentuemer(DateOnly von)
        {
            Von = von;
        }
    }
}
