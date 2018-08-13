# CodingMonkey.CodeExecutor

[![Codacy Badge](https://api.codacy.com/project/badge/Grade/62e7bcd4984e415aa3314f9252d27b82)](https://app.codacy.com/app/thomas.shipley/CodingMonkey.CodeExecutor?utm_source=github.com&utm_medium=referral&utm_content=tdshipley/CodingMonkey.CodeExecutor&utm_campaign=badger)

API for executing code submitted by a user

## Required Secrets File

The code requires a secrets file which is not checked in called ```appsettings.secrets.json``` in ```src\CodingMonkey.CodeExecutor```. This file contains the details needed to interact with CodingMonkey.IdentityServer an example of the format and properties is below:

```
{
  "IdentityServer": {
    "Authority": "url_of_CodingMonkey.IdentityServer_authority",
    "ScopeName": "name_of_scope_for_app_defined_in_CodingMonkey.IdentityServer",
    "ScopeSecret": "shared_secret_for_scope_defined_in_CodingMonkey.IdentityServer"
  }
}
```
