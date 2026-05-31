
using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Deeplex.Saverwalter.WebAPI.Controllers.AbrechnungsresultatController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Services.DbServices
{
    public class AbrechnungsresultatDbService : WalterDbServiceBase<AbrechnungsresultatEntry, Guid, Abrechnungsresultat>
    {
        public AbrechnungsresultatDbService(SaverwalterContext dbContext, IAuthorizationService authorizationService)
            : base(dbContext, authorizationService)
        {
        }

        public async Task<ActionResult<IEnumerable<AbrechnungsresultatEntry>>> GetList(ClaimsPrincipal user)
        {
            var list = await AbrechnungsresultatPermissionHandler.GetList(Ctx, user, VerwalterRolle.Keine);

            return await Task.WhenAll(list
                .Select(async e => new AbrechnungsresultatEntry(e, await Utils.GetPermissions(user, e, Auth))));
        }

        public override async Task<ActionResult<Abrechnungsresultat>> GetEntity(ClaimsPrincipal user, Guid id, OperationAuthorizationRequirement op)
        {
            var entity = await Ctx.Abrechnungsresultate
                .Include(r => r.Buchungssatz)
                    .ThenInclude(s => s.Buchungszeilen)
                        .ThenInclude(z => z.Buchungskonto)
                .Include(r => r.Vertrag)
                    .ThenInclude(v => v.ZahlungsKonto)
                .Include(r => r.Vertrag)
                    .ThenInclude(v => v.NkBuchungskonto)
                        .ThenInclude(k => k.Buchungszeilen)
                            .ThenInclude(z => z.Buchungssatz)
                .Include(r => r.Vertrag)
                    .ThenInclude(v => v.Wohnung)
                        .ThenInclude(w => w.Adresse)
                .FirstOrDefaultAsync(r => r.AbrechnungsresultatId == id);
            return await GetEntity(user, entity, op);
        }

        public async Task<ActionResult<AbrechnungsresultatEntry>> Get(
            ClaimsPrincipal user,
            int VertragId,
            int jahr)
        {
            var vertrag = await Ctx.Vertraege.FindAsync(VertragId);
            if (vertrag == null)
            {
                return new NotFoundResult();
            }

            var permission = await Auth.AuthorizeAsync(user, vertrag, Operations.Read);
            if (!permission.Succeeded)
            {
                return new ForbidResult();
            }

            var entity = await Ctx.Abrechnungsresultate
                .Include(r => r.Buchungssatz)
                    .ThenInclude(s => s.Buchungszeilen)
                        .ThenInclude(z => z.Buchungskonto)
                .Include(r => r.Vertrag)
                    .ThenInclude(v => v.ZahlungsKonto)
                .Include(r => r.Vertrag)
                    .ThenInclude(v => v.NkBuchungskonto)
                        .ThenInclude(k => k.Buchungszeilen)
                            .ThenInclude(z => z.Buchungssatz)
                .Include(r => r.Vertrag)
                    .ThenInclude(v => v.Wohnung)
                        .ThenInclude(w => w.Adresse)
                .Where(r => r.Vertrag.VertragId == VertragId &&
                            r.Buchungssatz != null &&
                            r.Buchungssatz.Buchungsjahr == jahr)
                .SingleOrDefaultAsync();

            if (entity == null)
            {
                return new NotFoundResult();
            }

            var permissions = await GetPermissions(user, entity, Auth);
            var entry = new AbrechnungsresultatEntry(entity, permissions);

            return entry;
        }



        public override async Task<ActionResult<AbrechnungsresultatEntry>> Get(ClaimsPrincipal user, Guid id)
        {
            return await HandleEntity(user, id, Operations.Read, async (entity) =>
            {
                var permissions = await GetPermissions(user, entity, Auth);
                var entry = new AbrechnungsresultatEntry(entity, permissions);

                return entry;
            });
        }

        public override async Task<ActionResult> Delete(ClaimsPrincipal user, Guid id)
        {
            return await HandleEntity(user, id, Operations.Delete, async (entity) =>
            {
                if (entity.Abgesendet)
                    return new ConflictObjectResult("Abgesendete Abrechnungen können nicht gelöscht werden.");

                Ctx.Abrechnungsresultate.Remove(entity);
                await Ctx.SaveChangesAsync();

                return new OkResult();
            });
        }

        public override async Task<ActionResult<AbrechnungsresultatEntry>> Post(
            ClaimsPrincipal user,
            AbrechnungsresultatEntry entry)
        {
            return await Task.Run(() => new BadRequestResult());
        }

        public override async Task<ActionResult<AbrechnungsresultatEntry>> Put(ClaimsPrincipal user, Guid id, AbrechnungsresultatEntry entry)
        {
            var resultat = await Ctx.Abrechnungsresultate.FindAsync(id);
            if (resultat == null)
            {
                return new NotFoundResult();
            }

            if (resultat.Abgesendet)
                return new ConflictObjectResult("Abgesendete Abrechnungen können nicht mehr geändert werden. Bitte zuerst stornieren.");

            var authRx = await Auth.AuthorizeAsync(user, resultat.Vertrag, Operations.Read);
            if (!authRx.Succeeded)
            {
                return new ForbidResult();
            }

            var permissions = new Permissions
            {
                Read = true,
                Update = true,
                Remove = true
            };

            resultat.Abgesendet = entry.Abgesendet;
            resultat.Notiz = entry.Notiz;
            Ctx.Abrechnungsresultate.Update(resultat);
            await Ctx.SaveChangesAsync();
            return new AbrechnungsresultatEntry(resultat, permissions);
        }
    }
}
