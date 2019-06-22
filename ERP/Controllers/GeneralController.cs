using ERP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

            if (!string.IsNullOrEmpty(productDetail.InputCode) && !string.IsNullOrEmpty(productDetail.InputMaterialDesc) && !string.IsNullOrEmpty(productDetail.OutputCode) && !string.IsNullOrEmpty(productDetail.OutputMaterialDesc))
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
                            existingProduct.ProductTypeId = productDetail.ProductTypeId;
                            existingProduct.InputCode = productDetail.InputCode;
                            existingProduct.InputMaterialDesc = productDetail.InputMaterialDesc;
                            existingProduct.OutputCode = productDetail.OutputCode;
                            existingProduct.OutputMaterialDesc = productDetail.OutputMaterialDesc;
                            existingProduct.ProjectName = productDetail.ProjectName;
                            existingProduct.SplitRatio = productDetail.SplitRatio;

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
                        challanProductModel.ChallanDetail = challanProduct.ChallanDetail;
                        challanProductModel.ChallanDeductions = challanProduct.ChallanDeductions;
                        if (challanProductModel.ChallanDeductions != null)
                        {
                            var inputQuantity = challanProductModel.ChallanProduct.InputQuantity ?? 0;
                            challanProductModel.RemainingQuantity = (inputQuantity - challanProductModel.ChallanDeductions.Sum(x => x.OutQuantity)) ?? inputQuantity;
                        }
                        challanProducts.Add(challanProductModel);
                    }

                    model.ChallanDetail = challan;
                    model.ChallanProducts = challanProducts.ToArray();

                    modelList.Add(model);
                }

                return Ok(modelList);
            }
        }

        [HttpGet, Route("GetProductRemainingQuantity")]
        public IHttpActionResult GetProductRemainingQuantity()
        {
            using (var context = new erpdbEntities())
            {
                try
                {
                    var products = context.ProductDetails.ToList();

                    List<ProductQuantity> productQnts = new List<ProductQuantity>();
                    foreach (var product in products)
                    {
                        var inQnt = context.ChallanProducts.Where(x => x.ProductId == product.ProductId).Sum(l => l.InputQuantity * l.ProductDetail.SplitRatio) ?? 0;
                        var outQnt = context.ChallanDeductions.Where(x => x.ChallanProduct.ProductId == product.ProductId).Sum(l => l.OutQuantity) ?? 0;

                        var remainingQuantity = Convert.ToInt32(inQnt) - Convert.ToInt32(outQnt);
                        if (remainingQuantity > 0)
                        {
                            ProductQuantity productQnty = new ProductQuantity();
                            productQnty.ProductId = Convert.ToInt32(product.ProductId);
                            productQnty.ProductName = product.InputMaterialDesc;
                            productQnty.RemainingQuantity = Convert.ToInt32(remainingQuantity);
                            productQnts.Add(productQnty);
                        }
                    }

                    return Ok(productQnts);
                }
                catch (Exception e)
                {
                    return InternalServerError();
                }
            }
        }

        [HttpPost, Route("AddOrUpdateVendorChallan")]
        public IHttpActionResult AddOrUpdateVendorChallan(VendorChallanModel model)
        {
            GetChallanDeductions(model.OutStocks);

            SuccessResponse response = new SuccessResponse();

            using (var context = new erpdbEntities())
            {
                try
                {
                    VendorChallan vendorChallan = new VendorChallan();
                    vendorChallan.VendorChallanDate = model.VendorChallanDate;
                    vendorChallan.CreateDate = DateTime.Now;
                    vendorChallan.EditDate = DateTime.Now;

                    context.VendorChallans.Add(vendorChallan);
                    context.SaveChanges();

                    foreach (OutStockModel outStockModel in model.OutStocks)
                    {
                        OutStock outStock = new OutStock();
                        outStock.VendorChallanNo = vendorChallan.VendorChallanNo;
                        outStock.OutputQuantity = outStockModel.OutputQuantity;
                        outStock.CreateDate = DateTime.Now;
                        outStock.EditDate = DateTime.Now;

                        context.OutStocks.Add(outStock);
                        context.SaveChanges();

                        foreach (ChallanDeductionModel challanDeductionModel in outStockModel.ChallanDeductions)
                        {
                            ChallanDeduction challanDeduction = new ChallanDeduction();
                            challanDeduction.CreateDate = DateTime.Now;
                            challanDeduction.EditDate = DateTime.Now;
                            challanDeduction.OutStockId = outStock.OutStockId;
                            challanDeduction.OutQuantity = challanDeductionModel.OutQuantity;
                            challanDeduction.ChallanProductId = challanDeductionModel.ChallanProductId;

                            context.ChallanDeductions.Add(challanDeduction);
                            context.SaveChanges();
                        }
                    }

                    response.Id = vendorChallan.VendorChallanNo;
                    response.Message = "Vendor challan successfully saved.";
                    response.StatusCode = HttpStatusCode.OK;

                    return Ok(response);
                }
                catch (Exception e)
                {
                    return InternalServerError();
                }
            }
        }

        public void GetChallanDeductions(OutStockModel[] outStocks)
        {
            foreach (var outStock in outStocks)
            {
                var productIdModel = new ProductIdModel();
                productIdModel.ProductId = outStock.ProductId;
                var result = GetAllBASFChallanByProductIdPrivate(productIdModel);

                var basfChallanSelection = result.BASFChallanSelections;

                var outputQnt = outStock.OutputQuantity;

                List<ChallanDeductionModel> challanDeductions = new List<ChallanDeductionModel>();
                foreach (var challan in basfChallanSelection)
                {
                    var challanDeduction = new ChallanDeductionModel();

                    if (outputQnt > 0)
                    {
                        if (challan.RemainingQuantity < outputQnt)
                        {
                            challan.OutQuantity = challan.RemainingQuantity;
                            outputQnt -= challan.RemainingQuantity;
                            challan.QntAfterDeduction = 0;
                        }
                        else
                        {
                            challan.OutQuantity = outputQnt;
                            outputQnt = 0;
                            challan.QntAfterDeduction = challan.RemainingQuantity - challan.OutQuantity;
                        }

                        challan.IsChecked = true;

                        challanDeduction.ChallanProductId = challan.ChallanProduct.ChallanProductId;
                        challanDeduction.OutQuantity = challan.OutQuantity;

                        challanDeductions.Add(challanDeduction);
                        outStock.ChallanDeductions = challanDeductions.ToArray();
                    }
                    else
                    {
                        challan.QntAfterDeduction = challan.RemainingQuantity;
                    }
                }
            }
        }

        private BASFChallanDeduction GetAllBASFChallanByProductIdPrivate(ProductIdModel model)
        {
            int productId = model.ProductId;
            using (var context = new erpdbEntities())
            {
                try
                {
                    BASFChallanDeduction basfChallanDeduction = new BASFChallanDeduction();

                    BASFChallanSelection[] basfChallanSelection = context.ChallanDetails.Select(x => new BASFChallanSelection { ChallanDetail = x, ChallanProduct = x.ChallanProducts.Where(p => p.ProductId == productId).FirstOrDefault(), InputQuantity = x.ChallanProducts.Where(p => p.ProductId == productId).Sum(p => p.InputQuantity * p.ProductDetail.SplitRatio), OutputQuantity = context.ChallanDeductions.Where(z => z.ChallanProduct.ProductId == productId).Sum(p => p.OutQuantity).Value }).OrderBy(x => x.ChallanDetail.ChallanDate).ToArray();

                    List<BASFChallanSelection> selection = new List<BASFChallanSelection>();
                    foreach (var basfChallan in basfChallanSelection)
                    {
                        //basfChallan.RemainingQuantity = (basfChallan.InputQuantity ?? 0) - (basfChallan.OutputQuantity ?? 0);

                        if (basfChallan.ChallanProduct != null)
                        {
                            basfChallan.InputQuantity = (basfChallan.ChallanProduct.InputQuantity ?? 0) * (basfChallan.ChallanProduct.ProductDetail.SplitRatio ?? 1);
                            basfChallan.OutputQuantity = basfChallan.ChallanProduct.ChallanDeductions.Where(x => x.ChallanProductId == basfChallan.ChallanProduct.ChallanProductId).Sum(x => x.OutQuantity) ?? 0;
                            basfChallan.RemainingQuantity = (basfChallan.InputQuantity ?? 0) - (basfChallan.OutputQuantity ?? 0);

                            if (basfChallan.RemainingQuantity > 0)
                                selection.Add(basfChallan);
                        }
                    }

                    basfChallanDeduction.BASFChallanSelections = selection.ToArray();

                    return basfChallanDeduction;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }

        [HttpPost, Route("GetAllBASFChallanByProductId")]
        public IHttpActionResult GetAllBASFChallanByProductId(ProductIdModel model)
        {
            int productId = model.ProductId;
            using (var context = new erpdbEntities())
            {
                try
                {
                    BASFChallanDeduction basfChallanDeduction = new BASFChallanDeduction();

                    BASFChallanSelection[] basfChallanSelection = context.ChallanDetails.Select(x => new BASFChallanSelection { ChallanDetail = x, ChallanProduct = x.ChallanProducts.Where(p => p.ProductId == productId).FirstOrDefault(), InputQuantity = x.ChallanProducts.Where(p => p.ProductId == productId).Sum(p => p.InputQuantity * p.ProductDetail.SplitRatio), OutputQuantity = context.ChallanDeductions.Where(z => z.ChallanProduct.ProductId == productId).Sum(p => p.OutQuantity).Value }).OrderBy(x => x.ChallanDetail.ChallanDate).ToArray();

                    List<BASFChallanSelection> selection = new List<BASFChallanSelection>();
                    foreach (var basfChallan in basfChallanSelection)
                    {
                        //basfChallan.RemainingQuantity = (basfChallan.InputQuantity ?? 0) - (basfChallan.OutputQuantity ?? 0);

                        if (basfChallan.ChallanProduct != null)
                        {
                            basfChallan.InputQuantity = (basfChallan.ChallanProduct.InputQuantity ?? 0) * (basfChallan.ChallanProduct.ProductDetail.SplitRatio ?? 1);
                            basfChallan.OutputQuantity = basfChallan.ChallanProduct.ChallanDeductions.Where(x => x.ChallanProductId == basfChallan.ChallanProduct.ChallanProductId).Sum(x => x.OutQuantity) ?? 0;
                            basfChallan.RemainingQuantity = (basfChallan.InputQuantity ?? 0) - (basfChallan.OutputQuantity ?? 0);

                            if (basfChallan.RemainingQuantity > 0)
                                selection.Add(basfChallan);
                        }
                    }

                    basfChallanDeduction.BASFChallanSelections = selection.ToArray();

                    return Ok(basfChallanDeduction);
                }
                catch (Exception e)
                {
                    return InternalServerError();
                }
            }
        }

        [HttpGet, Route("GetAllVendorChallans")]
        public IHttpActionResult GetAllVendorChallans()
        {
            using (var context = new erpdbEntities())
            {
                try
                {
                    var vendorChallans = context.VendorChallans.ToList();

                    List<VendorChallanModel> modelList = new List<VendorChallanModel>();
                    foreach (var vendorChallan in vendorChallans)
                    {
                        VendorChallanModel model = new VendorChallanModel();
                        model.VendorChallanNo = vendorChallan.VendorChallanNo;
                        model.VendorChallanDate = vendorChallan.VendorChallanDate ?? new DateTime();
                        model.CreateDate = vendorChallan.CreateDate ?? new DateTime();
                        model.EditDate = vendorChallan.EditDate ?? new DateTime();

                        List<OutStockModel> outStockModelList = new List<OutStockModel>();
                        foreach (var outStock in vendorChallan.OutStocks)
                        {
                            OutStockModel outStockModel = new OutStockModel();
                            outStockModel.VendorChallanNo = outStock.VendorChallanNo ?? 0;
                            outStockModel.OutputCode = outStock.OutStockId;
                            outStockModel.OutputQuantity = outStock.OutputQuantity ?? 0;
                            outStockModel.CreateDate = outStock.CreateDate ?? new DateTime();
                            outStockModel.EditDate = outStock.EditDate ?? new DateTime();

                            List<ChallanDeductionModel> challanDeductionModelList = new List<ChallanDeductionModel>();
                            foreach (var challanDeduction in outStock.ChallanDeductions)
                            {
                                ChallanDeductionModel challanDeductionModel = new ChallanDeductionModel();
                                challanDeductionModel.ChallanDeductionId = challanDeduction.ChallanDeductionId;

                                ChallanProductModel challanProductModel = new ChallanProductModel();
                                challanProductModel.ChallanDeductions = null;
                                challanProductModel.ChallanProduct = challanDeduction.ChallanProduct;
                                challanProductModel.ProductDetail = challanDeduction.ChallanProduct.ProductDetail;
                                challanProductModel.ChallanDetail = challanDeduction.ChallanProduct.ChallanDetail;

                                var inputQuantity = (challanProductModel.ChallanProduct.InputQuantity ?? 0) * (challanProductModel.ProductDetail.SplitRatio ?? 1);
                                challanProductModel.RemainingQuantity = (inputQuantity - challanDeduction.ChallanProduct.ChallanDeductions.Sum(x => x.OutQuantity)) ?? inputQuantity;

                                challanDeductionModel.ChallanProduct = challanProductModel;
                                challanDeductionModel.ChallanProductId = challanDeduction.ChallanProductId ?? 0;
                                challanDeductionModel.CreateDate = challanDeduction.CreateDate ?? new DateTime();
                                challanDeductionModel.EditDate = challanDeduction.EditDate ?? new DateTime();
                                challanDeductionModel.OutputCode = challanDeduction.OutStockId ?? 0;
                                challanDeductionModel.OutQuantity = challanDeduction.OutQuantity ?? 0;

                                challanDeductionModelList.Add(challanDeductionModel);
                            }

                            outStockModel.ChallanDeductions = challanDeductionModelList.ToArray();

                            outStockModelList.Add(outStockModel);
                        }

                        model.OutStocks = outStockModelList.ToArray();

                        modelList.Add(model);
                    }

                    return Ok(modelList);
                }
                catch (Exception e)
                {
                    return InternalServerError();
                }
            }
        }

        [HttpPost, Route("GetVendorChallanByVendorChallanNo")]
        public IHttpActionResult GetVendorChallanByVendorChallanNo(VendorChallanNoModel vendorChallanNoModel)
        {
            using (var context = new erpdbEntities())
            {
                try
                {
                    var vendorChallan = context.VendorChallans.Where(x => x.VendorChallanNo == vendorChallanNoModel.VendorChallanNo).FirstOrDefault();

                    VendorChallanModel model = new VendorChallanModel();
                    model.VendorChallanNo = vendorChallan.VendorChallanNo;
                    model.VendorChallanDate = vendorChallan.VendorChallanDate ?? new DateTime();
                    model.CreateDate = vendorChallan.CreateDate ?? new DateTime();
                    model.EditDate = vendorChallan.EditDate ?? new DateTime();

                    List<OutStockModel> outStockModelList = new List<OutStockModel>();

                    foreach (var outStock in vendorChallan.OutStocks)
                    {
                        OutStockModel outStockModel = new OutStockModel();
                        outStockModel.VendorChallanNo = outStock.VendorChallanNo ?? 0;
                        outStockModel.OutputCode = outStock.OutStockId;
                        outStockModel.OutputQuantity = outStock.OutputQuantity ?? 0;
                        outStockModel.CreateDate = outStock.CreateDate ?? new DateTime();
                        outStockModel.EditDate = outStock.EditDate ?? new DateTime();

                        List<ChallanDeductionModel> challanDeductionModelList = new List<ChallanDeductionModel>();
                        foreach (var challanDeduction in outStock.ChallanDeductions)
                        {
                            ChallanDeductionModel challanDeductionModel = new ChallanDeductionModel();
                            challanDeductionModel.ChallanDeductionId = challanDeduction.ChallanDeductionId;

                            ChallanProductModel challanProductModel = new ChallanProductModel();
                            challanProductModel.ChallanDeductions = null;
                            challanProductModel.ChallanProduct = challanDeduction.ChallanProduct;
                            challanProductModel.ProductDetail = challanDeduction.ChallanProduct.ProductDetail;
                            challanProductModel.ChallanDetail = challanDeduction.ChallanProduct.ChallanDetail;

                            var inputQuantity = (challanProductModel.ChallanProduct.InputQuantity ?? 0) * (challanProductModel.ProductDetail.SplitRatio ?? 1);
                            challanProductModel.RemainingQuantity = (inputQuantity - challanDeduction.ChallanProduct.ChallanDeductions.Sum(x => x.OutQuantity)) ?? inputQuantity;

                            challanDeductionModel.ChallanProduct = challanProductModel;
                            challanDeductionModel.ChallanProductId = challanDeduction.ChallanProductId ?? 0;
                            challanDeductionModel.CreateDate = challanDeduction.CreateDate ?? new DateTime();
                            challanDeductionModel.EditDate = challanDeduction.EditDate ?? new DateTime();
                            challanDeductionModel.OutputCode = challanDeduction.OutStockId ?? 0;
                            challanDeductionModel.OutQuantity = challanDeduction.OutQuantity ?? 0;

                            challanDeductionModelList.Add(challanDeductionModel);
                        }

                        outStockModel.ChallanDeductions = challanDeductionModelList.ToArray();

                        outStockModelList.Add(outStockModel);
                    }

                    model.OutStocks = outStockModelList.ToArray();

                    return Ok(model);
                }
                catch (Exception e)
                {
                    return InternalServerError();
                }
            }
        }

        [HttpGet, Route("GetAllProductTypes")]
        public IHttpActionResult GetAllProductTypes()
        {
            using (var context = new erpdbEntities())
            {
                return Ok(context.ProductTypes.ToArray());
            }
        }
    }
}
