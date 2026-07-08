FROM node:20-bookworm-slim AS frontend-build

WORKDIR /src/Deeplex.Saverwalter.WebAPI/svelte

COPY Deeplex.Saverwalter.WebAPI/svelte/package.json ./package.json
COPY Deeplex.Saverwalter.WebAPI/svelte/yarn.lock ./yarn.lock
COPY Deeplex.Saverwalter.WebAPI/svelte/.yarnrc.yml ./.yarnrc.yml
COPY Deeplex.Saverwalter.WebAPI/svelte/.yarn ./.yarn

RUN corepack enable && yarn install --immutable

COPY Deeplex.Saverwalter.WebAPI/svelte ./

RUN yarn build

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS publish

WORKDIR /src

COPY Directory.Packages.props ./
COPY Deeplex.Saverwalter.BetriebskostenabrechnungService/*.csproj Deeplex.Saverwalter.BetriebskostenabrechnungService/
COPY Deeplex.Saverwalter.Model/*.csproj Deeplex.Saverwalter.Model/
COPY Deeplex.Saverwalter.PrintService/*.csproj Deeplex.Saverwalter.PrintService/
COPY Deeplex.Saverwalter.WebAPI/*.csproj Deeplex.Saverwalter.WebAPI/

RUN dotnet restore Deeplex.Saverwalter.WebAPI/Deeplex.Saverwalter.WebAPI.csproj

COPY . .
COPY --from=frontend-build /src/Deeplex.Saverwalter.WebAPI/wwwroot ./Deeplex.Saverwalter.WebAPI/wwwroot

RUN dotnet publish Deeplex.Saverwalter.WebAPI/Deeplex.Saverwalter.WebAPI.csproj --configuration Release --no-restore --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["./Deeplex.Saverwalter.WebAPI"]
