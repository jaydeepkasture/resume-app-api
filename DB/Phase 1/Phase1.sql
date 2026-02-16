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


=============================Billing====================
/* ============================================================
   SCHEMAS
   ============================================================ */
CREATE SCHEMA IF NOT EXISTS master;
CREATE SCHEMA IF NOT EXISTS billing;

/* ============================================================
   MASTER TABLE
   ============================================================ */
CREATE TABLE IF NOT EXISTS master.master_values
(
    master_value_id BIGSERIAL PRIMARY KEY,
    master_type VARCHAR(50) NOT NULL,
    code VARCHAR(50) NOT NULL,
    display_name VARCHAR(100) NOT NULL,
    description TEXT,
    sort_order INT DEFAULT 0,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT (now() AT TIME ZONE 'UTC'),
    CONSTRAINT uq_master UNIQUE (master_type, code)
    );

/* ============================================================
   MASTER DATA SEED
   ============================================================ */

-- PLAN CODES
INSERT INTO master.master_values (master_type, code, display_name, sort_order)
VALUES
    ('PLAN_CODE', 'FREE', 'Free Plan', 1),
    ('PLAN_CODE', 'PRO', 'Pro Plan', 2)
    ON CONFLICT DO NOTHING;

-- BILLING CYCLE
INSERT INTO master.master_values (master_type, code, display_name)
VALUES
    ('BILLING_CYCLE', 'MONTHLY', 'Monthly')
    ON CONFLICT DO NOTHING;

-- CURRENCY
INSERT INTO master.master_values (master_type, code, display_name)
VALUES
    ('CURRENCY', 'INR', 'Indian Rupee')
    ON CONFLICT DO NOTHING;

-- SUBSCRIPTION STATUS
INSERT INTO master.master_values (master_type, code, display_name)
VALUES
    ('SUBSCRIPTION_STATUS', 'ACTIVE', 'Active'),
    ('SUBSCRIPTION_STATUS', 'CANCELLED', 'Cancelled'),
    ('SUBSCRIPTION_STATUS', 'EXPIRED', 'Expired')
    ON CONFLICT DO NOTHING;

-- SUBSCRIPTION CHANGE TYPE
INSERT INTO master.master_values (master_type, code, display_name)
VALUES
    ('SUBSCRIPTION_CHANGE_TYPE', 'RENEWAL', 'Renewal'),
    ('SUBSCRIPTION_CHANGE_TYPE', 'UPGRADE', 'Upgrade'),
    ('SUBSCRIPTION_CHANGE_TYPE', 'DOWNGRADE', 'Downgrade'),
    ('SUBSCRIPTION_CHANGE_TYPE', 'CANCEL', 'Cancel')
    ON CONFLICT DO NOTHING;

-- PAYMENT STATUS
INSERT INTO master.master_values (master_type, code, display_name)
VALUES
    ('PAYMENT_STATUS', 'PENDING', 'Pending'),
    ('PAYMENT_STATUS', 'SUCCESS', 'Success'),
    ('PAYMENT_STATUS', 'FAILED', 'Failed')
    ON CONFLICT DO NOTHING;

-- PAYMENT PROVIDER
INSERT INTO master.master_values (master_type, code, display_name)
VALUES
    ('PAYMENT_PROVIDER', 'RAZORPAY', 'Razorpay')
    ON CONFLICT DO NOTHING;

/* ============================================================
   SUBSCRIPTION PLANS
   ============================================================ */
CREATE TABLE IF NOT EXISTS billing.subscription_plans
(
    plan_id BIGSERIAL PRIMARY KEY,
    plan_code_id BIGINT NOT NULL,
    plan_name VARCHAR(100) NOT NULL,
    description TEXT,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT (now() AT TIME ZONE 'UTC'),
    CONSTRAINT fk_plan_code
    FOREIGN KEY (plan_code_id)
    REFERENCES master.master_values (master_value_id)
    );

/* ============================================================
   PLAN PRICING
   ============================================================ */
CREATE TABLE IF NOT EXISTS billing.subscription_plan_prices
(
    plan_price_id BIGSERIAL PRIMARY KEY,
    plan_id BIGINT NOT NULL,
    billing_cycle_id BIGINT NOT NULL,
    currency_id BIGINT NOT NULL,
    price NUMERIC(10,2) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT (now() AT TIME ZONE 'UTC'),
    CONSTRAINT fk_price_plan
    FOREIGN KEY (plan_id)
    REFERENCES billing.subscription_plans (plan_id),
    CONSTRAINT fk_price_cycle
    FOREIGN KEY (billing_cycle_id)
    REFERENCES master.master_values (master_value_id),
    CONSTRAINT fk_price_currency
    FOREIGN KEY (currency_id)
    REFERENCES master.master_values (master_value_id)
    );

/* ============================================================
   USER SUBSCRIPTIONS (CURRENT STATE)
   ============================================================ */
