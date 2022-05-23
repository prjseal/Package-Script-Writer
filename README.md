# Package-Script-Writer

This is the code repository for the Package Script Writer website. 

[https://psw.codeshare.co.uk](https://psw.codeshare.co.uk/)

The site is built in ASP.NET Core 6.

There is no database so it is very easy to get up and running. 

You need to have the .NET 6 SDK installed first.

Just fork the repository, clone it to your machine and then open the solution file and run the project or navigate to the PSW folder with all the files in and run the site using the command dotnet run

## Testing the API

To test the api, there is a self-documenting script which relies on the [Rest Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) VSCode Extension, or you can use POSTMan. You will need to use VS Code, and run the site using the following

    dotnet watch run --project .\PSW\PSW\

And then opening the file Api Request/API Testing.http file