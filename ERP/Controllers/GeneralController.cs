using ERP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Cors;
using static ERP.Enums.EProductCategory;

namespace ERP.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/general")]
    public class GeneralController : ApiController
    {
        [HttpPost, Route("AddOrUpdateProductDetail")]
        public IHttpActionResult AddOrUpdateProductDetail(ProductDetail productDetail)
        {
            SuccessResponse response = new SuccessResponse();

            //if (!string.IsNullOrEmpty(productDetail.InputCode) && !string.IsNullOrEmpty(productDetail.InputMaterialDesc) && !string.IsNullOrEmpty(productDetail.OutputCode) && !string.IsNullOrEmpty(productDetail.OutputMaterialDesc))
            //{
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
            //}
            //else
            //{
            //    response.Message = "Product details doesn't seem to be entered correctly.";
            //    response.StatusCode = HttpStatusCode.BadRequest;

            //    return BadRequest(response.Message);
            //}

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
                        challanProductModel.AccChallanDeductions = challanProduct.AccChallanDeductions;
                        if (challanProductModel.ChallanDeductions != null && challanProductModel.ChallanDeductions.Count > 0)
                        {
                            var inputQuantity = (challanProductModel.ChallanProduct.InputQuantity ?? 0) * (challanProductModel.ProductDetail.SplitRatio ?? 1);
                            challanProductModel.RemainingQuantity = ((inputQuantity - challanProductModel.ChallanDeductions.Sum(x => x.OutQuantity)) ?? inputQuantity) / (challanProductModel.ProductDetail.SplitRatio ?? 1);
                        }
                        else if (challanProductModel.AccChallanDeductions != null && challanProductModel.AccChallanDeductions.Count > 0)
                        {
                            var inputQuantity = (challanProductModel.ChallanProduct.InputQuantity ?? 0) * (challanProductModel.ProductDetail.SplitRatio ?? 1);
                            challanProductModel.RemainingQuantity = ((inputQuantity - challanProductModel.AccChallanDeductions.Sum(x => x.OutQuantity)) ?? inputQuantity) / (challanProductModel.ProductDetail.SplitRatio ?? 1);
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

        [HttpGet, Route("GetMainProductRemainingQuantity")]
        public IHttpActionResult GetMainProductRemainingQuantity()
        {
            using (var context = new erpdbEntities())
            {
                try
                {
                    int main = Convert.ToInt32(EProductCategorys.Main);
                    var products = context.ProductDetails.Where(x => x.ProductType.ProductCategoryId == main).ToList();

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

        [HttpGet, Route("GetAccProductRemainingQuantity")]
        public IHttpActionResult GetAccProductRemainingQuantity()
        {
            using (var context = new erpdbEntities())
            {
                try
                {
                    int acc = Convert.ToInt32(EProductCategorys.Accessories);
                    var products = context.ProductDetails.Where(x => x.ProductType.ProductCategoryId == acc).ToList();

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


                        foreach (OutAccModel outAccModel in outStockModel.OutAccs)
                        {
                            OutAcc outAcc = new OutAcc();
                            outAcc.OutStockId = outStock.OutStockId;
                            outAcc.OutputQuantity = outAccModel.OutputQuantity;
                            outAcc.CreateDate = DateTime.Now;
                            outAcc.EditDate = DateTime.Now;

                            context.OutAccs.Add(outAcc);
                            context.SaveChanges();

                            foreach (AccChallanDeductionModel accChallanDeductionModel in outAccModel.AccChallanDeductions)
                            {
                                AccChallanDeduction accChallanDeduction = new AccChallanDeduction();
                                accChallanDeduction.CreateDate = DateTime.Now;
                                accChallanDeduction.EditDate = DateTime.Now;
                                accChallanDeduction.OutAccId = outAcc.OutAccId;
                                accChallanDeduction.OutQuantity = accChallanDeductionModel.OutQuantity;
                                accChallanDeduction.ChallanProductId = accChallanDeductionModel.ChallanProductId;

                                context.AccChallanDeductions.Add(accChallanDeduction);
                                context.SaveChanges();
                            }
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
            foreach (OutStockModel outStock in outStocks)
            {
                if (outStock.ChallanDeductions == null || (outStock.ChallanDeductions != null && outStock.ChallanDeductions.Length == 0))
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

                foreach (OutAccModel outAcc in outStock.OutAccs)
                {
                    if (outAcc.AccChallanDeductions == null || (outAcc.AccChallanDeductions != null && outAcc.AccChallanDeductions.Length == 0))
                    {
                        var productIdModel = new ProductIdModel();
                        productIdModel.ProductId = outAcc.ProductId;
                        var result = GetAllBASFChallanByProductIdPrivate(productIdModel);

                        var basfChallanSelection = result.BASFChallanSelections;

                        var outputQnt = outAcc.OutputQuantity;

                        List<AccChallanDeductionModel> accChallanDeductions = new List<AccChallanDeductionModel>();
                        foreach (var challan in basfChallanSelection)
                        {
                            var accChallanDeduction = new AccChallanDeductionModel();

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

                                accChallanDeduction.ChallanProductId = challan.ChallanProduct.ChallanProductId;
                                accChallanDeduction.OutQuantity = challan.OutQuantity;

                                accChallanDeductions.Add(accChallanDeduction);
                                outAcc.AccChallanDeductions = accChallanDeductions.ToArray();
                            }
                            else
                            {
                                challan.QntAfterDeduction = challan.RemainingQuantity;
                            }
                        }
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
                            outStockModel.OutStockId = outStock.OutStockId;
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
                                challanProductModel.RemainingQuantity = ((inputQuantity - challanDeduction.ChallanProduct.ChallanDeductions.Sum(x => x.OutQuantity)) ?? inputQuantity) / (challanProductModel.ProductDetail.SplitRatio ?? 1);

                                challanDeductionModel.ChallanProduct = challanProductModel;
                                challanDeductionModel.ChallanProductId = challanDeduction.ChallanProductId ?? 0;
                                challanDeductionModel.CreateDate = challanDeduction.CreateDate ?? new DateTime();
                                challanDeductionModel.EditDate = challanDeduction.EditDate ?? new DateTime();
                                challanDeductionModel.OutStockId = challanDeduction.OutStockId ?? 0;
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
                        outStockModel.OutStockId = outStock.OutStockId;
                        outStockModel.VendorChallanNo = outStock.VendorChallanNo ?? 0;
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
                            challanProductModel.RemainingQuantity = ((inputQuantity - challanDeduction.ChallanProduct.ChallanDeductions.Where(x => x.CreateDate <= challanDeduction.CreateDate).Sum(x => x.OutQuantity)) ?? inputQuantity) / (challanProductModel.ProductDetail.SplitRatio ?? 1);

                            challanDeductionModel.ChallanProduct = challanProductModel;
                            challanDeductionModel.ChallanProductId = challanDeduction.ChallanProductId ?? 0;
                            challanDeductionModel.CreateDate = challanDeduction.CreateDate ?? new DateTime();
                            challanDeductionModel.EditDate = challanDeduction.EditDate ?? new DateTime();
                            challanDeductionModel.OutStockId = challanDeduction.OutStockId ?? 0;
                            challanDeductionModel.OutQuantity = (challanDeduction.OutQuantity ?? 0) / (challanProductModel.ProductDetail.SplitRatio ?? 1);

                            challanDeductionModelList.Add(challanDeductionModel);
                        }

                        outStockModel.ChallanDeductions = challanDeductionModelList.ToArray();

                        List<OutAccModel> outAccModelList = new List<OutAccModel>();
                        foreach (var outAcc in outStock.OutAccs)
                        {
                            OutAccModel outAccModel = new OutAccModel();
                            outAccModel.OutAccId = outAcc.OutAccId;
                            outAccModel.OutStockId = outAcc.OutStockId ?? 0;
                            outAccModel.OutputQuantity = outAcc.OutputQuantity ?? 0;
                            outAccModel.CreateDate = outAcc.CreateDate ?? new DateTime();
                            outAccModel.EditDate = outAcc.EditDate ?? new DateTime();

                            List<AccChallanDeductionModel> accChallanDeductionModelList = new List<AccChallanDeductionModel>();
                            foreach (var accChallanDeduction in outAcc.AccChallanDeductions)
                            {
                                AccChallanDeductionModel accChallanDeductionModel = new AccChallanDeductionModel();
                                accChallanDeductionModel.AccChallanDeductionId = accChallanDeduction.AccChallanDeductionId;

                                ChallanProductModel challanProductModel = new ChallanProductModel();
                                challanProductModel.AccChallanDeductions = null;
                                challanProductModel.ChallanProduct = accChallanDeduction.ChallanProduct;
                                challanProductModel.ProductDetail = accChallanDeduction.ChallanProduct.ProductDetail;
                                challanProductModel.ChallanDetail = accChallanDeduction.ChallanProduct.ChallanDetail;

                                var inputQuantity = (challanProductModel.ChallanProduct.InputQuantity ?? 0) * (challanProductModel.ProductDetail.SplitRatio ?? 1);
                                challanProductModel.RemainingQuantity = ((inputQuantity - accChallanDeduction.ChallanProduct.AccChallanDeductions.Where(x => x.CreateDate <= accChallanDeduction.CreateDate).Sum(x => x.OutQuantity)) ?? inputQuantity) / (challanProductModel.ProductDetail.SplitRatio ?? 1);

                                accChallanDeductionModel.ChallanProduct = challanProductModel;
                                accChallanDeductionModel.ChallanProductId = accChallanDeduction.ChallanProductId ?? 0;
                                accChallanDeductionModel.CreateDate = accChallanDeduction.CreateDate ?? new DateTime();
                                accChallanDeductionModel.EditDate = accChallanDeduction.EditDate ?? new DateTime();
                                accChallanDeductionModel.OutAccId = accChallanDeduction.OutAccId ?? 0;
                                accChallanDeductionModel.OutQuantity = (accChallanDeduction.OutQuantity ?? 0) / (challanProductModel.ProductDetail.SplitRatio ?? 1);

                                accChallanDeductionModelList.Add(accChallanDeductionModel);
                            }

                            outAccModel.AccChallanDeductions = accChallanDeductionModelList.ToArray();

                            outAccModelList.Add(outAccModel);
                        }

                        outStockModel.OutAccs = outAccModelList.ToArray();

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

        [HttpPost, Route("GetBASFChallanByBASFChallanId")]
        public IHttpActionResult GetBASFChallanByBASFChallanId(VendorChallanNoModel vendorChallanNoModel)
        {
            using (var context = new erpdbEntities())
            {
                try
                {
                    var challanDetail = context.ChallanDetails.Where(x => x.ChallanId == vendorChallanNoModel.VendorChallanNo).FirstOrDefault();

                    ViewChallanDetailModel model = new ViewChallanDetailModel();

                    List<ChallanProductModel> challanProducts = new List<ChallanProductModel>();
                    foreach (var challanProduct in challanDetail.ChallanProducts)
                    {
                        ChallanProductModel challanProductModel = new ChallanProductModel();
                        challanProductModel.ChallanProduct = challanProduct;
                        challanProductModel.ProductDetail = challanProduct.ProductDetail;
                        challanProductModel.ChallanDetail = challanProduct.ChallanDetail;
                        challanProductModel.ChallanDeductions = challanProduct.ChallanDeductions;
                        challanProductModel.AccChallanDeductions = challanProduct.AccChallanDeductions;
                        if (challanProductModel.ChallanDeductions != null && challanProductModel.ChallanDeductions.Count > 0)
                        {
                            var inputQuantity = (challanProductModel.ChallanProduct.InputQuantity ?? 0) * (challanProductModel.ProductDetail.SplitRatio ?? 1);
                            challanProductModel.RemainingQuantity = ((inputQuantity - challanProductModel.ChallanDeductions.Sum(x => x.OutQuantity)) ?? inputQuantity) / (challanProductModel.ProductDetail.SplitRatio ?? 1);
                        }
                        else if (challanProductModel.AccChallanDeductions != null && challanProductModel.AccChallanDeductions.Count > 0)
                        {
                            var inputQuantity = (challanProductModel.ChallanProduct.InputQuantity ?? 0) * (challanProductModel.ProductDetail.SplitRatio ?? 1);
                            challanProductModel.RemainingQuantity = ((inputQuantity - challanProductModel.AccChallanDeductions.Sum(x => x.OutQuantity)) ?? inputQuantity) / (challanProductModel.ProductDetail.SplitRatio ?? 1);
                        }

                        challanProducts.Add(challanProductModel);
                    }

                    model.ChallanDetail = challanDetail;
                    model.ChallanProducts = challanProducts.ToArray();

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
