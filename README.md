# Redis.SQL

Redis.SQL is a project aiming to provide an interface for developers to work with redis in the similar manner to relational database systems. Redis is a powerful in-memory database which can be used for quick retrieval of data, however, its NoSQL nature can cause developers to be confused on the issue of how to store their data in Redis, here Redis.SQL comes into play where it abstracts this problem by providing familiar interfaces for developers to manage their Redis datastore.

Redis.SQL is built over .NET Core and uses StackExchange's Redis client for communicating with the Redis datastore. (More on: https://github.com/StackExchange/StackExchange.Redis)

Using the management console, you only need to provide the connection string for your redis data store and provide sql like statements to manage your store.

## Creation
In Redis.SQL an entity is analogous to a SQL table. You can create an entity using the format

create Entity (property: type,...)

Example:

*create user (name:string, id:int64)*

Redis.SQL currently supports the following data types:

-string
-int32
-int64
-boolean
-char
-datetime
-timespan

## Insertion

After creating your entity, you can insert new data using the following format:

insert entity (comma_separated_properties) values (comma_separated_values)

Example: 

*insert user (name, id, joined) values ('Ahmed', 1, '9/24/2017')*

Note that all char, string, datetime and timespan values should be enclosed within single quotes.

## Projection

To project an entity from your data store, use the following format:

select [*/comma_separated_properties] from entity where property [=/!=/>=/<=/</>] value [and/or] ...

Note that providing the where expression is not obligatory.

Example

*select * from user where (name = 'ahmed' and age = 23) or verified = false*

*select user.joined, user.verified from user where verified = true*

## Deletion

To delete an entity from your data store, use the following format:

delete entity where [condition]

Note that providing the where expression is not obligatory.

Example:

*delete user where name = 'ahmed'*

*delete user*

## Updating

To update an entity, use the following format

update entity set property=value where [condition]

Note that providing the where expression is not obligatory.

Example:

*update user set name = 'john', age = 50, verified = false where id = 30*

*update user set deleted = true*

# ORM

Frameworks such as EntityFramework provide developers with the ability to map their database entities to strongly typed objects in their code. Redis.SQL aims to provide developers with a similar functionality by translating provided expression trees into Redis.SQL language.

Example:

![alt text](https://raw.githubusercontent.com/asarnaout/Redis.SQL/master/MapperExample.png)

Note that all Redis.SQL APIs are async APIs that return Tasks.

Please refer to Redis.SQL.Client.Demo for more examples.