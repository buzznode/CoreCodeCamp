using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
    [ApiController]
    [Route("api/camps/{moniker}/talks")]
    public class TalksController : ControllerBase
    {
        private readonly LinkGenerator _linkgen;
        private readonly IMapper _mapper;
        private readonly ICampRepository _repo;

        public TalksController(LinkGenerator linkgen, IMapper mapper, ICampRepository repo)
        {
            _linkgen = linkgen;
            _mapper = mapper;
            _repo = repo;
        }

        [HttpGet]
        public async Task<ActionResult<TalkModel[]>> Get(string moniker)
        {
            try
            {
                var talks = await _repo.GetTalksByMonikerAsync(moniker, true);

                return _mapper.Map<TalkModel[]>(talks);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to get talks");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TalkModel>> Get(string moniker, int id)
        {
            try
            {
                var talk = await _repo.GetTalkByMonikerAsync(moniker, id, true);

                if (talk == null)
                {
                    return NotFound();
                }

                return _mapper.Map<TalkModel>(talk);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to get a talk");
            }
        }

        [HttpPost]
        public async Task<ActionResult<TalkModel>> Post(string moniker, TalkModel model)
        {
            try
            {
                var camp = await _repo.GetCampAsync(moniker);

                if (camp == null)
                {
                    return BadRequest("Camp does not exist");
                }

                var talk = _mapper.Map<Talk>(model);
                talk.Camp = camp;

                if (model.Speaker == null)
                {
                    return BadRequest("Speaker ID is required");
                }

                var speaker = await _repo.GetSpeakerAsync(model.Speaker.SpeakerId);

                if (speaker == null)
                {
                    return BadRequest("Speaker could not be found");
                }

                talk.Speaker = speaker;

                _repo.Add(talk);

                if (await _repo.SaveChangesAsync())
                {
                    var url = _linkgen.GetPathByAction(HttpContext, "Get", values: new { moniker, id = talk.TalkId });
                    return Created(url, _mapper.Map<TalkModel>(talk));
                }
                else
                {
                    return BadRequest("Failed to save new talk");
                }
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create a talk");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<TalkModel>> Put(string moniker, int id, TalkModel model)
        {
            try
            {
                var talk = await _repo.GetTalkByMonikerAsync(moniker, id, true);

                if (talk == null)
                {
                    return NotFound();
                }

                _mapper.Map(model, talk);

                if (model.Speaker != null)
                {
                    var speaker = await _repo.GetSpeakerAsync(model.Speaker.SpeakerId);

                    if (speaker != null)
                    {
                        talk.Speaker = speaker;
                    }
                }

                if (await _repo.SaveChangesAsync())
                {
                    return _mapper.Map<TalkModel>(talk);
                }
                else
                {
                    return BadRequest("Failed to update database");
                }
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to put a talk");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(string moniker, int id)
        {
            try
            {
                var talk = await _repo.GetTalkByMonikerAsync(moniker, id);

                if (talk == null)
                {
                    return NotFound();
                }

                _repo.Delete(talk);

                if (await _repo.SaveChangesAsync())
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("Failed to delete talk");
                }
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to delete the talk");
            }
        }
    }
}
