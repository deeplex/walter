using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.WebAPI.Services
{
    public class AnhangEntry
    {
        private Anhang Entity { get; }
        public string Filename => Entity.FileName;
        public DateTime CreationTime => Entity.CreationTime;

        public AnhangEntry(Anhang entity)
        {
            Entity = entity;
        }
    }
}
