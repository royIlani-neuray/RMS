/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json.Serialization;
using WebService.Context;
using WebService.Entites;

namespace WebService.Actions.Users;

public class AddUserArgs 
    {
        [JsonPropertyName("first_name")]
        public String FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public String LastName { get; set; }

        [JsonPropertyName("employee_id")]
        public String EmployeeId { get; set; }

        [JsonPropertyName("email")]
        public String Email { get; set; }

        public AddUserArgs()
        {
            FirstName = String.Empty;
            LastName = String.Empty;
            EmployeeId = String.Empty;
            Email = String.Empty;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(FirstName))
                throw new HttpRequestException("User first name not defined");
            if (string.IsNullOrWhiteSpace(LastName))
                throw new HttpRequestException("User last name not defined");
        }
    }

public class AddUserAction : IAction 
{
    AddUserArgs args;

    public AddUserAction(AddUserArgs args)
    {
        this.args = args;
    }

    public void Run()
    {
        args.Validate();

        User user = new User();
        user.FirstName = args.FirstName;
        user.LastName = args.LastName;
        user.Email = args.Email;
        user.EmployeeId = args.EmployeeId;

        System.Console.WriteLine($"Adding new user - [{user.FirstName} {user.LastName}]");
 
        UserContext.Instance.AddUser(user);

        System.Console.WriteLine($"User added.");
    }
}