using System;
using System.Text.Json;
using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;
using Microsoft.Extensions.DependencyInjection;

namespace CommandsService.EventProcessing
{
    public class EventProcessor : IEventProcessor
    {
        private readonly IServiceScopeFactory _scopedFactory;
        private readonly IMapper _mapper;

        public EventProcessor(IServiceScopeFactory scopeFactory, IMapper mapper)
        {
            _scopedFactory = scopeFactory;
            _mapper = mapper;
        }
        public void ProcessEvent(string message)
        {
            var eventType = DetermineEvent(message);

            switch(eventType)
            {
                case EventType.PlatformPublished:
                    addPlatform(message);
                    break;
                default: 
                    break;
            }
        }

        private void addPlatform(string platformPublishedMessage)
        {
            using (var scope = _scopedFactory.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<ICommandRepo>();

                var platformPublishedDto = JsonSerializer.Deserialize<PlatformPublishedDto>(platformPublishedMessage);

                try
                {
                    var platform = _mapper.Map<Platform>(platformPublishedDto);

                    if(repo.ExternalPlatformExists(platform.ExternalID))
                    {
                        System.Console.WriteLine("platform already exists");
                        return;
                    }

                    repo.CreatePlatform(platform);
                    repo.SaveChanges();
                    System.Console.WriteLine("--> Platform added");
                }
                catch(Exception ex)
                {
                    System.Console.WriteLine($"--> coudn't add platform to the DB{ex.Message}");
                }
            }
        }

        private EventType DetermineEvent(string notificationMessage)
        {
            System.Console.WriteLine("--> Dermening event");

            var eventType = JsonSerializer.Deserialize<GenericEventDto>(notificationMessage);

            return eventType.Event switch
            {
                "Platform_Published" => EventType.PlatformPublished,
                _=> EventType.Undetermined
            };
        }
    }

    enum EventType
    {
        PlatformPublished,
        Undetermined
    }
}