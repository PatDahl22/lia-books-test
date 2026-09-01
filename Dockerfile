FROM node:20-alpine AS frontend-build
WORKDIR /src/frontend
COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci
COPY frontend/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend-build
WORKDIR /src
COPY global.json ./
COPY backend/BookQuote.Api/BookQuote.Api.csproj backend/BookQuote.Api/
RUN dotnet restore backend/BookQuote.Api/BookQuote.Api.csproj
COPY backend/BookQuote.Api/ backend/BookQuote.Api/
RUN dotnet publish backend/BookQuote.Api/BookQuote.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore
COPY --from=frontend-build /src/frontend/dist/lia-books/browser /app/publish/wwwroot

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
USER root
RUN mkdir -p /data && chown -R app:app /data
COPY --from=backend-build --chown=app:app /app/publish ./
USER app
ENV ASPNETCORE_HTTP_PORTS=10000
ENV ConnectionStrings__DefaultConnection="Data Source=/data/books.db"
EXPOSE 10000
ENTRYPOINT ["dotnet", "BookQuote.Api.dll"]
