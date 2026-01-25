CREATE
EXTENSION IF NOT EXISTS "pgcrypto";
CREATE SCHEMA IF NOT EXISTS "auth";
CREATE TABLE auth.users
(
    user_id        BIGSERIAL PRIMARY KEY,                                  -- internal PK
    global_user_id UUID         NOT NULL UNIQUE DEFAULT gen_random_uuid(), -- global ID

    email          VARCHAR(255) NOT NULL UNIQUE,

    password_hash  BYTEA        NOT NULL,
    password_salt  BYTEA        NOT NULL,

    is_active      BOOLEAN      NOT NULL        DEFAULT TRUE,

    created_at     TIMESTAMPTZ  NOT NULL        DEFAULT (NOW() AT TIME ZONE 'UTC'),
    updated_at     TIMESTAMPTZ
);

CREATE TABLE auth.user_profiles
(
    user_profile_id      BIGSERIAL PRIMARY KEY,
    user_id              BIGINT      NOT NULL, -- FK to users.id
    global_user_profile_ UUID        NOT NULL, -- optional (denormalized)

    first_name           VARCHAR(100),
    last_name            VARCHAR(100),
    phone                VARCHAR(10),
    country_code         VARCHAR(3),

    created_at           TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),

    CONSTRAINT fk_profile_user FOREIGN KEY (user_id) REFERENCES auth.users (user_id) ON DELETE CASCADE
);
