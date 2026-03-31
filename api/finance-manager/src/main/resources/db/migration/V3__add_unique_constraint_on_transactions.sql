CREATE UNIQUE INDEX idx_unique_transaction ON transactions(date, title, amount)
WHERE deleted_at IS NULL;