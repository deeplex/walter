using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.MietminderungController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class MietminderungDbService : ICRUDService<MietminderungEntry>
    {
        public SaverwalterContext Ctx { get; }
        private readonly IAuthorizationService Auth;

        public MietminderungDbService(SaverwalterContext ctx, IAuthorizationService authorizationService)
        {
            Ctx = ctx;
            Auth = authorizationService;
        }

        public async Task<ActionResult<IEnumerable<MietminderungEntryBase>>> GetList(ClaimsPrincipal user)
        {
            var list = await MietminderungPermissionHandler.GetList(Ctx, user, VerwalterRolle.Keine);

            return await Task.WhenAll(list
                .Select(async e => new MietminderungEntryBase(e, await Utils.GetPermissions(user, e, Auth))));
        }

        public async Task<ActionResult<MietminderungEntry>> Get(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.Mietminderungen.FindAsync(id);
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
                var entry = new MietminderungEntry(entity, permissions);
                return entry;
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        public async Task<ActionResult> Delete(ClaimsPrincipal user, int id)
        {
            var entity = await Ctx.Mietminderungen.FindAsync(id);
            if (entity == null)
            {
                return new NotFoundResult();
            }

            var authRx = await Auth.AuthorizeAsync(user, entity, [Operations.Delete]);
            if (!authRx.Succeeded)
            {
                return new ForbidResult();
            }

            Ctx.Mietminderungen.Remove(entity);
            Ctx.SaveChanges();

            return new OkResult();
        }

        public async Task<ActionResult<MietminderungEntry>> Post(ClaimsPrincipal user, MietminderungEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }

            try
            {
                var vertrag = (await Ctx.Vertraege.FindAsync(entry.Vertrag!.Id));
                var authRx = await Auth.AuthorizeAsync(user, vertrag, [Operations.SubCreate]);
                if (!authRx.Succeeded)
                {
                    return new ForbidResult();
                }

                return await Add(entry);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private async Task<MietminderungEntry> Add(MietminderungEntry entry)
        {
            var vertrag = await Ctx.Vertraege.FindAsync(entry.Vertrag.Id);
            var entity = new Mietminderung(entry.Beginn, entry.Minderung)
            {
                Vertrag = vertrag!
            };

            SetOptionalValues(entity, entry);
            Ctx.Mietminderungen.Add(entity);
            Ctx.SaveChanges();

            return new MietminderungEntry(entity, entry.Permissions);
        }

        public async Task<ActionResult<MietminderungEntry>> Put(ClaimsPrincipal user, int id, MietminderungEntry entry)
        {
            var entity = await Ctx.Mietminderungen.FindAsync(id);
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
                return Update(entry, entity);
            }
            catch
            {
                return new BadRequestResult();
            }
        }

        private MietminderungEntry Update(MietminderungEntry entry, Mietminderung entity)
        {
            entity.Ende = entry.Ende;
            entity.Beginn = entry.Beginn;
            entity.Minderung = entry.Minderung;

            SetOptionalValues(entity, entry);
            Ctx.Mietminderungen.Update(entity);
            Ctx.SaveChanges();

            return new MietminderungEntry(entity, entry.Permissions);
        }

        private void SetOptionalValues(Mietminderung entity, MietminderungEntry entry)
        {
            if (entity.MietminderungId != entry.Id)
            {
                throw new Exception();
            }

            entity.Notiz = entry.Notiz;
        }
    }
}
