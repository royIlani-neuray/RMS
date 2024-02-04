/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Context;
using WebService.Entites;

namespace WebService.Actions;

public abstract class UserAction : EntityAction<User>
{
    public UserAction(string userId) : base(UserContext.Instance, userId) {}

    protected abstract void RunUserAction(User user);

    protected override void RunAction(User user)
    {
        RunUserAction(user);
    }

    protected override void RunPostActionTask(User user)
    {
    }
}