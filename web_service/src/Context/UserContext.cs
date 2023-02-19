/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Database;
using WebService.Entites;

namespace WebService.Context;

public sealed class UserContext : EntityContext<User> {

    #region Singleton
    
    private static object singletonLock = new object();
    private static volatile UserContext? instance; 

    public static UserContext Instance {
        get 
        {
            if (instance == null)
            {
                lock (singletonLock)
                {
                    if (instance == null)
                        instance = new UserContext();
                }
            }

            return instance;
        }
    }

    private UserContext() : base(IEntity.EntityTypes.User) {}

    #endregion

    public void LoadUsersFromStorage()
    {
        LoadEntitiesFromStorage(StorageDatabase.UserStoragePath);
    }

    public bool IsUserExist(string userId)
    {
        return IsEntityExist(userId);
    }

    public bool IsEmployeeIdExist(string employeeId)
    {
        if (String.IsNullOrWhiteSpace(employeeId))
            return false;

        if (entities.Values.ToList().Exists(user => user.EmployeeId == employeeId))
            return true;
        
        return false;
    }    

    public bool IsEmailExist(string email)
    {
        if (String.IsNullOrWhiteSpace(email))
            return false;

        if (entities.Values.ToList().Exists(user => user.Email == email))
            return true;
        
        return false;
    } 

    public User GetUser(string userId)
    {
        return GetEntity(userId);
    }

    public void AddUser(User user)
    {
        if (IsUserExist(user.Id))
            throw new Exception("Cannot add user. Another user with the same ID already exist.");

        if (IsEmployeeIdExist(user.Id))
            throw new Exception("Cannot add user. Another user with the employee ID already exist.");

        if (IsEmailExist(user.Id))
            throw new Exception("Cannot add user. Another user with the given email already exist.");

        AddEntity(user);
    }

    public void UpdateUser(User user)
    {
        UpdateEntity(user);
    }

    public void DeleteUser(User user)
    {
        DeleteEntity(user);
    }

    public List<User.UserBrief> GetUsersBrief()
    {
        return entities.Values.ToList().ConvertAll<User.UserBrief>(user => new User.UserBrief(user));
    }
}