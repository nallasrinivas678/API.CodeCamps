using AutoMapper;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using TheCodeCamp.Data;
using TheCodeCamp.Models;

namespace TheCodeCamp.Controllers
{
    [RoutePrefix("api/camps")]
    public class CampsController : ApiController
    {
        private readonly ICampRepository _repository;
        private readonly IMapper _mapper;
        public CampsController(ICampRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [Route()]
        public async Task<IHttpActionResult> Get(bool includeTalks = false)
        {
            try
            {
                var result = await _repository.GetAllCampsAsync(includeTalks);
                //Mapping
                var mappedResult = _mapper.Map<IEnumerable<CampModel>>(result);
                return Ok(mappedResult);
            }
            catch (Exception ex)
            {
                //Add logging functionality to log the errors
                return InternalServerError(ex);
            }
        }

        [Route("{moniker}", Name = "GetCamp")]
        public async Task<IHttpActionResult> Get(string moniker, bool includeTalks = false)
        {
            try
            {
                var result = await _repository.GetCampAsync(moniker, includeTalks);
                //Mapping
                if (result == null) return NotFound();
                var mappedResult = _mapper.Map<CampModel>(result);
                return Ok(mappedResult);
            }
            catch (Exception ex)
            {
                //Add logging functionality to log the errors
                return InternalServerError(ex);
            }
        }

        //Test URL : http://localhost:6600/api/camps/searchByDate/2018-10-18
        [Route("searchByDate/{eventDate:datetime}")]
        [HttpGet]
        public async Task<IHttpActionResult> SearchByEventDate(DateTime eventDate, bool includeTalks = false)
        {
            try
            {
                var result = await _repository.GetAllCampsByEventDate(eventDate, includeTalks);
                return Ok(_mapper.Map<CampModel[]>(result));
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }
        }


        //Post method means - to create new record
        [Route()]
        public async Task<IHttpActionResult> Post(CampModel model)
        {
            try
            {
                //check for unique record
                if (await _repository.GetCampAsync(model.Moniker) != null)
                {
                    ModelState.AddModelError("Moniker", "Moniker in use");
                }

                //To check if campmodel model is valid (required fields)
                if (ModelState.IsValid)
                {

                    var camp = _mapper.Map<Camp>(model);
                    _repository.AddCamp(camp);
                    if (await _repository.SaveChangesAsync())
                    {
                        var newModel = _mapper.Map<CampModel>(camp);
                        return CreatedAtRoute("GetCamp", new { moniker = newModel.Moniker }, newModel);
                    }
                }
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }
            return BadRequest(ModelState);
        }


        //Put method is meant for updating existing record
        [Route("{moniker}")]
        public async Task<IHttpActionResult> Put(string moniker, CampModel model)
        {
            try
            {
                var camp = await _repository.GetCampAsync(moniker);
                if (camp == null) return NotFound();
                _mapper.Map(model, camp);
                if (await _repository.SaveChangesAsync())
                {
                    return Ok(_mapper.Map<CampModel>(camp));
                }
                else return InternalServerError();
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }
        }

        [Route("{moniker}")]
        public async Task<IHttpActionResult> Delete(string moniker)
        {
            try
            {
                var camp = await _repository.GetCampAsync(moniker);
                if (camp == null) return NotFound();
                _repository.DeleteCamp(camp);
                if (await _repository.SaveChangesAsync())
                {
                    return Ok();
                }
                else return InternalServerError();
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }
        }
    }
}