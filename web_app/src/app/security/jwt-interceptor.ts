/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";

/*
* This class injects the JWT authentication token to any http request
* in order to authenticate the user against the web service.
*/

@Injectable()
export class JwtInterceptor implements HttpInterceptor 
{
  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> 
  {
    let jwtToken = localStorage.getItem('auth');
    if (jwtToken) 
    {
      request = request.clone({
        setHeaders: {
          Authorization: `Bearer ${jwtToken}`
        }
      });
    }
    return next.handle(request);
  }
 }