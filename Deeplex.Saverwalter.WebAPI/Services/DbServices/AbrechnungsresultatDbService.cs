
using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.AbrechnungsresultatController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class AbrechnungsresultatDbService
    {
        protected IAuthorizationService Auth { get; }
        protected SaverwalterContext Ctx { get; }
        public AbrechnungsresultatDbService(SaverwalterContext dbContext, IAuthorizationService authorizationService)
        {
            Auth = authorizationService;
            Ctx = dbContext;
        }

        public async Task<ActionResult> Delete(ClaimsPrincipal user, Guid id)
        {
            var resultat = await Ctx.Abrechnungsresultate.FindAsync(id);
            if (resultat == null)
            {
                return new NotFoundResult();
            }

            var authRx = await Auth.AuthorizeAsync(user, resultat.Vertrag, Operations.Read);
            if (!authRx.Succeeded)
            {
                return new ForbidResult();
            }

            Ctx.Abrechnungsresultate.Remove(resultat);
            await Ctx.SaveChangesAsync();

            return new OkResult();
        }

        public async Task<ActionResult<AbrechnungsresultatEntry>> Get(ClaimsPrincipal user, Guid id)
        {
            var resultat = await Ctx.Abrechnungsresultate.FindAsync(id);
            if (resultat == null)
            {
                return new NotFoundResult();
            }

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

            var entry = new AbrechnungsresultatEntry(resultat, permissions);

            return entry;

        }

        public async Task<ActionResult<AbrechnungsresultatEntry>> Put(ClaimsPrincipal user, Guid id, AbrechnungsresultatEntry entry)
        {
            var resultat = await Ctx.Abrechnungsresultate.FindAsync(id);
            if (resultat == null)
            {
                return new NotFoundResult();
            }

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

            resultat.Jahr = entry.Jahr;
            resultat.Kaltmiete = entry.Kaltmiete;
            resultat.Vorauszahlung = entry.Vorauszahlung;
            resultat.Rechnungsbetrag = entry.Rechnungsbetrag;
            resultat.Minderung = entry.Minderung;
            resultat.Abgesendet = entry.Abgesendet;
            resultat.Saldo = entry.Saldo;
            resultat.Notiz = entry.Notiz;
            Ctx.Abrechnungsresultate.Update(resultat);
            await Ctx.SaveChangesAsync();
            return new AbrechnungsresultatEntry(resultat, permissions);
        }
    }
}