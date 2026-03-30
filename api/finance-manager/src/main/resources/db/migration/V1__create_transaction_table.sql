CREATE EXTENSION IF NOT EXISTS "pgcrypto";

CREATE TABLE transactions (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    date TIMESTAMP NOT NULL,
    title VARCHAR(100) NOT NULL,
    amount DECIMAL NOT NULL,
    updated_at TIMESTAMP,
    created_at TIMESTAMP NOT NULL
);