CREATE TABLE IF NOT EXISTS billing.user_subscriptions
(
    user_subscription_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id BIGINT NOT NULL,
    global_user_id UUID NOT NULL,
    plan_id BIGINT NOT NULL,
    plan_price_id BIGINT NOT NULL,
    status_id BIGINT NOT NULL,
    start_date TIMESTAMPTZ NOT NULL,
    end_date TIMESTAMPTZ NOT NULL,
    auto_renew BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT (now() AT TIME ZONE 'UTC'),
    updated_at TIMESTAMPTZ,
    CONSTRAINT uq_user_subscription UNIQUE (user_id),
    CONSTRAINT fk_sub_user
    FOREIGN KEY (user_id)
    REFERENCES auth.users (user_id)
    ON DELETE CASCADE,
    CONSTRAINT fk_sub_plan
    FOREIGN KEY (plan_id)
    REFERENCES billing.subscription_plans (plan_id),
    CONSTRAINT fk_sub_price
    FOREIGN KEY (plan_price_id)
    REFERENCES billing.subscription_plan_prices (plan_price_id),
    CONSTRAINT fk_sub_status
    FOREIGN KEY (status_id)
    REFERENCES master.master_values (master_value_id)
    );

/* ============================================================
   SUBSCRIPTION HISTORY
   ============================================================ */
CREATE TABLE IF NOT EXISTS billing.user_subscription_history
(
    history_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_subscription_id UUID NOT NULL,
    old_plan_id BIGINT,
    new_plan_id BIGINT NOT NULL,
    old_plan_price_id BIGINT,
    new_plan_price_id BIGINT NOT NULL,
    change_type_id BIGINT NOT NULL,
    effective_from TIMESTAMPTZ NOT NULL,
    effective_to TIMESTAMPTZ,
    changed_at TIMESTAMPTZ NOT NULL DEFAULT (now() AT TIME ZONE 'UTC'),
    CONSTRAINT fk_hist_subscription
    FOREIGN KEY (user_subscription_id)
    REFERENCES billing.user_subscriptions (user_subscription_id)
    ON DELETE CASCADE,
    CONSTRAINT fk_hist_change_type
    FOREIGN KEY (change_type_id)
    REFERENCES master.master_values (master_value_id)
    );

/* ============================================================
   PAYMENTS (RAZORPAY)
   ============================================================ */
CREATE TABLE IF NOT EXISTS billing.subscription_payments
(
    payment_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_subscription_id UUID,
    payment_provider_id BIGINT NOT NULL,
    payment_status_id BIGINT NOT NULL,
    amount NUMERIC(10,2) NOT NULL,
    currency_id BIGINT NOT NULL,
    provider_payment_id VARCHAR(100),
    provider_order_id VARCHAR(100),
    paid_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT (now() AT TIME ZONE 'UTC'),
    CONSTRAINT fk_payment_subscription
    FOREIGN KEY (user_subscription_id)
    REFERENCES billing.user_subscriptions (user_subscription_id),
    CONSTRAINT fk_payment_provider
    FOREIGN KEY (payment_provider_id)
    REFERENCES master.master_values (master_value_id),
    CONSTRAINT fk_payment_status
    FOREIGN KEY (payment_status_id)
    REFERENCES master.master_values (master_value_id),
    CONSTRAINT fk_payment_currency
    FOREIGN KEY (currency_id)
    REFERENCES master.master_values (master_value_id)
    );

/* ============================================================
   INSERT FREE & PRO PLANS
   ============================================================ */

-- FREE PLAN
INSERT INTO billing.subscription_plans (plan_code_id, plan_name, description)
SELECT master_value_id, 'Free Plan', 'Free plan with limited features'
FROM master.master_values
WHERE master_type = 'PLAN_CODE' AND code = 'FREE'
    ON CONFLICT DO NOTHING;

-- PRO PLAN
INSERT INTO billing.subscription_plans (plan_code_id, plan_name, description)
SELECT master_value_id, 'Pro Plan', 'Pro plan with full access'
FROM master.master_values
WHERE master_type = 'PLAN_CODE' AND code = 'PRO'
    ON CONFLICT DO NOTHING;

/* ============================================================
   PLAN PRICES
   ============================================================ */

-- FREE PLAN PRICE = ₹0
INSERT INTO billing.subscription_plan_prices
(plan_id, billing_cycle_id, currency_id, price)
SELECT
    sp.plan_id,
    bc.master_value_id,
    cur.master_value_id,
    0
FROM billing.subscription_plans sp
         JOIN master.master_values pc ON sp.plan_code_id = pc.master_value_id
         JOIN master.master_values bc ON bc.master_type = 'BILLING_CYCLE' AND bc.code = 'MONTHLY'
         JOIN master.master_values cur ON cur.master_type = 'CURRENCY' AND cur.code = 'INR'
WHERE pc.code = 'FREE'
    ON CONFLICT DO NOTHING;

