using System.Collections.Generic;
using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommandsService.Controllers
{
    [Route("api/c/platforms/{platformId}/[controller]")]
    [ApiController]
    public class CommandsController : ControllerBase
    {
        private readonly ICommandRepo _repository;
        private readonly IMapper _mapper;

        public CommandsController(ICommandRepo repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
        [HttpGet]
        public ActionResult<IEnumerable<CommandReadDto>> GetCommands(int platformId)
        {
            System.Console.WriteLine("get commands working");
            if(!_repository.PlatformExists(platformId))
                return NotFound();
            
            var commands = _repository.GetCommandsForPlatform(platformId);

            return Ok(_mapper.Map<IEnumerable<CommandReadDto>>(commands));
        }

        [HttpGet("{commandId}", Name = "GetCommandForPlatform")]
        public ActionResult<CommandReadDto> GetCommandForPlatform(int platformId, int commandId)
        {
            System.Console.WriteLine("get command working");
            
            if(!_repository.PlatformExists(platformId))
                return NotFound();
            
            var commands = _repository.GetCommand(platformId, commandId);

            return Ok(_mapper.Map<CommandReadDto>(commands));
        }

        [HttpPost]
        public ActionResult<CommandReadDto> CreateCommand(int platformId, CommandCreateDto commandCreateDto)
        {
            System.Console.WriteLine("create command working");
            
            if(!_repository.PlatformExists(platformId))
                return NotFound();
            
            var domainCommand = _mapper.Map<Command>(commandCreateDto);

            _repository.CreateCommand(platformId, domainCommand);
            _repository.SaveChanges();

            var commandReadDto =  _mapper.Map<CommandReadDto>(domainCommand);
            return CreatedAtRoute(nameof(GetCommandForPlatform), new {platformId = platformId, commandId = commandReadDto.Id}, commandReadDto);
        }
        
    }
}