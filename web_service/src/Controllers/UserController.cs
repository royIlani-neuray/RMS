/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;
using WebService.Context;
using WebService.Actions.Users;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebService.Security;
using Microsoft.AspNetCore.Authorization;

namespace WebService.Controllers;

[Authorize]
[ApiController]
[Route("users")]
public class UserController : ControllerBase
{
    private readonly JWTAuthentication JwtAuth;

    public UserController(JWTAuthentication authentication)
    {
        JwtAuth = authentication;
    }

    private void ValidateUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out _))
            throw new BadRequestException("invalid user id provided.");
    }

    [HttpGet]
    public List<User.UserBrief> GetUsers()
    {
        return UserContext.Instance.GetUsersBrief();
    }

    [HttpGet("{userId}")]
    public User GetUser(string userId)
    {
        ValidateUserId(userId);        
        if (!UserContext.Instance.IsUserExist(userId))
            throw new NotFoundException("There is no user with the provided id");

        // remove the password hash from the user data
        var user = UserContext.Instance.GetUser(userId).ShallowClone();
        user.Password = "RESTRICTED_DATA";
        return user;
    }

    [HttpPost]
    public void AddUser([FromBody] AddUserArgs args)
    {
        AddUserAction action = new AddUserAction(args);
        action.Run();
        return;
    }

    [HttpDelete("{userId}")]
    [Authorize(Roles = "Administrator")]
    public void DeleteUser(string userId)
    {        
        ValidateUserId(userId); 
        var action = new DeleteUserAction(userId);
        action.Run();
    }

    [AllowAnonymous]
    [HttpPost("auth")]
    public IActionResult AuthenticateUser([FromBody] AuthRequest args)
    {
        if (!JwtAuth.AuthenticateUser(args, out string jwtToken))
        {
            return Unauthorized();
        }

        return Ok(new { token = jwtToken });
    }

}