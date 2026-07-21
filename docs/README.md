# Finance Manager documentation

## Feature implementation

- [People](./features/people.md): person storage, CRUD endpoints, validation, soft deletion, and transaction assignment.
- [Transactions](./features/transactions.md): transaction storage, CRUD endpoints, filtering, duplicate detection, and person data in responses.
- [File processing and import status](./features/file-processing.md): file persistence, RabbitMQ processing, CSV mappings, import statuses, and live client updates.

## Development guides

- [API request validation](./api/request-validation.md)
- [Create database migrations](./database/create-migrations.md)
- [Install the Entity Framework CLI](./database/install-dotnet-ef.md)
- [Define local secrets](./secrets/define-secrets.md)
