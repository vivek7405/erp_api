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

        [HttpPost, Route("AddOrUpdateProductDetail")]
        public IHttpActionResult AddOrUpdateProductDetail(ProductDetail productDetail)
        {
            SuccessResponse response = new SuccessResponse();

            if (!string.IsNullOrEmpty(productDetail.ProductName))
            {
                using (var context = new erpdbEntities())
                {
                    try
                    {
                        if (productDetail.ProductId == 0)   // Add
                        {
                            productDetail.CreateDate = DateTime.Now;
                            productDetail.EditDate = DateTime.Now;
                            context.ProductDetails.Add(productDetail);

                            response.Id = context.SaveChanges();
                            response.Message = "Product added successfully";
                        }
                        else
                        {
                            var existingProduct = context.ProductDetails.Where(x => x.ProductId == productDetail.ProductId).FirstOrDefault();
                            existingProduct.EditDate = DateTime.Now;
                            existingProduct.InputCode = productDetail.InputCode;
                            existingProduct.ProductName = productDetail.ProductName;
                            existingProduct.SplitQuantity = productDetail.SplitQuantity;

                            response.Id = context.SaveChanges();
                            response.Message = "Product updated successfully.";
                        }

                        response.StatusCode = HttpStatusCode.OK;
                    }
                    catch (Exception e)
                    {
                        response.Message = "Some error occurred while performing transaction.";
                        response.StatusCode = HttpStatusCode.InternalServerError;

                        return InternalServerError();
                    }
                }
            }
            else
            {
                response.Message = "Product details doesn't seem to be entered correctly.";
                response.StatusCode = HttpStatusCode.BadRequest;

                return BadRequest(response.Message);
            }

            return Ok(response);
        }

        [HttpGet, Route("GetAllProductDetails")]
        public IHttpActionResult GetAllProductDetails()
        {
            using (var context = new erpdbEntities())
            {
                return Ok(context.ProductDetails.ToArray());
            }
        }

        [HttpPost, Route("AddOrUpdateChallan")]
        public IHttpActionResult AddOrUpdateChallan(ChallanDetailModel model)
        {
            SuccessResponse response = new SuccessResponse();

            var challanDetail = model.ChallanDetail;
            var challanProducts = model.ChallanProducts;

            if (!string.IsNullOrEmpty(challanDetail.ChallanNo) && challanProducts.Length > 0)
            {
                using (var context = new erpdbEntities())
                {
                    try
                    {
                        if (challanDetail.ChallanId == 0)   // Add
                        {
                            challanDetail.CreateDate = DateTime.Now;
                            challanDetail.EditDate = DateTime.Now;
                            context.ChallanDetails.Add(challanDetail);

                            context.SaveChanges();
                            response.Id = challanDetail.ChallanId;
                            response.Message = "Challan added successfully";
                        }
                        else
                        {
                            var existingChallan = context.ChallanDetails.Where(x => x.ChallanId == challanDetail.ChallanId).FirstOrDefault();
                            existingChallan.EditDate = DateTime.Now;
                            existingChallan.ChallanNo = challanDetail.ChallanNo;
                            existingChallan.ChallanDate = challanDetail.ChallanDate;

                            context.SaveChanges();
                            response.Id = challanDetail.ChallanId;
                            response.Message = "Challan updated successfully.";
                        }

                        response.StatusCode = HttpStatusCode.OK;

                        foreach (var challanProduct in challanProducts)
                        {
                            challanProduct.ChallanId = challanDetail.ChallanId;
                            if (challanProduct.ChallanId > 0 && challanProduct.ProductId > 0)
                            {

                                try
                                {
                                    if (challanProduct.ChallanProductId == 0)   // Add
                                    {
                                        challanProduct.CreateDate = DateTime.Now;
                                        challanProduct.EditDate = DateTime.Now;
                                        context.ChallanProducts.Add(challanProduct);

                                        response.Message = "Challan product added successfully.";
                                    }
                                    else
                                    {
                                        var existingChallanProduct = context.ChallanProducts.Where(x => x.ChallanProductId == challanProduct.ChallanProductId).FirstOrDefault();
                                        existingChallanProduct.EditDate = DateTime.Now;
                                        existingChallanProduct.ChallanId = challanProduct.ChallanId;
                                        existingChallanProduct.ProductId = challanProduct.ProductId;
                                        existingChallanProduct.InputQuantity = challanProduct.InputQuantity;

                                        response.Message = "Challan Product updated successfully.";
                                    }

                                    response.StatusCode = HttpStatusCode.OK;
                                }
                                catch (Exception e)
                                {
                                    response.Message = "Some error occurred while performing transaction.";
                                    response.StatusCode = HttpStatusCode.InternalServerError;

                                    return InternalServerError();
                                }
                            }
                            else
                            {
                                response.Message = "Challan Product details doesn't seem to be entered correctly.";
                                response.StatusCode = HttpStatusCode.BadRequest;

                                return BadRequest(response.Message);
                            }
                        }

                        context.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        response.Message = "Some error occurred while performing transaction.";
                        response.StatusCode = HttpStatusCode.InternalServerError;

                        return InternalServerError();
                    }
                }
            }
            else
            {
                response.Message = "Please enter a challan number.";
                response.StatusCode = HttpStatusCode.BadRequest;

                return BadRequest(response.Message);
            }

            return Ok(response);
        }

        [HttpPost, Route("AddOrUpdateChallanDetail")]
        public IHttpActionResult AddOrUpdateChallanDetail(ChallanDetail challanDetail)
        {
            SuccessResponse response = new SuccessResponse();

            if (!string.IsNullOrEmpty(challanDetail.ChallanNo))
            {
                using (var context = new erpdbEntities())
                {
                    try
                    {
                        if (challanDetail.ChallanId == 0)   // Add
                        {
                            challanDetail.CreateDate = DateTime.Now;
                            challanDetail.EditDate = DateTime.Now;
                            context.ChallanDetails.Add(challanDetail);

                            response.Id = context.SaveChanges();
                            response.Message = "Challan added successfully";
                        }
                        else
                        {
                            var existingChallan = context.ChallanDetails.Where(x => x.ChallanId == challanDetail.ChallanId).FirstOrDefault();
                            existingChallan.EditDate = DateTime.Now;
                            existingChallan.ChallanNo = challanDetail.ChallanNo;
                            existingChallan.ChallanDate = challanDetail.ChallanDate;

                            response.Id = context.SaveChanges();
                            response.Message = "Challan updated successfully.";
                        }

                        response.StatusCode = HttpStatusCode.OK;
                    }
                    catch (Exception e)
                    {
                        response.Message = "Some error occurred while performing transaction.";
                        response.StatusCode = HttpStatusCode.InternalServerError;

                        return InternalServerError();
                    }
                }
            }
            else
            {
                response.Message = "Please enter a challan number.";
                response.StatusCode = HttpStatusCode.BadRequest;

                return BadRequest(response.Message);
            }

            return Ok(response);
        }

        [HttpPost, Route("AddOrUpdateChallanProduct")]
        public IHttpActionResult AddOrUpdateChallanProduct(ChallanProduct[] challanProducts)
        {
            SuccessResponse response = new SuccessResponse();
            using (var context = new erpdbEntities())
            {
                foreach (var challanProduct in challanProducts)
                {
                    if (challanProduct.ChallanId > 0 && challanProduct.ProductId > 0)
                    {

                        try
                        {
                            if (challanProduct.ChallanProductId == 0)   // Add
                            {
                                challanProduct.CreateDate = DateTime.Now;
                                challanProduct.EditDate = DateTime.Now;
                                context.ChallanProducts.Add(challanProduct);

                                response.Message = "Challan product added successfully.";
                            }
                            else
                            {
                                var existingChallanProduct = context.ChallanProducts.Where(x => x.ChallanProductId == challanProduct.ChallanProductId).FirstOrDefault();
                                existingChallanProduct.EditDate = DateTime.Now;
                                existingChallanProduct.ChallanId = challanProduct.ChallanId;
                                existingChallanProduct.ProductId = challanProduct.ProductId;
                                existingChallanProduct.InputQuantity = challanProduct.InputQuantity;

                                response.Message = "Challan Product updated successfully.";
                            }

                            response.StatusCode = HttpStatusCode.OK;
                        }
                        catch (Exception e)
                        {
                            response.Message = "Some error occurred while performing transaction.";
                            response.StatusCode = HttpStatusCode.InternalServerError;

                            return InternalServerError();
                        }
                    }
                    else
                    {
                        response.Message = "Challan Product details doesn't seem to be entered correctly.";
                        response.StatusCode = HttpStatusCode.BadRequest;

                        return BadRequest(response.Message);
                    }
                }

                context.SaveChanges();
            }

            return Ok(response);
        }

        [HttpGet, Route("GetAllChallanDetails")]
        public IHttpActionResult GetAllChallanDetails()
        {
            using (var context = new erpdbEntities())
            {
                var challanDetails = context.ChallanDetails.ToArray();
                List<ViewChallanDetailModel> modelList = new List<ViewChallanDetailModel>();

                foreach (var challan in challanDetails)
                {
                    ViewChallanDetailModel model = new ViewChallanDetailModel();

                    List<ChallanProductModel> challanProducts = new List<ChallanProductModel>();                    
                    foreach (var challanProduct in challan.ChallanProducts)
                    {
                        ChallanProductModel challanProductModel = new ChallanProductModel();
                        challanProductModel.ChallanProduct = challanProduct;
                        challanProductModel.ProductDetail = challanProduct.ProductDetail;
                        challanProducts.Add(challanProductModel);
                    }

                    model.ChallanDetail = challan;
                    model.ChallanProducts = challanProducts.ToArray();

                    modelList.Add(model);
                }

                return Ok(modelList);
            }
        }        
    }
}
