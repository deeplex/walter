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

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Deeplex.Saverwalter.Model
{
    public class HKVO
    {
        public int HKVOId { get; set; }

        [Required]
        public double HKVO_P7 { get; set; }
        [Required]
        public double HKVO_P8 { get; set; }
        [Required]
        public HKVO_P9A2 HKVO_P9 { get; set; }
        [Required]
        public double Strompauschale { get; set; }

        public int HeizkostenId { get; set; }
        [Required]
        public virtual Umlage Heizkosten { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078
        [Required]
        public virtual Umlage Betriebsstrom { get; set; } = null!; // See https://github.com/dotnet/efcore/issues/12078

        public string? Notiz { get; set; }

        public DateTime CreatedAt { get; private set; }
        public DateTime LastModified { get; set; }

        public HKVO(double hKVO_P7, double hKVO_P8, HKVO_P9A2 hKVO_P9, double strompauschale)
        {
            HKVO_P7 = hKVO_P7;
            HKVO_P8 = hKVO_P8;
            HKVO_P9 = hKVO_P9;

            // TODO this could be calculated by a zaehler, too... Then it would be different for each year...
            Strompauschale = strompauschale;
        }
    }

    public enum HKVO_P9A2
    {
        [Description("Satz 1")]
        Satz_1 = 1,
        [Description("Satz 2")]
        Satz_2 = 2,
        [Description("Satz 4")]
        Satz_4 = 4,
    }
}
