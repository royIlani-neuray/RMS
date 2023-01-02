/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json.Serialization;

namespace WebService.Entites;

public class User 
{
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

    [JsonPropertyName("registered_at")]
    public DateTime RegisteredAt { get; set; }

    [JsonIgnore]
    public ReaderWriterLockSlim userLock;

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
        userLock = new ReaderWriterLockSlim();
        Id = Guid.NewGuid().ToString();
        FirstName = String.Empty;
        LastName = String.Empty;
        EmployeeId = String.Empty;
        Email = String.Empty;
        RegisteredAt = DateTime.UtcNow;
    }
}