-- PRO PLAN PRICE = ₹199
INSERT INTO billing.subscription_plan_prices
(plan_id, billing_cycle_id, currency_id, price)
SELECT
    sp.plan_id,
    bc.master_value_id,
    cur.master_value_id,
    199
FROM billing.subscription_plans sp
         JOIN master.master_values pc ON sp.plan_code_id = pc.master_value_id
         JOIN master.master_values bc ON bc.master_type = 'BILLING_CYCLE' AND bc.code = 'MONTHLY'
         JOIN master.master_values cur ON cur.master_type = 'CURRENCY' AND cur.code = 'INR'
WHERE pc.code = 'PRO'
    ON CONFLICT DO NOTHING;

/* ============================================================
   END OF SCRIPT
   ============================================================ */

===================Plan======================
/* ============================================================
   PLAN BENEFITS (FEATURE MASTER)
   ============================================================ */
CREATE TABLE IF NOT EXISTS billing.plan_benefits
(
    benefit_id BIGSERIAL PRIMARY KEY,

    benefit_code VARCHAR(50) NOT NULL UNIQUE,
    benefit_name VARCHAR(100) NOT NULL,
    description TEXT,

    value_type VARCHAR(20) NOT NULL,
    -- NUMBER

    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT (now() AT TIME ZONE 'UTC')
    );

/* ============================================================
   PLAN ↔ BENEFIT MAPPING
   ============================================================ */
CREATE TABLE IF NOT EXISTS billing.plan_benefit_map
(
    plan_benefit_map_id BIGSERIAL PRIMARY KEY,

    plan_id BIGINT NOT NULL,
    benefit_id BIGINT NOT NULL,

    benefit_value VARCHAR(50) NOT NULL,

    created_at TIMESTAMPTZ NOT NULL DEFAULT (now() AT TIME ZONE 'UTC'),

    CONSTRAINT uq_plan_benefit UNIQUE (plan_id, benefit_id),

    CONSTRAINT fk_pb_plan
    FOREIGN KEY (plan_id)
    REFERENCES billing.subscription_plans (plan_id)
    ON DELETE CASCADE,

    CONSTRAINT fk_pb_benefit
    FOREIGN KEY (benefit_id)
    REFERENCES billing.plan_benefits (benefit_id)
    ON DELETE CASCADE
    );

/* ============================================================
   DEFINE BENEFITS
   ============================================================ */
INSERT INTO billing.plan_benefits
(benefit_code, benefit_name, description, value_type, is_active)
VALUES
    ('TEMPLATE_LIMIT', 'Template Limit', 'Number of templates user can access', 'NUMBER', TRUE),
    ('RATE_LIMIT_PER_MINUTE', 'Rate Limit Per Minute', 'Max API or action requests per minute', 'NUMBER', TRUE)
    ON CONFLICT DO NOTHING;

/* ============================================================
   MAP BENEFITS → FREE PLAN
   ============================================================ */
INSERT INTO billing.plan_benefit_map
(plan_id, benefit_id, benefit_value)
SELECT
    sp.plan_id,
    b.benefit_id,
    v.benefit_value
FROM billing.subscription_plans sp
         JOIN master.master_values pc
              ON pc.master_value_id = sp.plan_code_id
         JOIN (
    VALUES
        ('TEMPLATE_LIMIT', '3'),
        ('RATE_LIMIT_PER_MINUTE', '30')
) AS v(benefit_code, benefit_value)
              ON TRUE
         JOIN billing.plan_benefits b
              ON b.benefit_code = v.benefit_code
WHERE pc.master_type = 'PLAN_CODE'
  AND pc.code = 'FREE'
    ON CONFLICT DO NOTHING;

/* ============================================================
   MAP BENEFITS → PRO PLAN
   ============================================================ */
INSERT INTO billing.plan_benefit_map
(plan_id, benefit_id, benefit_value)
SELECT
    sp.plan_id,
    b.benefit_id,
    v.benefit_value
FROM billing.subscription_plans sp
         JOIN master.master_values pc
              ON pc.master_value_id = sp.plan_code_id
         JOIN (
    VALUES
        ('TEMPLATE_LIMIT', '50'),
        ('RATE_LIMIT_PER_MINUTE', '300')
) AS v(benefit_code, benefit_value)
              ON TRUE
         JOIN billing.plan_benefits b
              ON b.benefit_code = v.benefit_code
WHERE pc.master_type = 'PLAN_CODE'
  AND pc.code = 'PRO'
    ON CONFLICT DO NOTHING;

/* ============================================================
   END OF SCRIPT
   ============================================================ */
-- Add reset_token and reset_token_expiry_time
ALTER TABLE auth.users ADD COLUMN IF NOT EXISTS reset_token VARCHAR(255);
ALTER TABLE auth.users ADD COLUMN IF NOT EXISTS reset_token_expiry_time TIMESTAMPTZ;