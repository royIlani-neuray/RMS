/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json.Serialization;
using WebService.Database;

namespace WebService.Entites;

public class User : IEntity
{
    [JsonIgnore]
    public IEntity.EntityTypes EntityType => IEntity.EntityTypes.User;

    [JsonIgnore]
    public string StoragePath => StorageDatabase.UserStoragePath;

    [JsonPropertyName("user_id")]
    public String Id { get; set; }

    [JsonPropertyName("first_name")]
    public String FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public String LastName { get; set; }

    [JsonPropertyName("employee_id")]
    public String EmployeeId { get; set; }

    [JsonPropertyName("email")]
    public String Email { get; set; }

    [JsonPropertyName("password")]
    public String Password { get; set; }

    [JsonPropertyName("roles")]
    public List<String> Roles { get; set; }

    [JsonPropertyName("registered_at")]
    public DateTime RegisteredAt { get; set; }

    [JsonIgnore]
    public ReaderWriterLockSlim EntityLock { get; set; }

    public class UserBrief 
    {
        [JsonPropertyName("user_id")]
        public String Id { get; set; }

        [JsonPropertyName("first_name")]
        public String FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public String LastName { get; set; }

        public UserBrief(User user)
        {
            Id = user.Id;
            FirstName = user.FirstName;
            LastName = user.LastName;
        }
    }

    public User()
    {
        EntityLock = new ReaderWriterLockSlim();
        Id = Guid.NewGuid().ToString();
        FirstName = String.Empty;
        LastName = String.Empty;
        EmployeeId = String.Empty;
        Email = String.Empty;
        Password = String.Empty;
        Roles = [];
        RegisteredAt = DateTime.UtcNow;
    }

    public User ShallowClone()
    {
        return (User) this.MemberwiseClone();
    }
}