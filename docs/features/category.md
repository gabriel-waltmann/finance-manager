# Category implementation

A category classifies transactions. Categories are managed independently and transactions may have any number of active category assignments.

## Data model

`CategoryModel` maps to `categories` and stores an application-generated GUID, a required title, an optional description, and creation, update, and soft-deletion timestamps. Titles are unique among active categories. The API trims titles and descriptions and stores a blank description as `null`.

`TransactionCategoryModel` maps to `transactions_category`. It contains GUID foreign keys to a transaction and category plus the standard timestamps. A partial unique index on `(transaction_id, category_id)` prevents duplicate active links while allowing multiple different categories on one transaction. Foreign keys use restricted deletion.

Soft-deleting a category does not delete its relationship rows. Normal transaction responses retain those `transactionCategories` links but omit the deleted category from the visible `categories` array.

## API endpoints

| Method | Route | Behavior |
| --- | --- | --- |
| `GET` | `/categories` | Lists categories with search, title ordering, optional pagination, and `withDeleted`. |
| `GET` | `/category/{id}` | Gets one active category. |
| `POST` | `/category` | Creates a category. |
| `PUT` | `/category/{id}` | Updates an active category. |
| `DELETE` | `/category/{id}` | Soft-deletes a category idempotently. |
| `PUT` | `/transaction/{id}/categories` | Atomically replaces the transaction's visible category set. |

Category create and update bodies use:

```json
{
  "title": "Housing",
  "description": "Rent, utilities, and household maintenance"
}
```

Titles are required with a maximum length of 120 characters. Descriptions are optional with a maximum length of 500 characters. Duplicate active titles return `409`.

The category-set endpoint accepts distinct, non-empty category IDs:

```json
{
  "categoryIds": ["...", "..."]
}
```

An empty array clears all visible categories. The service validates the active transaction and every requested active category before applying changes in one save. Links to deleted categories are preserved.
