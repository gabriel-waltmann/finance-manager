# Finance Manager documentation

## Basic fundamentals

- [How the system communicates](./basic-fundamentals/README.md): DNS, HTTP/HTTPS, firewalls, Nginx, the browser, the Vue frontend, the ASP.NET backend, and the complete local and production request flows.

## Feature implementation

- [Person](./features/people.md): person storage, CRUD endpoints, validation, soft deletion, and transaction assignment.
- [Transactions](./features/transactions.md): transaction storage, CRUD endpoints, filtering, duplicate detection, and person data in responses.
- [File processing and import status](./features/file-processing.md): file persistence, RabbitMQ processing, CSV mappings, import statuses, and live client updates.

## Development guides

- [Client data-access pattern](./client/data-access-pattern.md): entity, controller, TanStack Query, Vue view, and API middleware boundaries.
- [Client view and component pattern](./client/view-component-pattern.md): page folders, view controllers, local and global components, and component naming.
- [API request validation](./api/request-validation.md)
- [Create database migrations](./database/create-migrations.md)
- [Install the Entity Framework CLI](./database/install-dotnet-ef.md)
- [Define local secrets](./secrets/define-secrets.md)
