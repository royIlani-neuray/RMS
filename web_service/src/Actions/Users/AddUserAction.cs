/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Serilog;
using WebService.Context;
using WebService.Entites;
using WebService.Security;

namespace WebService.Actions.Users;

public partial class AddUserArgs 
{
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

    public AddUserArgs()
    {
        FirstName = String.Empty;
        LastName = String.Empty;
        EmployeeId = String.Empty;
        Email = String.Empty;
        Password = String.Empty;
    }

    private bool IsValidPassword()
    {
        // Define a regular expression for a strong password
        // ^(?=.*[a-z]) - Ensures at least one lowercase letter.
        // (?=.*[A-Z]) - Ensures at least one uppercase letter
        // (?=.*\d)- Ensures at least one digit.
        // (?=.*[@$!%*?&]) -  Ensures at least one special character
        // [A-Za-z\d@$!%*?&]{8,}$ - Ensures the password is at least 8 characters long.
        var passwordPattern = PasswordRegex();

        // Check if the password matches the pattern
        return passwordPattern.IsMatch(Password);
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Email))
            throw new HttpRequestException("User email not defined");
        if (string.IsNullOrWhiteSpace(Password))
            throw new HttpRequestException("User password is missing");
        if (!IsValidPassword())
            throw new HttpRequestException("invalid password provided - password is not strong enough.");
    }

    [GeneratedRegex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")]
    private static partial Regex PasswordRegex();
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

        Log.Information($"Adding new user - [{user.FirstName} {user.LastName}]");
 
        UserContext.Instance.AddUser(user);

        try
        {
            user.Password = JWTAuthentication.HashUserPassword(user.Id, args.Password);
            UserContext.Instance.UpdateUser(user);
        }
        catch (Exception ex)
        {
            UserContext.Instance.DeleteUser(user);
            Log.Error("failed to store user password", ex);
            throw;
        }

        Log.Information($"User '{user.Email}' added successfuly.");
    }
}