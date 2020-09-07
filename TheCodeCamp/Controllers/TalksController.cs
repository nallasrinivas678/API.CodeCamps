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

    //This is an association controller for Camp Controller
    [RoutePrefix("api/camps/{moniker}/talks")]
    public class TalksController : ApiController
    {
        private readonly ICampRepository _repository;
        private readonly IMapper _mapper;

        public TalksController(ICampRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [Route()]
        public async Task<IHttpActionResult> Get(string moniker, bool includeSpeakers = false)
        {
            try
            {
                var results = await _repository.GetTalksByMonikerAsync(moniker, includeSpeakers);
                return Ok(_mapper.Map<IEnumerable<TalkModel>>(results));
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }
        }

        [Route("{id:int}", Name = "GetTalk")]

        public async Task<IHttpActionResult> Get(string moniker, int id, bool includeSpeakers = false)
        {
            try
            {
                var talk = await _repository.GetTalkByMonikerAsync(moniker, id, includeSpeakers);
                if (talk == null) return NotFound();
                return Ok(_mapper.Map<TalkModel>(talk));
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }
        }

        [Route()]
        public async Task<IHttpActionResult> Post(string moniker, TalkModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var camp = await _repository.GetCampAsync(moniker);
                    if (camp != null)
                    {
                        var talk = _mapper.Map<Talk>(model);
                        talk.Camp = camp;

                        //Map the speaker if provided
                        if (model.Speaker != null)
                        {
                            var speaker = await _repository.GetSpeakerAsync(model.Speaker.SpeakerId);
                            if (speaker != null) talk.Speaker = speaker;
                        }

                        _repository.AddTalk(talk);
                        if (await _repository.SaveChangesAsync())
                        {
                            return CreatedAtRoute("GetTalk",
                                new { moniker = moniker, id = talk.TalkId }, _mapper.Map<TalkModel>(talk));
                        }
                    }
                }
              
            }
            catch (Exception ex)
            {

                return InternalServerError();
            }
            return BadRequest(ModelState);
        }

        [Route("{talkId:int}")]
        public async Task<IHttpActionResult> Put(string moniker,int talkId,TalkModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var talk = await _repository.GetTalkByMonikerAsync(moniker,talkId,true);
                    if (talk == null) return NotFound();
                    //dont update the speaker, configure in mapping profile
                    _mapper.Map(model, talk);

                    //update speaker if necessary
                    if (talk.Speaker.SpeakerId != model.Speaker.SpeakerId)
                    {
                        var speaker = await _repository.GetSpeakerAsync(model.Speaker.SpeakerId);
                        if (speaker != null) talk.Speaker = speaker;
                    }

                    if (await _repository.SaveChangesAsync())
                    {
                        return Ok(_mapper.Map<TalkModel>(talk));
                    }
                }
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }
            return BadRequest(ModelState);
        }

        //To delete a  talk
        [Route("{talkId:int}")]
        public async Task<IHttpActionResult> Delete(string moniker, int talkId)
        {
            try
            {
                var talk = await _repository.GetTalkByMonikerAsync(moniker,talkId);
                if (talk == null) return NotFound();
                _repository.DeleteTalk(talk);
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