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

The fixture applies `20261116100919_InitialCartSchema` to provision Cart.
The later `20261116141740_CREATE_Db` migration on this branch contains the
same table creation statements. The suite therefore verifies API behavior
against the initial schema, without running the full Cart migration chain.
