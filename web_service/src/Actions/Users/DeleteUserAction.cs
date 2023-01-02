/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;
using WebService.Context;

namespace WebService.Actions.Users;

public class DeleteUserAction : UserAction 
{
    public DeleteUserAction(string userId) : base(userId) {}

    protected override void RunUserAction(User user)
    {
        System.Console.WriteLine($"Deleting user - {user.Id}");
        UserContext.Instance.DeleteUser(user);
    }
}