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
    /// <summary>
    /// Camps Controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("1.1")]
    public class CampsController : ControllerBase
    {
        private readonly LinkGenerator _linkgen;
        private readonly IMapper _mapper;
        private readonly ICampRepository _repo;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="repo"></param>
        public CampsController(LinkGenerator linkgen, IMapper mapper, ICampRepository repo)
        {
            _linkgen = linkgen;
            _mapper = mapper;
            _repo = repo;
        }

        /// <summary>
        /// Get Camps
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<CampModel[]>> Get(bool includeTalks = true)
        {
            try
            {
                var results = await _repo.GetAllCampsAsync(includeTalks);

                return _mapper.Map<CampModel[]>(results);
            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("{moniker}")]
        [MapToApiVersion("1.0")]
        public async Task<ActionResult<CampModel>> Get(string moniker)
        {
            try
            {
                var result = await _repo.GetCampAsync(moniker, false);

                if (result == null)
                {
                    return NotFound();
                }

                return _mapper.Map<CampModel>(result);
            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("{moniker}")]
        [MapToApiVersion("1.1")]
        public async Task<ActionResult<CampModel>> Get11(string moniker)
        {
            try
            {
                var result = await _repo.GetCampAsync(moniker, true);

                if (result == null)
                {
                    return NotFound();
                }

                return _mapper.Map<CampModel>(result);
            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<CampModel[]>> SearchByDate(DateTime theDate, bool includeTalks = true)
        {
            try
            {
                var results = await _repo.GetAllCampsByEventDate(theDate, includeTalks);

                if (!results.Any())
                {
                    return NotFound();
                }

                return _mapper.Map<CampModel[]>(results);
            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpPost]
        public async Task<ActionResult<CampModel>> Post(CampModel model)
        {
            try
            {
                var c = await _repo.GetCampAsync(model.Moniker);

                if (c != null)
                {
                    return BadRequest("Moniker already in use. Try again.");
                }

                var location = _linkgen.GetPathByAction("Get", "Camps", new { moniker = model.Moniker });

                if (string.IsNullOrWhiteSpace(location))
                {
                    return BadRequest("Could not use current moniker");
                }

                // Create a new Camp
                var camp = _mapper.Map<Camp>(model);

                _repo.Add(camp);

                if (await _repo.SaveChangesAsync())
                {
                    return Created("", _mapper.Map<CampModel>(camp));
                }
            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

            return BadRequest();
        }

        [HttpPut("{moniker}")]
        public async Task<ActionResult<CampModel>> Put(string moniker, CampModel model)
        {
            try
            {
                var old = await _repo.GetCampAsync(model.Moniker);

                if (old == null)
                {
                    return NotFound($"Could not find camp with moniker: {moniker}");
                }

                _mapper.Map(model, old);

                if (await _repo.SaveChangesAsync())
                {
                    return _mapper.Map<CampModel>(old);
                }                
            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

            return BadRequest();
        }

        [HttpDelete("{moniker}")]
        public async Task<IActionResult> Delete(string moniker)
        {
            try
            {
                var camp = await _repo.GetCampAsync(moniker);

                if (camp == null)
                {
                    return NotFound();
                }

                _repo.Delete(camp);

                if (await _repo.SaveChangesAsync())
                {
                    return Ok();
                }
            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

            return BadRequest();
        }
    }
}
