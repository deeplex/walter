using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;

namespace Deeplex.Saverwalter.WebAPI.Services
{
    public sealed class PersonMap
    {
        Dictionary<Guid, IPerson> Personen = new();

        public IPerson? FindPerson(Guid PersonId)
        {
            if (PersonId == Guid.Empty)
            {
                return null;
            }
            else if (Personen.ContainsKey(PersonId))
            {
                return Personen[PersonId];
            }
            else
            {
                return null;
            }
        }

        public PersonMap(IWalterDbService dbService)
        {
            dbService.ctx.NatuerlichePersonen.ToList().ForEach(e => Personen.Add(e.PersonId, e));
            dbService.ctx.JuristischePersonen.ToList().ForEach(e => Personen.Add(e.PersonId, e));
        }
    }
}
