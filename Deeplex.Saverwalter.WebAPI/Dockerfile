FROM mcr.microsoft.com/dotnet/aspnet:8.0

COPY ./bin /app/bin
COPY ./wwwroot /app/wwwroot

RUN ln -s /app/bin/release/net8.0/Deeplex.Saverwalter.WebAPI /usr/bin/walter
RUN chmod +x /usr/bin/walter

WORKDIR /app
CMD ["walter"]
