# DefaultJwtAuthentication
A default implementation for authentication using jwt and refresh tokens with identity core. It also allows for easy extending with your own user properties and dbsets.

## How to use

Using dependency injection:

```

AddDefaultJwtAuthentication<TUserAuthenticationService>(this IServiceCollection services, 
            TokenValidationParameters parameters) where TUserAuthenticationService : class, IUserAuthenticationService
            
AddDefaultJwtDbContext<TAppUser, TDbContext>(this IServiceCollection services,
            string connectionString) where TAppUser : AppUser where TDbContext : AbstractAppDbContext<TAppUser>
            
```

For this to work `IUserAuthenticationService`, `AppUser`, `AbstractAppDbContext<TAppUser>` must be implemented.

### Implementing IUserAuthenticationService

This is the service which handles the authentication of users. It is recommended to use `UserRepository<T>` to interact with the db.

<b> Authenticate </b>

In this method the `AppUserLogin` attempt should be authenticated. Usage of `IUserRepository<T>.Authenticate` is recommended.

<b> Create </b>

In this method the `AppUserCreate` attempt should be used to create a user. Usage of `IUserRepository<T>.Create` is recommended.

<b> Refresh </b>

In this method the jwt and the refresh token should be used to authenticate a user. Usage of `IUserRepository<T>.RefreshToken` is recommended.

### Implementing AppUser

An abstract class which extends the `IdentityUser` class from `Microsoft.AspNetCore.Identity` with the following fields `FirstName`, `LastName` and `RefreshTokens`. 

The `AppUser` class should be extended with a custom user class. If needed, additional properties can be added to the class.

### Implementing AbstractAppDbContext<TAppUser>

An abstract class which extends the `IdentityDbContext<T>` class from `Microsoft.AspNetCore.Identity` with a DbSet of refresh tokens. 

The `AbstractAppDbContext<TAppUser>` class should be extended with a DbContext class. If needed, additional DbSets can be added. Of course, TAppUser should be your own implementation of `AppUser` as mentioned before.

### IUserActionResult

A data object used to return the results of actions performed by the `IUserRepository`.

## Additional extendability

`AppUserCreate` and `AppUserLogin` can both be extended with custom properties.

<sub> Written for version 1.0.0 </sub>
