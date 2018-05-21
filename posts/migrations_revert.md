# How to revert migration in Entity Framework Core

For example we are in situation when we have some migrations and one of them is actually buggy - this could happen because some developers tend to edit generated migration files by themselves. So we have:

* migration1
* migration2
* migration3 <---- buggy one
* migration4
* migration5

To fix buggy migration we have to go back to migration2 which can be done using following command:

```
dotnet ef database update migration2
```

Now we are befor migration3 and we can add fixing migration:

```
dotnet ef migrations add FixingMigration
```