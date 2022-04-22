using System;
using System.Collections.Generic;
using System.Text;

namespace Deeplex.Saverwalter.Model
{

    // JOIN_TABLE
    public sealed class Mieter
    {
        public int MieterId { get; set; }
        public Guid PersonId { get; set; }
        public Guid VertragId { get; set; }
    }

}
