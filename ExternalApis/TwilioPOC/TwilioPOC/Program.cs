﻿using Microsoft.Extensions.Options;
using System.Web;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Twilio;
using Twilio.Base;
using Twilio.Jwt.AccessToken;
using Twilio.Rest.Video.V1;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// For IOptions API
//builder.Services.AddOptions<TwilioOptions>()
//    .Bind(builder.Configuration.GetSection(nameof(TwilioOptions)));

// For Singleton
var twilioConfig = builder.Configuration.GetSection(nameof(TwilioOptions)).Get<TwilioOptions>();
if (twilioConfig is null)
    throw new InvalidOperationException("Twilio configuration is missing");
builder.Services.AddSingleton(twilioConfig);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();



app.MapGet("/create", (TwilioOptions settings, ILogger<Program> logger, string roomName) =>
    {
        logger.LogInformation($"Attempting to create new room {roomName}");

        TwilioClient.Init(settings.AccountSid, settings.AuthToken);

        var roomResource = RoomResource.Read(uniqueName: roomName, limit: 1).FirstOrDefault();

        if (roomResource is not null)
        {
            logger.LogInformation($"Room {roomName} already exists with status {roomResource.Status}");
        }
        else
        {
            // with postback url
            roomResource = RoomResource.Create(uniqueName: roomName, 
                type: RoomResource.RoomTypeEnum.Group,
                recordParticipantsOnConnect: true,
                statusCallback: new Uri("https://twiliopoc20250409080904-bqa0g9d8e8ejd0dy.canadacentral-01.azurewebsites.net/events"), // 👈 This is key!
    statusCallbackMethod: Twilio.Http.HttpMethod.Post);
        }

        logger.LogInformation(
            @$"Room SID {roomResource!.Sid}, Status {roomResource!.Status}, Type : {roomResource.Type}");

        var videoGrant = new VideoGrant
        {
            Room = roomName
        };

        var token = new Token(settings.AccountSid, settings.ApiKeySid, settings.ApiSecret, settings.Identity,
            grants: [videoGrant]);

        logger.LogInformation($"Token ID : {token.ToJwt()}");
        return new RoomDetails(token.ToJwt(), roomName);
    })
    .WithName("Create")
    .WithOpenApi()
    .Produces(StatusCodes.Status404NotFound)
    .Produces<RoomDetails>();


app.MapPost("/events", async (ILogger<Program> logger,  HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();
    var parsed = HttpUtility.ParseQueryString(body);

    var eventType = parsed["StatusCallbackEvent"];
    var room = parsed["RoomName"];
    var participant = parsed["ParticipantIdentity"];

    logger.LogInformation($"Twilio Event: {eventType}, Room: {room}, Participant: {participant}");

    return Results.Ok();
})
    .WithName("Events")
    .WithOpenApi();


app.Run();


public record RoomDetails(string TokenId, string RoomName);
public record TwilioOptions
{
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string ApiKeySid { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public string RoomType { get; set; } = "group"; // Default if not set
    public string Identity { get; set; } = string.Empty;
}

