﻿namespace App.RestContracts.Auth.Requests;

public class RegisterUserRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}
