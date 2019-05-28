using ERP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace ERP.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/general")]
    public class GeneralController : ApiController
    {
        [HttpPost, Route("AddOrUpdatePartType")]
        public IHttpActionResult AddOrUpdatePartType(PartType partType)
        {
            SuccessResponse response = new SuccessResponse();

            if (!string.IsNullOrEmpty(partType.PartTypeName))
            {
                using (var context = new erpdbEntities())
                {
                    try
                    {
                        if (partType.PartTypeId == 0)   // Add
                        {
                            partType.CreateDate = DateTime.Now;
                            partType.EditDate = DateTime.Now;
                            context.PartTypes.Add(partType);

                            response.Id = context.SaveChanges();
                            response.Message = "Part Type added successfully";
                        }
                        else
                        {
                            var existingPartType = context.PartTypes.Where(x => x.PartTypeId == partType.PartTypeId).FirstOrDefault();
                            existingPartType.EditDate = DateTime.Now;

                            response.Id = context.SaveChanges();
                            response.Message = "Part Type updated successfully.";
                        }

                        response.StatusCode = HttpStatusCode.OK;
                    }
                    catch (Exception e)
                    {
                        response.Message = "Some error occurred while performing transaction.";
                        response.StatusCode = HttpStatusCode.InternalServerError;
                    }
                }
            }
            else
            {
                response.Message = "Please enter Part Type.";
                response.StatusCode = HttpStatusCode.BadRequest;
            }

            return Ok(response);
        }

        [HttpGet, Route("GetAllPartTypes")]
        public IHttpActionResult GetAllPartTypes()
        {
            using (var context = new erpdbEntities())
            {                
                return Ok(context.PartTypes.ToArray());
            }
        }
    }
}
