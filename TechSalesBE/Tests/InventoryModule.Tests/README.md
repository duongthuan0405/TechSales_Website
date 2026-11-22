# Inventory tests

Run these commands from `TechSalesBE`.

```sh
dotnet test Tests/InventoryModule.Tests/InventoryModule.Tests.csproj
```

Use a dedicated PostgreSQL test server and a role with `CREATEDB` to enable the HTTP integration suite. Set `INVENTORY_TEST_POSTGRES` to a connection string for an existing maintenance database on that server.

```sh
export INVENTORY_TEST_POSTGRES='Host=127.0.0.1;Database=postgres;Username=inventory_test'
dotnet test Tests/InventoryModule.Tests/InventoryModule.Tests.csproj
```

The fixture creates two databases with unique names, starts Auth and Inventory on a loopback Kestrel port, and signs in synthetic accounts through the login endpoint. It drops its own databases when the suite finishes. The integration tests cover routing, authorization, validation, PostgreSQL locking and rollback. Without the connection setting, the integration tests are reported as skipped.
