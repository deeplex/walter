FROM mcr.microsoft.com/dotnet/aspnet:6.0

COPY ./bin /app/bin
COPY ./wwwroot /app/wwwroot

RUN ln -s /app/bin/release/net6.0/Deeplex.Saverwalter.WebAPI /bin/walter

WORKDIR /app
