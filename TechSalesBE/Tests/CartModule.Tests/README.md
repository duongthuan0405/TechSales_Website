# Cart tests

Run the tests from `TechSalesBE`.

```sh
dotnet test Tests/CartModule.Tests/CartModule.Tests.csproj
```

The unit tests run without a database. To also run the HTTP tests, set
`CART_TEST_POSTGRES` to a connection string for a maintenance database on a
dedicated PostgreSQL test server. The test role needs `CREATEDB`.

```sh
CART_TEST_POSTGRES='Host=127.0.0.1;Database=postgres;Username=cart_test' \
  dotnet test Tests/CartModule.Tests/CartModule.Tests.csproj
```

The fixture creates and drops its own database, starts Auth and Cart on a
loopback address, and signs in synthetic users through the Auth login endpoint.
The HTTP cases verify JWT authorization, request validation and responses,
cart ownership, persisted state, locking, and rollback.

The fixture applies the full Cart migration chain with `MigrateAsync()`.
`20261116100919_InitialCartSchema` owns the Cart tables and indexes.
The duplicate `20261116141740_CREATE_Db` migration has been removed from this checkout.

`CartMigrationTests` generates PostgreSQL SQL without connecting to a database.
It checks fresh installation, upgrading an up-to-date database, and consistency
between the model and snapshot.

The isolated HTTP fixture registers a synthetic Catalog port. Real host adapter
mapping is covered separately by ModuleContracts.Tests.
