# API request validation

The API validates all user-controlled route, query, body, and multipart form inputs before a controller action runs. Validation rules live in the dedicated `api/Validators` layer and use FluentValidation.

## Architecture

`Program.cs` enables FluentValidation's automatic ASP.NET Core integration and registers every validator in the assembly containing `ValidatorAssemblyMarker`. Binding errors and FluentValidation failures are added to MVC's model state and returned through `ValidationProblemDetailsFactory`.

Route IDs and standalone query flags use request models from `api/Requests/Common`:

- `RouteIdRequest` binds an endpoint's `{id}` value as a `Guid` and rejects an empty GUID.
- `WithDeletedRequest` binds `withDeleted` as a boolean. ASP.NET Core rejects values that cannot be converted to `true` or `false`.

Controllers may normalize already-valid values, such as trimming search text or lowercasing sort order. Database-dependent rules such as record existence and uniqueness remain in services and continue to produce their existing `404` or `409` responses.

## Rules

| Request area | Rules |
| --- | --- |
| Pagination | `page` is at least 1, `limit` is between 1 and 100, and `order` is `asc` or `desc` (case-insensitive and whitespace-tolerant). |
| Transaction filters | Search is at most 200 characters; an optional person ID cannot be empty; `personId` and `unassigned` cannot be combined; start date cannot be after end date. |
| Dashboard filters | Pagination rules apply; an optional person ID cannot be empty; start date cannot be after end date. |
| Import filters | Pagination rules apply; search is at most 200 characters; status is `Submitted`, `Processing`, `Finished`, or `Failed`. |
| Person create/update | Name is nonblank and at most 120 characters; email is nonblank, valid, and at most 254 characters; phone number is nonblank and at most 32 characters. |
| Transaction create/update | Date is not the default value; title is nonblank and at most 200 characters; amount is nonzero. Positive and negative amounts are accepted. |
| Assignment create/update | `personId` and `transactionId` are non-empty GUIDs. |
| Transaction upload | File is present, non-empty, has a `.csv` extension, and is no larger than 10 MB; category is `CreditCard` or `Extrato`. MIME type is not used as an authority. |
| Entity routes and deleted records | Route IDs are valid non-empty GUIDs; `withDeleted`, when supplied, is a valid boolean. |

Transaction dates may be in the past or future. Phone numbers intentionally have no locale-specific format rule.

## Error response

Validation failures return HTTP `400` with content type `application/problem+json`. The `errors` object groups one or more messages by input field.

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "instance": "/person",
  "errors": {
    "Email": [
      "'Email' is not a valid email address."
    ]
  }
}
```

Malformed route GUIDs and malformed boolean, date, enum, or numeric query values use the same response contract. Before this validation layer, some malformed route IDs reached `Guid.Parse` and produced a `500`, while invalid `withDeleted` values were treated as `false`. They now return `400`.

The web client reads the first field-level error from `errors`, then falls back to `detail`, `title`, and the API's older top-level error properties.

## Adding validation for a request

1. Add or update the request model under `api/Requests` and use typed properties wherever model binding can validate the wire value.
2. Add an `AbstractValidator<TRequest>` under the matching `api/Validators` domain directory.
3. Keep syntactic and cross-field request rules in the validator. Keep database lookups, uniqueness, permissions, and other service-level domain checks out of the validator.
4. Accept the request model from the appropriate controller source with `[FromRoute]`, `[FromQuery]`, `[FromBody]`, or `[FromForm]`. Automatic registration discovers the validator; no per-validator `Program.cs` change is required.
5. Add boundary and invalid-input coverage under `api.Tests/Validators`. For changes to the HTTP contract, also add a case to the isolated validation pipeline tests.

Run the validation suite with:

```shell
dotnet test api.Tests/api.Tests.csproj
```

Build both applications with:

```shell
dotnet build api/api.csproj
npm --prefix client run build
```
