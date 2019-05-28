using ERP.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace ERP.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/stockinflow")]
    public class StockInflowController : ApiController
    {
        //public void Test1()
        //{
        //    using (erpdbEntities context = new erpdbEntities())
        //    {
        //        using (DbContextTransaction transaction = context.Database.BeginTransaction())
        //        {
        //            try
        //            {
        //                var InStocks = context.InStocks.Add(new InStock() { name = "PU" });
        //                context.SaveChanges();

        //                transaction.Commit();
        //            }
        //            catch (Exception e)
        //            {
        //                transaction.Rollback();
        //            }
        //        }
        //    }
        //}

        //public void Test2()
        //{
        //    using (erpdbEntities context = new erpdbEntities())
        //    {
        //        try
        //        {
        //            var InStocks = context.InStocks.Add(new InStock() { name = "PU" });
        //            context.SaveChanges();
        //        }
        //        catch (Exception e)
        //        {
        //        }
        //    }
        //}

        [HttpPost, Route("AddOrUpdatePurchaseOrder")]
        public IHttpActionResult AddOrUpdatePurchaseOrder(PurchaseOrder purchaseOrder)
        {
            SuccessResponse response = new SuccessResponse();

            if (purchaseOrder.OutputCode != null)
            {
                if (purchaseOrder.InputCodes != null && purchaseOrder.InputCodes.Length > 0)
                {
                    using (var context = new erpdbEntities())
                    {
                        string outputCodeSuccessMsg = "Output Code added successfully.";
                        string outputCodeErrorMsg = "Some error occurred while adding Output Codes.";
                        try
                        {
                            OutputCode output = null;

                            if (purchaseOrder.OutputCode.OutputCodeId == 0)
                            {
                                purchaseOrder.OutputCode.CreateDate = DateTime.Now;
                                purchaseOrder.OutputCode.EditDate = DateTime.Now;

                                output = context.OutputCodes.Add(purchaseOrder.OutputCode);
                            }
                            else
                            {
                                outputCodeSuccessMsg = "Output Code updated successfully.";
                                outputCodeErrorMsg = "Some error occurred while updating Output Codes.";

                                purchaseOrder.OutputCode.EditDate = DateTime.Now;
                                output = context.OutputCodes.Where(x => x.OutputCodeId == purchaseOrder.OutputCode.OutputCodeId).FirstOrDefault();

                                output.OutputCodeNo = purchaseOrder.OutputCode.OutputCodeNo;
                                output.OutputMaterialDesc = purchaseOrder.OutputCode.OutputMaterialDesc;
                                output.OutputQuantity = purchaseOrder.OutputCode.OutputQuantity;
                                output.ProjectName = purchaseOrder.OutputCode.ProjectName;
                            }

                            response.Id = output.OutputCodeId;
                            response.Message = outputCodeSuccessMsg;
                            response.StatusCode = HttpStatusCode.OK;

                            string inputCodeSuccessMsg = "Input Codes added successfully.";
                            string inputCodeErrorMsg = "Some error occurred while adding Input Codes.";
                            try
                            {
                                if (purchaseOrder.OutputCode.OutputCodeId > 0)
                                {
                                    context.InputCodes.RemoveRange(output.InputCodes);
                                    inputCodeSuccessMsg = "Input Codes updated successfully.";
                                    inputCodeErrorMsg = "Some error occurred while updating Input Codes.";
                                }

                                foreach (var inputCode in purchaseOrder.InputCodes)
                                {
                                    inputCode.InputCodeId = 0;
                                    inputCode.CreateDate = DateTime.Now;
                                    inputCode.EditDate = DateTime.Now;
                                    inputCode.OutputCodeId = output.OutputCodeId;

                                    context.InputCodes.Add(inputCode);
                                }

                                response.Id = output.OutputCodeId;
                                response.Message = inputCodeSuccessMsg;
                                response.StatusCode = HttpStatusCode.OK;

                                context.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                response.Message = inputCodeErrorMsg;
                                response.StatusCode = HttpStatusCode.InternalServerError;
                            }
                        }
                        catch (Exception e)
                        {
                            response.Message = outputCodeErrorMsg;
                            response.StatusCode = HttpStatusCode.InternalServerError;
                        }
                    }
                }
                else
                {
                    response.Message = "Please enter atleast one Input Code.";
                    response.StatusCode = HttpStatusCode.BadRequest;
                }
            }
            else
            {
                response.Message = "Output Code cannot be null.";
                response.StatusCode = HttpStatusCode.BadRequest;
            }

            return Ok(response);
        }

        [HttpGet, Route("GetAllPurchaseOrders")]
        public IHttpActionResult GetAllPurchaseOrders()
        {
            using (var context = new erpdbEntities())
            {
                List<PurchaseOrderModel> purchaseOrders = new List<PurchaseOrderModel>();

                var outputCodes = context.OutputCodes.ToList();

                foreach (var outputCode in outputCodes)
                {
                    PurchaseOrderModel purchaseOrder = new PurchaseOrderModel();
                    purchaseOrder.OutputCode = outputCode;
                    purchaseOrder.InputCodes = new List<InputCodeModel>();

                    foreach(var inputCode in outputCode.InputCodes)
                    {
                        InputCodeModel inputCodeModel = new InputCodeModel();
                        inputCodeModel.BASFChallanNo = inputCode.BASFChallanNo;
                        inputCodeModel.CreateDate = inputCode.CreateDate;
                        inputCodeModel.EditDate = inputCode.EditDate;
                        inputCodeModel.InputCodeId = inputCode.InputCodeId;
                        inputCodeModel.InputCodeNo = inputCode.InputCodeNo;
                        inputCodeModel.InputMaterialDesc = inputCode.InputMaterialDesc;
                        inputCodeModel.InputQuantity = inputCode.InputQuantity;
                        inputCodeModel.OutputCodeId = inputCode.OutputCodeId;
                        inputCodeModel.PartTypeId = inputCode.PartTypeId;
                        inputCodeModel.PartTypeName = inputCode.PartType.PartTypeName;
                        inputCodeModel.SplitQuantity = inputCode.SplitQuantity;

                        purchaseOrder.InputCodes.Add(inputCodeModel);
                    }

                    purchaseOrders.Add(purchaseOrder);
                }

                return Ok(purchaseOrders);
            }
        }
    }    
}
