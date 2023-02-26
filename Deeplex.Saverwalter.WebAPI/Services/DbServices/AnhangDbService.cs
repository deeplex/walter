﻿using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.Details.AnhangController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class AnhangDbService : BaseDbService<AnhangEntry> //, IControllerService<AnhangEntry> // TODO because Guid => Change Guid with int
    {
        private void SetValues(Anhang entity, AnhangEntry entry)
        {
            if (entity.AnhangId != entry.Id)
            {
                throw new Exception();
            }

            // TODO
            //entity.Notiz = entry.Notiz;
        }

        public AnhangDbService(IWalterDbService dbService) : base(dbService)
        {
        }

        public IActionResult Get(Guid id)
        {
            var entity = DbService.ctx.Anhaenge.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                var entry = new AnhangEntry(entity, DbService);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public IActionResult Delete(Guid id)
        {
            var entity = DbService.ctx.Anhaenge.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            DbService.ctx.Anhaenge.Remove(entity);

            return Save(null!);
        }

        public IActionResult Post(AnhangEntry entry)
        {
            if (entry.Id != Guid.Empty)
            {
                return new BadRequestResult();
            }
            var entity = new Anhang();

            try
            {
                SetValues(entity, entry);
                DbService.ctx.Anhaenge.Add(entity);
                return Save(entry);
            }
            catch
            {
                return new BadRequestResult();
            }

        }

        public IActionResult Put(Guid id, AnhangEntry entry)
        {
            var entity = DbService.ctx.Anhaenge.Find(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            try
            {
                SetValues(entity, entry);
                DbService.ctx.Anhaenge.Update(entity);
                return Save(entry);
            }
            catch
            {
                return new BadRequestResult();
            }

        }
    }
}
