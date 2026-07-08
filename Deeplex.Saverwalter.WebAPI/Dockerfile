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
COPY Deeplex.Saverwalter.sln ./
COPY Deeplex.Saverwalter.BetriebskostenabrechnungService/*.csproj Deeplex.Saverwalter.BetriebskostenabrechnungService/
COPY Deeplex.Saverwalter.BetriebskostenabrechnungService.Tests/*.csproj Deeplex.Saverwalter.BetriebskostenabrechnungService.Tests/
COPY Deeplex.Saverwalter.CLI/*.csproj Deeplex.Saverwalter.CLI/
COPY Deeplex.Saverwalter.ErhaltungsaufwendungService/*.csproj Deeplex.Saverwalter.ErhaltungsaufwendungService/
COPY Deeplex.Saverwalter.ErhaltungsaufwendungService.Tests/*.csproj Deeplex.Saverwalter.ErhaltungsaufwendungService.Tests/
COPY Deeplex.Saverwalter.InitiateTestDbs/*.csproj Deeplex.Saverwalter.InitiateTestDbs/
COPY Deeplex.Saverwalter.Model/*.csproj Deeplex.Saverwalter.Model/
COPY Deeplex.Saverwalter.Model.Tests/*.csproj Deeplex.Saverwalter.Model.Tests/
COPY Deeplex.Saverwalter.PrintService/*.csproj Deeplex.Saverwalter.PrintService/
COPY Deeplex.Saverwalter.PrintService.Tests/*.csproj Deeplex.Saverwalter.PrintService.Tests/
COPY Deeplex.Saverwalter.WebAPI/*.csproj Deeplex.Saverwalter.WebAPI/
COPY Deeplex.Saverwalter.WebAPI.Tests/*.csproj Deeplex.Saverwalter.WebAPI.Tests/

RUN dotnet restore Deeplex.Saverwalter.sln

COPY . .
COPY --from=frontend-build /src/Deeplex.Saverwalter.WebAPI/wwwroot ./Deeplex.Saverwalter.WebAPI/wwwroot

RUN dotnet publish Deeplex.Saverwalter.WebAPI/Deeplex.Saverwalter.WebAPI.csproj --configuration Release --no-restore --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["./Deeplex.Saverwalter.WebAPI"]
