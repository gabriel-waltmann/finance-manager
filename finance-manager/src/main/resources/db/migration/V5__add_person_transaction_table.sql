CREATE TABLE person_transactions (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    person_id UUID NOT NULL,
    CONSTRAINT person_id FOREIGN KEY (person_id) REFERENCES persons(id),
    transaction_id UUID NOT NULL,
    CONSTRAINT transaction_id FOREIGN KEY (transaction_id) REFERENCES transactions(id),
    deleted_at TIMESTAMP,
    updated_at TIMESTAMP,
    created_at TIMESTAMP NOT NULL
)