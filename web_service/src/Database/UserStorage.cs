/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;
using System.Text.Json;

namespace WebService.Database;

public class UserStorage {

    public static readonly string StoragePath = "./data/users";
    public static readonly string UserFileExtention = ".json";
    
    public static void SaveUser(User user)
    {
        string jsonString = JsonSerializer.Serialize(user);
        File.WriteAllText(System.IO.Path.Combine(StoragePath, user.Id + UserFileExtention), jsonString);
    }

    public static void DeleteUser(User user)
    {
        string filePath = System.IO.Path.Combine(StoragePath, user.Id + UserFileExtention);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        else
        {
            Console.WriteLine($"Warning: could not delete user file for user: {user.Id}");
        }
    }

    public static Dictionary<string, User> LoadAllUsers()
    {
        Dictionary<string, User> users = new Dictionary<string, User>();

        var files = System.IO.Directory.GetFiles(StoragePath, "*" + UserFileExtention);
        foreach (string filePath in files)
        {
            string jsonString = File.ReadAllText(filePath);
            
            User? user = JsonSerializer.Deserialize<User>(jsonString);

            if (user == null)
                throw new Exception("deserialze failed!");

            users.Add(user.Id, user);
        }

        Console.WriteLine($"Loaded {users.Keys.Count} users from storage.");
        return users;
    }
}