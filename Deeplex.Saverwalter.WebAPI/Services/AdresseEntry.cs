﻿using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.WebAPI.Services
{
    public class AdresseEntry
    {
        private Adresse Entity { get; } 
        public string Strasse => Entity.Strasse;
        public string Hausnummer => Entity.Hausnummer;
        public string Postleitzahl => Entity.Postleitzahl;
        public string Stadt => Entity.Stadt;

        public AdresseEntry(Adresse entity)
        {
            Entity = entity;
        }
    }
}
