/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Context;
using WebService.Entites;

public abstract class UserAction : IAction
{
    protected readonly string userId;
    public UserAction(string userId)
    {
        this.userId = userId;
    }

    protected abstract void RunUserAction(User user);

    public void Run()
    {
        var user = UserContext.Instance.GetUser(userId);
        user.userLock.EnterUpgradeableReadLock();

        if (!UserContext.Instance.IsUserExist(userId))
        {
            user.userLock.ExitUpgradeableReadLock();
            throw new NotFoundException($"Cannot find user with id '{userId}' in context. action failed.");
        }

        try
        {
            user.userLock.EnterWriteLock();
            RunUserAction(user);
        }
        finally
        {
            if (UserContext.Instance.IsUserExist(userId))
                UserContext.Instance.UpdateUser(user);
                
            user.userLock.ExitWriteLock();
            user.userLock.ExitUpgradeableReadLock();
        }
    }
}