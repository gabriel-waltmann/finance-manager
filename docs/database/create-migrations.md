# Create migrations
```bash
# Create migration
dotnet ef migrations add MigrationName

# Apply migrations - development
dotnet ef database update

# Apply migrations - production 
dotnet ef migrations script --idempotent -o migrations.sql
```
