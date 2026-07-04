# Clean docker compose data
```bash
docker compose down --volumes
```

# Apply migrations
```bash
cd api
dotnet ef database update
```