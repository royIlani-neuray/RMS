/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { JwtHelperService } from '@auth0/angular-jwt';

export function getJwtToken() {
    return localStorage.getItem('auth');
}

@Injectable({
  providedIn: 'root'
})
export class RoleGuard implements CanActivate {

  constructor(private jwtHelper: JwtHelperService, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean 
  {
    let expectedRole = route.data['role'];

    let jwtToken = getJwtToken();

    if ((jwtToken == null) || this.jwtHelper.isTokenExpired(jwtToken))
    {
        this.router.navigate(['login']);
        return false;
    }

    if (expectedRole)
    {
        const decodedToken = this.jwtHelper.decodeToken(jwtToken);
        const roles = decodedToken?.role || [];

        if (!roles.includes(expectedRole)) 
        {
            this.router.navigate(['login']);
            return false;
        }
    } 

    return true;
  }
}
