# Package-Script-Writer

This is the code repository for the Package Script Writer website. 

[https://psw.codeshare.co.uk](https://psw.codeshare.co.uk/)

The site is built in ASP.NET Core 6.

There is no database so it is very easy to get up and running. 

You need to have the .NET 6 SDK installed first.

## How to contribute

The process to follow when contributing is to:

1. Raise an issue if there isn’t one already. This can be for a bug or a feature. Give a detailed description about the problem, what were you expecting and what actually happened etc.
2. We can discuss new features in the issue to make sure that your PR is something I want to include on the site before you go and spend a long time working on a PR that might not get merged in.
3. Fork the repository in your own GitHub account.
4. Work on the code in a branch on your fork.
5. Push your changes in your branch up to your repository.
6. Create a PR back to the main repository that shows what the problem was, how you fixed it and what happens now.
7. Hopefully I will be able to review the PR, test it out, and all being well, merge it in.

## Testing the API

To test the api, there is a self-documenting script which relies on the [Rest Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) VSCode Extension, or you can use POSTMan. You will need to use VS Code, and run the site using the following

    dotnet watch run --project .\PSW\PSW\

And then opening the file Api Request/API Testing.http file
