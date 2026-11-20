FROM mcr.microsoft.com/dotnet/sdk:8.0 AS b
WORKDIR /s
COPY . .
RUN dotnet publish Padma.Api -c Release -o /o
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /a
COPY --from=b /o .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet","Padma.Api.dll"]
