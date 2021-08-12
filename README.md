# Overview
This exercise is intentionally left open ended.  Within you will find a skeleton code base and a json file intended to simulate a database.

# Requirements
 - API listing all customers
 - API retrieving a customer
 - API adding a customer
 - API updating a customer
 - Unit tests
 - Use .NET Core 3.1 or NET 5+

# Expectations
Implement the above listed requirements in a manner you see fitting.  Demonstrate design and implementation aspects you feel are important in a software project.

# Publish
Publish your implementation under your own github account.

---

# Getting started
### Option 1 (running with docker)
Prerequisites:
- [Docker](https://docs.docker.com/get-docker/)

```bash
cd grocery-store-api/src/GroceryStoreApi
docker bulid -t grocery-store-api .
docker run -dp 80:80 grocery-store-api
```

A GET request for the customers can be made at `http://localhost/api/v1/customers`


### Option 2 (running with dotnet)
Prerequisites:
- [.NET 5 (at least the ASP.NET Core Runtime)](https://dotnet.microsoft.com/download/dotnet/5.0)

```bash
cd grocery-store-api/src/GroceryStoreApi
dotnet run
```

The API should now be running. You can use the Swagger page to test the endpoints or access them via Postman or another API service.

The Swagger page is available at `https://localhost:5001/swagger/index.html`

A GET request for the customers can be made at `https://localhost:5001/api/v1/customers`