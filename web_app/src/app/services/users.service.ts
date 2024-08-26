/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { User, UserBrief } from '../entities/user';

export interface AuthUserResponse {
  token: string
}

@Injectable({
  providedIn: 'root'
})
export class UsersService {

  constructor(private http:HttpClient) { }

  public getUsers()
  {
    return this.http.get<UserBrief[]>("/api/users")
  }

  public getUser(userId : string)
  {
    return this.http.get<User>("/api/users/" + userId)
  }

  public authenticateUser(email : string, password : string)
  {
    return this.http.post<AuthUserResponse>("/api/users/auth", 
    {
      email: email,
      password: password
    })  
  }
}
