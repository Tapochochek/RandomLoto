CREATE TABLE IF NOT EXISTS generation_requests (
    id SERIAL PRIMARY KEY,
    min_value NUMERIC NOT NULL,
    max_value NUMERIC NOT NULL,
    count INT NOT NULL,
    method VARCHAR(50),
    created_at TIMESTAMP DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS generated_values (
    id SERIAL PRIMARY KEY,
    generation_id INT REFERENCES generation_requests(id),
    value NUMERIC,
    position INT,
    created_at TIMESTAMP DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS statistics_cache (
    id SERIAL PRIMARY KEY,
    generation_id INT REFERENCES generation_requests(id),
    mean_value FLOAT,
    std_dev FLOAT,
    distribution JSONB,
    created_at TIMESTAMP DEFAULT NOW()
);
