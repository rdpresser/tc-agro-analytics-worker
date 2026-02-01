// Test connection to Supabase
using Npgsql;

var connectionString = "Host=db.sodwyfyhthybyqlqhdqy.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=!Fiap@2026#;SSL Mode=Require;Trust Server Certificate=false";

try
{
    using var connection = new NpgsqlConnection(connectionString);
    await connection.OpenAsync();
    
    Console.WriteLine("✅ Connection successful!");
    
    // Test schema
    using var cmd = new NpgsqlCommand("SELECT schema_name FROM information_schema.schemata WHERE schema_name = 'analytics'", connection);
    var result = await cmd.ExecuteScalarAsync();
    
    if (result != null)
    {
        Console.WriteLine($"✅ Schema 'analytics' exists!");
    }
    else
    {
        Console.WriteLine("❌ Schema 'analytics' NOT found! Run CREATE SCHEMA first.");
    }
    
    await connection.CloseAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Connection failed: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}
