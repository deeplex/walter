﻿using System.Security.Claims;
using Deeplex.Saverwalter.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.VertragVersionController;

namespace Deeplex.Saverwalter.WebAPI.Services.ControllerService
{
    public class VertragVersionDbService : WalterDbServiceBase<VertragVersionEntry, VertragVersion>
    {
        public VertragVersionDbService(SaverwalterContext ctx, IAuthorizationService authorizationService) : base(ctx, authorizationService)
        {
        }

        public override async Task<ActionResult<VertragVersion>> GetEntity(ClaimsPrincipal user, int id, OperationAuthorizationRequirement op)
        {
            var entity = await Ctx.VertragVersionen.FindAsync(id);
            return await GetEntity(user, entity, op);
        }

        public override async Task<ActionResult<VertragVersionEntry>> Get(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, async (entity) =>
            {
                var permissions = await Utils.GetPermissions(user, entity, Auth);
                var entry = new VertragVersionEntry(entity, permissions);
                return entry;
            });
        }

        public override async Task<ActionResult> Delete(ClaimsPrincipal user, int id)
        {
            return await HandleEntity(user, id, async (entity) =>
            {
                Ctx.VertragVersionen.Remove(entity);
                await Ctx.SaveChangesAsync();

                return new OkResult();
            });
        }

        public override async Task<ActionResult<VertragVersionEntry>> Post(ClaimsPrincipal user, VertragVersionEntry entry)
        {
            if (entry.Id != 0)
            {
                return new BadRequestResult();
            }

            try
            {
                var vertrag = await Ctx.Vertraege.FindAsync(entry.Vertrag!.Id);
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

        private async Task<VertragVersionEntry> Add(VertragVersionEntry entry)
        {
            var vertrag = await Ctx.Vertraege.FindAsync(entry.Vertrag!.Id);
            var entity = new VertragVersion(entry.Beginn, entry.Grundmiete, entry.Personenzahl)
            {
                Vertrag = vertrag!
            };

            SetOptionalValues(entity, entry);
            Ctx.VertragVersionen.Add(entity);
            Ctx.SaveChanges();

            return new VertragVersionEntry(entity, entry.Permissions);
        }

        public override async Task<ActionResult<VertragVersionEntry>> Put(ClaimsPrincipal user, int id, VertragVersionEntry entry)
        {
            return await HandleEntity(user, id, async (entity) =>
            {
                entity.Beginn = entry.Beginn;
                entity.Grundmiete = entry.Grundmiete;
                entity.Personenzahl = entry.Personenzahl;

                SetOptionalValues(entity, entry);
                Ctx.VertragVersionen.Update(entity);
                Ctx.SaveChanges();

                return new VertragVersionEntry(entity, entry.Permissions);
            });
        }

        private static void SetOptionalValues(VertragVersion entity, VertragVersionEntry entry)
        {
            if (entity.VertragVersionId != entry.Id)
            {
                throw new Exception();
            }

            entity.Notiz = entry.Notiz;
        }
    }
}
