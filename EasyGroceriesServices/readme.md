# Easy Groceries Demo

## Introduction
This project is for demonstration purposes only.

The solution is implemented using a microservices architecture and deployed onto Microsoft Azure.

The Microservices are developed as Azure Functions using C# as a programming language. The .NET version used is v6.

The data store used is Azure CosmosDB SQL API.

## Services
### User Service
This service is resposible to create and update the address of the user.

### Inventory Service
This service is responsible to create a product.

### Basket Service
This service is resposible to manage baskets/shopping carts for the user. It allows the addition and removal of products.
The service also allows you to view your cart summary as well as checkout the same.

The service also contains business rules to calculate discounts if a promotion is added.

### Order Service
This service is responsible to generate an order based on the basket of items and generate a delivery slip.


`Note: The services can be callec via postman. The collection is added to the repository`