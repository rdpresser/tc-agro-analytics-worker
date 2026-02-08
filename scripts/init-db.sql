-- TC Agro Analytics Database Initialization Script
-- PostgreSQL 16

-- Create database if not exists
SELECT 'CREATE DATABASE tc_agro_analytics'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'tc_agro_analytics')\gexec

-- Connect to the database
\c tc_agro_analytics

-- Create extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm"; -- For text search optimization

-- Create schema (if using custom schema)
CREATE SCHEMA IF NOT EXISTS public;

-- Grant permissions
GRANT ALL PRIVILEGES ON DATABASE tc_agro_analytics TO postgres;
GRANT ALL PRIVILEGES ON SCHEMA public TO postgres;

-- Comments
COMMENT ON DATABASE tc_agro_analytics IS 'TC Agro Analytics Service Database';

-- Display success message
DO $$
BEGIN
  RAISE NOTICE '✅ TC Agro Analytics Database initialized successfully!';
  RAISE NOTICE '📊 Database: tc_agro_analytics';
  RAISE NOTICE '🔧 Extensions: uuid-ossp, pg_trgm';
END $$;
