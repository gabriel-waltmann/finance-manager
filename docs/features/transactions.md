# Transactions implementation

A transaction is a dated financial entry with a title and a signed decimal amount. Transactions can be created through the CRUD API or by the asynchronous CSV import described in [File processing and import status](./file-processing.md).

## Data model

`TransactionModel` maps to the `transactions` table and stores:

| Field | Purpose |
| --- | --- |
| `id` | Application-generated GUID. |
| `date` | Date and time supplied for the financial entry. |
| `title` | Transaction description. |
| `amount` | Signed decimal amount. Positive and negative values are valid; zero is rejected. |
| `created_at` | UTC creation time. |
| `updated_at` | UTC time of the latest update, when applicable. |
| `deleted_at` | UTC soft-deletion time; `null` means the transaction is active. |

The API treats an active row with the same `date`, `title`, and `amount` as a duplicate. `TransactionService.Create` throws `ExistsTransactionException`, and the create controller returns `409`. Imported duplicates are skipped instead of failing the entire file.

## API endpoints

| Method | Route | Behavior |
| --- | --- | --- |
| `GET` | `/transactions` | Returns a filtered, ordered, paginated list with person-assignment data. |
| `GET` | `/transaction/{id}` | Returns one active transaction with its active assignment and person, or `404`. |
| `POST` | `/transaction` | Creates a transaction and returns it with `201`; an active duplicate returns `409`. |
| `PUT` | `/transaction/{id}` | Replaces `date`, `title`, and `amount`, and sets `updated_at`. |
| `DELETE` | `/transaction/{id}` | Sets `deleted_at` and returns `200`. A missing record also returns `200`. |
| `POST` | `/transaction/upload` | Accepts a CSV for asynchronous import. See [file processing](./file-processing.md). |

Create and update requests use the same shape:

```json
{
  "date": "2026-07-21T00:00:00Z",
  "title": "Electricity bill",
  "amount": -125.40
}
```

The date must not be the default .NET value, the title must be nonblank and at most 200 characters, and the amount must not be zero.

## Listing and filtering

`GET /transactions` supports:

| Query parameter | Meaning |
| --- | --- |
| `search` | Case-insensitive substring search over transaction titles and assigned person names. SQL wildcard characters are escaped and treated literally. |
| `startDate` | Inclusive lower date boundary. |
| `endDate` | Inclusive calendar-day upper boundary, implemented as less than the next day. |
| `personId` | Only transactions actively assigned to this person. |
| `unassigned` | Only transactions without an active assignment. It cannot be combined with `personId`. |
| `page` | One-based page number; default `1`. |
| `limit` | Page size from 1 to 100; default `20`. |
| `order` | `asc` or `desc` by transaction date, then creation time; default `desc`. |
| `withDeleted` | Includes soft-deleted transactions and related records when `true`. |

The service counts the filtered query before applying `Skip` and `Take`, so the response contains `page`, `limit`, `total`, and `totalPages` alongside `transactions`.

Each list item and the single-transaction endpoint return this structure:

```json
{
  "transaction": {
    "id": "...",
    "date": "2026-07-21T00:00:00Z",
    "title": "Electricity bill",
    "amount": -125.40,
    "created_at": "...",
    "updated_at": null,
    "deleted_at": null
  },
  "transactionPerson": null,
  "person": null
}
```

When an assignment exists, `transactionPerson` contains the link and `person` contains the related person. The list implementation loads the page of transactions first, then fetches assignments and people for only those transaction IDs.

## Soft deletion and current behavior

Reads exclude soft-deleted rows by default, and creation only considers active rows when checking for duplicates. Deletion is logical rather than physical, preserving history and import links.

The current update implementation looks up a transaction directly by primary key. Consequently, it can update a soft-deleted transaction, and a missing ID is currently handled by the controller's generic `500` path rather than a `404`. This describes the current behavior and is not a recommended API contract.

## Code map

- Controllers: `api/Controllers/Transaction`
- Service: `api/Services/Transaction/TransactionService.cs`
- Model: `api/Models/Transaction/TransactionModel.cs`
- Requests and list filters: `api/Requests/Transaction`
- Validators: `api/Validators/Transaction`
- Responses: `api/Responses/Transaction`
