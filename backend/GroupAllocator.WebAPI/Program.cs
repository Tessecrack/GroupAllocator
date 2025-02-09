using GroupAllocator.DAL.Context;
using GroupAllocator.DAL.Entities;
using GroupAllocator.DAL.Repositories;
using GroupAllocator.DAL.Unit;
using GroupAllocator.DAL.Interfaces.Repositories;
using GroupAllocator.DAL.Interfaces.UnitOfWork;
using GroupAllocator.Models.Constants;
using GroupAllocator.Models.GroupModels;
using GroupAllocator.Models.ServiceModels;
using GroupAllocator.Models.TelegramModels;
using GroupAllocator.Models.UserModels;
using Microsoft.EntityFrameworkCore;
using GroupAllocator.TelegramBotService.Services;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

var stringConnectionDb = builder.Configuration.GetConnectionString("DefaultConnection");
var accessApiToken = builder.Configuration.GetSection("ACCESS_API_TOKEN").Value ?? string.Empty;

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(stringConnectionDb));

builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<ITelegramUserRepository, TelegramUserRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

var tokenBot = builder.Configuration.GetSection("TELEGRAM_BOT_TOKEN").Value;

builder.Services.AddSingleton<TelegramBotClientOptions>((provider) => new TelegramBotClientOptions(tokenBot));
builder.Services.AddSingleton<TelegramUsersLocalStorage>();

builder.Services.AddHostedService<TelegramBotConnector>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseAccessTokenMiddleware(accessApiToken);

#region /api/roles

app.MapGet("/api/roles", async (IUnitOfWork db) => await db.GetAllUserRoles());

app.MapGet("/api/roles/{id}", async (Guid id, IUnitOfWork db) =>
{
    UserRole? userRole = await db.GetUserRole(id);

    if (userRole == null)
    {
        return Results.NotFound(new ErrorResponse() { Message = "Role not found" });
    }

    return Results.Ok(userRole);
});

app.MapDelete("/api/roles/{id}", async (Guid id, IUnitOfWork db) =>
{
    UserRole? userRole = await db.DeleteUserRole(id);

    if (userRole == null)
    {
        return Results.NotFound(new ErrorResponse { Message = "Role not found" });
    }

    return Results.Ok(userRole);
});

// create
app.MapPost("/api/roles", async (UserRoleModel model, IUnitOfWork db) =>
{
    var entity = new UserRole()
    {
        Id = Guid.NewGuid(),
        Name = model.Name,
        Description = model.Description,
    };

    var newEntity = await db.CreateUserRole(entity);
    return Results.Ok(newEntity);
});

//update
app.MapPut("/api/roles", async (UserRole userRole, IUnitOfWork db) =>
{
    var updated = await db.UpdateUserRole(userRole);

    if (updated == null)
    {
        return Results.NotFound(new { message = "Role not found" });
    }

    return Results.Ok(userRole);
});

#endregion

#region /api/groups

app.MapGet("/api/groups", async (IUnitOfWork db) => await db.GetAllGroups());

app.MapGet("/api/groups/{id}", async (Guid id, IUnitOfWork db) =>
{
    var foundGroup = await db.GetGroup(id);

    if (foundGroup == null)
    {
        return Results.NotFound(new ErrorResponse { Message = "Group not found" });
    }

    return Results.Ok(foundGroup);
});

app.MapDelete("/api/groups/{id}", async (Guid id, IUnitOfWork db, TelegramUsersLocalStorage usersStorage) =>
{
    var deleted = await db.DeleteGroup(id);

    if (deleted == null)
    {
        return Results.NotFound(new ErrorResponse { Message = "Group not found" });
    }

    await usersStorage.UpdateState();
    return Results.Ok(deleted);
});

//create
app.MapPost("/api/groups", async (GroupModel model, IUnitOfWork db, TelegramUsersLocalStorage usersStorage) =>
{
    var entity = new Group()
    {
        Id = Guid.NewGuid(),
        Name = model.Name,
        Description = model.Description,
    };

    var newEntity = await db.CreateGroup(entity);

    await usersStorage.UpdateState();
    return Results.Ok(newEntity);
});

//update
app.MapPut("/api/groups", async (Group group, IUnitOfWork db, TelegramUsersLocalStorage usersStorage) =>
{
    var updated = await db.UpdateGroup(group);
    if (updated == null)
    {
        return Results.NotFound(new ErrorResponse { Message = "Group not found" });
    }

    await usersStorage.UpdateState();
    return Results.Ok(updated);
});

#endregion

#region /api/telegram_users

app.MapGet("/api/telegram_users", async (IUnitOfWork db) => await db.GetAllTelegramUsers());

app.MapGet("/api/telegram_users/{id}", async (Guid id, IUnitOfWork db) =>
{
    var found = await db.GetTelegramUser(id);

    if (found == null)
    {
        return Results.NotFound(new ErrorResponse { Message = "Telegram user not found" });
    }

    return Results.Ok(found);
});

