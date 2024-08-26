/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using WebService.Entites;
using System.IdentityModel.Tokens.Jwt;
using WebService.Context;
using System.Security.Cryptography;

namespace WebService.Security;

public class JWTAuthentication(IConfiguration configuration)
{
    private readonly JWTSettings jwtSettings = configuration.GetSection("Jwt").Get<JWTSettings>()!;

    public static string HashUserPassword(string userId, string password)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(password))
            throw new Exception("invalid user id and/or password for hash");
        
        var passwordBytes = Encoding.Default.GetBytes(password + "neuRay Labs" + userId);
        var hashedPassword = SHA256.HashData(passwordBytes);
        return Convert.ToHexString(hashedPassword);
    }

    public bool AuthenticateUser(AuthRequest request, out string jwtToken)
    {  
        jwtToken = "";
        request.Validate();

        User user = UserContext.Instance.GetUserByEmail(request.Email);

        var hashedPassword = HashUserPassword(user.Id, request.Password);

        if (hashedPassword != user.Password)
            return false;
        
        jwtToken = GenerateJwtToken(user);
        return true;
    }

    public string GenerateJwtToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptior = new SecurityTokenDescriptor
        {
            Subject = GenerateClaims(user),
            Expires = DateTime.UtcNow.AddMinutes(jwtSettings.TokenExpiryInMinutes),
            SigningCredentials = credentials,
            Issuer = jwtSettings.Issuer,
            Audience = jwtSettings.Audience
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptior);
        return tokenHandler.WriteToken(securityToken);
    }

    private ClaimsIdentity GenerateClaims(User user)
    {
        var claims = new ClaimsIdentity();

        claims.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
        claims.AddClaim(new Claim(ClaimTypes.Email, user.Email));

        foreach (var role in user.Roles)
            claims.AddClaim(new Claim(ClaimTypes.Role, role));

        return claims;
    }
}