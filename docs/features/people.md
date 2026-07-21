# People implementation

People represent the individuals who can be assigned to transactions. The implementation is split across the person controllers, `PersonService`, request validators, and the `persons` database table.

## Data model

`PersonModel` maps to the `persons` table and stores:

| Field | Purpose |
| --- | --- |
| `id` | Application-generated GUID. |
| `name` | Display name. |
| `email` | Email address and active-person uniqueness key. |
| `phone_number` | Phone number as entered by the user. |
| `created_at` | UTC creation time. |
| `updated_at` | UTC time of the latest update, when applicable. |
| `deleted_at` | UTC soft-deletion time; `null` means the person is active. |

The database has a partial unique index on `email` where `deleted_at IS NULL`. This allows an email from a soft-deleted person to be used by a new person while preventing two active people from sharing it. The service performs the same active-email check before an insert or update so the API can return a conflict response.

Email comparison and storage currently use the values supplied by the client; the service does not trim or lowercase them.

## API endpoints

| Method | Route | Behavior |
| --- | --- | --- |
| `GET` | `/persons?withDeleted=false` | Lists people. Soft-deleted rows are excluded unless `withDeleted=true`. |
| `GET` | `/person/{id}` | Gets one active person or returns `404`. |
| `POST` | `/person` | Creates a person and returns the model with `201`. An active duplicate email returns `409`. |
| `PUT` | `/person/{id}` | Updates an active person. A missing person returns `404`, and an active duplicate email returns `409`. |
| `DELETE` | `/person/{id}` | Sets `deleted_at`. Deleting a missing or already deleted person is idempotent and returns `200`. |

Create and update requests use the same body shape:

```json
{
  "name": "Maria Silva",
  "email": "maria@example.com",
  "phoneNumber": "+55 11 99999-0000"
}
```

Request validation requires a nonblank name of at most 120 characters, a valid nonblank email of at most 254 characters, and a nonblank phone number of at most 32 characters. Invalid requests use the API's standard `400 application/problem+json` response described in [API request validation](../api/request-validation.md).

## Transaction assignment

People and transactions are connected by `TransactionPersonModel`, stored in `transactions_person`. The assignment API is separate from the person API:

- `POST /transaction-person` creates an assignment from `personId` and `transactionId`.
- `PUT /transaction-person/{id}` changes an assignment.
- `DELETE /transaction-person/{id}` soft-deletes an assignment.
- `GET /transaction-person/{id}` and `GET /transaction-persons` read assignments.

The service verifies that both the person and transaction are active. Only one active assignment is allowed per transaction; this is enforced both in the service and by a partial unique database index on `transaction_id` where `deleted_at IS NULL`. Foreign keys use restricted deletion, which is compatible with the application's soft-delete approach.

Deleting a person does not delete an existing assignment. Normal transaction queries hide the deleted person, while `withDeleted=true` can include deleted related data.

## Code map

- Controllers: `api/Controllers/Person`
- Service: `api/Services/Person/PersonService.cs`
- Model: `api/Models/Person/PersonModel.cs`
- Requests: `api/Requests/Person`
- Validators: `api/Validators/Person`
- Assignments: `api/Controllers/TransactionPerson` and `api/Services/TransactionPerson`