app.MapDelete("/api/telegram_users/{id}", async (Guid id, IUnitOfWork db, TelegramUsersLocalStorage usersStorage) =>
{
    var deleted = await db.DeleteTelegramUser(id);
    if (deleted == null)
    {
        return Results.NotFound(new ErrorResponse { Message = "Telegram user not found" });
    }

    await usersStorage.UpdateState();
    return Results.Ok(deleted);
});

//create
app.MapPost("/api/telegram_users", async (TelegramUserModel model, IUnitOfWork db, TelegramUsersLocalStorage usersStorage) =>
{
    var userRoleName = model.UserRole is null ? Role.PARTICIPANT : model.UserRole.Name;

    var userRole = await db.GetUserRoleByName(userRoleName);

    if (userRole == null)
    {
        return Results.BadRequest(new ErrorResponse { Message = "Incorrect role" });
    }

    if (model.Group == null)
    {
        return Results.BadRequest(new ErrorResponse { Message = "Incorrect group" });
    }

    var group = await db.GetGroupByName(model.Group.Name);

    if (group == null)
    {
        return Results.BadRequest(new ErrorResponse { Message = "Incorrect group" });
    }

    var entity = new TelegramUser()
    {
        Id = Guid.NewGuid(),
        ChatId = model.ChatId,
        Username = model.Username,
        FirstName = model.FirstName,
        LastName = model.LastName,
        UserRoleId = userRole.Id,
        GroupId = group.Id
    };

    var newEntity = await db.CreateTelegramUser(entity);

    await usersStorage.UpdateState();
    return Results.Ok(newEntity);

});

//update
app.MapPut("/api/telegram_users", async (TelegramUser telegramUser, IUnitOfWork db, TelegramUsersLocalStorage usersStorage) =>
{
    var updated = await db.UpdateTelegramUser(telegramUser);

    if (updated == null)
    {
        return Results.NotFound(new ErrorResponse { Message = "Telegram user not found" });
    }

    await usersStorage.UpdateState();
    return Results.Ok(updated);
});

#endregion

#region /api/users

app.MapGet("/api/users", async (IUnitOfWork db) => await db.GetAllUsers());

app.MapGet("/api/users/{id}", async (Guid id, IUnitOfWork db, TelegramUsersLocalStorage usersStorage) =>
{
    var found = await db.GetUser(id);

    if (found == null)
    {
        return Results.NotFound(new ErrorResponse { Message = "User not found" });
    }

    await usersStorage.UpdateState();
    return Results.Ok(found);
});

app.MapDelete("/api/users/{id}", async (Guid id, IUnitOfWork db, TelegramUsersLocalStorage usersStorage) =>
{
    var deleted = await db.DeleteUser(id);

    if (deleted == null)
    {
        return Results.NotFound(new ErrorResponse { Message = "User not found" });
    }

    await usersStorage.UpdateState();
    return Results.Ok(deleted);
});

//create
app.MapPost("/api/users", async (UserModel model, IUnitOfWork db, TelegramUsersLocalStorage usersStorage) =>
{
    if (model.TelegramUserModel == null)
    {
        return Results.BadRequest(new ErrorResponse { Message = "User is not linked to telegram account" });
    }

    var telegramUser = await db.GetTelegramUserByTelegramId(model.TelegramUserModel.ChatId);

    if (telegramUser == null)
    {
        return Results.BadRequest(new ErrorResponse { Message = "User has incorrect telegram data" });
    }

    var entity = new User()
    {
        Id = Guid.NewGuid(),
        FirstName = model.FirstName,
        LastName = model.LastName,
        TelegramUserId = telegramUser.Id,
    };

    var newEntity = await db.CreateUser(entity);

    await usersStorage.UpdateState();

    return Results.Ok(entity);
});

//update
app.MapPut("/api/users", async (User user, IUnitOfWork db, TelegramUsersLocalStorage usersStorage) =>
{
    var updated = await db.UpdateUser(user);

    if (updated == null)
    {
        return Results.NotFound(new ErrorResponse { Message = "User not found" });
    }

    await usersStorage.UpdateState();
    return Results.Ok(updated);
});

#endregion

#region /api/group_telegram_users

app.MapGet("/api/group_telegram_users/{id}", async (Guid id, IUnitOfWork db) =>
{
    var group = await db.GetGroup(id);
    if (group == null)
    {
        return Results.NotFound(new ErrorResponse() { Message = "Incorrect group" });
    }

    var users = await db.GetTelegramUsersByGroupId(id);
    return Results.Ok(users);
});

#endregion

#region /api/group_users
app.MapGet("/api/group_users/{id}", async (Guid id, IUnitOfWork db) =>
{
    var group = await db.GetGroup(id);
    if (group == null)
    {
        return Results.NotFound(new ErrorResponse() { Message = "Incorrect group" });
    }

    var users = await db.GetUsersByGroupId(id);
    return Results.Ok(users);
});
#endregion

app.Run();
