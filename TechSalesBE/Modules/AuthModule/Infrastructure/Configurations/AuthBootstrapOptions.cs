using System;

namespace AuthModule.Infrastructure.Configurations;

public class AuthBootstrapOptions
{
    public string Email {get; set;} = "";
    public string Password {get; set;} = "";
    public string FullName {get; set;} = "";
    public string Phone {get; set;} = "";
}
