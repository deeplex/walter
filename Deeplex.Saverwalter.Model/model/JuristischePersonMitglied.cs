using System;
using System.Collections.Generic;
using System.Text;

namespace Deeplex.Saverwalter.Model
{
    // JOIN_TABLE

    public sealed class JuristischePersonenMitglied
    {
        public int JuristischePersonenMitgliedId { get; set; }
        public Guid PersonId { get; set; }
        public int JuristischePersonId { get; set; }
        public JuristischePerson JuristischePerson { get; set; } = null!;
    }

}
