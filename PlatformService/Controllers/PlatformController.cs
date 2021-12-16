using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.Models;
using PlatformService.SyncDataServices.Http;

namespace PlatformService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlatformsController : ControllerBase
    {
        private readonly IPlatformRepo _repository;
        private readonly ICommandDataClient _commandDataClient;
        private readonly IMessageBusClient _messageBusClient;
        private readonly IMapper _mapper;
        
        public PlatformsController(IPlatformRepo repo, IMapper mapper, ICommandDataClient commandDataClient, IMessageBusClient messageBusClient) 
        {
            _mapper = mapper;
            _repository = repo;
            _commandDataClient = commandDataClient;
            _messageBusClient = messageBusClient;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms()
        {
            var platformItem = _repository.GetAllPlatforms();

            return Ok(_mapper.Map<IEnumerable<PlatformReadDto>>(platformItem));
        }

        [HttpGet("{id}", Name = "GetPlatformById")]
        public ActionResult<PlatformReadDto> GetPlatformById(int id)
        {
            var platformItem = _repository.GetPlatformById(id);

            if(platformItem is null)
                return NotFound();

            return Ok(_mapper.Map<PlatformReadDto>(platformItem));
        }

        [HttpPost]
        public async Task<ActionResult<IEnumerable<PlatformReadDto>>> CreatePlatform(PlatformCreateDto dto)
        {
            var platformModel = _mapper.Map<Platform>(dto);

            _repository.CreatePlatform(platformModel);
            _repository.SaveChanges();
            
            var response = _mapper.Map<PlatformReadDto>(platformModel);

            SendAsynchronously(response);
            await SendSynchronously(response);

            return CreatedAtRoute(nameof(GetPlatformById), new {Id = response.Id}, response);
        }

        private async Task SendSynchronously(PlatformReadDto platform)
        {
            try 
            {
                await _commandDataClient.SendPlatformToCommand(platform);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Couldn't send platform synchronously {ex.Message}");
            }
        }

        private void SendAsynchronously(PlatformReadDto platform)
        {
            var publishDto = _mapper.Map<PlatformPublishedDto>(platform);
            publishDto.Event = "Platform_Published";
            try 
            {
                _messageBusClient.PublishNewPlatform(publishDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Couldn't send platform asynchronously {ex.Message}");
            }
        }
    }
}