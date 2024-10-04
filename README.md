# 三竹簡訊 C# SDK
[![Build Status](https://dev.azure.com/lettucebo/Github.Build/_apis/build/status%2FMitake.Sms.Core%2FMitake.Sms.Core.Build?branchName=master)](https://dev.azure.com/lettucebo/Github.Build/_build/latest?definitionId=36&branchName=master)
[![Nuget](https://img.shields.io/nuget/v/Mitake.Sms.Core.svg)](https://www.nuget.org/packages/Mitake.Sms.Core/)

## Setup

``` ps1
dotnet user-secrets init
dotnet user-secrets set "SmsAccount" "account"
dotnet user-secrets set "SmsPassword" "password"
dotnet user-secrets set "Mobile" "phone number"
```