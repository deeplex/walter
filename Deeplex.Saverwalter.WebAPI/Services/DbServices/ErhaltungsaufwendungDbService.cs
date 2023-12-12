﻿using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.ErhaltungsaufwendungController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class ErhaltungsaufwendungDbService : ICRUDService<ErhaltungsaufwendungEntry>
    {
        public SaverwalterContext Ctx { get; }
        private readonly IAuthorizationService Auth;

        public ErhaltungsaufwendungDbService(SaverwalterContext ctx, IAuthorizationService authorizationService)
        {
            Ctx = ctx;
            Auth = authorizationService;
        }

        public async Task<IActionResult> GetList(ClaimsPrincipal user)
        {
            var list = await ErhaltungsaufwendungPermissionHandler.GetList(Ctx, user, VerwalterRolle.Keine);

            return new OkObjectResult(list.Select(e => new ErhaltungsaufwendungEntryBase(e, new(true))).ToList());
        }

        public async Task<IActionResult> Get(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.Erhaltungsaufwendungen.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            var permissions = await Utils.GetPermissions(user, entity, Auth);
            if (!permissions.Read)
            {
                return new ForbidResult();
            }


            try
            {
                var entry = new ErhaltungsaufwendungEntry(entity, permissions);
                return new OkObjectResult(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public async Task<IActionResult> Delete(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.Erhaltungsaufwendungen.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            var authRx = await Auth.AuthorizeAsync(user, entity, [Operations.Delete]);
            if (!authRx.Succeeded)
            {
                return new ForbidResult();
            }

            Ctx.Erhaltungsaufwendungen.Remove(entity);
            Ctx.SaveChanges();

            return new OkResult();
        }

        public async Task<IActionResult> Post(ClaimsPrincipal user, ErhaltungsaufwendungEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }

            try
            {
                var wohnung = await Ctx.Wohnungen.FindAsync(entry.Wohnung.Id);
                var authRx = await Auth.AuthorizeAsync(user, wohnung, [Operations.SubCreate]);
                if (!authRx.Succeeded)
                {
                    return new ForbidResult();
                }

                return new OkObjectResult(await Add(entry));
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private async Task<ErhaltungsaufwendungEntry> Add(ErhaltungsaufwendungEntry entry)
        {
            var aussteller = (await Ctx.Kontakte.FindAsync(entry.Aussteller.Id))!;
            var wohnung = (await Ctx.Wohnungen.FindAsync(entry.Wohnung.Id))!;
            var entity = new Erhaltungsaufwendung(entry.Betrag, entry.Bezeichnung, entry.Datum)
            {
                Aussteller = aussteller,
                Wohnung = wohnung,
            };

            SetOptionalValues(entity, entry);
            Ctx.Erhaltungsaufwendungen.Add(entity);
            await Ctx.SaveChangesAsync();

            return new ErhaltungsaufwendungEntry(entity, new());
        }

        public async Task<IActionResult> Put(ClaimsPrincipal user, int id, ErhaltungsaufwendungEntry entry)
        {
            var entity = await Ctx.Erhaltungsaufwendungen.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            var authRx = await Auth.AuthorizeAsync(user, entity, [Operations.Update]);
            if (!authRx.Succeeded)
            {
                return new ForbidResult();
            }

            try
            {
                return new OkObjectResult(await Update(entry, entity));
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private async Task<ErhaltungsaufwendungEntry> Update(ErhaltungsaufwendungEntry entry, Erhaltungsaufwendung entity)
        {
            entity.Betrag = entry.Betrag;
            entity.Wohnung = (await Ctx.Wohnungen.FindAsync(entry.Wohnung.Id))!;
            entity.Bezeichnung = entry.Bezeichnung;
            entity.Aussteller = (await Ctx.Kontakte.FindAsync(entry.Aussteller.Id))!;
            entity.Datum = entry.Datum;

            SetOptionalValues(entity, entry);
            Ctx.Erhaltungsaufwendungen.Update(entity);
            Ctx.SaveChanges();

            return new ErhaltungsaufwendungEntry(entity, entry.Permissions);
        }

        private void SetOptionalValues(Erhaltungsaufwendung entity, ErhaltungsaufwendungEntry entry)
        {
            if (entity.ErhaltungsaufwendungId != entry.Id)
            {
                throw new Exception();
            }

            entity.Notiz = entry.Notiz;
        }
    }
}
