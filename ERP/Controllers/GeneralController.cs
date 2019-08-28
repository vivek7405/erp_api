using ERP.Models;
using IronPdf;
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
                return Ok(context.ProductDetails.OrderBy(x => new { x.InputMaterialDesc, x.OutputMaterialDesc }).ToArray());
            }
        }

        [HttpGet, Route("GetAllProductDetailsForPO")]
        public IHttpActionResult GetAllProductDetailsForPO()
        {
            using (var context = new erpdbEntities())
            {
                int main = Convert.ToInt32(EProductCategorys.Main);
                return Ok(context.ProductDetails.Where(x => x.ProductType.ProductCategory.ProductCategoryId == main).ToArray());
            }
        }

        [HttpPost, Route("GetProductDetailsByProductId")]
        public IHttpActionResult GetProductDetailsByProductId(ProductIdModel model)
        {
            int productId = model.ProductId;
            using (var context = new erpdbEntities())
            {
                ProductDetail productDetail = context.ProductDetails.Where(x => x.ProductId == productId).FirstOrDefault();

                ProductDetailModel productDetailModel = new ProductDetailModel();
                productDetailModel.ProductDetail = productDetail;
                productDetailModel.ProductMappings = productDetail.ProductMappings.ToArray();

                return Ok(productDetailModel);
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
                            if (model.isPO)
                            {
                                var poDetail = new PODetail();
                                poDetail.PONo = challanDetail.ChallanNo;
                                poDetail.PODate = challanDetail.ChallanDate;
                                poDetail.CreateDate = DateTime.Now;
                                poDetail.EditDate = DateTime.Now;
                                context.PODetails.Add(poDetail);

                                context.SaveChanges();
                                response.Id = poDetail.POId;
                                response.Message = "BASF PO added successfully.";
                            }
                            else
                            {
                                challanDetail.CreateDate = DateTime.Now;
                                challanDetail.EditDate = DateTime.Now;
                                context.ChallanDetails.Add(challanDetail);

                                context.SaveChanges();
                                response.Id = challanDetail.ChallanId;
                                response.Message = "BASF Challan added successfully";
                            }
                        }
                        else
                        {
                            if (model.isPO)
                            {
                                var existingChallan = context.PODetails.Where(x => x.POId == challanDetail.ChallanId).FirstOrDefault();
                                existingChallan.EditDate = DateTime.Now;
                                existingChallan.PONo = challanDetail.ChallanNo;
                                existingChallan.PODate = challanDetail.ChallanDate;

                                context.SaveChanges();
                                response.Id = existingChallan.POId;
                                response.Message = "BASF PO updated successfully.";
                            }
                            else
                            {
                                var existingChallan = context.ChallanDetails.Where(x => x.ChallanId == challanDetail.ChallanId).FirstOrDefault();
                                existingChallan.EditDate = DateTime.Now;
                                existingChallan.ChallanNo = challanDetail.ChallanNo;
                                existingChallan.ChallanDate = challanDetail.ChallanDate;

                                context.SaveChanges();
                                response.Id = existingChallan.ChallanId;
                                response.Message = "BASF Challan updated successfully.";
                            }
                        }

                        response.StatusCode = HttpStatusCode.OK;

                        if (model.isPO)
                        {
                            var existingPOProducts = context.POProducts.Where(x => x.POId == response.Id).ToArray();
                            //context.POProducts.RemoveRange(existingPOProducts);

                            foreach (var challanProduct in challanProducts)
                            {
                                var poProduct = new POProduct();
                                poProduct.POId = response.Id;
                                poProduct.InputQuantity = challanProduct.InputQuantity;
                                poProduct.POProductId = challanProduct.ChallanProductId;
                                poProduct.ProductId = challanProduct.ProductId;

                                if (poProduct.POId > 0 && poProduct.ProductId > 0)
                                {
                                    try
                                    {
                                        var existingPOProduct = existingPOProducts.Where(x => x.POProductId == poProduct.POProductId).FirstOrDefault();

                                        if (existingPOProduct == null)
                                        {
                                            poProduct.POProductId = 0;
                                            poProduct.CreateDate = DateTime.Now;
                                            poProduct.EditDate = DateTime.Now;
                                            context.POProducts.Add(poProduct);
                                        }
                                        else
                                        {
                                            existingPOProduct.EditDate = DateTime.Now;
                                            existingPOProduct.InputQuantity = poProduct.InputQuantity;
                                        }

                                        response.StatusCode = HttpStatusCode.OK;
                                    }
                                    catch (Exception e)
                                    {
                                        response.Message = "Some error occurred while performing transaction.";
                                        response.StatusCode = HttpStatusCode.InternalServerError;

                                        return InternalServerError(new Exception("Something went worng while saving Products!"));
                                    }
                                }
                                else
                                {
                                    response.Message = "PO Product details doesn't seem to be entered correctly.";
                                    response.StatusCode = HttpStatusCode.BadRequest;

                                    //return BadRequest(response.Message);
                                    return InternalServerError(new Exception("PO Product details doesn't seem to be entered correctly!"));
                                }
                            }

                            foreach (var existingPOProd in existingPOProducts)
                            {
                                var poContains = challanProducts.Where(x => x.ChallanProductId == existingPOProd.POProductId).FirstOrDefault();
                                if (poContains == null)
                                {
                                    context.POProducts.Remove(existingPOProd);
                                }
                            }
                        }
                        else
                        {
                            var existingChallanProducts = context.ChallanProducts.Where(x => x.ChallanId == response.Id).ToArray();
                            //context.ChallanProducts.RemoveRange(existingChallanProducts);

                            foreach (var challanProduct in challanProducts)
                            {
                                challanProduct.ChallanId = response.Id;
                                if (challanProduct.ChallanId > 0 && challanProduct.ProductId > 0)
                                {
                                    try
                                    {
                                        var existingChallanProduct = existingChallanProducts.Where(x => x.ChallanProductId == challanProduct.ChallanProductId).FirstOrDefault();

                                        if (existingChallanProduct == null)
                                        {
                                            challanProduct.ChallanProductId = 0;
                                            challanProduct.CreateDate = DateTime.Now;
                                            challanProduct.EditDate = DateTime.Now;
                                            context.ChallanProducts.Add(challanProduct);
                                        }
                                        else
                                        {
                                            if (challanProduct.InputQuantity < existingChallanProduct.InputQuantity)
                                            {
                                                int main = Convert.ToInt32(EProductCategorys.Main);
                                                int assembly = Convert.ToInt32(EProductCategorys.Assembly);
                                                int acc = Convert.ToInt32(EProductCategorys.Accessories);

                                                int? totalDeductions = 0;
                                                if (existingChallanProduct.ProductDetail.ProductType.ProductCategory.ProductCategoryId == main)
                                                    totalDeductions = existingChallanProduct.ChallanDeductions.Sum(x => x.OutQuantity);
                                                else if (existingChallanProduct.ProductDetail.ProductType.ProductCategory.ProductCategoryId == assembly)
                                                    totalDeductions = existingChallanProduct.AssemblyChallanDeductions.Sum(x => x.OutQuantity);
                                                else if (existingChallanProduct.ProductDetail.ProductType.ProductCategory.ProductCategoryId == acc)
                                                    totalDeductions = existingChallanProduct.AccChallanDeductions.Sum(x => x.OutQuantity);

                                                int? currentRemainingQnt = existingChallanProduct.InputQuantity - totalDeductions;
                                                int? difference = existingChallanProduct.InputQuantity - challanProduct.InputQuantity;
                                                if (difference <= currentRemainingQnt)
                                                {
                                                    existingChallanProduct.EditDate = DateTime.Now;
                                                    existingChallanProduct.InputQuantity = challanProduct.InputQuantity;
                                                }
                                                else
                                                {
                                                    return InternalServerError(new Exception("The new quanity is not allowed as the remaining quantity will become less than zero!"));
                                                }
                                            }
                                            else
                                            {
                                                existingChallanProduct.EditDate = DateTime.Now;
                                                existingChallanProduct.InputQuantity = challanProduct.InputQuantity;
                                            }
                                        }

                                        response.StatusCode = HttpStatusCode.OK;
                                    }
                                    catch (Exception e)
                                    {
                                        response.Message = "Some error occurred while performing transaction.";
                                        response.StatusCode = HttpStatusCode.InternalServerError;

                                        return InternalServerError(new Exception("Something went wrong while saving Products!"));
                                    }
                                }
                                else
                                {
                                    response.Message = "Challan Product details doesn't seem to be entered correctly.";
                                    response.StatusCode = HttpStatusCode.BadRequest;

                                    //return BadRequest(response.Message);
                                    return InternalServerError(new Exception("Challan Product details doesn't seem to be entered correctly!"));
                                }
                            }

                            foreach (var existingChallanProd in existingChallanProducts)
                            {
                                var challanContains = challanProducts.Where(x => x.ChallanProductId == existingChallanProd.ChallanProductId).FirstOrDefault();
                                if (challanContains == null)
                                {
                                    context.ChallanProducts.Remove(existingChallanProd);
                                }
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
                if (model.isPO)
                    response.Message = "Please enter a PO Number.";
                else
                    response.Message = "Please enter a Challan Number.";
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

        [HttpPost, Route("AddOrUpdateProductMappings")]
        public IHttpActionResult AddOrUpdateProductMappings(ProductMapping[] productMappings)
        {
            SuccessResponse response = new SuccessResponse();
            using (var context = new erpdbEntities())
            {
                if (productMappings.Length > 0)
                {
                    int productId = Convert.ToInt32(productMappings[0].ProductId);
                    var existingProductMappings = context.ProductMappings.Where(x => x.ProductId == productId).ToArray();
                    context.ProductMappings.RemoveRange(existingProductMappings);

                    foreach (var productMapping in productMappings)
                    {
                        if (productMapping.ProductId > 0 && productMapping.MappingProductId > 0)
                        {
                            try
                            {
                                productMapping.CreateDate = DateTime.Now;
                                productMapping.EditDate = DateTime.Now;
                                productMapping.ProductId = productMapping.ProductId;
                                productMapping.MappingProductId = productMapping.MappingProductId;

                                context.ProductMappings.Add(productMapping);

                                response.Message = "Product Mappings successful.";
                                response.StatusCode = HttpStatusCode.OK;
                            }
                            catch (Exception e)
                            {
                                response.Message = "Some error occurred while performing transaction.";
                                response.StatusCode = HttpStatusCode.InternalServerError;

                                return InternalServerError();
                            }
                        }
                        //else
                        //{
                        //    response.Message = "Product Mapping details doesn't seem to be entered correctly.";
                        //    response.StatusCode = HttpStatusCode.BadRequest;

                        //    return BadRequest(response.Message);
                        //}
                    }
                }
                else
                {
                    response.Message = "Product Mapping details doesn't seem to be entered correctly.";
                    response.StatusCode = HttpStatusCode.BadRequest;

                    return BadRequest(response.Message);
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
                var challanDetails = context.ChallanDetails.OrderByDescending(x => new { x.ChallanDate, x.CreateDate, x.ChallanNo }).ToArray();
                List<ViewChallanDetailModel> modelList = new List<ViewChallanDetailModel>();

                foreach (var challan in challanDetails)
                {
                    ViewChallanDetailModel model = new ViewChallanDetailModel();

                    List<ChallanProductModel> challanProducts = new List<ChallanProductModel>();
                    foreach (var challanProduct in challan.ChallanProducts)
                    {
                        ChallanProductModel challanProductModel = new ChallanProductModel();
                        challanProductModel.ChallanProduct = challanProduct;
                        challanProductModel.ProductDetail = new ProductDetailWithProductType();
                        challanProductModel.ProductDetail.CreateDate = challanProduct.ProductDetail.CreateDate;
                        challanProductModel.ProductDetail.EditDate = challanProduct.ProductDetail.EditDate;
                        challanProductModel.ProductDetail.InputCode = challanProduct.ProductDetail.InputCode;
                        challanProductModel.ProductDetail.InputMaterialDesc = challanProduct.ProductDetail.InputMaterialDesc;
                        challanProductModel.ProductDetail.OutputCode = challanProduct.ProductDetail.OutputCode;
                        challanProductModel.ProductDetail.OutputMaterialDesc = challanProduct.ProductDetail.OutputMaterialDesc;
                        challanProductModel.ProductDetail.ProductId = challanProduct.ProductDetail.ProductId;
                        challanProductModel.ProductDetail.ProductTypeId = challanProduct.ProductDetail.ProductTypeId;
                        challanProductModel.ProductDetail.ProductTypeName = challanProduct.ProductDetail.ProductType.ProductTypeName;
                        challanProductModel.ProductDetail.ProjectName = challanProduct.ProductDetail.ProjectName;
                        challanProductModel.ProductDetail.SplitRatio = challanProduct.ProductDetail.SplitRatio;

                        challanProductModel.ChallanDetail = challanProduct.ChallanDetail;
                        challanProductModel.ChallanDeductions = challanProduct.ChallanDeductions;
                        challanProductModel.AccChallanDeductions = challanProduct.AccChallanDeductions;
                        challanProductModel.AssemblyChallanDeductions = challanProduct.AssemblyChallanDeductions;
                        if (challanProductModel.ChallanDeductions != null && challanProductModel.ChallanDeductions.Count > 0)
                        {
                            var inputQuantity = challanProductModel.ChallanProduct.InputQuantity ?? 0;
                            challanProductModel.RemainingQuantity = ((inputQuantity - challanProductModel.ChallanDeductions.Sum(x => x.OutQuantity)) ?? inputQuantity);
                        }
                        else if (challanProductModel.AccChallanDeductions != null && challanProductModel.AccChallanDeductions.Count > 0)
                        {
                            var inputQuantity = (challanProductModel.ChallanProduct.InputQuantity ?? 0);
                            challanProductModel.RemainingQuantity = (inputQuantity - challanProductModel.AccChallanDeductions.Sum(x => x.OutQuantity)) ?? inputQuantity;
                        }
                        else if (challanProductModel.AssemblyChallanDeductions != null && challanProductModel.AssemblyChallanDeductions.Count > 0)
                        {
                            var inputQuantity = (challanProductModel.ChallanProduct.InputQuantity ?? 0);
                            challanProductModel.RemainingQuantity = (inputQuantity - challanProductModel.AssemblyChallanDeductions.Sum(x => x.OutQuantity)) ?? inputQuantity;
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

        [HttpGet, Route("GetAllPODetails")]
        public IHttpActionResult GetAllPODetails()
        {
            using (var context = new erpdbEntities())
            {
                var poDetails = context.PODetails.OrderByDescending(x => new { x.CreateDate, x.EditDate }).ToArray();
                List<ViewPODetailModel> modelList = new List<ViewPODetailModel>();

                foreach (var po in poDetails)
                {
                    ViewPODetailModel model = new ViewPODetailModel();

                    List<POProductModel> poProducts = new List<POProductModel>();
                    foreach (var poProduct in po.POProducts)
                    {
                        POProductModel poProductModel = new POProductModel();
                        poProductModel.POProduct = poProduct;
                        poProductModel.ProductDetail = new ProductDetailWithProductType();
                        poProductModel.ProductDetail.CreateDate = poProduct.ProductDetail.CreateDate;
                        poProductModel.ProductDetail.EditDate = poProduct.ProductDetail.EditDate;
                        poProductModel.ProductDetail.InputCode = poProduct.ProductDetail.InputCode;
                        poProductModel.ProductDetail.InputMaterialDesc = poProduct.ProductDetail.InputMaterialDesc;
                        poProductModel.ProductDetail.OutputCode = poProduct.ProductDetail.OutputCode;
                        poProductModel.ProductDetail.OutputMaterialDesc = poProduct.ProductDetail.OutputMaterialDesc;
                        poProductModel.ProductDetail.ProductId = poProduct.ProductDetail.ProductId;
                        poProductModel.ProductDetail.ProductTypeId = poProduct.ProductDetail.ProductTypeId;
                        poProductModel.ProductDetail.ProductTypeName = poProduct.ProductDetail.ProductType.ProductTypeName;
                        poProductModel.ProductDetail.ProjectName = poProduct.ProductDetail.ProjectName;
                        poProductModel.ProductDetail.SplitRatio = poProduct.ProductDetail.SplitRatio;

                        poProductModel.PODetail = poProduct.PODetail;
                        poProductModel.PODeductions = poProduct.PODeductions;
                        poProductModel.AccPODeductions = poProduct.AccPODeductions;
                        poProductModel.AssemblyPODeductions = poProduct.AssemblyPODeductions;
                        if (poProductModel.PODeductions != null && poProductModel.PODeductions.Count > 0)
                        {
                            var inputQuantity = poProductModel.POProduct.InputQuantity ?? 0;
                            poProductModel.RemainingQuantity = (inputQuantity - poProductModel.PODeductions.Sum(x => x.OutQuantity)) ?? inputQuantity;
                        }
                        else if (poProductModel.AccPODeductions != null && poProductModel.AccPODeductions.Count > 0)
                        {
                            var inputQuantity = poProductModel.POProduct.InputQuantity ?? 0;
                            poProductModel.RemainingQuantity = (inputQuantity - poProductModel.AccPODeductions.Sum(x => x.OutQuantity)) ?? inputQuantity;
                        }
                        else if (poProductModel.AssemblyPODeductions != null && poProductModel.AssemblyPODeductions.Count > 0)
                        {
                            var inputQuantity = poProductModel.POProduct.InputQuantity ?? 0;
                            poProductModel.RemainingQuantity = (inputQuantity - poProductModel.AssemblyPODeductions.Sum(x => x.OutQuantity)) ?? inputQuantity;
                        }

                        poProducts.Add(poProductModel);
                    }

                    model.PODetail = po;
                    model.POProducts = poProducts.ToArray();

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
                    foreach (var mainProduct in products)
                    {
                        var inMainQnt = context.ChallanProducts.Where(x => x.ProductId == mainProduct.ProductId).Sum(l => l.InputQuantity) ?? 0;
                        var outMainQnt = context.ChallanDeductions.Where(x => x.ChallanProduct.ProductId == mainProduct.ProductId).Sum(l => l.OutQuantity) ?? 0;
                        int mainRemainingQuantity = Convert.ToInt32(inMainQnt) - Convert.ToInt32(outMainQnt);

                        var inMainQntPO = context.POProducts.Where(x => x.ProductId == mainProduct.ProductId).Sum(l => l.InputQuantity) ?? 0;
                        var outMainQntPO = context.PODeductions.Where(x => x.POProduct.ProductId == mainProduct.ProductId).Sum(l => l.OutQuantity) ?? 0;
                        int mainRemainingQuantityPO = Convert.ToInt32(inMainQntPO) - Convert.ToInt32(outMainQntPO);


                        bool assemblyProductsQntPresent = true;

                        List<ProductDetail> mappedAssemblyProductDetails = mainProduct.ProductMappings.Select(x => x.ProductDetail1).ToList();
                        List<ProductQuantity> assemblyProductsQuantity = new List<ProductQuantity>();
                        foreach (var assemblyProduct in mappedAssemblyProductDetails)
                        {
                            var inAssemblyQnt = context.ChallanProducts.Where(x => x.ProductId == assemblyProduct.ProductId).Sum(l => l.InputQuantity) ?? 0;
                            var outAssemblyQnt = context.AssemblyChallanDeductions.Where(x => x.ChallanProduct.ProductId == assemblyProduct.ProductId).Sum(l => l.OutQuantity) ?? 0;
                            int assemblyRemainingQuantity = Convert.ToInt32(inAssemblyQnt) - Convert.ToInt32(outAssemblyQnt);

                            var inAssemblyQntPO = context.POProducts.Where(x => x.ProductId == assemblyProduct.ProductId).Sum(l => l.InputQuantity) ?? 0;
                            var outAssemblyQntPO = context.AssemblyPODeductions.Where(x => x.POProduct.ProductId == assemblyProduct.ProductId).Sum(l => l.OutQuantity) ?? 0;
                            int assemblyRemainingQuantityPO = Convert.ToInt32(inAssemblyQntPO) - Convert.ToInt32(outAssemblyQntPO);

                            ProductQuantity mappedAssemblyProductDetail = new ProductQuantity();
                            mappedAssemblyProductDetail.ProductId = assemblyProduct.ProductId;
                            mappedAssemblyProductDetail.ProductName = assemblyProduct.InputMaterialDesc;
                            mappedAssemblyProductDetail.SplitRatio = Convert.ToInt32(assemblyProduct.SplitRatio);
                            mappedAssemblyProductDetail.RemainingQuantity = assemblyRemainingQuantity * mappedAssemblyProductDetail.SplitRatio;
                            mappedAssemblyProductDetail.RemainingQuantityPO = assemblyRemainingQuantityPO;
                            assemblyProductsQuantity.Add(mappedAssemblyProductDetail);

                            //if (assemblyRemainingQuantity <= 0 || assemblyRemainingQuantityPO <= 0)
                            if (assemblyRemainingQuantity <= 0)
                                assemblyProductsQntPresent = false;
                        }


                        if (mainRemainingQuantity > 0 && mainRemainingQuantityPO > 0 && assemblyProductsQntPresent)
                        {
                            ProductQuantity productQnty = new ProductQuantity();
                            productQnty.ProductId = Convert.ToInt32(mainProduct.ProductId);
                            productQnty.ProductName = mainProduct.ProjectName;
                            productQnty.SplitRatio = Convert.ToInt32(mainProduct.SplitRatio);
                            productQnty.RemainingQuantity = mainRemainingQuantity * productQnty.SplitRatio;
                            productQnty.RemainingQuantityPO = mainRemainingQuantityPO;
                            productQnty.AssemblyProductQnts = assemblyProductsQuantity.ToArray();
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

        [HttpGet, Route("GetMainProductRemainingQuantityBASFInvoice")]
        public IHttpActionResult GetMainProductRemainingQuantityBASFInvoice()
        {
            using (var context = new erpdbEntities())
            {
                try
                {
                    int main = Convert.ToInt32(EProductCategorys.Main);
                    var products = context.ProductDetails.Where(x => x.ProductType.ProductCategoryId == main).ToList();

                    List<ProductQuantity> productQnts = new List<ProductQuantity>();
                    foreach (var mainProduct in products)
                    {
                        var inMainQnt = context.ChallanProducts.Where(x => x.ProductId == mainProduct.ProductId).Sum(l => l.InputQuantity) ?? 0;
                        var outMainQnt = context.InvoiceChallanDeductions.Where(x => x.ChallanProduct.ProductId == mainProduct.ProductId).Sum(l => l.OutQuantity) ?? 0;
                        var ngOutMainQnt = context.ChallanDeductions.Where(x => x.ChallanProduct.ProductId == mainProduct.ProductId && x.OutStock.VendorChallan.IsNg == 1).Sum(l => l.OutQuantity) ?? 0;

                        int mainRemainingQuantity = Convert.ToInt32(inMainQnt) - Convert.ToInt32(outMainQnt) - Convert.ToInt32(ngOutMainQnt);


                        if (mainRemainingQuantity > 0)
                        {
                            ProductQuantity productQnty = new ProductQuantity();
                            productQnty.ProductId = Convert.ToInt32(mainProduct.ProductId);
                            productQnty.ProductName = mainProduct.ProjectName;
                            productQnty.SplitRatio = Convert.ToInt32(mainProduct.SplitRatio);
                            productQnty.RemainingQuantity = mainRemainingQuantity * productQnty.SplitRatio;
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
                        var inQnt = context.ChallanProducts.Where(x => x.ProductId == product.ProductId).Sum(l => l.InputQuantity) ?? 0;
                        var outQnt = context.AccChallanDeductions.Where(x => x.ChallanProduct.ProductId == product.ProductId).Sum(l => l.OutQuantity) ?? 0;
                        var remainingQuantity = Convert.ToInt32(inQnt) - Convert.ToInt32(outQnt);

                        var inQntPO = context.POProducts.Where(x => x.ProductId == product.ProductId).Sum(l => l.InputQuantity) ?? 0;
                        var outQntPO = context.AccPODeductions.Where(x => x.POProduct.ProductId == product.ProductId).Sum(l => l.OutQuantity) ?? 0;
                        var remainingQuantityPO = Convert.ToInt32(inQntPO) - Convert.ToInt32(outQntPO);

                        //if (remainingQuantity > 0 && remainingQuantityPO > 0)
                        if (remainingQuantity > 0)
                        {
                            ProductQuantity productQnty = new ProductQuantity();
                            productQnty.ProductId = Convert.ToInt32(product.ProductId);
                            productQnty.ProductName = product.InputMaterialDesc;
                            productQnty.SplitRatio = Convert.ToInt32(product.SplitRatio);
                            productQnty.RemainingQuantity = remainingQuantity * productQnty.SplitRatio;
                            productQnty.RemainingQuantityPO = remainingQuantityPO;
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

        [HttpGet, Route("GetAssemblyProductRemainingQuantity")]
        public IHttpActionResult GetAssemblyProductRemainingQuantity()
        {
            using (var context = new erpdbEntities())
            {
                try
                {
                    int assembly = Convert.ToInt32(EProductCategorys.Assembly);
                    var products = context.ProductDetails.Where(x => x.ProductType.ProductCategoryId == assembly).ToList();

                    List<ProductQuantity> productQnts = new List<ProductQuantity>();
                    foreach (var product in products)
                    {
                        var inQnt = context.ChallanProducts.Where(x => x.ProductId == product.ProductId).Sum(l => l.InputQuantity) ?? 0;
                        var outQnt = context.AssemblyChallanDeductions.Where(x => x.ChallanProduct.ProductId == product.ProductId).Sum(l => l.OutQuantity) ?? 0;
                        var remainingQuantity = Convert.ToInt32(inQnt) - Convert.ToInt32(outQnt);

                        var inQntPO = context.POProducts.Where(x => x.ProductId == product.ProductId).Sum(l => l.InputQuantity) ?? 0;
                        var outQntPO = context.AssemblyPODeductions.Where(x => x.POProduct.ProductId == product.ProductId).Sum(l => l.OutQuantity) ?? 0;
                        var remainingQuantityPO = Convert.ToInt32(inQntPO) - Convert.ToInt32(outQntPO);

                        if (remainingQuantity > 0)
                        {
                            ProductQuantity productQnty = new ProductQuantity();
                            productQnty.ProductId = Convert.ToInt32(product.ProductId);
                            productQnty.ProductName = product.InputMaterialDesc;
                            productQnty.SplitRatio = Convert.ToInt32(product.SplitRatio);
                            productQnty.RemainingQuantity = remainingQuantity * productQnty.SplitRatio;
                            productQnty.RemainingQuantityPO = remainingQuantityPO;
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

        [HttpGet, Route("GetAllAssemblyProducts")]
        public IHttpActionResult GetAllAssemblyProducts()
        {
            using (var context = new erpdbEntities())
            {
                try
                {
                    int assembly = Convert.ToInt32(EProductCategorys.Assembly);
                    ProductDetail[] products = context.ProductDetails.Where(x => x.ProductType.ProductCategoryId == assembly).ToArray();

                    return Ok(products);
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
            SuccessResponse response = new SuccessResponse();

            using (var context = new erpdbEntities())
            {
                try
                {
                    VendorChallan vendorChallan = new VendorChallan();
                    vendorChallan.VendorChallanDate = model.VendorChallanDate;
                    vendorChallan.IsNg = model.IsNg ? 1 : 0;
                    vendorChallan.CreateDate = DateTime.Now;
                    vendorChallan.EditDate = DateTime.Now;

                    context.VendorChallans.Add(vendorChallan);
                    context.SaveChanges();

                    foreach (OutStockModel outStockModel in model.OutStocks)
                    {
                        GetChallanDeductionsByOutStock(outStockModel, model.IsNg);
                        GetPODeductionsByOutStock(outStockModel, model.IsNg);

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

                        foreach (PODeductionModel poDeductionModel in outStockModel.PODeductions)
                        {
                            PODeduction poDeduction = new PODeduction();
                            poDeduction.CreateDate = DateTime.Now;
                            poDeduction.EditDate = DateTime.Now;
                            poDeduction.OutStockId = outStock.OutStockId;
                            poDeduction.OutQuantity = poDeductionModel.OutQuantity;
                            poDeduction.POProductId = poDeductionModel.POProductId;

                            context.PODeductions.Add(poDeduction);
                            context.SaveChanges();
                        }


                        if (!model.IsNg)
                        {
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

                                foreach (AccPODeductionModel accPODeductionModel in outAccModel.AccPODeductions)
                                {
                                    AccPODeduction accPODeduction = new AccPODeduction();
                                    accPODeduction.CreateDate = DateTime.Now;
                                    accPODeduction.EditDate = DateTime.Now;
                                    accPODeduction.OutAccId = outAcc.OutAccId;
                                    accPODeduction.OutQuantity = accPODeductionModel.OutQuantity;
                                    accPODeduction.POProductId = accPODeductionModel.POProductId;

                                    context.AccPODeductions.Add(accPODeduction);
                                    context.SaveChanges();
                                }
                            }


                            foreach (OutAssemblyModel outAssemblyModel in outStockModel.OutAssemblys)
                            {
                                OutAssembly outAssembly = new OutAssembly();
                                outAssembly.OutStockId = outStock.OutStockId;
                                outAssembly.OutputQuantity = outAssemblyModel.OutputQuantity;
                                outAssembly.CreateDate = DateTime.Now;
                                outAssembly.EditDate = DateTime.Now;

                                context.OutAssemblys.Add(outAssembly);
                                context.SaveChanges();

                                foreach (AssemblyChallanDeductionModel assemblyChallanDeductionModel in outAssemblyModel.AssemblyChallanDeductions)
                                {
                                    AssemblyChallanDeduction assemblyChallanDeduction = new AssemblyChallanDeduction();
                                    assemblyChallanDeduction.CreateDate = DateTime.Now;
                                    assemblyChallanDeduction.EditDate = DateTime.Now;
                                    assemblyChallanDeduction.OutAssemblyId = outAssembly.OutAssemblyId;
                                    assemblyChallanDeduction.OutQuantity = assemblyChallanDeductionModel.OutQuantity;
                                    assemblyChallanDeduction.ChallanProductId = assemblyChallanDeductionModel.ChallanProductId;

                                    context.AssemblyChallanDeductions.Add(assemblyChallanDeduction);
                                    context.SaveChanges();
                                }

                                foreach (AssemblyPODeductionModel assemblyPODeductionModel in outAssemblyModel.AssemblyPODeductions)
                                {
                                    AssemblyPODeduction assemblyPODeduction = new AssemblyPODeduction();
                                    assemblyPODeduction.CreateDate = DateTime.Now;
                                    assemblyPODeduction.EditDate = DateTime.Now;
                                    assemblyPODeduction.OutAssemblyId = outAssembly.OutAssemblyId;
                                    assemblyPODeduction.OutQuantity = assemblyPODeductionModel.OutQuantity;
                                    assemblyPODeduction.POProductId = assemblyPODeductionModel.POProductId;

                                    context.AssemblyPODeductions.Add(assemblyPODeduction);
                                    context.SaveChanges();
                                }
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

        [HttpPost, Route("AddOrUpdateBASFInvoice")]
        public IHttpActionResult AddOrUpdateBASFInvoice(BASFInvoiceModel model)
        {
            SuccessResponse response = new SuccessResponse();

            using (var context = new erpdbEntities())
            {
                try
                {
                    BASFInvoice basfInvoice = new BASFInvoice();
                    basfInvoice.BASFInvoiceNo = model.BASFInvoiceNo;
                    basfInvoice.BASFInvoiceDate = model.BASFInvoiceDate;
                    basfInvoice.IsNg = model.IsNg ? 1 : 0;
                    basfInvoice.CreateDate = DateTime.Now;
                    basfInvoice.EditDate = DateTime.Now;

                    context.BASFInvoices.Add(basfInvoice);
                    context.SaveChanges();

                    foreach (InvoiceOutStockModel outStockModel in model.InvoiceOutStocks)
                    {
                        GetInvoiceChallanDeductionsByInvoiceOutStock(outStockModel);

                        InvoiceOutStock outStock = new InvoiceOutStock();
                        outStock.BASFInvoiceId = basfInvoice.BASFInvoiceId;
                        outStock.OutputQuantity = outStockModel.OutputQuantity;
                        outStock.CreateDate = DateTime.Now;
                        outStock.EditDate = DateTime.Now;

                        context.InvoiceOutStocks.Add(outStock);
                        context.SaveChanges();

                        foreach (InvoiceChallanDeductionModel challanDeductionModel in outStockModel.InvoiceChallanDeductions)
                        {
                            InvoiceChallanDeduction challanDeduction = new InvoiceChallanDeduction();
                            challanDeduction.CreateDate = DateTime.Now;
                            challanDeduction.EditDate = DateTime.Now;
                            challanDeduction.InvoiceOutStockId = outStock.InvoiceOutStockId;
                            challanDeduction.OutQuantity = challanDeductionModel.OutQuantity;
                            challanDeduction.ChallanProductId = challanDeductionModel.ChallanProductId;

                            context.InvoiceChallanDeductions.Add(challanDeduction);
                            context.SaveChanges();
                        }
                    }

                    response.Id = basfInvoice.BASFInvoiceId;
                    response.Message = "BASF Invoice successfully saved.";
                    response.StatusCode = HttpStatusCode.OK;

                    return Ok(response);
                }
                catch (Exception e)
                {
                    return InternalServerError();
                }
            }
        }

        //public void GetChallanDeductions(OutStockModel[] outStocks)
        //{
        //    foreach (OutStockModel outStock in outStocks)
        //    {
        //        if (outStock.ChallanDeductions == null || (outStock.ChallanDeductions != null && outStock.ChallanDeductions.Length == 0))
        //        {
        //            var productIdModel = new ProductIdModel();
        //            productIdModel.ProductId = outStock.ProductId;
        //            var result = GetAllBASFChallanByProductIdPrivate(productIdModel);

        //            var basfChallanSelection = result.BASFChallanSelections;

        //            var outputQnt = outStock.OutputQuantity;

        //            List<ChallanDeductionModel> challanDeductions = new List<ChallanDeductionModel>();
        //            foreach (var challan in basfChallanSelection)
        //            {
        //                var challanDeduction = new ChallanDeductionModel();

        //                if (outputQnt > 0)
        //                {
        //                    if (challan.RemainingQuantity < outputQnt)
        //                    {
        //                        challan.OutQuantity = challan.RemainingQuantity;
        //                        outputQnt -= challan.RemainingQuantity;
        //                        challan.QntAfterDeduction = 0;
        //                    }
        //                    else
        //                    {
        //                        challan.OutQuantity = outputQnt;
        //                        outputQnt = 0;
        //                        challan.QntAfterDeduction = challan.RemainingQuantity - challan.OutQuantity;
        //                    }

        //                    challan.IsChecked = true;

        //                    challanDeduction.ChallanProductId = challan.ChallanProduct.ChallanProductId;
        //                    challanDeduction.OutQuantity = challan.OutQuantity;

        //                    challanDeductions.Add(challanDeduction);
        //                    outStock.ChallanDeductions = challanDeductions.ToArray();
        //                }
        //                else
        //                {
        //                    challan.QntAfterDeduction = challan.RemainingQuantity;
        //                }
        //            }
        //        }

        //        foreach (OutAccModel outAcc in outStock.OutAccs)
        //        {
        //            if (outAcc.AccChallanDeductions == null || (outAcc.AccChallanDeductions != null && outAcc.AccChallanDeductions.Length == 0))
        //            {
        //                var productIdModel = new ProductIdModel();
        //                productIdModel.ProductId = outAcc.ProductId;
        //                var result = GetAllBASFChallanByProductIdPrivate(productIdModel);

        //                var basfChallanSelection = result.BASFChallanSelections;

        //                var outputQnt = outAcc.OutputQuantity;

        //                List<AccChallanDeductionModel> accChallanDeductions = new List<AccChallanDeductionModel>();
        //                foreach (var challan in basfChallanSelection)
        //                {
        //                    var accChallanDeduction = new AccChallanDeductionModel();

        //                    if (outputQnt > 0)
        //                    {
        //                        if (challan.RemainingQuantity < outputQnt)
        //                        {
        //                            challan.OutQuantity = challan.RemainingQuantity;
        //                            outputQnt -= challan.RemainingQuantity;
        //                            challan.QntAfterDeduction = 0;
        //                        }
        //                        else
        //                        {
        //                            challan.OutQuantity = outputQnt;
        //                            outputQnt = 0;
        //                            challan.QntAfterDeduction = challan.RemainingQuantity - challan.OutQuantity;
        //                        }

        //                        challan.IsChecked = true;

        //                        accChallanDeduction.ChallanProductId = challan.ChallanProduct.ChallanProductId;
        //                        accChallanDeduction.OutQuantity = challan.OutQuantity;

        //                        accChallanDeductions.Add(accChallanDeduction);
        //                        outAcc.AccChallanDeductions = accChallanDeductions.ToArray();
        //                    }
        //                    else
        //                    {
        //                        challan.QntAfterDeduction = challan.RemainingQuantity;
        //                    }
        //                }
        //            }
        //        }

        //        foreach (OutAssemblyModel outAssembly in outStock.OutAssemblys)
        //        {
        //            if (outAssembly.AssemblyChallanDeductions == null || (outAssembly.AssemblyChallanDeductions != null && outAssembly.AssemblyChallanDeductions.Length == 0))
        //            {
        //                var productIdModel = new ProductIdModel();
        //                productIdModel.ProductId = outAssembly.ProductId;
        //                var result = GetAllBASFChallanByProductIdPrivate(productIdModel);

        //                var basfChallanSelection = result.BASFChallanSelections;

        //                var outputQnt = outAssembly.OutputQuantity;

        //                List<AssemblyChallanDeductionModel> assemblyChallanDeductions = new List<AssemblyChallanDeductionModel>();
        //                foreach (var challan in basfChallanSelection)
        //                {
        //                    var assemblyChallanDeduction = new AssemblyChallanDeductionModel();

        //                    if (outputQnt > 0)
        //                    {
        //                        if (challan.RemainingQuantity < outputQnt)
        //                        {
        //                            challan.OutQuantity = challan.RemainingQuantity;
        //                            outputQnt -= challan.RemainingQuantity;
        //                            challan.QntAfterDeduction = 0;
        //                        }
        //                        else
        //                        {
        //                            challan.OutQuantity = outputQnt;
        //                            outputQnt = 0;
        //                            challan.QntAfterDeduction = challan.RemainingQuantity - challan.OutQuantity;
        //                        }

        //                        challan.IsChecked = true;

        //                        assemblyChallanDeduction.ChallanProductId = challan.ChallanProduct.ChallanProductId;
        //                        assemblyChallanDeduction.OutQuantity = challan.OutQuantity;

        //                        assemblyChallanDeductions.Add(assemblyChallanDeduction);
        //                        outAssembly.AssemblyChallanDeductions = assemblyChallanDeductions.ToArray();
        //                    }
        //                    else
        //                    {
        //                        challan.QntAfterDeduction = challan.RemainingQuantity;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //public void GetPODeductions(OutStockModel[] outStocks)
        //{
        //    foreach (OutStockModel outStock in outStocks)
        //    {
        //        if (outStock.PODeductions == null || (outStock.PODeductions != null && outStock.PODeductions.Length == 0))
        //        {
        //            var productIdModel = new ProductIdModel();
        //            productIdModel.ProductId = outStock.ProductId;
        //            var result = GetAllBASFPOByProductIdPrivate(productIdModel);

        //            var basfPOSelection = result.BASFPOSelections;

        //            var outputQnt = outStock.OutputQuantity;

        //            List<PODeductionModel> PODeductions = new List<PODeductionModel>();
        //            foreach (var po in basfPOSelection)
        //            {
        //                var PODeduction = new PODeductionModel();

        //                if (outputQnt > 0)
        //                {
        //                    if (po.RemainingQuantity < outputQnt)
        //                    {
        //                        po.OutQuantity = po.RemainingQuantity;
        //                        outputQnt -= po.RemainingQuantity;
        //                        po.QntAfterDeduction = 0;
        //                    }
        //                    else
        //                    {
        //                        po.OutQuantity = outputQnt;
        //                        outputQnt = 0;
        //                        po.QntAfterDeduction = po.RemainingQuantity - po.OutQuantity;
        //                    }

        //                    po.IsChecked = true;

        //                    PODeduction.POProductId = po.POProduct.POProductId;
        //                    PODeduction.OutQuantity = po.OutQuantity;

        //                    PODeductions.Add(PODeduction);
        //                    outStock.PODeductions = PODeductions.ToArray();
        //                }
        //                else
        //                {
        //                    po.QntAfterDeduction = po.RemainingQuantity;
        //                }
        //            }
        //        }

        //        foreach (OutAccModel outAcc in outStock.OutAccs)
        //        {
        //            if (outAcc.AccPODeductions == null || (outAcc.AccPODeductions != null && outAcc.AccPODeductions.Length == 0))
        //            {
        //                var productIdModel = new ProductIdModel();
        //                productIdModel.ProductId = outAcc.ProductId;
        //                var result = GetAllBASFPOByProductIdPrivate(productIdModel);

        //                var basfPOSelection = result.BASFPOSelections;

        //                var outputQnt = outAcc.OutputQuantity;

        //                List<AccPODeductionModel> accPODeductions = new List<AccPODeductionModel>();
        //                foreach (var po in basfPOSelection)
        //                {
        //                    var accPODeduction = new AccPODeductionModel();

        //                    if (outputQnt > 0)
        //                    {
        //                        if (po.RemainingQuantity < outputQnt)
        //                        {
        //                            po.OutQuantity = po.RemainingQuantity;
        //                            outputQnt -= po.RemainingQuantity;
        //                            po.QntAfterDeduction = 0;
        //                        }
        //                        else
        //                        {
        //                            po.OutQuantity = outputQnt;
        //                            outputQnt = 0;
        //                            po.QntAfterDeduction = po.RemainingQuantity - po.OutQuantity;
        //                        }

        //                        po.IsChecked = true;

        //                        accPODeduction.POProductId = po.POProduct.POProductId;
        //                        accPODeduction.OutQuantity = po.OutQuantity;

        //                        accPODeductions.Add(accPODeduction);
        //                        outAcc.AccPODeductions = accPODeductions.ToArray();
        //                    }
        //                    else
        //                    {
        //                        po.QntAfterDeduction = po.RemainingQuantity;
        //                    }
        //                }
        //            }
        //        }

        //        foreach (OutAssemblyModel outAssembly in outStock.OutAssemblys)
        //        {
        //            if (outAssembly.AssemblyPODeductions == null || (outAssembly.AssemblyPODeductions != null && outAssembly.AssemblyPODeductions.Length == 0))
        //            {
        //                var productIdModel = new ProductIdModel();
        //                productIdModel.ProductId = outAssembly.ProductId;
        //                var result = GetAllBASFPOByProductIdPrivate(productIdModel);

        //                var basfPOSelection = result.BASFPOSelections;

        //                var outputQnt = outAssembly.OutputQuantity;

        //                List<AssemblyPODeductionModel> assemblyPODeductions = new List<AssemblyPODeductionModel>();
        //                foreach (var PO in basfPOSelection)
        //                {
        //                    var assemblyPODeduction = new AssemblyPODeductionModel();

        //                    if (outputQnt > 0)
        //                    {
        //                        if (PO.RemainingQuantity < outputQnt)
        //                        {
        //                            PO.OutQuantity = PO.RemainingQuantity;
        //                            outputQnt -= PO.RemainingQuantity;
        //                            PO.QntAfterDeduction = 0;
        //                        }
        //                        else
        //                        {
        //                            PO.OutQuantity = outputQnt;
        //                            outputQnt = 0;
        //                            PO.QntAfterDeduction = PO.RemainingQuantity - PO.OutQuantity;
        //                        }

        //                        PO.IsChecked = true;

        //                        assemblyPODeduction.POProductId = PO.POProduct.POProductId;
        //                        assemblyPODeduction.OutQuantity = PO.OutQuantity;

        //                        assemblyPODeductions.Add(assemblyPODeduction);
        //                        outAssembly.AssemblyPODeductions = assemblyPODeductions.ToArray();
        //                    }
        //                    else
        //                    {
        //                        PO.QntAfterDeduction = PO.RemainingQuantity;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}


        public void GetChallanDeductionsByOutStock(OutStockModel outStock, bool isNg)
        {
            if (outStock.ChallanDeductions == null || (outStock.ChallanDeductions != null && outStock.ChallanDeductions.Length == 0))
            {
                var productIdModel = new ProductIdModel();
                productIdModel.ProductId = outStock.ProductId;
                var result = GetAllBASFChallanByProductIdPrivate(productIdModel);

                var basfChallanSelection = result.BASFChallanSelections;

                var outputQnt = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(outStock.OutputQuantity) / Convert.ToDouble(outStock.SplitRatio)));

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
                    }
                    else
                    {
                        challan.QntAfterDeduction = challan.RemainingQuantity;
                    }
                }

                outStock.ChallanDeductions = challanDeductions.ToArray();
            }

            if (!isNg)
            {
                foreach (OutAccModel outAcc in outStock.OutAccs)
                {
                    if (outAcc.AccChallanDeductions == null || (outAcc.AccChallanDeductions != null && outAcc.AccChallanDeductions.Length == 0))
                    {
                        var productIdModel = new ProductIdModel();
                        productIdModel.ProductId = outAcc.ProductId;
                        var result = GetAllBASFChallanByProductIdPrivate(productIdModel);

                        var basfChallanSelection = result.BASFChallanSelections;

                        var outputQnt = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(outAcc.OutputQuantity) / Convert.ToDouble(outAcc.SplitRatio)));

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
                            }
                            else
                            {
                                challan.QntAfterDeduction = challan.RemainingQuantity;
                            }
                        }

                        outAcc.AccChallanDeductions = accChallanDeductions.ToArray();
                    }
                }

                foreach (OutAssemblyModel outAssembly in outStock.OutAssemblys)
                {
                    if (outAssembly.AssemblyChallanDeductions == null || (outAssembly.AssemblyChallanDeductions != null && outAssembly.AssemblyChallanDeductions.Length == 0))
                    {
                        var productIdModel = new ProductIdModel();
                        productIdModel.ProductId = outAssembly.ProductId;
                        var result = GetAllBASFChallanByProductIdPrivate(productIdModel);

                        var basfChallanSelection = result.BASFChallanSelections;

                        var outputQnt = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(outAssembly.OutputQuantity) / Convert.ToDouble(outAssembly.SplitRatio)));

                        List<AssemblyChallanDeductionModel> assemblyChallanDeductions = new List<AssemblyChallanDeductionModel>();
                        foreach (var challan in basfChallanSelection)
                        {
                            var assemblyChallanDeduction = new AssemblyChallanDeductionModel();

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

                                assemblyChallanDeduction.ChallanProductId = challan.ChallanProduct.ChallanProductId;
                                assemblyChallanDeduction.OutQuantity = challan.OutQuantity;

                                assemblyChallanDeductions.Add(assemblyChallanDeduction);
                            }
                            else
                            {
                                challan.QntAfterDeduction = challan.RemainingQuantity;
                            }
                        }

                        outAssembly.AssemblyChallanDeductions = assemblyChallanDeductions.ToArray();
                    }
                }
            }
        }

        public void GetPODeductionsByOutStock(OutStockModel outStock, bool isNg)
        {
            if (outStock.PODeductions == null || (outStock.PODeductions != null && outStock.PODeductions.Length == 0))
            {
                var productIdModel = new ProductIdModel();
                productIdModel.ProductId = outStock.ProductId;
                var result = GetAllBASFPOByProductIdPrivate(productIdModel);

                var basfPOSelection = result.BASFPOSelections;

                var outputQnt = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(outStock.OutputQuantity)));

                List<PODeductionModel> PODeductions = new List<PODeductionModel>();
                foreach (var po in basfPOSelection)
                {
                    var PODeduction = new PODeductionModel();

                    if (outputQnt > 0)
                    {
                        if (po.RemainingQuantity < outputQnt)
                        {
                            po.OutQuantity = po.RemainingQuantity;
                            outputQnt -= po.RemainingQuantity;
                            po.QntAfterDeduction = 0;
                        }
                        else
                        {
                            po.OutQuantity = outputQnt;
                            outputQnt = 0;
                            po.QntAfterDeduction = po.RemainingQuantity - po.OutQuantity;
                        }

                        po.IsChecked = true;

                        PODeduction.POProductId = po.POProduct.POProductId;
                        PODeduction.OutQuantity = po.OutQuantity;

                        PODeductions.Add(PODeduction);
                    }
                    else
                    {
                        po.QntAfterDeduction = po.RemainingQuantity;
                    }
                }

                outStock.PODeductions = PODeductions.ToArray();
            }

            //if (!isNg)
            //{
            //    foreach (OutAccModel outAcc in outStock.OutAccs)
            //    {
            //        if (outAcc.AccPODeductions == null || (outAcc.AccPODeductions != null && outAcc.AccPODeductions.Length == 0))
            //        {
            //            var productIdModel = new ProductIdModel();
            //            productIdModel.ProductId = outAcc.ProductId;
            //            var result = GetAllBASFPOByProductIdPrivate(productIdModel);

            //            var basfPOSelection = result.BASFPOSelections;

            //            var outputQnt = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(outAcc.OutputQuantity)));

            //            List<AccPODeductionModel> accPODeductions = new List<AccPODeductionModel>();
            //            foreach (var po in basfPOSelection)
            //            {
            //                var accPODeduction = new AccPODeductionModel();

            //                if (outputQnt > 0)
            //                {
            //                    if (po.RemainingQuantity < outputQnt)
            //                    {
            //                        po.OutQuantity = po.RemainingQuantity;
            //                        outputQnt -= po.RemainingQuantity;
            //                        po.QntAfterDeduction = 0;
            //                    }
            //                    else
            //                    {
            //                        po.OutQuantity = outputQnt;
            //                        outputQnt = 0;
            //                        po.QntAfterDeduction = po.RemainingQuantity - po.OutQuantity;
            //                    }

            //                    po.IsChecked = true;

            //                    accPODeduction.POProductId = po.POProduct.POProductId;
            //                    accPODeduction.OutQuantity = po.OutQuantity;

            //                    accPODeductions.Add(accPODeduction);
            //                    outAcc.AccPODeductions = accPODeductions.ToArray();
            //                }
            //                else
            //                {
            //                    po.QntAfterDeduction = po.RemainingQuantity;
            //                }
            //            }
            //        }
            //    }

            //    foreach (OutAssemblyModel outAssembly in outStock.OutAssemblys)
            //    {
            //        if (outAssembly.AssemblyPODeductions == null || (outAssembly.AssemblyPODeductions != null && outAssembly.AssemblyPODeductions.Length == 0))
            //        {
            //            var productIdModel = new ProductIdModel();
            //            productIdModel.ProductId = outAssembly.ProductId;
            //            var result = GetAllBASFPOByProductIdPrivate(productIdModel);

            //            var basfPOSelection = result.BASFPOSelections;

            //            var outputQnt = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(outAssembly.OutputQuantity)));

            //            List<AssemblyPODeductionModel> assemblyPODeductions = new List<AssemblyPODeductionModel>();
            //            foreach (var PO in basfPOSelection)
            //            {
            //                var assemblyPODeduction = new AssemblyPODeductionModel();

            //                if (outputQnt > 0)
            //                {
            //                    if (PO.RemainingQuantity < outputQnt)
            //                    {
            //                        PO.OutQuantity = PO.RemainingQuantity;
            //                        outputQnt -= PO.RemainingQuantity;
            //                        PO.QntAfterDeduction = 0;
            //                    }
            //                    else
            //                    {
            //                        PO.OutQuantity = outputQnt;
            //                        outputQnt = 0;
            //                        PO.QntAfterDeduction = PO.RemainingQuantity - PO.OutQuantity;
            //                    }

            //                    PO.IsChecked = true;

            //                    assemblyPODeduction.POProductId = PO.POProduct.POProductId;
            //                    assemblyPODeduction.OutQuantity = PO.OutQuantity;

            //                    assemblyPODeductions.Add(assemblyPODeduction);
            //                    outAssembly.AssemblyPODeductions = assemblyPODeductions.ToArray();
            //                }
            //                else
            //                {
            //                    PO.QntAfterDeduction = PO.RemainingQuantity;
            //                }
            //            }
            //        }
            //    }
            //}
        }

        public void GetInvoiceChallanDeductionsByInvoiceOutStock(InvoiceOutStockModel outStock)
        {
            if (outStock.InvoiceChallanDeductions == null || (outStock.InvoiceChallanDeductions != null && outStock.InvoiceChallanDeductions.Length == 0))
            {
                var productIdModel = new ProductIdModel();
                productIdModel.ProductId = outStock.ProductId;
                var result = GetAllBASFInvoiceChallanByProductIdPrivate(productIdModel);

                var basfChallanSelection = result.BASFChallanSelections;

                var outputQnt = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(outStock.OutputQuantity) / Convert.ToDouble(outStock.SplitRatio)));

                List<InvoiceChallanDeductionModel> challanDeductions = new List<InvoiceChallanDeductionModel>();
                foreach (var challan in basfChallanSelection)
                {
                    var challanDeduction = new InvoiceChallanDeductionModel();

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
                    }
                    else
                    {
                        challan.QntAfterDeduction = challan.RemainingQuantity;
                    }
                }

                outStock.InvoiceChallanDeductions = challanDeductions.ToArray();
            }
        }

        private BASFChallanDeduction GetAllBASFInvoiceChallanByProductIdPrivate(ProductIdModel model)
        {
            int productId = model.ProductId;
            using (var context = new erpdbEntities())
            {
                try
                {
                    BASFChallanDeduction basfChallanDeduction = new BASFChallanDeduction();

                    BASFChallanSelection[] basfChallanSelection = context.ChallanDetails.Select(x => new BASFChallanSelection { ChallanDetail = x, ChallanProduct = x.ChallanProducts.Where(p => p.ProductId == productId).FirstOrDefault(), InputQuantity = x.ChallanProducts.Where(p => p.ProductId == productId).Sum(p => p.InputQuantity), OutputQuantity = context.InvoiceChallanDeductions.Where(z => z.ChallanProduct.ProductId == productId).Sum(p => p.OutQuantity).Value }).Where(q => q.ChallanProduct.ProductId == productId).OrderBy(x => x.ChallanDetail.ChallanDate).ToArray();

                    List<BASFChallanSelection> selection = new List<BASFChallanSelection>();
                    foreach (var basfChallan in basfChallanSelection)
                    {
                        //basfChallan.RemainingQuantity = (basfChallan.InputQuantity ?? 0) - (basfChallan.OutputQuantity ?? 0);

                        if (basfChallan.ChallanProduct != null)
                        {
                            basfChallan.InputQuantity = basfChallan.ChallanProduct.InputQuantity ?? 0;
                            basfChallan.RemainingQuantity = basfChallan.InputQuantity ?? 0;

                            if (basfChallan.ChallanProduct.InvoiceChallanDeductions != null && basfChallan.ChallanProduct.InvoiceChallanDeductions.Count > 0)
                            {
                                basfChallan.OutputQuantity = basfChallan.ChallanProduct.InvoiceChallanDeductions.Where(x => x.ChallanProductId == basfChallan.ChallanProduct.ChallanProductId).Sum(x => x.OutQuantity) ?? 0;
                                basfChallan.RemainingQuantity = (basfChallan.InputQuantity - basfChallan.ChallanProduct.InvoiceChallanDeductions.Sum(x => x.OutQuantity)) ?? basfChallan.InputQuantity ?? 0;
                            }

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

        private BASFChallanDeduction GetAllBASFChallanByProductIdPrivate(ProductIdModel model)
        {
            int productId = model.ProductId;
            using (var context = new erpdbEntities())
            {
                try
                {
                    BASFChallanDeduction basfChallanDeduction = new BASFChallanDeduction();

                    BASFChallanSelection[] basfChallanSelection = context.ChallanDetails.Select(x => new BASFChallanSelection { ChallanDetail = x, ChallanProduct = x.ChallanProducts.Where(p => p.ProductId == productId).FirstOrDefault(), InputQuantity = x.ChallanProducts.Where(p => p.ProductId == productId).Sum(p => p.InputQuantity), OutputQuantity = context.ChallanDeductions.Where(z => z.ChallanProduct.ProductId == productId).Sum(p => p.OutQuantity).Value }).Where(q => q.ChallanProduct.ProductId == productId).OrderBy(x => x.ChallanDetail.ChallanDate).ToArray();

                    List<BASFChallanSelection> selection = new List<BASFChallanSelection>();
                    foreach (var basfChallan in basfChallanSelection)
                    {
                        //basfChallan.RemainingQuantity = (basfChallan.InputQuantity ?? 0) - (basfChallan.OutputQuantity ?? 0);

                        if (basfChallan.ChallanProduct != null)
                        {
                            //basfChallan.InputQuantity = basfChallan.ChallanProduct.InputQuantity ?? 0;
                            //basfChallan.OutputQuantity = basfChallan.ChallanProduct.ChallanDeductions.Where(x => x.ChallanProductId == basfChallan.ChallanProduct.ChallanProductId).Sum(x => x.OutQuantity) ?? 0;
                            //basfChallan.RemainingQuantity = (basfChallan.InputQuantity ?? 0) - (basfChallan.OutputQuantity ?? 0);

                            basfChallan.InputQuantity = basfChallan.ChallanProduct.InputQuantity ?? 0;
                            basfChallan.RemainingQuantity = basfChallan.InputQuantity ?? 0;

                            if (basfChallan.ChallanProduct.ChallanDeductions != null && basfChallan.ChallanProduct.ChallanDeductions.Count > 0)
                            {
                                basfChallan.OutputQuantity = basfChallan.ChallanProduct.ChallanDeductions.Where(x => x.ChallanProductId == basfChallan.ChallanProduct.ChallanProductId).Sum(x => x.OutQuantity) ?? 0;
                                basfChallan.RemainingQuantity = (basfChallan.InputQuantity - basfChallan.ChallanProduct.ChallanDeductions.Sum(x => x.OutQuantity)) ?? basfChallan.InputQuantity ?? 0;
                            }
                            else if (basfChallan.ChallanProduct.AccChallanDeductions != null && basfChallan.ChallanProduct.AccChallanDeductions.Count > 0)
                            {
                                basfChallan.OutputQuantity = basfChallan.ChallanProduct.AccChallanDeductions.Where(x => x.ChallanProductId == basfChallan.ChallanProduct.ChallanProductId).Sum(x => x.OutQuantity) ?? 0;
                                basfChallan.RemainingQuantity = (basfChallan.InputQuantity - basfChallan.ChallanProduct.AccChallanDeductions.Sum(x => x.OutQuantity)) ?? basfChallan.InputQuantity ?? 0;
                            }
                            else if (basfChallan.ChallanProduct.AssemblyChallanDeductions != null && basfChallan.ChallanProduct.AssemblyChallanDeductions.Count > 0)
                            {
                                basfChallan.OutputQuantity = basfChallan.ChallanProduct.AssemblyChallanDeductions.Where(x => x.ChallanProductId == basfChallan.ChallanProduct.ChallanProductId).Sum(x => x.OutQuantity) ?? 0;
                                basfChallan.RemainingQuantity = (basfChallan.InputQuantity - basfChallan.ChallanProduct.AssemblyChallanDeductions.Sum(x => x.OutQuantity)) ?? basfChallan.InputQuantity ?? 0;
                            }

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
            try
            {
                return Ok(GetAllBASFChallanByProductIdPrivate(model));
            }
            catch (Exception e)
            {
                return InternalServerError();
            }
        }

        private BASFPODeduction GetAllBASFPOByProductIdPrivate(ProductIdModel model)
        {
            int productId = model.ProductId;
            using (var context = new erpdbEntities())
            {
                try
                {
                    BASFPODeduction basfPODeduction = new BASFPODeduction();

                    BASFPOSelection[] basfPOSelection = context.PODetails.Select(x => new BASFPOSelection { PODetail = x, POProduct = x.POProducts.Where(p => p.ProductId == productId).FirstOrDefault(), InputQuantity = x.POProducts.Where(p => p.ProductId == productId).Sum(p => p.InputQuantity), OutputQuantity = context.PODeductions.Where(z => z.POProduct.ProductId == productId).Sum(p => p.OutQuantity).Value }).Where(q => q.POProduct.ProductId == productId).OrderBy(x => x.PODetail.PODate).ToArray();

                    List<BASFPOSelection> selection = new List<BASFPOSelection>();
                    foreach (var basfPO in basfPOSelection)
                    {
                        //basfChallan.RemainingQuantity = (basfChallan.InputQuantity ?? 0) - (basfChallan.OutputQuantity ?? 0);

                        if (basfPO.POProduct != null)
                        {
                            //basfChallan.InputQuantity = basfChallan.ChallanProduct.InputQuantity ?? 0;
                            //basfChallan.OutputQuantity = basfChallan.ChallanProduct.ChallanDeductions.Where(x => x.ChallanProductId == basfChallan.ChallanProduct.ChallanProductId).Sum(x => x.OutQuantity) ?? 0;
                            //basfChallan.RemainingQuantity = (basfChallan.InputQuantity ?? 0) - (basfChallan.OutputQuantity ?? 0);

                            basfPO.InputQuantity = basfPO.POProduct.InputQuantity ?? 0;
                            basfPO.RemainingQuantity = basfPO.InputQuantity ?? 0;

                            if (basfPO.POProduct.PODeductions != null && basfPO.POProduct.PODeductions.Count > 0)
                            {
                                basfPO.OutputQuantity = basfPO.POProduct.PODeductions.Where(x => x.POProductId == basfPO.POProduct.POProductId).Sum(x => x.OutQuantity) ?? 0;
                                basfPO.RemainingQuantity = (basfPO.InputQuantity - basfPO.POProduct.PODeductions.Sum(x => x.OutQuantity)) ?? basfPO.InputQuantity ?? 0;
                            }
                            else if (basfPO.POProduct.AccPODeductions != null && basfPO.POProduct.AccPODeductions.Count > 0)
                            {
                                basfPO.OutputQuantity = basfPO.POProduct.AccPODeductions.Where(x => x.POProductId == basfPO.POProduct.POProductId).Sum(x => x.OutQuantity) ?? 0;
                                basfPO.RemainingQuantity = (basfPO.InputQuantity - basfPO.POProduct.AccPODeductions.Sum(x => x.OutQuantity)) ?? basfPO.InputQuantity ?? 0;
                            }
                            else if (basfPO.POProduct.AssemblyPODeductions != null && basfPO.POProduct.AssemblyPODeductions.Count > 0)
                            {
                                basfPO.OutputQuantity = basfPO.POProduct.AssemblyPODeductions.Where(x => x.POProductId == basfPO.POProduct.POProductId).Sum(x => x.OutQuantity) ?? 0;
                                basfPO.RemainingQuantity = (basfPO.InputQuantity - basfPO.POProduct.AssemblyPODeductions.Sum(x => x.OutQuantity)) ?? basfPO.InputQuantity ?? 0;
                            }

                            if (basfPO.RemainingQuantity > 0)
                                selection.Add(basfPO);
                        }
                    }

                    basfPODeduction.BASFPOSelections = selection.ToArray();

                    return basfPODeduction;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }

        [HttpPost, Route("GetAllBASFPOByProductId")]
        public IHttpActionResult GetAllBASFPOByProductId(ProductIdModel model)
        {
            try
            {
                return Ok(GetAllBASFPOByProductIdPrivate(model));
            }
            catch (Exception e)
            {
                return null;
            }
        }


        [HttpGet, Route("GetAllVendorChallans")]
        public IHttpActionResult GetAllVendorChallans()
        {
            using (var context = new erpdbEntities())
            {
                try
                {
                    var vendorChallans = context.VendorChallans.Where(x => x.IsNg == 0).OrderByDescending(x => new { x.VendorChallanDate, x.CreateDate, x.VendorChallanNo }).ToList();

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

                            //List<ChallanDeductionModel> challanDeductionModelList = new List<ChallanDeductionModel>();
                            //foreach (var challanDeduction in outStock.ChallanDeductions)
                            //{
                            //    ChallanDeductionModel challanDeductionModel = new ChallanDeductionModel();
                            //    challanDeductionModel.ChallanDeductionId = challanDeduction.ChallanDeductionId;

                            //    ChallanProductModel challanProductModel = new ChallanProductModel();
                            //    challanProductModel.ChallanDeductions = null;
                            //    challanProductModel.ChallanProduct = challanDeduction.ChallanProduct;
                            //    challanProductModel.ProductDetail = challanDeduction.ChallanProduct.ProductDetail;
                            //    challanProductModel.ChallanDetail = challanDeduction.ChallanProduct.ChallanDetail;

                            //    var inputQuantity = challanProductModel.ChallanProduct.InputQuantity ?? 0;
                            //    challanProductModel.RemainingQuantity = (inputQuantity - challanDeduction.ChallanProduct.ChallanDeductions.Sum(x => x.OutQuantity)) ?? inputQuantity;

                            //    challanDeductionModel.ChallanProduct = challanProductModel;
                            //    challanDeductionModel.ChallanProductId = challanDeduction.ChallanProductId ?? 0;
                            //    challanDeductionModel.CreateDate = challanDeduction.CreateDate ?? new DateTime();
                            //    challanDeductionModel.EditDate = challanDeduction.EditDate ?? new DateTime();
                            //    challanDeductionModel.OutStockId = challanDeduction.OutStockId ?? 0;
                            //    challanDeductionModel.OutQuantity = challanDeduction.OutQuantity ?? 0;

                            //    challanDeductionModelList.Add(challanDeductionModel);
                            //}

                            //outStockModel.ChallanDeductions = challanDeductionModelList.ToArray();

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

        [HttpGet, Route("GetAllBASFInvoices")]
        public IHttpActionResult GetAllBASFInvoices()
        {
            using (var context = new erpdbEntities())
            {
                try
                {
                    var basfInvoices = context.BASFInvoices.Where(x => x.IsNg == 0).OrderByDescending(x => new { x.BASFInvoiceDate, x.CreateDate, x.BASFInvoiceNo, x.BASFInvoiceId }).ToList();

                    List<BASFInvoiceModel> modelList = new List<BASFInvoiceModel>();
                    foreach (var basfInvoice in basfInvoices)
                    {
                        BASFInvoiceModel model = new BASFInvoiceModel();
                        model.BASFInvoiceId = basfInvoice.BASFInvoiceId;
                        model.BASFInvoiceNo = basfInvoice.BASFInvoiceNo;
                        model.BASFInvoiceDate = basfInvoice.BASFInvoiceDate ?? new DateTime();
                        model.CreateDate = basfInvoice.CreateDate ?? new DateTime();
                        model.EditDate = basfInvoice.EditDate ?? new DateTime();

                        List<InvoiceOutStockModel> outStockModelList = new List<InvoiceOutStockModel>();
                        foreach (var outStock in basfInvoice.InvoiceOutStocks)
                        {
                            InvoiceOutStockModel outStockModel = new InvoiceOutStockModel();
                            outStockModel.BASFInvoiceId = outStock.BASFInvoiceId ?? 0;
                            outStockModel.InvoiceOutStockId = outStock.InvoiceOutStockId;
                            outStockModel.OutputQuantity = outStock.OutputQuantity ?? 0;
                            outStockModel.CreateDate = outStock.CreateDate ?? new DateTime();
                            outStockModel.EditDate = outStock.EditDate ?? new DateTime();

                            outStockModelList.Add(outStockModel);
                        }

                        model.InvoiceOutStocks = outStockModelList.ToArray();

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

        [HttpGet, Route("GetAllNgVendorChallans")]
        public IHttpActionResult GetAllNgVendorChallans()
        {
            using (var context = new erpdbEntities())
            {
                try
                {
                    var vendorChallans = context.VendorChallans.Where(x => x.IsNg == 1).OrderByDescending(x => new { x.VendorChallanDate, x.CreateDate, x.VendorChallanNo }).ToList();

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

                            //List<ChallanDeductionModel> challanDeductionModelList = new List<ChallanDeductionModel>();
                            //foreach (var challanDeduction in outStock.ChallanDeductions)
                            //{
                            //    ChallanDeductionModel challanDeductionModel = new ChallanDeductionModel();
                            //    challanDeductionModel.ChallanDeductionId = challanDeduction.ChallanDeductionId;

                            //    ChallanProductModel challanProductModel = new ChallanProductModel();
                            //    challanProductModel.ChallanDeductions = null;
                            //    challanProductModel.ChallanProduct = challanDeduction.ChallanProduct;
                            //    challanProductModel.ProductDetail = challanDeduction.ChallanProduct.ProductDetail;
                            //    challanProductModel.ChallanDetail = challanDeduction.ChallanProduct.ChallanDetail;

                            //    var inputQuantity = challanProductModel.ChallanProduct.InputQuantity ?? 0;
                            //    challanProductModel.RemainingQuantity = (inputQuantity - challanDeduction.ChallanProduct.ChallanDeductions.Sum(x => x.OutQuantity)) ?? inputQuantity;

                            //    challanDeductionModel.ChallanProduct = challanProductModel;
                            //    challanDeductionModel.ChallanProductId = challanDeduction.ChallanProductId ?? 0;
                            //    challanDeductionModel.CreateDate = challanDeduction.CreateDate ?? new DateTime();
                            //    challanDeductionModel.EditDate = challanDeduction.EditDate ?? new DateTime();
                            //    challanDeductionModel.OutStockId = challanDeduction.OutStockId ?? 0;
                            //    challanDeductionModel.OutQuantity = challanDeduction.OutQuantity ?? 0;

                            //    challanDeductionModelList.Add(challanDeductionModel);
                            //}

                            //outStockModel.ChallanDeductions = challanDeductionModelList.ToArray();

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
                    return Ok(GetVendorChallanByVendorChallanNoPrivate(vendorChallanNoModel.VendorChallanNo));
                }
                catch (Exception e)
                {
                    return InternalServerError();
                }
            }
        }

        [HttpPost, Route("GetVendorChallanGridByVendorChallanNo")]
        public IHttpActionResult GetVendorChallanGridByVendorChallanNo(VendorChallanNoModel vendorChallanNoModel)
        {
            using (var context = new erpdbEntities())
            {
                try
                {
                    return Ok(GetVendorChallanGridByVendorChallanNoPrivate(vendorChallanNoModel.VendorChallanNo));
                }
                catch (Exception e)
                {
                    return InternalServerError();
                }
            }
        }

        private List<VendorChallanGridModel> GetVendorChallanGridByVendorChallanNoPrivate(int vendorChallanNo)
        {
            using (var context = new erpdbEntities())
            {
                try
                {
                    VendorChallanModel model = GetVendorChallanByVendorChallanNoPrivate(vendorChallanNo);

                    List<VendorChallanGridModel> list = new List<VendorChallanGridModel>();

                    foreach (var outStock in model.OutStocks)
                    {
                        foreach (var challanDeduction in outStock.ChallanDeductions)
                        {
                            outStock.MainQntSum += challanDeduction.OutQuantity;
                        }

                        foreach (var outAssembly in outStock.OutAssemblys)
                        {
                            foreach (var assemblyChallanDeduction in outAssembly.AssemblyChallanDeductions)
                            {
                                outAssembly.AssemblyQntSum += assemblyChallanDeduction.OutQuantity;
                            }
                        }

                        foreach (var outAcc in outStock.OutAccs)
                        {
                            foreach (var accChallanDeduction in outAcc.AccChallanDeductions)
                            {
                                outAcc.AccQntSum += accChallanDeduction.OutQuantity;
                            }
                        }
                    }

                    foreach (var outStock in model.OutStocks)
                    {
                        int index = 0;
                        foreach (var challanDeduction in outStock.ChallanDeductions)
                        {
                            VendorChallanGridModel item = new VendorChallanGridModel();

                            if (index == 0)
                            {
                                item.ProjectName = challanDeduction.ChallanProduct.ProductDetail.ProjectName;
                                item.OutputCode = challanDeduction.ChallanProduct.ProductDetail.OutputCode;
                                item.OutputMaterialDesc = challanDeduction.ChallanProduct.ProductDetail.OutputMaterialDesc;
                                item.OutputQuantity = outStock.OutputQuantity.ToString();

                                if (outStock.PODeductions != null && outStock.PODeductions.Length > 0)
                                {
                                    //item.BASFPONumber = "<ul>";
                                    int cnt = 0;
                                    foreach (var poDeduction in outStock.PODeductions)
                                    {
                                        //item.BASFPONumber += "<li>" + poDeduction.POProduct.PODetail.PONo + "</li>";
                                        if (cnt == outStock.PODeductions.Length - 1)
                                            item.BASFPONumber += poDeduction.POProduct.PODetail.PONo;
                                        else
                                            item.BASFPONumber += poDeduction.POProduct.PODetail.PONo + "\n";
                                        cnt++;
                                    }
                                    //item.BASFPONumber += "</ul>";
                                }
                                else
                                {
                                    item.BASFPONumber = "NA";
                                }
                            }

                            item.InputCode = challanDeduction.ChallanProduct.ProductDetail.InputCode;
                            item.InputMaterialDesc = challanDeduction.ChallanProduct.ProductDetail.InputMaterialDesc;
                            item.InputQuantity = challanDeduction.OutQuantity.ToString();
                            item.PartType = challanDeduction.ChallanProduct.ProductDetail.ProductTypeName;
                            item.BASFChallanNo = challanDeduction.ChallanProduct.ChallanDetail.ChallanNo;
                            item.Balance = challanDeduction.ChallanProduct.RemainingQuantity.ToString();

                            int challanDedLen = outStock.ChallanDeductions.Length + 1;    //+1 for SUBTOTAL row

                            int assembChallanDedLen = 0;
                            foreach (var outAssembly in outStock.OutAssemblys)
                            {
                                assembChallanDedLen += outAssembly.AssemblyChallanDeductions.Length;
                            }
                            assembChallanDedLen = assembChallanDedLen == 0 ? 0 : assembChallanDedLen + 1;


                            int accChallanDedLen = 0;
                            foreach (var outAcc in outStock.OutAccs)
                            {
                                accChallanDedLen += outAcc.AccChallanDeductions.Length;
                            }
                            accChallanDedLen = accChallanDedLen == 0 ? 0 : accChallanDedLen + 1;

                            if (index == 0)
                                item.RowSpan = challanDedLen + assembChallanDedLen + accChallanDedLen;

                            list.Add(item);

                            index++;
                        }

                        VendorChallanGridModel item2 = new VendorChallanGridModel();
                        item2.InputMaterialDesc = "SUBTOTAL";
                        item2.InputQuantity = outStock.MainQntSum.ToString();
                        list.Add(item2);

                        foreach (var outAssembly in outStock.OutAssemblys)
                        {
                            foreach (var assemblyChallanDeduction in outAssembly.AssemblyChallanDeductions)
                            {
                                VendorChallanGridModel item = new VendorChallanGridModel();

                                item.InputCode = assemblyChallanDeduction.ChallanProduct.ProductDetail.InputCode;
                                item.InputMaterialDesc = assemblyChallanDeduction.ChallanProduct.ProductDetail.InputMaterialDesc;
                                item.InputQuantity = assemblyChallanDeduction.OutQuantity.ToString();
                                item.PartType = assemblyChallanDeduction.ChallanProduct.ProductDetail.ProductTypeName;
                                item.BASFChallanNo = assemblyChallanDeduction.ChallanProduct.ChallanDetail.ChallanNo;
                                item.Balance = assemblyChallanDeduction.ChallanProduct.RemainingQuantity.ToString();

                                list.Add(item);
                            }

                            if (outAssembly.AssemblyQntSum > 0)
                            {
                                VendorChallanGridModel item3 = new VendorChallanGridModel();
                                item3.InputMaterialDesc = "SUBTOTAL";
                                item3.InputQuantity = outAssembly.AssemblyQntSum.ToString();
                                list.Add(item3);
                            }
                        }

                        foreach (var outAcc in outStock.OutAccs)
                        {
                            foreach (var accChallanDeductions in outAcc.AccChallanDeductions)
                            {
                                VendorChallanGridModel item = new VendorChallanGridModel();

                                item.InputCode = accChallanDeductions.ChallanProduct.ProductDetail.InputCode;
                                item.InputMaterialDesc = accChallanDeductions.ChallanProduct.ProductDetail.InputMaterialDesc;
                                item.InputQuantity = accChallanDeductions.OutQuantity.ToString();
                                item.PartType = accChallanDeductions.ChallanProduct.ProductDetail.ProductTypeName;
                                item.BASFChallanNo = accChallanDeductions.ChallanProduct.ChallanDetail.ChallanNo;
                                item.Balance = accChallanDeductions.ChallanProduct.RemainingQuantity.ToString();

                                list.Add(item);
                            }

                            if (outAcc.AccQntSum > 0)
                            {
                                VendorChallanGridModel item3 = new VendorChallanGridModel();
                                item3.InputMaterialDesc = "SUBTOTAL";
                                item3.InputQuantity = outAcc.AccQntSum.ToString();
                                list.Add(item3);
                            }
                        }
                    }

                    return list;
                }
                catch (Exception e)
                {
                    throw new Exception();
                }
            }
        }

        [HttpPost, Route("GetBASFInvoiceByBASFInvoiceId")]
        public IHttpActionResult GetBASFInvoiceByBASFInvoiceId(VendorChallanNoModel vendorChallanNoModel)
        {
            using (var context = new erpdbEntities())
            {
                try
                {
                    return Ok(GetBASFInvoiceByBASFInvoiceIdPrivate(vendorChallanNoModel.VendorChallanNo));
                }
                catch (Exception e)
                {
                    return InternalServerError();
                }
            }
        }

        [HttpPost, Route("PrintVendorChallanByVendorChallanNo")]
        public IHttpActionResult PrintVendorChallanByVendorChallanNo(VendorChallanNoModel vendorChallanNoModel)
        {
            using (var context = new erpdbEntities())
            {
                try
                {
                    VendorChallanModel vendorChallan = GetVendorChallanByVendorChallanNoPrivate(vendorChallanNoModel.VendorChallanNo);

                    string html = "";

                    html += "<html><div style=\"margin: 1%\"><table style=\"border: 1px solid; width: 100%;\"><tr style=\"width: 100%\"><td style=\"width: 33%\"><b>Vibrant Challan No: </b>" + vendorChallan.VendorChallanNo + "</td><td style=\"width: 33%\"><b>Vibrant Challan Date: </b>" + vendorChallan.VendorChallanDate.ToShortDateString() + "</td>" + "<td style=\"width: 33%\"><b>Total Stock Out: </b>" + vendorChallan.OutStocks.Sum(x => x.OutputQuantity).ToString() + "</td></tr></table>";

                    html += "<br>";

                    html += "<table style=\"border: 1px solid; border-collapse: collapse; width: 100%;\">";
                    html += "<tr style=\"width: 100%\">";
                    html += "<th style=\"border: 1px solid\">Project Name</th>";
                    html += "<th style=\"border: 1px solid\">Output Code</th>";
                    html += "<th style=\"border: 1px solid\">Output Material Description</th>";
                    html += "<th style=\"border: 1px solid\">Output Quantity</th>";
                    html += "<th style=\"border: 1px solid\">Input Code</th>";
                    html += "<th style=\"border: 1px solid\">Input Material Description</th>";
                    html += "<th style=\"border: 1px solid\">Input Quantity</th>";
                    html += "<th style=\"border: 1px solid\">Part Type</th>";
                    html += "<th style=\"border: 1px solid\">BASF Challan No</th>";
                    html += "<th style=\"border: 1px solid\">Balance</th>";
                    html += "<th style=\"border: 1px solid\">BASF PO Number</th>";
                    html += "</tr>";

                    int outStockIndex = 0;
                    foreach (var outStock in vendorChallan.OutStocks)
                    {
                        var mainOutQnt = 0;
                        int challanDeductionIndex = 0;
                        foreach (var challanDeduction in outStock.ChallanDeductions)
                        {
                            html += "<tr style=\"width: 100%\">";

                            if (challanDeductionIndex == 0)
                            {
                                var projName = challanDeduction.ChallanProduct.ProductDetail.ProjectName;
                                html += "<td style=\"border: 1px solid\">" + projName + "</td>";

                                var outputCode = challanDeduction.ChallanProduct.ProductDetail.OutputCode;
                                html += "<td style=\"border: 1px solid\">" + outputCode + "</td>";

                                var outputMatDesc = challanDeduction.ChallanProduct.ProductDetail.OutputMaterialDesc;
                                html += "<td style=\"border: 1px solid\">" + outputMatDesc + "</td>";

                                var outputQnt = outStock.OutputQuantity;
                                html += "<td style=\"border: 1px solid\">" + outputQnt + "</td>";
                            }
                            else
                            {
                                var projName = "";
                                html += "<td style=\"border: 1px solid\">" + projName + "</td>";

                                var outputCode = "";
                                html += "<td style=\"border: 1px solid\">" + outputCode + "</td>";

                                var outputMatDesc = "";
                                html += "<td style=\"border: 1px solid\">" + outputMatDesc + "</td>";

                                var outputQnt = "";
                                html += "<td style=\"border: 1px solid\">" + outputQnt + "</td>";
                            }

                            var inputCode = challanDeduction.ChallanProduct.ProductDetail.InputCode;
                            html += "<td style=\"border: 1px solid\">" + inputCode + "</td>";

                            var inputMatDesc = challanDeduction.ChallanProduct.ProductDetail.InputMaterialDesc;
                            html += "<td style=\"border: 1px solid\">" + inputMatDesc + "</td>";

                            var inputQnt = challanDeduction.OutQuantity;
                            html += "<td style=\"border: 1px solid\">" + inputQnt + "</td>";
                            mainOutQnt += Convert.ToInt32(inputQnt);

                            var partType = challanDeduction.ChallanProduct.ProductDetail.ProductTypeName;
                            html += "<td style=\"border: 1px solid\">" + partType + "</td>";

                            var basfChallanNo = challanDeduction.ChallanProduct.ChallanDetail.ChallanNo;
                            html += "<td style=\"border: 1px solid\">" + basfChallanNo + "</td>";

                            var balance = challanDeduction.ChallanProduct.RemainingQuantity;
                            html += "<td style=\"border: 1px solid\">" + balance + "</td>";

                            if (challanDeductionIndex == 0)
                            {
                                var poNos = "<ul>";
                                foreach (var poDeduction in outStock.PODeductions)
                                {
                                    var poNo = poDeduction.POProduct.PODetail.PONo;
                                    poNos += "<li>" + poNo + "</li>";
                                }

                                poNos += "</ul>";

                                var outAssemblyChallanDeductionsCount = 0;
                                foreach (var outAssemb in outStock.OutAssemblys)
                                {
                                    outAssemblyChallanDeductionsCount += outAssemb.AssemblyChallanDeductions.Count() + 1;
                                }

                                var outAccChallanDeductionsCount = 0;
                                foreach (var outAccec in outStock.OutAccs)
                                {
                                    outAccChallanDeductionsCount += outAccec.AccChallanDeductions.Count() + 1;
                                }

                                int rowSpanCount = outStock.ChallanDeductions.Count() + 1 + outAssemblyChallanDeductionsCount + outAccChallanDeductionsCount;

                                html += "<td style=\"border: 1px solid\" rowspan=\"" + rowSpanCount + "\">" + poNos + "</td>";
                            }

                            html += "</tr>";

                            challanDeductionIndex++;
                        }

                        html += "<tr style=\"width: 100%\">";
                        html += "<td style=\"border: 1px solid\"></td>";
                        html += "<td style=\"border: 1px solid\"></td>";
                        html += "<td style=\"border: 1px solid\"></td>";
                        html += "<td style=\"border: 1px solid\"></td>";
                        html += "<td style=\"border: 1px solid\"></td>";
                        html += "<td style=\"border: 1px solid; font-weight: bold; font-style: italic; text-decoration: underline;\">SUBTOTAL</td>";
                        html += "<td style=\"border: 1px solid; font-weight: bold; font-style: italic; text-decoration: underline;\">" + mainOutQnt + "</td>";
                        html += "<td style=\"border: 1px solid\"></td>";
                        html += "<td style=\"border: 1px solid\"></td>";
                        html += "<td style=\"border: 1px solid\"></td>";
                        html += "</tr>";

                        foreach (var outAssembly in outStock.OutAssemblys)
                        {
                            var assemblyQntSum = 0;
                            foreach (var assemblyChallanDeduction in outAssembly.AssemblyChallanDeductions)
                            {
                                html += "<tr style=\"width: 100%\">";

                                var projName = "";
                                html += "<td style=\"border: 1px solid\">" + projName + "</td>";

                                var outputCode = "";
                                html += "<td style=\"border: 1px solid\">" + outputCode + "</td>";

                                var outputMatDesc = "";
                                html += "<td style=\"border: 1px solid\">" + outputMatDesc + "</td>";

                                var outputQnt = "";
                                html += "<td style=\"border: 1px solid\">" + outputQnt + "</td>";

                                var inputCode = assemblyChallanDeduction.ChallanProduct.ProductDetail.InputCode;
                                html += "<td style=\"border: 1px solid\">" + inputCode + "</td>";

                                var inputMatDesc = assemblyChallanDeduction.ChallanProduct.ProductDetail.InputMaterialDesc;
                                html += "<td style=\"border: 1px solid\">" + inputMatDesc + "</td>";

                                var inputQnt = assemblyChallanDeduction.OutQuantity;
                                html += "<td style=\"border: 1px solid\">" + inputQnt + "</td>";
                                assemblyQntSum += Convert.ToInt32(inputQnt);

                                var partType = assemblyChallanDeduction.ChallanProduct.ProductDetail.ProductTypeName;
                                html += "<td style=\"border: 1px solid\">" + partType + "</td>";

                                var basfChallanNo = assemblyChallanDeduction.ChallanProduct.ChallanDetail.ChallanNo;
                                html += "<td style=\"border: 1px solid\">" + basfChallanNo + "</td>";

                                var balance = assemblyChallanDeduction.ChallanProduct.RemainingQuantity;
                                html += "<td style=\"border: 1px solid\">" + balance + "</td>";

                                html += "</tr>";
                            }

                            html += "<tr style=\"width: 100%\">";
                            html += "<td style=\"border: 1px solid\"></td>";
                            html += "<td style=\"border: 1px solid\"></td>";
                            html += "<td style=\"border: 1px solid\"></td>";
                            html += "<td style=\"border: 1px solid\"></td>";
                            html += "<td style=\"border: 1px solid\"></td>";
                            html += "<td style=\"border: 1px solid; font-weight: bold; font-style: italic; text-decoration: underline;\">SUBTOTAL</td>";
                            html += "<td style=\"border: 1px solid; font-weight: bold; font-style: italic; text-decoration: underline;\">" + assemblyQntSum + "</td>";
                            html += "<td style=\"border: 1px solid\"></td>";
                            html += "<td style=\"border: 1px solid\"></td>";
                            html += "<td style=\"border: 1px solid\"></td>";
                            html += "</tr>";
                        }

                        foreach (var outAcc in outStock.OutAccs)
                        {
                            var accQntSum = 0;
                            foreach (var assemblyChallanDeduction in outAcc.AccChallanDeductions)
                            {
                                html += "<tr style=\"width: 100%\">";

                                var projName = "";
                                html += "<td style=\"border: 1px solid\">" + projName + "</td>";

                                var outputCode = "";
                                html += "<td style=\"border: 1px solid\">" + outputCode + "</td>";

                                var outputMatDesc = "";
                                html += "<td style=\"border: 1px solid\">" + outputMatDesc + "</td>";

                                var outputQnt = "";
                                html += "<td style=\"border: 1px solid\">" + outputQnt + "</td>";

                                var inputCode = assemblyChallanDeduction.ChallanProduct.ProductDetail.InputCode;
                                html += "<td style=\"border: 1px solid\">" + inputCode + "</td>";

                                var inputMatDesc = assemblyChallanDeduction.ChallanProduct.ProductDetail.InputMaterialDesc;
                                html += "<td style=\"border: 1px solid\">" + inputMatDesc + "</td>";

                                var inputQnt = assemblyChallanDeduction.OutQuantity;
                                html += "<td style=\"border: 1px solid\">" + inputQnt + "</td>";
                                accQntSum += Convert.ToInt32(inputQnt);

                                var partType = assemblyChallanDeduction.ChallanProduct.ProductDetail.ProductTypeName;
                                html += "<td style=\"border: 1px solid\">" + partType + "</td>";

                                var basfChallanNo = assemblyChallanDeduction.ChallanProduct.ChallanDetail.ChallanNo;
                                html += "<td style=\"border: 1px solid\">" + basfChallanNo + "</td>";

                                var balance = assemblyChallanDeduction.ChallanProduct.RemainingQuantity;
                                html += "<td style=\"border: 1px solid\">" + balance + "</td>";

                                html += "</tr>";
                            }

                            html += "<tr style=\"width: 100%\">";
                            html += "<td style=\"border: 1px solid\"></td>";
                            html += "<td style=\"border: 1px solid\"></td>";
                            html += "<td style=\"border: 1px solid\"></td>";
                            html += "<td style=\"border: 1px solid\"></td>";
                            html += "<td style=\"border: 1px solid\"></td>";
                            html += "<td style=\"border: 1px solid; font-weight: bold; font-style: italic; text-decoration: underline;\">SUBTOTAL</td>";
                            html += "<td style=\"border: 1px solid; font-weight: bold; font-style: italic; text-decoration: underline;\">" + accQntSum + "</td>";
                            html += "<td style=\"border: 1px solid\"></td>";
                            html += "<td style=\"border: 1px solid\"></td>";
                            html += "<td style=\"border: 1px solid\"></td>";
                            html += "</tr>";
                        }

                        outStockIndex++;
                    }

                    html += "</table></div></html>";

                    HtmlToPdf renderer = new HtmlToPdf();
                    renderer.PrintOptions.PrintHtmlBackgrounds = true;
                    renderer.PrintOptions.MarginTop = 2.5;
                    renderer.PrintOptions.MarginBottom = 0;
                    renderer.PrintOptions.MarginLeft = 1;
                    renderer.PrintOptions.MarginRight = 2.5;
                    renderer.PrintOptions.PaperSize = PdfPrintOptions.PdfPaperSize.A4;
                    renderer.PrintOptions.PaperOrientation = PdfPrintOptions.PdfPaperOrientation.Portrait;
                    renderer.PrintOptions.RenderDelay = 500;

                    string date = DateTime.Now.ToShortDateString();
                    renderer.PrintOptions.Footer = new HtmlHeaderFooter()
                    {
                        Height = 15,
                        HtmlFragment = "<div><p style=\"float: left\"><i>{page} of {total-pages}<i></p><p style=\"float: right\"><i>" + date + "<i></p></div>",
                        DrawDividerLine = true
                    };

                    renderer.PrintOptions.Header = new HtmlHeaderFooter()
                    {
                        Height = 60,
                        HtmlFragment = "<img src='data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAABl4AAAF/CAYAAAAo4LQKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAEnQAABJ0Ad5mH3gAAAAhdEVYdENyZWF0aW9uIFRpbWUAMjAxOTowODoyOCAxMzo1Njo1MJf4a20AAP94SURBVHhe7P0LfBTloT/+f/zbdNOvTar0LHiModpQUBEsChGMB12LhHih4dKgXA5ogHNAwdjDpQF7jK0EJZzKRUm/JoAeEm6VQEEIREogJdwFAQGJBCgYK6TQEtovJKn/+T0z88zuzO7sNRsI+Hn7GpnMzu7MPPPcZp6Z57lBEUBERERERERERERERERN9v+T/xIREREREREREREREVETseGFiIiIiIiIiIiIiIgoStjwQkREREREREREREREFCVseCEiIiIiIiIiIiIiIooSNrwQERERERERERERERFFCRteiIiIiIiIiIiIiIiIooQNL0RERERERERERERERFHChhciIiIiIiIiIiIiIqIoYcMLERERERERERERERFRlLDhhYiIiIiIiIiIiIiIKErY8EJERERERERERERERBQlbHghIiIiIiIiIiIiIiKKEja8EBERERERERERERERRQkbXoiIiIiIiIiIiIiIiKKEDS9ERERERERERERERERRwoYXIiIiIiIiIiIiIiKiKGHDCxERERERERERERERUZSw4YWIiIiIiIiIiIiIiChK2PBCREREREREREREREQUJWx4ISIiIiIiIiIiIiIiihI2vBAREREREREREREREUUJG16IiIiIiIiIiIiIiIiihA0vREREREREREREREREUcKGFyIiIiIiIiIiIiIioihhwwsREREREREREREREVGUsOGFiIiIiIiIiIiIiIgoStjwQkREREREREREREREFCVseCEiIiIiIiIiIiIiIooSNrwQERERERERERERERFFCRteiIiIiIiIiIiIiIiIooQNL0RERERERERERERERFHChhciIiIiomtKDVYMiscNN9ygTfFDfo8z8hMiomvWX9biP1rHyrwtHn2KquUHRERERNceNrwQEREREV1LDq/Aa8svyj+AOx78EdrIeSKia9a//Bg9u9bLPy5iw88XYnuj/JOIiIjoGsOGFyIiouteDdaNaYdY+XS88STpvTkVqJNrBFSz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oihhwwsREREREREREREREVGUsOGFiIiIiIiIiIiIiIgoStjwQkREREREREREREREFCVseCEiIiIiIiIiIiIiIooSNrwQERERERERERERERFFCRteiIiIiIiIiIiIiIiIooQNL0RERERERERERERERFHChhciIiIiIiIiIiIiIqIoYcMLERERERERERERERFRlLDhhYiIiIiIiIiIiIiIKErY8EJERERERERERERERBQlbHghIiIiIiIiIiIiIiKKEja8EBERERERERERERERRQkbXoiIiIiIiIiIiIiIiKKEDS9ERERERERERERERERRwoYXIiIiIiIiIiIiIiKiKGHDCxERERERERERERERUZSw4YWIiIiIiIiIiIiIiChK2PBCREREREREREREREQUJWx4ISIiIiIiIiIiIiIiihI2vBAREREREREREREREUUJG16IiIiIiIiIiIiIiIiihA0vREREREREREREREREUcKGFyIiIiIiIiIiIiIioihhwwsREREREREREREREVGUsOGFiIiIiIiIiIiIiIgoStjwQkREREREREREREREFCVseCEiIiIiIiIiIiIiIooSNrwQERERERERERERERFFCRteiIiIiIiIiIiIiIiIooQNL0RERERERERERERERFHChhciIiIiIiIiIiIiIqIoYcMLERERERERERERERFRlLDhhYiIiIiIiIiIiIiIKErY8EJERERERERERERERBQlbHghIiIiIiIiIiIiIiKKEja8EBERERERERERERERRQkbXoiIiIiIiIiIiIiIiKKEDS9ERERERERERERERERRwoYXIiIiIiIiIiIiIiKiKGHDCxERERERXQU1WDEoHjfccANuaJ2DnXIpERERERHRtY4NL0RERBRFjTi9tRDjU9uhdewN+g3VG+Jx+4P9kbv+BC7Lta6abVMQL/dpyO/PyIWBbMOUeP044of8HqF843rRePojTEm93R1etw8sQrX87Bup8TQ+mpKK2434cPtAFH2jAyQKajZj/vKL2qyj74O4W5u7vmybIhuW4seirE4uJCKKsvP7CvHc/a0Rq+Y3sa1xf04FmOUQBRDomuD8PhQ+d7+8lolF6/tzUHGtJCjTcY1lxYPoqmPDCxERUVSZnuAWU2y7IVhW1Sg/8zhZ1Ee/OI6fgm1y2bWvEVWFqfjRv43C3LJq1NbLxbiIml0rMTXtbvzbnANircg07noNbWW4qhdB7XJ3hf9b9ZfE3qgu4k+1f9fmAqvHJf0LuPinWoTyjetC4y7kpvTG9LIad3jVrPgYp7R5G3vz0M7d0CbifetHkbv9vPzQxvlyTGoX617/hth2mNKir2gbsSs3Bb2nl6HGiA81K/Cx3wChUFSX5qNUm3NgzJBeiNfmgxHn4rW2lriTuyvSXKX51bszkAM4VavPXm3WvNQzxbZuh9QRs7H+xFVvIqerrPHsIazM7Y8HbzfqM/IBipWHcNad3ER9Z4h+oz+23ZRr56bk9ahmGTJ7jMJ7+2pFrUWor8W+jVXwl+XULOun10G9pvjbH0T/KcXY4znJRC3C5RObUTg+Fe1ay7pjbGu0Sx2Pws1NeKjL7zVBDZZl9sCo9/bJa5l61O7biKpwynBRj36trSdtxbbLxRWrqpiO68BVqXi0lLKBZRS1DGx4ISIiiqoqbJVPcKvqqxdjeP5WnwaCry//Xb84vnhJ//d6cKYEk0eV68eTmIXSmktQlAac2T0dKdoK9dgzqQjbtflwNWLrkuk4Lf9Sf6v6V0uwlfcGmsf2DzBdBnaHX+3GJXEe687/Cg/ri3xU7/09qk0Rub52C6ZOLPH7hsyuuYOQZ/lCNSrCuqK90rbjA0+AYPclBQ115/ErfwHSVGd+jyHGmzVTrp+mWavDWDunUp91TkDGwzH6fDCNW7HEOBcqEXd+tcQ3j71i3I2OsehTdFIubNlOHfjIlJd61NdWo+z9LKT9sD1eLg/QcErXt+oiPN3uXvSfuhK7jJZmtfFdfYCi/72YvlUuUus7i/Ub/fXVv8GHn+hLW5RvRF4KnCz7v1gli9RBy8+gQbmE82uGIklf5KNqxyrbuufFml1YOX0ourVNRaHNQ0NEV0NdxRTce7cLo+aWodp4qqu+FtVlczHKNSz6bx+fLMP/9SQonGlQcOn8Ggz1l6BsNG5d4q5Hq+qrf4UlUbto2Ys8+fBSbJ8itLyaxxUsGwLm8ddAGUXfCGx4aU7nj3g9KSQyRvVJsvEt4SmSkyjqIzPrdnki6w7O/XR2bDvkhfKF68Z57Ct8DvfLpytiW9+PHDaXE1EoOnVCJ/FPfX4xNn4Tso3DO7BKzmJAX/S5LVbMxKB11/4YmqwvRn0jvpazYWncitX5+kVQYmKi9q8IWKxmy0uzqK7a5r4pk/ZwV8SK8xh3S5z4f3Dp6en6TOUcrD2sz1o0bsfqd/RGlpQUvUmuxauuwjZPgKCriNoxcbcgLsS2grD9vRZ/Ml6UuGRs+DpzYB3mHdRnnS/0RY+Q211WQ88KEkVeoC0SWcHqq9cIe+FL2ehYj79fjih3u6pSCo5BURQol2qwPc8Fh7b0NGalTUIpq7vfQHUonT4SG7T8JxGZJVWoaxDxo6EOp7YXYVx3J2Ju1FYU2uPhwU4tzjiSfo6nfqwvbVG+CXmpcOJAuZxLQa/7W4uyOha33KLWwYLL2iTOr8gDGuqqUJJpZKrlGDUwH3ZFONGVdRjvvThdL2cdLuRtr8Elrcw6jyOl0/BU0ndxoztPipITB+BOUb3uR2tRP4m95RaRqkLViK2r8/V6tKio6KmqHvmro/WQyAV8KR9eqv/75ciuq5rVFSwbAubx10AZRd8Izdrw0rgr19TtRDx65G6H77NTRt/p185TYiE5X46Xf3yP15NCImNUnySbOxTd2o1BacQPktVgWT9T9xzxg7CiRn4Usq9x+e8ys67+UmTdwbmfzq6vxpehfOE6UbMsEz1GvYd98umK+tp92Oj3qVyvc+Oe9NfzpxTvMb2eT0TXvZ9MxuQ08W/9fMxdE3ZGfe25MUbetBOOfilyRF1j1SYU7ZJ/dLoTbeRsOOo2FsubrSn47/f/2/0GTfQuYiha7njmGehNLwdRuNH3tk3jnrV4VytGUzBiRDdtGX3z7Fo5A0e1uQ6Y1M9omQ2mDhuL5c2MlP/G+/8tG+7YCNt0sbeh+4QCvG20hYpyawkfNPoG+hhl8/VrHnQYj/H9fqQ3MMfEIbH7EMzZfhYzeuofAwkYUHwWlxUFl4/lomdofQVSCxUT9yP0y38fWUZF7uA8rDsg54muluptWC4f0kDGzzG6+216A0jsLbirzxSsObYez92hfdpy1G1EsXxYLOW/34enqnIVHxK5olpK2cAyilqGZm14OXVgnanbiYvYMXUiSnxeAzT6Tr82nxLzR+1CY5b2aqEDrvwD+pNCl45j+TCn9jkuzkfe7yNsaKouxVzj1UfVxeV4q5SjuzaPkyj7v8ar2IOw/EyD9nTFGr/vmVZhh/ncuOmv508f2g1tUwvBN7eJviluw9PjMrXGiNKX87E9lLR/+QQ2F05B/weNQc3FFH87Huyf69PvfsUkvaFXfbW68ewe6yCQj45H8T61hb8Rpz/KRf97jQGe70X/2X+0bQS+fGI9cvs/6B483N92/erRDxNkMScOGLkffYkT6yfhwc5joHcolIis2UNxjzYfjjpUrijS8+JOGXjI5cII90WMv7eJLottT0Gq8dapOO7nCvfg7D/lxzbU4zcPJn/vc4XY4+cLJxc+pv2u+op/9dk/IvdR2Yew5ZV/dR9E2JvOpdZ/eu56+ASpOnC7+S1ZNey17ZtOVCjr+NWIs3uKMT61nYwjYpL9YxfvOetpvDq5EI+Jz9qNkl1ACbMe09d/bGGI9ZbWj+IZ+dLLwXnrYL1v04ity2fq/c6njIDLb7cNIuw2F2KKOT4aYwys9+rPW+7zDbF9UHRShrkxwLAIo9QpH+G0JYiMh368z5dwsgh9ZPi0zVVf7z2Jherxtxsl47AwSz/3Nzy2UH5XhO2hlZg9wtT3uJoG26VivJ8HLs4fWSnSmpFe1XjhWVdL13bbu6E1XpMNmO4B27Vj1pfpPG8039A21/NGszmMqs/ij7mP6tv2+n6oeUBo8T8A01tP6DQWT3TWZ4Oqq8SKIr2e1SnjIbhcIzyNsMUbfQeSrpgk48EUbGs8iz3F4915gvoG83OF+2weChO7J/LTYnNf8rbnU8aNx2bJv4HKUe3k+v2wzGucXk39CayfkirDVx2s9zkUavm0t1DzDrkP2gNs1Tj7x1w8qu1zpA+0JSHJ1BZ6/Ct5jszhePkwip+7V98vrzHKfOKPvzQrNZ7eqvXZb6yvnhPfYwwjHz2/z3LetJ4GpnitF3I+6rtdf+khlPRVXdRHhpm6jr6sxTt5AKeCPCjozovih8AyNrWoy6wX4Wz0GOAzdZ6jv00Rdv4thFhPCp6XmvJLn/H2TOXEoBVwH1pIacG3vA2/14sQ45/cH082VIlR7dT1YzGpQi4KR0xbdOwq53EUNef0udDy/BDrGSaBykI3I+9uJ+OFbX6sazz9kSUPUvMKre5nXi+UfELls90A5zHEPJKa6JPPcNIuInmJ/HqmApPUuGhTrseGkaDqKldAr6p0QsZDLrg8Fy0o9rloCS8f0tPiY/Ak+VFop35XTP3sKx4iL/FcDzW97qMzX4NePlyM5+R1ptHdl13ZYOQjdpMlfKOWxwcoo4RIj9f4XvC6HJGkNKNjBSmKugmgg9Kpk0Ob7zT7kPzUsEnJ0taBklJwTC671h1TClL0YwJSFPNhNZSNlcsjP153uDoSlUSn3E5KgdhqOMz7mCXOQnCe8wklK5QvXBc88TO0MDat7w7XBqWuqkTJTDSW26UDIrp+mPIBNbNs2KnkaOnfoWSuuyDXMeep5jz4kDK7k/yu3eRIV5Z+IVcVNmXJ5SmpSmqcaT33+i5l8OAkxeG9XEyJOTtF7uRxbtNEJcnhu542id8pOGpe279za4bZbg+OLkpOxRnLNkN2YZ2SKfetQ95+bdGh2Z3kb1vDVdegHC1w2e5HXFyce95cDjccLVBcdscv1o8z5k3lgPv8dXApLlP+7lnnnLJpon3Yq5PDVaB4gvQLZWm6Xk/ymdwFbijr+HNO2ZbTxe++qGHoKjiqn5sTCxSX7TpQXAtOaL9mx7uO8EVRmvzbqYio5mE6l1r4b8pyf898Pjzn125yKOnmhHCsQBGXtGJ5B8WV6ie+T9yieGJJgLLd/Vti0sL1hLLAJf/2nlwLxKfqIWUGCFu7tJalJNqsp07pS79Stkz0c55NYelO+171THEA9vU7cxi5EuXn1u+HkwcEj/+BNWybqjjld8KpE3nCuoOiZwWmPNORqfhkBe74laKkpnrSvmcScX+BdY/PbctRuvgLBzF50m6AuAGRV3+l/ZzpXDlE/mNzbkXYWnchnLzDc747uFyWeBXoOsOcXr3X8+yvKc2bwtHlMh+Dp657dGmG33itTokiPZ3T1tX5zXPF5HRnGmGEhbus9Z08xxhiPtpwVFmaYU4n3lOiyOdMRxNC+rIN1xbJWg9xJA1WZpcfVy7JT73Z5kUi/Aos8cRm6jRbbEkIO/8OvZ4UPC8NdD3sp5wIJS0sSPXUG7ynRLGeOSHYCSf+bZnoJ304FBFsfpnjo7UKYQ4Tz2+EUucJuZ4hBSsLNSIsFtjm3fpkyVe+WKqk+8lT3McYUj4hnNum5HQJEIe968VB4wVFzFRvVKe47tlKyaf+ryfCup5xnzfj/G9RJvr5riNQgrK4oKzLlHGgQ56iV1VmK52M38lcZ8rPVOHlQycWuOS6vpM73ZiOyyGuY3zTZVPqPjp3HpIi8gTz92Risysbvlqa7lnPe3In0mjm8fb7oYr8eP1fc3sFKZHbFWp4SVHyi+QFlnOqss2SS3oyk0AXCNcW84WYU5nqPuBzStlYp1xud6MoFJ6MyJFVppRlGZmNNSMJLlAGb8/7pso3Q4CbM7ZM63uFa4MoAN2FnlEIE9F1yJQPyMzSfRM6rUgxbhd78lRzXiHy+K5O5ZHsEuWTE+f1Gx0NdcqBfE8jgrmsNF84IzFDKTokvtFwRpQN5gt2h5I0sVSpER9dOpTvqRw7xHaN4umC2GfZkO9w5Snb1ZXFJU1d1XJlmNHAb9r3QBpOLVcGfU9+R04/GrpQ2X3G2JipHAvxIsb3ZquwP0/pIH/f5yLGfJEmKsL5Mlx251kbYzxhabpIUi9G8g+JsG9QzuzOs15MmMoBc5mo3gTJLFFvTF1Szp/XzpoI0ix5Y1n8Xt52LfzVc1m1fJj7hnNakQxR07GkzFG3LVyqUbYXDBbnTsaOUNbxw7MvXuf3QJGS4b4B0Ukx3/8Ot8z3Wf+LIiVN/u2cus19gXxhzTC5nqy3+Fz06g7N7qo4HxEX15+cUPQgVffXFH/NZbK5sUSEd5ecci28G86UKmON+Gu6ER6wbPdpeJH8LRfU+OnsOEKZXX5EOVunH+mlmlIlywhbS1rzipsH6rSwuVSzXSnISPRcNAfYnsrfhaT4on39zhJGUBIzS5TjIowunZf5TJh5QLD4H1iD2P9I6q82NzOE/Xkd5G/Z1K1N8Uvdz4yiA4p6ii4dmuMJD2eO4m4bNIWDen7yttdo4dNQd0ApMt0ItTQW+YnDBp98Wjvnl5RDczxh6GloUHchjLzDcr7FlJiplOgnVqYbe+bzZ9nnhr3K9A7G75kaTS3hqO7XbuVMg4gf5/X4a4lf6j5UGfG63HTz0nx+zDdWEpWsUiOcq5TSiV20xkpVOGHhyVucythS/aac9nvZ3ZVU4xhDzEfN4aOmlSotXV9SaspNN2rMDX3B0pe6yiJ5Mz4uVVkUcpy/OuxuiDucjyjZJZ+K8y5XkuzyIk+Z7Tm3l4578pLEaR/rK6osYRdK/h1ePcny+z55aXg3PDVhpAVL3Nk+3b3cXCbaCTv+Cf7LBHvmfMkSLOdKlEFyuTncg+X5YdczQiwLPdtVt1ml5d9amp1uLPfca/GUBSnKHLXeJ2i/NzhJMZJ3SPmEKGs2ZXnu2ZjzngNFpgZmo/FQFSxeUBPYP0wVp9W7vBqFw72eMZ03S77hb3koTHHbeFhMxE4lzyhbfR4SiSAf8rtcssTH5qn7WOo26nd2i/Qk0sh5LZGaP/eXJ51T1gwz6gedxPWdkVKimcf72Y+mHm8IdTkisyvW8FJwzFPB9lwsqDyZhjVTa1DOfFqizBreW0lyeirszqTeyrgitSCTq6mMpzMdoiJ74pJyvDRb6Z0gn4yI66j0m1amnFLXP7dXKRr3iOLUMkLxW4+MU1bIQtlCvTlTNE7pneSUidvPdgPwPOkpppR85VDNbq0SYSzzbkENmbu1XL94Md/M9/fEYMMZse0RXUzHna2UHj/kP4NXj79ghNJFhrtW0S49rhwyVbg8eZrxVECckl0pMp6iEUpHrQVY/VuuIqj7UDTOdC4dTiWp9zilSM2g5TqGc3uLlHG9kzz7K8Je3b75TIWyjl+Xjiul0/opyUYcEVNcQrKIJ6XaRZLBbwu6Y6I4an9MhaB3uFouLIzPjEY6h5IqrsLOVExTHtHCSP3b9DScT5xUz0uS0ntckelmptk55XDJNKVfF2P9OCXBJryv1nnx2a5c1y6NGechTkSoS4eKlBEd9fOm/q25sEXJTtLDLCnb/EQc0dViygeMzLJhmzJVq+B5KpaeMtK7km3DT6XSUwnsoEzfa0o8pptLGFTieRpQpOyyscZ3TDdJTBei3nVGz4Ws+caHPb9PD1me8LQJn4BMN1vNFwliLsddabZexHiOx3zRozonwsxTFrvrHRfWKMPkMu+GccsNKNPFjfkmhFMchzXvuSAuJuR3LPusMl18pS9VtCA1PbHqvhnoLZR1bJn2xatxRWWuryTnH5VLrccXymnyXf8LpShNbtf90M1XytJ0ucwIy7AubkNoVLDEd3P8NT/9a4qD3hes/i7gglzY2bG72DM/7WetD3sJsj3/F7QhhJFTLPcqLMPNAwLH/yDMN9xCbNDVmL5nubjdmeO54efdCGuKXx2m7zXVbczpwqUYLyCY8w6ferWpMRHJ+Yo7tQSJw37zaXO+I9+esuxXKHmH5Xw7RdoL7UyYz597ny/VKOXmJ9bNacN0jHY3eQI1fpnfbsLYMv0cmM6Z/5vQ4YWF5ylgzw1VHyHlo4FukDUo26YaN2ShjC2TWwmSvq49Dcqpsmylu81TvY4uOco2UyZrlxd54pcpDxLc65rjkDnsQsq//fCXZwbMS5vY8BIwLXjXmQLFZ7MI4p/gv0yw51nfU8Z733Q05+2B8/zw6xmhlYV2eZ5kyj+N8sBz7e5pePEWUj5hzpvNjSsaU90GyYq72hQkXlBTnVP2qg3kNtcYiRlL3ffVwr6e8Vd+BynXA/E0PFv3YWeOvwewm7fhpbnqPuY8JHDdxz5PuiDKY+P6KqR6ZER5vP1+NO14Q6nLEVk16xgvVvdgwNR0vZ/7N1fofboGUFc6Bm3v7Y+s98tQLQc1V/snrK0uw9yh3dA1d5enn9Cvv0aD+m/9SSz4z3txd9p0lBkD2l88hJVTeyNl6HPoc8f9GDp3C/SfE7+1ZS4G3P8slpnHO26swsKn26Hb0Lkoq67V+5M3b3dSuW1/iN4SMt5AXif5R+UYdEzohlHLtUFfkJhRhL3rRqK9OlBhmA5vLIQ+tlgG0lPiEfNAbwxVA1U4WLjRN1zPl2NSV7Ht9/aZjns60u7ujp/v0dbwch7lk7qim2Uw+S2YnnY3utt+4Ws0aqtdRMUrT+D+oe/hkBb0F3FJ/7oI0oV4ul03Efamc1lfi+qyuRjarSsmlXtCtHHXa/jx/UMxt6zas78i7KenDXP3hxzKOv40Vi3DoPY/RNrUldhlxBHhYs0uEU/S8MP2L8O0O83HEYMbtZmv8bUeeXFywSh07TkVW7SDMo15dH47XnuwrVecVIOwGmVzh6Jb21QUWgaNEefw5R/jnv5TsXKfsf5F1Gjh/R8okX1bXq3zcn77a3iwrdd25bpqGvMeA+drPYLhYsUreEL8/nt6BMNFI4LVVqFCG0yqHtW/+RCf6EuJWpaYHhj8ilooHMQrc2zGITDT+pRW+7U19ZFt7r/W1r/gX+JNhcpNN4sl0r/ejFZyVuwIvvVtOWvyScVyOVeLnAflNuV030R9+Gv1s4t/l7N2qhdioCsH+9Tk6OiCnNISZIkateb0LKQlT9Lz1zNn3f2Bd0j4vpwLoK4CS+RAv84X0uAZgjsZaS849dn6Iqyo9ITqyc+MkfyBtAfMg0e0gqvvADlvcvIzuL+R9gAs33D1hc03LIb0dcE6XuMn8ARpDh40hecNN9wHT5BehBakP07DGHkoleM74ubWj2J84WZ8ftGUGYayji3TvjgHoLvXADsJ3fq6w3TXUXOFqKkS8OgQcemiqn0Xa/eI/azZiPdX6YtSRrjgd3gXjd5XvDrGi6fv5XYwDT1jzxLf1aRgpIR6NDbjUIJqf+LqGC93GeNGiMnURbjb0Y9L5VwHPNYpQc5fYUP6wuU1wGhT8gDf+B+Yp+9zkdyGPCpiSmjqKpZAzwqceCHNNBh/cho8WcEKmLICi3/5F1FvlvMQe3yTO/tpUC8lNJ5wcGKAb2JBX09iQfipxSufjr8Jnl0Q9UFtJsy8w2II+nqf2BC4x6X5TgJcOftEShESM7B0wXDbNOqbdk/i43XGjqXh8fut+xDT9RH8TM5j/59wSvxz8pC4HtOXoE/3jqbzYhZeWNzRc7Ac76cW89La4PviOjJ35X58ae7SP5R89OTH8BzO47AeTgy6PuI+GnE46tF4sUlf154YJD6ei+1na/BJSTYeccqLTaF+Xw5cWR8GvB7+bpwMZKzA6o/0sT0un/gdFhbrSx3dktBWn7UKNf+OqJ4UfYHTwio8c6s5zn4PTy+SH9X+Df+Qsz6aGv8iYIzj9u34zhgq71c4uuTg97+0z9sD1nlCrGeEVBaaw2LVM7jVHZZi+t7T8ASnHpo/Thsjcm5VJcZ3vFkb67Bw8+cwJ++Q8olPKuA5nO5e4xImoJunIIBdtSl4/YbC1wpdRhbjiLjuL589Ah3j5GLh9PJnMHD2AS2ficr1TJPUoWLJfL0cdb4grn20hZrktBdk/KxH0YrKwNeCUdT8dZ8UjPA/YKO9xgN498U86ENiD8MC77ymmfP4ph1vKHU5Iqsr2PAi4nD/Sfqguwdfx+IQRhiO7zgCs8uP4Gxdg/pmDi7VlLpv4pyevgRbfX7iKMo3fIF7cipwpkGsfygfLllPPL38PWxAKgoO1KGh4QzKjB+qX4W5poHpq99/Hs9vUG/sJiKzpEoOil+D7dP1Ivr0rEH4TSijI8fcjkd/ZsppVd97CFNLj6Nq2RDcE6s2Lo2UA7U9htDGqz2MjYV6swuGZaCnmjvFpyA9Q1+Eg4XY6NXycrjoJTnIvziirDI9XI6XILP1RVyUF70Wh4vwkucLKNMGkz+OkszWuGj7BY/K8nLAlYfd4jsNdefxq4fVpdV4//nnoQdpJkqqRPhr53I79CA9jVmDfiMHnK7Dhrdz9AzYORal6raVBtRVlSK7+3fVpUIo6/ij7ssz0OuT1vNbntNFaxTUbg5OKtUKwp4zLovf3oQsdbkqpQDH1LfELs9AT7koHOcPbPQUFmkPiGqm1VERfqfVMDp+yTSAfx3Kf/1T5Oh3MkXwbkfNJbEPDXU4UJQhjkKoL8eogfnuRre60klIk+fQ4crHAS39XELN9gJkGDdBr9Z5qSvHr39q3Jh1IW97DS6J7TbUHUCR3Ln68lEYmG/TNFtZjnKI7+w+I9JwHc7rEUxsrz16Jqlnz4Gknz+FH+tLiVqce0a8iUwRVevnz8Uaf3fsGqtQmKo2tE7Hyl1GI2bzMxo4A3Ik4JYA2eyuoskolz+T8vbv8GqffnjrE5GHGsVtdR4e+/HLKF7+PvR77w6RFd6tzQVSJyrH+sW1E6N7d9HmDF16j3ZfxMxfUqHl3apzNcbFVYjO1YgaRDQZDyUE5ki4BVqQxrvwyw0FGKzlZeJoardg7igX2jvv9jSCh7KOLdO+tE/wXBwYbvyWn5ueTZfQvb/7BsfM5Vvxp83F0G+1BLtIa0RVYSradhuK6St3mRrpW6bz5S+j/Q/TtIeFjpoe6rDjSWv/gptvkrMtQDTygNCYbkogHcN7hdzsgorl8jabczSsWUEX9B4tb/TWz8eSishvZ3jCoT1824VvxLeaK7G4hZl3RJUDzqTeGD6rFMe1B5VCPVjjISLVHWjdRs4aYr4F7/b+rz1fwPdvsru1qwozLJKGY0lZNrrLG3IXD63E1P4/RkLrPlhoPNETSj5qPNCnuqM1fA/H5umF61XsbbivXy4219R6rjuE+kXz8ZHdGM5Sm/7TMEe7DBbXE73b4Nvievc7P8zAIrW1TdT/3x7zcOTlzlWqJ4XGnBYCSHTie3LWx1WNf3FISO6H7KLdOLXzVfQwt4IFFH49I6Sy0BwWASQ69dCMd/0SGwoGQ0/e9dpDtqNc7eG8Wz74owoln/i6UZZR6uH4HI04nGYvCMiPmLgf4dHxC/HpWdP9G+HgjJXYJ/5t9rrMrtfQWjbkeKZYjCyV9Y66CniqKr1F7cSkS294qipL0ISqStRd6bpPdVEWXpG3NVPefhVPmfOaK5DHX/26Hn3TXNGGF/WJ3zFvqU8/igvw/A0Bn5SJTyvE2U8XYvyjd8EZp8f82Nv6wP2gav1uVNs95DFoCTa++m9oLb4Se89g/NxolEAHTN+yBiM7xSEmpjUeHzPF/eRF5SHjhw5g5Ux5azw9D9P6/QjapkWls/vYX2CY9kEt3i1Ts/UARGaxbNCP0e2/PU/cai58jG1fNMo3HcQv1XwmC3VPa3NAhzfCaHdJf7KbbBWOR7cn07U59UnqQkvLywGsmye/IPZ+3muP6+FyZz/kbyxwN0qZHVg3T75RI74x7zU8rn8B/fI3osDuCxYpeLtgArqK78TE3aKH3YGV8ATpNPT7kQh/MR97W3eM/YUeouqTsHqQnkftF9oSkxjE/agPcrevx0jtHk0o6/hh2hdH5v/Fb0zn99Ep7+iNgkL9/A+xM4S2tdA14uLBYox5aZn824ms8Wk+FWlt+fu/Qb87Y8U+3YJbxD9q4b1wlnwmsNMMvD2hO25Tl8fEodOQ32C6fJgY4vcrqtSZMyh917ihkYb5i/4TnfSDxG3dR2LZqZUYpG74Kp2XuoqF8BzO25jQ/TaxZ+rhdMKQ30wXe6w7WFwB7XC8pLxdgAldW4s0HIdbZL6A+J7IPaY2kl3GsdyeXk9gEbUg8b0w/nX1rZdSvLnCpnFRqNs4Ay/K1ovEzBIcOX9JxG0FyrECeQO7uSUj/6jYnrpN7+nyMgzwzbjc/vE34/nlZAztKRN9K5fW+DJR3uRSG7eHjpdPOTrHoO8DMh37ZbrZKsrfaQ9923Kx8+2Hpoml0qLl7ouYm26WGXqobrpZNuA0g+R8HLULTzFdXjbAXRa06jISxUdqUVVegOynkvSLyfpq5KVNgnE9F8o6Af3F5inbr/+pPSXYLJJcGCEjbn3+JAx8U577lBEI2O5StxEzXizXyzL14YAj57VGekU5hoIrkxDCcBhFL83SHzzQHig4pT/UIfZ3k/vJDTt/gXxIt4WJPA8ISc0azDOS9LBMpIX6e6abGaidhoe+bb7p8W08NM2dE4iswNMIGzm78/M1/tlsicVGiHlHU6UUHJO/exlnj23Aey/1gVoVjcw5/MM78Bv/GfDm6TmfL9gIKSzMb2nMwnD3ndUNeN70gFJY+ei5f/jEpcZ/hnIr+DqjXXfk4r/d+W/gJ8Yb9/0OeeplsCMJXbo49TB2ONGl3zSUHlmHkZF0+yBd/XpSqMaiTJYFPtOpKbhfrhXQFYp/WZuMfavDFztLkDukq3bPICJh1zNCLAvHlmkPCfqEpZhOTTFC0/RWREE2njIaWKvz3A9WhppPGP5is3NfX9GCgGyp929e/R+MkX/6vkXWzHUZPzwPi6lVlYe0Rmd3XeXbD8FTVVmE5S2p5cXtCtR9zn+I18bIOn6nPMwbbr0guLJ5fAuo69E3wpVteBESnh6nP/G7aAqK7O87uYXabYOF5RVl86t0Xq+E2T3hGebrrPbqUDG1F54xdStWmu+SrfH1KB/VFU8vrNIqH+dqjFvLP8StIdzx8TSKpOOZRz2lRZtHnxFLdJbuxupO4YBxPCk9cbfpjnRMexcGd5V/uNXhlOcL6Gn9Aly+X7CyuZFy8uN17qeIVz1zqyc8xfQ9z/vWMsO7Az0Hy+y0dp64GP8+7u2fi5X7v4Tn7d9Q1rFn3pe0x++33qCP6QrPm9v7EZ03t2fhMe1Yv434zkPlmzYOdMn5PX5p2weBTRcRYb/qfBSeN7cfg/83t6/Oebnir7EStSgx6PzsZK2B8eDri7Hjn/pSM0+D/A8w9oV+uEtrgRWa8+a4cGdnl5zbhdW7/b2OE9iNMXpJp/7GtirTxUQrF2bs2o48r8b7W54fgIftskIz883WoDwXMT+4y1M1L/34gJxTnUf56hVy3uQHd3kq86Ufw/KN8tWw+UYQd8ITpKsRcpDGxOFHj45E7poj2D5dvhdZX4Rycx+KoaxjYdqXo5tw0Gtfanavdnezlu6dLzdZElzulpc92GM83RasG47aGnymJwT8YOwL6HfXLVojffQuhm6Cu22uqgbn5Kz6oERV+WLY9sTq1xmcMJ5YSR+H57sn6g91iN+yuz/mSWtHscn7ZITB07hYhRrPAaCxqhyLwzsATTTygFDUuN96ciDz2dAfljDfzAjK1AgbroDnp2Y3VnsSi1edLFoizDuuqiS0f8jI39djxyFrIm3cswW/k/POxztraT+p/UPy2kh8Y8chP+VbhGGhvaXxEt7bfhIlg+Syg6uw09y7QKB8NKk9PIezA9bDacSeLe6jweOdv0n1UnP+G+jatRGb35dvwadPx/qdZ3FZu9l5FntLpqBP5K16mujWk74Ld69oOImzprd4Iiv7zWnhd9iidrEZrmsy/oVfzwipLDSHxe+2INTg1N6KGJmLNUe2w5O8y2GpJgXKJ+7sDM/hHPS6Jq3Bbk9B4NOtGl1B5kb9DvqbVs1el0l+FWfNjTjadBmFaWptxvywWHCeh0SinQ+F78rVfUQYvTEWi7RMPBET3x6Nzl43Za/EtfDVr+vRN80Vb3hBfBp+PkPv5/71xdv9Jp5wum2ImjBfZ7V1phRv5emNLnBm4f2CIejznxtwoMBofLmIDc93Rupv38OKd2STt1ejiL0DKCuUt8rTBuJhcyt9m4cx0P2qwDysM+4Y1X6F43I2NLX4KrwvBGXuSsC/RBhBmjR8Ccqyu0N/9uQiDq2civ4/TkDrPgvd436Eso4d877c4dsPgu24B1ETl4Dkftko2n0KO1/tYem/OKCwX3U2ver9LzfD/5vbV+e88LVO+sZLyMAvp4rade27KF7tm2F5+kX/Ez7celDrl/rylzsw8z+z3BetzeGO3pnuN85Ks3+Owh1Go+ll/PWznVg8pQfiXy7XlvjTo98E91sji8aO8vzG5b/is11LsHibkf51f31zMIYWH7Y0znoz32xNX/qV14WOPl1Yl+m+gbdofinU65U2Dw90H8/RGTPxuxNiK2p/wTPFcqM7TTNzOXp0Bmb+7oTYL3WMkZli+Sz9BlJY7kDvTHeIIvvnhdhhdCCuhsfOxZjSIx7uIK2YhNY9pngaqy/Xoua08V7wQ7hb7Qw/lHVsWfclc9hvcVDr8Fx9G/O3GJYpW+sdmRgd8usHoUtyjfB6Qi2ERvTvxrnj0p8+3Krv7+UvsWPmfyIrKgnhLjzQR87WvoO3tfN9GSdWjkGvUfIpvJCZGnE2bcQ29Tw3XsTn4rcy58nlJpa09vKLeO/gRa0urKbzwiHt0Mfoe7btD3CfPgcUr8cftZ/9q7uv+rs8B4B33v4d1Ch++cRKjOk1yt3lXziikQcEV43SfCO+jcGQXiE3u5huZqRj6Ve++YCiXMA69ckuzSLMLw3QD1IAlnDIHIbfyvPTePEgfjss09NoNNr05nJCB89b9CV/xJ/U6PrXv8rwC1eYeUcL4cn/azFt0CSsl/t8+cv1mDTIeDOxE14xumHu0c/9pnnttP/Aa5v1+NZ48XOsF3Gt9aQK8Vd4YXFyYR+0GzIHmz83zlkNvjDexnZ0RdId4t+Q8tEe6OfZOQyatF6O/3AZX66fhEHGI8udXoF3r9KBVBf1Qbz6kFN8n6BjUl5Vu15D23ZPa+NenPyrEd7W/NeR+aze5bWtS6g3Lt2XZaCN6e20+Nvvwl2p41G8Rx/3JRJh15MC5qVtcHeK0fnzKsxduAdnG5tS9lvTwswXpmK9jI9iozi9fwPmiHz+scJAEaB54l/zCr+eEVpZaA6LmXhh6nr3eEyNF09j/4Y5GNLuMRjBWTGpNXpMWYn9Rv5TWwNP8r5bG1copHzijt7wHE4mhv1Wj2fqOTz422HwHM7o0N/apIidWdYP8T3U8Xo+Q637/H+OlWMyYVSzOo19Qhuf8crUZfwwPyyWvhRf+dRTxHRhnfYQumbRfOhVlUjyoQR08FQ88Ee94gEjyw5XRHWfCDQeeBcvynuljmHz8AubgiS6eby9K3W8RG4iA2g2xwpSFHUT4iJbKTgmF6rOrVGGOcRyR6ay7sImJUtbB0qKe6VDyuxO+jI4XEre9lNKXYP+yaYsudz8m8cKFHFRry/P2iQX6mzXV9l9x7xsbJkiNxkev7/RoBxdkKrEGZ+ZJs9xB7A/TxHZsc937aYOefv173y1VEk3lqcUKNatHFMKUozvZCl6CHylLE03lnmFl+A5n1A8wew5f77bsH5nbFnoIXqp5hOlZNZwpXuc/G0xdZp9SH6qC2UdM/O+DFtzQS41NChlY43fMR974OPzZVrfHa6B2J0Hk01Z8jPTeTU5mp/s/lw/J6btd8hTfL+hu1rnxZMeOyi+h3NUyU82PveEhd80TNRimdKhV5mkOTRb6WR8rk2mtH9hizIx0fyZMTkUh1puqvOm3wyrjJPsvyPKqAKXIq4F5Ge+k2PiFrmuP+eUbTldAv4GEKd0HzdOSTXlD+LiRJQ+dgKXSW4XRJ3C/fvpylLtxxqUnTmJcpl1iktNdYeNufxt2JmjJJrWc09xqUqqkU+bygH7MtGk4ahS4HJ4fsdncijuIN0y0W+4JYofPxfqOv6IfVmQGmf7XW1yJCkTN1l/IejxefG/vqlOp07eZampnPOcjwvicO3Pn8NhhKkp3QSI7/7264IIT9vznehSXB3kvPkLgbaxwE/aEfuqLw89rbkWnJDrXVDWZfrGH/em/eYViYrL1UHOhxZGuvDygHDjh8aU9zmnbhNbDFHA+qzHhTXD3Pvkzlds45fOb17op76uTw4laaJ3evOK49rk+U3/9Rg/dcxw8o5g9Ug/zOcvpOuQAOGoO6dsmpgUIP7EKakLjlrO+Tnxm7ZpUEzuuBZGWJwQ6dDfOq4Cue1Q89Fzm5SJSQG2K8qFBUdNRxM0fZnjgTmdt0ABwkidHEkTFXNxYRe/G44WKC6jzmI7JYrzJq/Fws2/w6wnBc1Ljy2w31eHKAtsyv6Q0kKWffllTEHPf7jxT/Cfz9gzx0c/UdYiaJ4fdj0jxLJQhEWW7fk2JpdirLplor8wSxT7rG87pHxCaDi6wFpX9Zq800HweEGR8n/O9CkudYHiSQ5hXs/4O28RnM+vlqaH8J0Lypphnn1J1y9aws+HhEOzO/ms795ugP23zyvCr/sEy3PsPjfnOz5Tp9miNiVEOY9v/uP1U5cjMrnyb7yoWqVijPr0Qv18zF3zpVxoFl63DVET4eusFjfe6BlA8g/7cUTOqm9UtH9uDQ6tyXQPTKjrjdGpwV8TPlBWCKNrqGCOFpbp3aS0uRM/NhqMKytwxNTtgn1XFG1wp+cLqLB+AeUR9F1h7krgd1v2aC3JoYi97T70e+k9bD9ZAs/bvzth7iUglHXMAnZr0LgHnje3H0fLeXM73FedA7/qbbha54WvddI3w/fglBl93HeMlGZyzwgUzjDeDhNVu6TbPIOsxvfEtMoyTOvXBU7tq/pgx9mlR7BmvP5bjhhjpDAx/x35K3E/gNM8UOSNsfiu3LT3PnzP2DnHdxHr/ilRRo3cgFO7i5DdLxkJxs6JvdQHWt2O47/qKZf50wo9Xt2JU5+WiP1PRpJ+AIIxYHMJPqk5i+1z5mD9yb0oGqeHQdy/3oLv6Ct68XSd6BB1gTR/+XJ8N/w0w9hWKT7WCssYJL/6CfYWjEAXuR8OZxeMKNiNY2ty8YQWBA4kmEbXjEl+FZ/sLcAIc3/0Iwqw+9ga5OpfsAxofWPsd+V6SbjN7kXYmPYYueEUdhdlo19ygvt8u9+A3H4c7iDtMQFbi8ahd5LctgyzcUW7sWeGS39LMpR1/BH78tyaY2JfzN9XDzEJvccVoPzIp5jhsv6C+/jEnttFY2/fdf5AHqP3+veg17iH3J9lvJxm7WbMIdKLtr4D33VHyHj0nFaJsmn9TOdP7Gt2KY6sGa/vlyPGPWaeJ747kOR1MvwdR3zPaagsy0ZvI7KL86L+/vGqBXj+Dn1FyxvOAdJU0nMfYLsatqbfSlbjzva38ZC24Ntq9VBS09o6HCmdZooX8lwW/BHvD1Uft1XFI23GdhSMMPIC9WdH4OH2+ryRV2T3Nn5DpNXe2Sg9XoUFz9+hH7N5AOcAYaQLLw8IGv9tHN5YKLvMdWL0k13FFkN09GP304fp47zij0l8t5/CkxV8rNebHd9xh88PLJmk+pE8SO+8UNTXj4lwGNc7yR32an6Q1HscCsqP4FOf9HYPxqw1nwtxPrv8FJ3lW2h+82mxnmcXYj3xOZy8Q3wrVkZMS1kShCe9mtNdAAHCUdcKrhmfesVrQdvnaSj59BjWPNfecs5buWZgj1c4xyUko9+0Euz7hTzAMMLijmfno1TkGcnuyKvH3Wmi/Fw3Um471HxU7SLz0yNev+fZv09FufCceZySoOkLaPtAqr7/cal4vqeRzlugnr8S53EWhvfuYM0HOujhdOrIDJiLC9/43YgTFYuhveSamIVN5+ST3g11OFWaJa+DTyOvUHazE27+HWY9KWhemvQcPthuU/af2oDXe+rHZi77Q0oLb1XhuBaG3nnIcMwqrcLv3fm8H+HGP8E+P/Pve7fJ8Y3MYRtAKHWe8OoZIZaF6liBVcdROmu47+8On4XSqt/DWLXHhK2ibtnbU/+U+XbR7j3ubYeUTwgx7Z/DmmO7rb/n3r9yHPnUmg6CxwuK1B1Dl8oywCs9aeftOM6ufw6e5BDm9Yy/8+ZeHmIZKRz1XLRgnP+LFnT7aYY7HpfqFy3h50PCPWPWoiy7t/sY1eucn3oqHvbHJUSr7uO/bqOz+9zTLXUAUc7j7fczmsfrpy5HZKa3vzQPz5MRNq2gXxQpaepnaWn6v2LytMbuVHKcstXQOVZZU3NJEZU1paok0/RklOk3Azwp47dl0vY7Dcq2qU65vkPpMrFUqZKv2jTUnVI+WT9bGZzk8j0Wiy+UojRjm+bfaFDqTm1XCjJ8n4BxdMlRKs5Yn1qx2q/kGU9fOnNE6NgxP23nVHK0lczHI8J3+m5F3cyl4yVKpqUV2fOEXMO2qYrTWJ4yXdmtf0EpybTutyeYg7TwNmxTphrn0tFFmVhaJd9eUsPjE2X97MFKksv43gllQWqSMnh2uSfMDsxxnyf96YRQ1vHDvC/qky+lNYqIWeorGkqp6ckk6xsc4bZgm9YP6cnDYE8qWuOTK/+AHn4iPRzI9zzN4chcpxjv8HxRlCbXh+JMX6gc0L5wSanZXiDib6r+VNDVOi9Gulcnh0vJP1An1lQP54CS736i0aFkrvO8keT/6QLpwhYlW3sqzKEkZW9xhwMRERG1FKHUZYnomndhnZIpn0pOzj8qF0oNZcpYrU4vJtcCcfVAREREdH27eg0vNq+EmV+DC6vbhqg1vAhhvM7qT8PRpUpGwN+A4kgarIwbbH4l32gssbEzx90Y4hD7qt0ft7E/z+haAorT+LEvlirptq8tJimpdl1RKF8oS9PtXtN1KEmpLnd3ZyE3vAiBuhLQJnfF+4SywGXzuTo5XEqB9g5pKOv4d27TRCXJLjzkZH1VVXW1G17U+BTmq84Bu2XwxN+rc16i/xqrJS07JioBmt6IiIjoajDVZe26TiWi64S5Xp4yXdmuPkCpulSjbM/zXN8H6h6aiIiI6HrRrF2NffeWBPmqnN0rr/FIyy7G4CRtDcH6GlxY3TYEeEXZ/lU64bu3IEFu2tJlRBivs/oT034QlonfKC8Q+9/B97X4gvIq1B4pxpziI/i8bBqeUsPA8QO0tnlFT3Xy0BY5KGUisp992P0KrLfOj/27uxuz2i2H9O6fEgZh+ec2XVEc+RTvv3CffnzmriiQgEHLP7e8thiXoL/a9+n7L+A+/QvuQdfVMPe8dee0vP5oaOV6C1XHSzFruO+rusNnlaLq90OhB+kdeHZ+qdZFjeftX72LgtIj6zBSe4c0lHX8U7s1+PSI1/fVMFFfVS35FMfWmF9VVYX76uD3cJsRp+O+444//gXvIiLsV53VbhnWeb2ebrw2+cf33fH36pyX6L/GCmd79NTCXKT/nz+FH+tLiYiIqIX47JP1erdCjq546Ql1CFwiui4luZDpkhX8ymz0SPiOPrj+dxLQY2I51B7IHF2moXAEOxUmIiKi698NauuLnCciIiIiIiIiiszlE9hc9BamzyzB9qM1uKgtVB9064V/Hz8FWRld0Trws3JERERE1wU2vBAREREREREREREREUVJs3Y1RkRERERERERERERE9E3ChhciIiIiIiIiIiIiIqIoYcMLERERERERERERERFRlLDhhYiIiIiIiIiIiIiIKErY8EJERERERERERERERBQlbHghIiIiIiIiIiIiIiKKEja8EBERERERERERERERRQkbXoiIiOjqOrkQj91wA2InVcgFust//Qpf/fWy/OvacHLhY7jhhlh4HQoRERERERERfYOw4YWIiCjKThb1QewNN+AG8xTbGu1Sx6Nw62k0yvXsNaJiUrz+nbavYVeglS+fwPrc/njwdrl+/O1IHV+MPWe9vnSyCH1iTftimYI3EjR7Y8LXX6NB/FPf+LX+t+rkQjzR6l/xr62exbIzclk0VEzSzs1jC0/KBdH19dfakcB8KERERERERET0zcKGFyIioij7+vLfUS/+7TRgMiZP1qexT7TG2bK5GPVvP0LGshp9RTuNW7FyzkV9/vQ7WL3HT8tL4y7k3ns30qauxJGbe2Gsuo1eN2P73KHo1jUPe+Vqmq8v4+/6Drn3xzP9Eo8m6Kv5c1Ua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g+Q7feAXFKmGbtHCE/mhV8xyVAbEYfP7cbCNmITDGRg1qwgNuA+RD5zBij9q67ed0uxjm2BnpvW1RVpsE5swTTahUO66C3WrZ2nhjCyhVmR9RyBTNLwMabbIWnB4hc7Fe1psE4zs0R8xc8TylvBA4hiU9QZiGNuEyN65L6FmShoSaM0tcVupwrpX01ATnoUVDqGZqD00YX/W8ygwxiPv1dHo6FTuRERERERERDcdfQCmg+xVks2hBGwnNWxDspJ70M2wIyrXoVO00BjmcCIy5Mdcsd7DSrZcj104jJbWbXXxsE14CZ9+SnRagXL2iv5ep/AaejgKSzgTLTTEtEz5Hn0Rl2EsdHo4B59k2y3yJPSJVeOFL5SNmdOUiN4uQqvkfeoyrI1HYUhc3hlyRTlbkKxEmENjmMNFiDp0L5yHLQ/WJTWWKmssIVlEu09cZbkq+9Lmqeo6BrgX20RtA/v1expORWpUKgptyy/7TqFS0eiq7zTTF6+cVQpsw6P4a2FGDmfLcCoth5RpE49ChFxU9mVGK6GB+r5hDoPSXMik1kKViMm+L+h17mI5bbINhSL2uy82KpnTXIRLchHuR3NROZw719o+lmOF4zGI6Ift0oEVyvz58cq4QLlvGZRJ61u9b/DWdGKxMsAwWFlsc5citaNLW5Q4g48St4X3uhAREREREdEPUyf5P9BNo2n/S/B/KA3BWaUo+v3NcxVu3Ue/xU/Gr0J49hnsmXkzX/9chbyx3RB7KB6FX76N0bzMlojIbRc+mIL7JqzGZYMRj/7pA2xMCePdCkREREREREREHuLAy01FHzSoy0Jp0e/tQzfdYMfevB+/SiyVKVrwN5M+8yakDVy9g/8o/BJvc9SFiIiIiIiIiIiIiK6z65PjhdxzcgPe2DkYi5c55Mu4YepQslLmwxiPmJdKAcMkPNxXf+mmVIcdy99Efdx7eJWDLkRERERERERERER0A/COF2rBBXww5T5MWH0ZBuOj+NMHG5ESxgENIiIiIiIiIiIiIqLmcOCFiIiIiIiIiIiIiIionTDUGBERERERERERERERUTvhwAsREREREREREREREVE74cALERERERERERERERFRO+HACxERERERERERERERUTvhwAsREREREREREREREVE74cALERERERERERERERFRO+HACxERERERERERERERUTvhwAsREREREREREREREVE74cALERERERERERERERFRO+HACxERERERERERERERUTvhwAsREREREREREREREVE74cALERERERERERERERFRO+HACxERERERERERERERUTvhwAsREREREREREREREVE74cALERERERERERERERFRO+HACxERERERERERERERUTvhwAsREREREREREREREVE74cALERERERERERERERFRO+HACxERERERERERERERUTvhwAsREREREREREREREVE74cALERERERERERERERFRO+HACxERERERERERERERUTvhwAsREREREREREREREVE74cALERERERERERERERFRO+HACxERERERERERERERUTvhwAsREREREREREREREVE74cALERERERERERERERFRO+HACxERERERERERERERUTvhwAsREREREREREREREVE74cALERERERERERERERFRO+HACxERERERERERERERUTvhwAsREREREREREREREVE74cALERERERERERERERFRO+HACxERERERERERERERUTvhwAsREREREREREREREVE74cALERERERERERERERFRO+HACxERERERERERERERUTvhwAsREREREREREREREVE74cALERERERERERERERFRO+HACxERERERERERERERUTvhwAsREREREREREREREVE74cALERERERERERERERFRO+HACxERERERERERERERUTvhwAsREREREREREREREVE74cALERERERERERERERFRO+HAy62mqRLbUyLRzbcTOnXqBN9uTyC3TH+NiIiIiIiIiIiIiIhuqA4deGkqSUeQtzZA0KmTL8LS96NWf81qH1LUQQRvjMk9p88j15pQkh6OiIWFqLqszblctQGfVWjPnTSV4JXu5vq3mbz9EBQ5HYu3lqNeX/SHqKkkA/3Vvucr+h5Hr4iIiIiIiIiIiIjo2nXowEvFsS0oa9D/wGV8mpqIjU7ntxtwRR1EaMB39VfVOZ67gA+m+GqDCr4p2KfPvf3sx/qFldrT3n/GwSsKGutq8eeHtVlOKo5hu764nYYalBW+h4SoXgh+vsjFYNgPQ8WxD3BC7XuXsW1FMTjsR0RERERERERERETX6jqFGuuNAQMM4nEvlnx8UpvVrr5DzT/Nt4BcgWWs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'>"
                        //BaseUrl = new Uri(@"C:\assets\images\").AbsoluteUri
                    };

                    var doc = renderer.RenderHtmlAsPdf(html);
                    var dataStream = doc.Stream;

                    return new FileResult(dataStream, Request, "Vibrant Challan " + vendorChallan.VendorChallanNo + ".pdf");
                }
                catch (Exception e)
                {
                    throw new Exception();
                }
            }
        }

        private VendorChallanModel GetVendorChallanByVendorChallanNoPrivate(int vendorChallanNo)
        {
            using (var context = new erpdbEntities())
            {
                try
                {
                    var vendorChallan = context.VendorChallans.Where(x => x.VendorChallanNo == vendorChallanNo).FirstOrDefault();

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
                            challanProductModel.ProductDetail = new ProductDetailWithProductType();
                            challanProductModel.ProductDetail.CreateDate = challanDeduction.ChallanProduct.ProductDetail.CreateDate;
                            challanProductModel.ProductDetail.EditDate = challanDeduction.ChallanProduct.ProductDetail.EditDate;
                            challanProductModel.ProductDetail.InputCode = challanDeduction.ChallanProduct.ProductDetail.InputCode;
                            challanProductModel.ProductDetail.InputMaterialDesc = challanDeduction.ChallanProduct.ProductDetail.InputMaterialDesc;
                            challanProductModel.ProductDetail.OutputCode = challanDeduction.ChallanProduct.ProductDetail.OutputCode;
                            challanProductModel.ProductDetail.OutputMaterialDesc = challanDeduction.ChallanProduct.ProductDetail.OutputMaterialDesc;
                            challanProductModel.ProductDetail.ProductId = challanDeduction.ChallanProduct.ProductDetail.ProductId;
                            challanProductModel.ProductDetail.ProductTypeId = challanDeduction.ChallanProduct.ProductDetail.ProductTypeId;
                            challanProductModel.ProductDetail.ProductTypeName = challanDeduction.ChallanProduct.ProductDetail.ProductType.ProductTypeName;
                            challanProductModel.ProductDetail.ProjectName = challanDeduction.ChallanProduct.ProductDetail.ProjectName;
                            challanProductModel.ProductDetail.SplitRatio = challanDeduction.ChallanProduct.ProductDetail.SplitRatio;

                            challanProductModel.ChallanDetail = challanDeduction.ChallanProduct.ChallanDetail;

                            var inputQuantity = challanProductModel.ChallanProduct.InputQuantity ?? 0;
                            challanProductModel.RemainingQuantity = (inputQuantity - challanDeduction.ChallanProduct.ChallanDeductions.Where(x => x.CreateDate <= challanDeduction.CreateDate).Sum(x => x.OutQuantity)) ?? inputQuantity;

                            challanDeductionModel.ChallanProduct = challanProductModel;
                            challanDeductionModel.ChallanProductId = challanDeduction.ChallanProductId ?? 0;
                            challanDeductionModel.CreateDate = challanDeduction.CreateDate ?? new DateTime();
                            challanDeductionModel.EditDate = challanDeduction.EditDate ?? new DateTime();
                            challanDeductionModel.OutStockId = challanDeduction.OutStockId ?? 0;
                            challanDeductionModel.OutQuantity = challanDeduction.OutQuantity ?? 0;

                            challanDeductionModelList.Add(challanDeductionModel);
                        }

                        outStockModel.ChallanDeductions = challanDeductionModelList.ToArray();


                        List<PODeductionModel> poDeductionModelList = new List<PODeductionModel>();
                        foreach (var poDeduction in outStock.PODeductions)
                        {
                            PODeductionModel poDeductionModel = new PODeductionModel();
                            poDeductionModel.PODeductionId = poDeduction.PODeductionId;

                            POProductModel poProductModel = new POProductModel();
                            poProductModel.PODeductions = null;
                            poProductModel.POProduct = poDeduction.POProduct;
                            poProductModel.ProductDetail = new ProductDetailWithProductType();
                            poProductModel.ProductDetail.CreateDate = poDeduction.POProduct.ProductDetail.CreateDate;
                            poProductModel.ProductDetail.EditDate = poDeduction.POProduct.ProductDetail.EditDate;
                            poProductModel.ProductDetail.InputCode = poDeduction.POProduct.ProductDetail.InputCode;
                            poProductModel.ProductDetail.InputMaterialDesc = poDeduction.POProduct.ProductDetail.InputMaterialDesc;
                            poProductModel.ProductDetail.OutputCode = poDeduction.POProduct.ProductDetail.OutputCode;
                            poProductModel.ProductDetail.OutputMaterialDesc = poDeduction.POProduct.ProductDetail.OutputMaterialDesc;
                            poProductModel.ProductDetail.ProductId = poDeduction.POProduct.ProductDetail.ProductId;
                            poProductModel.ProductDetail.ProductTypeId = poDeduction.POProduct.ProductDetail.ProductTypeId;
                            poProductModel.ProductDetail.ProductTypeName = poDeduction.POProduct.ProductDetail.ProductType.ProductTypeName;
                            poProductModel.ProductDetail.ProjectName = poDeduction.POProduct.ProductDetail.ProjectName;
                            poProductModel.ProductDetail.SplitRatio = poDeduction.POProduct.ProductDetail.SplitRatio;

                            poProductModel.PODetail = poDeduction.POProduct.PODetail;

                            var inputQuantity = poProductModel.POProduct.InputQuantity ?? 0;
                            poProductModel.RemainingQuantity = (inputQuantity - poDeduction.POProduct.PODeductions.Where(x => x.CreateDate <= poDeduction.CreateDate).Sum(x => x.OutQuantity)) ?? inputQuantity;

                            poDeductionModel.POProduct = poProductModel;
                            poDeductionModel.POProductId = poDeduction.POProductId ?? 0;
                            poDeductionModel.CreateDate = poDeduction.CreateDate ?? new DateTime();
                            poDeductionModel.EditDate = poDeduction.EditDate ?? new DateTime();
                            poDeductionModel.OutStockId = poDeduction.OutStockId ?? 0;
                            poDeductionModel.OutQuantity = poDeduction.OutQuantity ?? 0;

                            poDeductionModelList.Add(poDeductionModel);
                        }

                        outStockModel.PODeductions = poDeductionModelList.ToArray();


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
                                challanProductModel.ProductDetail = new ProductDetailWithProductType();
                                challanProductModel.ProductDetail.CreateDate = accChallanDeduction.ChallanProduct.ProductDetail.CreateDate;
                                challanProductModel.ProductDetail.EditDate = accChallanDeduction.ChallanProduct.ProductDetail.EditDate;
                                challanProductModel.ProductDetail.InputCode = accChallanDeduction.ChallanProduct.ProductDetail.InputCode;
                                challanProductModel.ProductDetail.InputMaterialDesc = accChallanDeduction.ChallanProduct.ProductDetail.InputMaterialDesc;
                                challanProductModel.ProductDetail.OutputCode = accChallanDeduction.ChallanProduct.ProductDetail.OutputCode;
                                challanProductModel.ProductDetail.OutputMaterialDesc = accChallanDeduction.ChallanProduct.ProductDetail.OutputMaterialDesc;
                                challanProductModel.ProductDetail.ProductId = accChallanDeduction.ChallanProduct.ProductDetail.ProductId;
                                challanProductModel.ProductDetail.ProductTypeId = accChallanDeduction.ChallanProduct.ProductDetail.ProductTypeId;
                                challanProductModel.ProductDetail.ProductTypeName = accChallanDeduction.ChallanProduct.ProductDetail.ProductType.ProductTypeName;
                                challanProductModel.ProductDetail.ProjectName = accChallanDeduction.ChallanProduct.ProductDetail.ProjectName;
                                challanProductModel.ProductDetail.SplitRatio = accChallanDeduction.ChallanProduct.ProductDetail.SplitRatio;

                                challanProductModel.ChallanDetail = accChallanDeduction.ChallanProduct.ChallanDetail;

                                var inputQuantity = challanProductModel.ChallanProduct.InputQuantity ?? 0;
                                challanProductModel.RemainingQuantity = (inputQuantity - accChallanDeduction.ChallanProduct.AccChallanDeductions.Where(x => x.CreateDate <= accChallanDeduction.CreateDate).Sum(x => x.OutQuantity)) ?? inputQuantity;

                                accChallanDeductionModel.ChallanProduct = challanProductModel;
                                accChallanDeductionModel.ChallanProductId = accChallanDeduction.ChallanProductId ?? 0;
                                accChallanDeductionModel.CreateDate = accChallanDeduction.CreateDate ?? new DateTime();
                                accChallanDeductionModel.EditDate = accChallanDeduction.EditDate ?? new DateTime();
                                accChallanDeductionModel.OutAccId = accChallanDeduction.OutAccId ?? 0;
                                accChallanDeductionModel.OutQuantity = accChallanDeduction.OutQuantity ?? 0;

                                accChallanDeductionModelList.Add(accChallanDeductionModel);
                            }

                            outAccModel.AccChallanDeductions = accChallanDeductionModelList.ToArray();


                            List<AccPODeductionModel> accPODeductionModelList = new List<AccPODeductionModel>();
                            foreach (var accPODeduction in outAcc.AccPODeductions)
                            {
                                AccPODeductionModel accPODeductionModel = new AccPODeductionModel();
                                accPODeductionModel.AccPODeductionId = accPODeduction.AccPODeductionId;

                                POProductModel poProductModel = new POProductModel();
                                poProductModel.AccPODeductions = null;
                                poProductModel.POProduct = accPODeduction.POProduct;
                                poProductModel.ProductDetail = new ProductDetailWithProductType();
                                poProductModel.ProductDetail.CreateDate = accPODeduction.POProduct.ProductDetail.CreateDate;
                                poProductModel.ProductDetail.EditDate = accPODeduction.POProduct.ProductDetail.EditDate;
                                poProductModel.ProductDetail.InputCode = accPODeduction.POProduct.ProductDetail.InputCode;
                                poProductModel.ProductDetail.InputMaterialDesc = accPODeduction.POProduct.ProductDetail.InputMaterialDesc;
                                poProductModel.ProductDetail.OutputCode = accPODeduction.POProduct.ProductDetail.OutputCode;
                                poProductModel.ProductDetail.OutputMaterialDesc = accPODeduction.POProduct.ProductDetail.OutputMaterialDesc;
                                poProductModel.ProductDetail.ProductId = accPODeduction.POProduct.ProductDetail.ProductId;
                                poProductModel.ProductDetail.ProductTypeId = accPODeduction.POProduct.ProductDetail.ProductTypeId;
                                poProductModel.ProductDetail.ProductTypeName = accPODeduction.POProduct.ProductDetail.ProductType.ProductTypeName;
                                poProductModel.ProductDetail.ProjectName = accPODeduction.POProduct.ProductDetail.ProjectName;
                                poProductModel.ProductDetail.SplitRatio = accPODeduction.POProduct.ProductDetail.SplitRatio;

                                poProductModel.PODetail = accPODeduction.POProduct.PODetail;

                                var inputQuantity = poProductModel.POProduct.InputQuantity ?? 0;
                                poProductModel.RemainingQuantity = (inputQuantity - accPODeduction.POProduct.AccPODeductions.Where(x => x.CreateDate <= accPODeduction.CreateDate).Sum(x => x.OutQuantity)) ?? inputQuantity;

                                accPODeductionModel.POProduct = poProductModel;
                                accPODeductionModel.POProductId = accPODeduction.POProductId ?? 0;
                                accPODeductionModel.CreateDate = accPODeduction.CreateDate ?? new DateTime();
                                accPODeductionModel.EditDate = accPODeduction.EditDate ?? new DateTime();
                                accPODeductionModel.OutAccId = accPODeduction.OutAccId ?? 0;
                                accPODeductionModel.OutQuantity = accPODeduction.OutQuantity ?? 0;

                                accPODeductionModelList.Add(accPODeductionModel);
                            }

                            outAccModel.AccPODeductions = accPODeductionModelList.ToArray();


                            outAccModelList.Add(outAccModel);
                        }

                        outStockModel.OutAccs = outAccModelList.ToArray();


                        List<OutAssemblyModel> outAssemblyModelList = new List<OutAssemblyModel>();
                        foreach (var outAssembly in outStock.OutAssemblys)
                        {
                            OutAssemblyModel outAssemblyModel = new OutAssemblyModel();
                            outAssemblyModel.OutAssemblyId = outAssembly.OutAssemblyId;
                            outAssemblyModel.OutStockId = outAssembly.OutStockId ?? 0;
                            outAssemblyModel.OutputQuantity = outAssembly.OutputQuantity ?? 0;
                            outAssemblyModel.CreateDate = outAssembly.CreateDate ?? new DateTime();
                            outAssemblyModel.EditDate = outAssembly.EditDate ?? new DateTime();


                            List<AssemblyChallanDeductionModel> assemblyChallanDeductionModelList = new List<AssemblyChallanDeductionModel>();
                            foreach (var assemblyChallanDeduction in outAssembly.AssemblyChallanDeductions)
                            {
                                AssemblyChallanDeductionModel assemblyChallanDeductionModel = new AssemblyChallanDeductionModel();
                                assemblyChallanDeductionModel.AssemblyChallanDeductionId = assemblyChallanDeduction.AssemblyChallanDeductionId;

                                ChallanProductModel challanProductModel = new ChallanProductModel();
                                challanProductModel.AssemblyChallanDeductions = null;
                                challanProductModel.ChallanProduct = assemblyChallanDeduction.ChallanProduct;
                                challanProductModel.ProductDetail = new ProductDetailWithProductType();
                                challanProductModel.ProductDetail.CreateDate = assemblyChallanDeduction.ChallanProduct.ProductDetail.CreateDate;
                                challanProductModel.ProductDetail.EditDate = assemblyChallanDeduction.ChallanProduct.ProductDetail.EditDate;
                                challanProductModel.ProductDetail.InputCode = assemblyChallanDeduction.ChallanProduct.ProductDetail.InputCode;
                                challanProductModel.ProductDetail.InputMaterialDesc = assemblyChallanDeduction.ChallanProduct.ProductDetail.InputMaterialDesc;
                                challanProductModel.ProductDetail.OutputCode = assemblyChallanDeduction.ChallanProduct.ProductDetail.OutputCode;
                                challanProductModel.ProductDetail.OutputMaterialDesc = assemblyChallanDeduction.ChallanProduct.ProductDetail.OutputMaterialDesc;
                                challanProductModel.ProductDetail.ProductId = assemblyChallanDeduction.ChallanProduct.ProductDetail.ProductId;
                                challanProductModel.ProductDetail.ProductTypeId = assemblyChallanDeduction.ChallanProduct.ProductDetail.ProductTypeId;
                                challanProductModel.ProductDetail.ProductTypeName = assemblyChallanDeduction.ChallanProduct.ProductDetail.ProductType.ProductTypeName;
                                challanProductModel.ProductDetail.ProjectName = assemblyChallanDeduction.ChallanProduct.ProductDetail.ProjectName;
                                challanProductModel.ProductDetail.SplitRatio = assemblyChallanDeduction.ChallanProduct.ProductDetail.SplitRatio;

                                challanProductModel.ChallanDetail = assemblyChallanDeduction.ChallanProduct.ChallanDetail;

                                var inputQuantity = challanProductModel.ChallanProduct.InputQuantity ?? 0;
                                challanProductModel.RemainingQuantity = (inputQuantity - assemblyChallanDeduction.ChallanProduct.AssemblyChallanDeductions.Where(x => x.CreateDate <= assemblyChallanDeduction.CreateDate).Sum(x => x.OutQuantity)) ?? inputQuantity;

                                assemblyChallanDeductionModel.ChallanProduct = challanProductModel;
                                assemblyChallanDeductionModel.ChallanProductId = assemblyChallanDeduction.ChallanProductId ?? 0;
                                assemblyChallanDeductionModel.CreateDate = assemblyChallanDeduction.CreateDate ?? new DateTime();
                                assemblyChallanDeductionModel.EditDate = assemblyChallanDeduction.EditDate ?? new DateTime();
                                assemblyChallanDeductionModel.OutAssemblyId = assemblyChallanDeduction.OutAssemblyId ?? 0;
                                assemblyChallanDeductionModel.OutQuantity = assemblyChallanDeduction.OutQuantity ?? 0;

                                assemblyChallanDeductionModelList.Add(assemblyChallanDeductionModel);
                            }

                            outAssemblyModel.AssemblyChallanDeductions = assemblyChallanDeductionModelList.ToArray();


                            List<AssemblyPODeductionModel> assemblyPODeductionModelList = new List<AssemblyPODeductionModel>();
                            foreach (var assemblyPODeduction in outAssembly.AssemblyPODeductions)
                            {
                                AssemblyPODeductionModel assemblyPODeductionModel = new AssemblyPODeductionModel();
                                assemblyPODeductionModel.AssemblyPODeductionId = assemblyPODeduction.AssemblyPODeductionId;

                                POProductModel poProductModel = new POProductModel();
                                poProductModel.AssemblyPODeductions = null;
                                poProductModel.POProduct = assemblyPODeduction.POProduct;
                                poProductModel.ProductDetail = new ProductDetailWithProductType();
                                poProductModel.ProductDetail.CreateDate = assemblyPODeduction.POProduct.ProductDetail.CreateDate;
                                poProductModel.ProductDetail.EditDate = assemblyPODeduction.POProduct.ProductDetail.EditDate;
                                poProductModel.ProductDetail.InputCode = assemblyPODeduction.POProduct.ProductDetail.InputCode;
                                poProductModel.ProductDetail.InputMaterialDesc = assemblyPODeduction.POProduct.ProductDetail.InputMaterialDesc;
                                poProductModel.ProductDetail.OutputCode = assemblyPODeduction.POProduct.ProductDetail.OutputCode;
                                poProductModel.ProductDetail.OutputMaterialDesc = assemblyPODeduction.POProduct.ProductDetail.OutputMaterialDesc;
                                poProductModel.ProductDetail.ProductId = assemblyPODeduction.POProduct.ProductDetail.ProductId;
                                poProductModel.ProductDetail.ProductTypeId = assemblyPODeduction.POProduct.ProductDetail.ProductTypeId;
                                poProductModel.ProductDetail.ProductTypeName = assemblyPODeduction.POProduct.ProductDetail.ProductType.ProductTypeName;
                                poProductModel.ProductDetail.ProjectName = assemblyPODeduction.POProduct.ProductDetail.ProjectName;
                                poProductModel.ProductDetail.SplitRatio = assemblyPODeduction.POProduct.ProductDetail.SplitRatio;

                                poProductModel.PODetail = assemblyPODeduction.POProduct.PODetail;

                                var inputQuantity = poProductModel.POProduct.InputQuantity ?? 0;
                                poProductModel.RemainingQuantity = (inputQuantity - assemblyPODeduction.POProduct.AssemblyPODeductions.Where(x => x.CreateDate <= assemblyPODeduction.CreateDate).Sum(x => x.OutQuantity)) ?? inputQuantity;

                                assemblyPODeductionModel.POProduct = poProductModel;
                                assemblyPODeductionModel.POProductId = assemblyPODeduction.POProductId ?? 0;
                                assemblyPODeductionModel.CreateDate = assemblyPODeduction.CreateDate ?? new DateTime();
                                assemblyPODeductionModel.EditDate = assemblyPODeduction.EditDate ?? new DateTime();
                                assemblyPODeductionModel.OutAssemblyId = assemblyPODeduction.OutAssemblyId ?? 0;
                                assemblyPODeductionModel.OutQuantity = assemblyPODeduction.OutQuantity ?? 0;

                                assemblyPODeductionModelList.Add(assemblyPODeductionModel);
                            }

                            outAssemblyModel.AssemblyPODeductions = assemblyPODeductionModelList.ToArray();


                            outAssemblyModelList.Add(outAssemblyModel);
                        }

                        outStockModel.OutAssemblys = outAssemblyModelList.ToArray();


                        outStockModelList.Add(outStockModel);
                    }

                    model.OutStocks = outStockModelList.ToArray();

                    return model;
                }
                catch (Exception e)
                {
                    throw new Exception();
                }
            }
        }

        private BASFInvoiceModel GetBASFInvoiceByBASFInvoiceIdPrivate(int basfInvoiceId)
        {
            using (var context = new erpdbEntities())
            {
                try
                {
                    var basfInvoice = context.BASFInvoices.Where(x => x.BASFInvoiceId == basfInvoiceId).FirstOrDefault();

                    BASFInvoiceModel model = new BASFInvoiceModel();
                    model.BASFInvoiceId = basfInvoice.BASFInvoiceId;
                    model.BASFInvoiceNo = basfInvoice.BASFInvoiceNo;
                    model.BASFInvoiceDate = basfInvoice.BASFInvoiceDate ?? new DateTime();
                    model.CreateDate = basfInvoice.CreateDate ?? new DateTime();
                    model.EditDate = basfInvoice.EditDate ?? new DateTime();

                    List<InvoiceOutStockModel> outStockModelList = new List<InvoiceOutStockModel>();

                    foreach (var outStock in basfInvoice.InvoiceOutStocks)
                    {
                        InvoiceOutStockModel outStockModel = new InvoiceOutStockModel();
                        outStockModel.InvoiceOutStockId = outStock.InvoiceOutStockId;
                        outStockModel.BASFInvoiceId = outStock.BASFInvoiceId ?? 0;
                        outStockModel.OutputQuantity = outStock.OutputQuantity ?? 0;
                        outStockModel.CreateDate = outStock.CreateDate ?? new DateTime();
                        outStockModel.EditDate = outStock.EditDate ?? new DateTime();


                        List<InvoiceChallanDeductionModel> challanDeductionModelList = new List<InvoiceChallanDeductionModel>();
                        foreach (var challanDeduction in outStock.InvoiceChallanDeductions)
                        {
                            InvoiceChallanDeductionModel challanDeductionModel = new InvoiceChallanDeductionModel();
                            challanDeductionModel.InvoiceChallanDeductionId = challanDeduction.InvoiceChallanDeductionId;

                            ChallanProductModel challanProductModel = new ChallanProductModel();
                            challanProductModel.ChallanDeductions = null;
                            challanProductModel.ChallanProduct = challanDeduction.ChallanProduct;
                            challanProductModel.ProductDetail = new ProductDetailWithProductType();
                            challanProductModel.ProductDetail.CreateDate = challanDeduction.ChallanProduct.ProductDetail.CreateDate;
                            challanProductModel.ProductDetail.EditDate = challanDeduction.ChallanProduct.ProductDetail.EditDate;
                            challanProductModel.ProductDetail.InputCode = challanDeduction.ChallanProduct.ProductDetail.InputCode;
                            challanProductModel.ProductDetail.InputMaterialDesc = challanDeduction.ChallanProduct.ProductDetail.InputMaterialDesc;
                            challanProductModel.ProductDetail.OutputCode = challanDeduction.ChallanProduct.ProductDetail.OutputCode;
                            challanProductModel.ProductDetail.OutputMaterialDesc = challanDeduction.ChallanProduct.ProductDetail.OutputMaterialDesc;
                            challanProductModel.ProductDetail.ProductId = challanDeduction.ChallanProduct.ProductDetail.ProductId;
                            challanProductModel.ProductDetail.ProductTypeId = challanDeduction.ChallanProduct.ProductDetail.ProductTypeId;
                            challanProductModel.ProductDetail.ProductTypeName = challanDeduction.ChallanProduct.ProductDetail.ProductType.ProductTypeName;
                            challanProductModel.ProductDetail.ProjectName = challanDeduction.ChallanProduct.ProductDetail.ProjectName;
                            challanProductModel.ProductDetail.SplitRatio = challanDeduction.ChallanProduct.ProductDetail.SplitRatio;

                            challanProductModel.ChallanDetail = challanDeduction.ChallanProduct.ChallanDetail;

                            var inputQuantity = challanProductModel.ChallanProduct.InputQuantity ?? 0;
                            var outQnt = challanDeduction.ChallanProduct.InvoiceChallanDeductions.Where(x => x.CreateDate <= challanDeduction.CreateDate).Sum(x => x.OutQuantity) ?? 0;
                            var ngOutQnt = challanDeduction.ChallanProduct.ChallanDeductions.Where(x => x.CreateDate <= challanDeduction.CreateDate && x.OutStock.VendorChallan.IsNg == 1).Sum(x => x.OutQuantity) ?? 0;
                            challanProductModel.RemainingQuantity = inputQuantity - outQnt - ngOutQnt;

                            challanDeductionModel.ChallanProduct = challanProductModel;
                            challanDeductionModel.ChallanProductId = challanDeduction.ChallanProductId ?? 0;
                            challanDeductionModel.CreateDate = challanDeduction.CreateDate ?? new DateTime();
                            challanDeductionModel.EditDate = challanDeduction.EditDate ?? new DateTime();
                            challanDeductionModel.InvoiceOutStockId = challanDeduction.InvoiceOutStockId ?? 0;
                            challanDeductionModel.OutQuantity = challanDeduction.OutQuantity ?? 0;

                            challanDeductionModelList.Add(challanDeductionModel);
                        }

                        outStockModel.InvoiceChallanDeductions = challanDeductionModelList.ToArray();

                        outStockModelList.Add(outStockModel);
                    }

                    model.InvoiceOutStocks = outStockModelList.ToArray();

                    return model;
                }
                catch (Exception e)
                {
                    throw new Exception();
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
                        challanProductModel.ProductDetail = new ProductDetailWithProductType();
                        challanProductModel.ProductDetail.CreateDate = challanProduct.ProductDetail.CreateDate;
                        challanProductModel.ProductDetail.EditDate = challanProduct.ProductDetail.EditDate;
                        challanProductModel.ProductDetail.InputCode = challanProduct.ProductDetail.InputCode;
                        challanProductModel.ProductDetail.InputMaterialDesc = challanProduct.ProductDetail.InputMaterialDesc;
                        challanProductModel.ProductDetail.OutputCode = challanProduct.ProductDetail.OutputCode;
                        challanProductModel.ProductDetail.OutputMaterialDesc = challanProduct.ProductDetail.OutputMaterialDesc;
                        challanProductModel.ProductDetail.ProductId = challanProduct.ProductDetail.ProductId;
                        challanProductModel.ProductDetail.ProductTypeId = challanProduct.ProductDetail.ProductTypeId;
                        challanProductModel.ProductDetail.ProductTypeName = challanProduct.ProductDetail.ProductType.ProductTypeName;
                        challanProductModel.ProductDetail.ProjectName = challanProduct.ProductDetail.ProjectName;
                        challanProductModel.ProductDetail.SplitRatio = challanProduct.ProductDetail.SplitRatio;

                        challanProductModel.ChallanDetail = challanProduct.ChallanDetail;
                        challanProductModel.ChallanDeductions = challanProduct.ChallanDeductions;
                        challanProductModel.AccChallanDeductions = challanProduct.AccChallanDeductions;
                        challanProductModel.AssemblyChallanDeductions = challanProduct.AssemblyChallanDeductions;

                        var inputQuantity = challanProductModel.ChallanProduct.InputQuantity ?? 0;
                        challanProductModel.RemainingQuantity = inputQuantity;

                        if (challanProductModel.ChallanDeductions != null && challanProductModel.ChallanDeductions.Count > 0)
                            challanProductModel.RemainingQuantity = (inputQuantity - challanProductModel.ChallanDeductions.Sum(x => x.OutQuantity)) ?? inputQuantity;
                        else if (challanProductModel.AccChallanDeductions != null && challanProductModel.AccChallanDeductions.Count > 0)
                            challanProductModel.RemainingQuantity = (inputQuantity - challanProductModel.AccChallanDeductions.Sum(x => x.OutQuantity)) ?? inputQuantity;
                        else if (challanProductModel.AssemblyChallanDeductions != null && challanProductModel.AssemblyChallanDeductions.Count > 0)
                            challanProductModel.RemainingQuantity = (inputQuantity - challanProductModel.AssemblyChallanDeductions.Sum(x => x.OutQuantity)) ?? inputQuantity;

                        challanProductModel.CanDelete = challanProductModel.RemainingQuantity == challanProductModel.ChallanProduct.InputQuantity;

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

        [HttpPost, Route("GetBASFPOByBASFPOId")]
        public IHttpActionResult GetBASFPOByBASFPOId(VendorChallanNoModel vendorChallanNoModel)
        {
            using (var context = new erpdbEntities())
            {
                try
                {
                    var poDetail = context.PODetails.Where(x => x.POId == vendorChallanNoModel.VendorChallanNo).FirstOrDefault();

                    ViewPODetailModel model = new ViewPODetailModel();

                    List<POProductModel> poProducts = new List<POProductModel>();
                    foreach (var poProduct in poDetail.POProducts)
                    {
                        POProductModel poProductModel = new POProductModel();
                        poProductModel.POProduct = poProduct;
                        poProductModel.ProductDetail = new ProductDetailWithProductType();
                        poProductModel.ProductDetail.CreateDate = poProduct.ProductDetail.CreateDate;
                        poProductModel.ProductDetail.EditDate = poProduct.ProductDetail.EditDate;
                        poProductModel.ProductDetail.InputCode = poProduct.ProductDetail.InputCode;
                        poProductModel.ProductDetail.InputMaterialDesc = poProduct.ProductDetail.InputMaterialDesc;
                        poProductModel.ProductDetail.OutputCode = poProduct.ProductDetail.OutputCode;
                        poProductModel.ProductDetail.OutputMaterialDesc = poProduct.ProductDetail.OutputMaterialDesc;
                        poProductModel.ProductDetail.ProductId = poProduct.ProductDetail.ProductId;
                        poProductModel.ProductDetail.ProductTypeId = poProduct.ProductDetail.ProductTypeId;
                        poProductModel.ProductDetail.ProductTypeName = poProduct.ProductDetail.ProductType.ProductTypeName;
                        poProductModel.ProductDetail.ProjectName = poProduct.ProductDetail.ProjectName;
                        poProductModel.ProductDetail.SplitRatio = poProduct.ProductDetail.SplitRatio;

                        poProductModel.PODetail = poProduct.PODetail;
                        poProductModel.PODeductions = poProduct.PODeductions;
                        poProductModel.AccPODeductions = poProduct.AccPODeductions;
                        poProductModel.AssemblyPODeductions = poProduct.AssemblyPODeductions;

                        var inputQuantity = poProductModel.POProduct.InputQuantity ?? 0;
                        poProductModel.RemainingQuantity = inputQuantity;

                        if (poProductModel.PODeductions != null && poProductModel.PODeductions.Count > 0)
                            poProductModel.RemainingQuantity = (inputQuantity - poProductModel.PODeductions.Sum(x => x.OutQuantity)) ?? inputQuantity;
                        else if (poProductModel.AccPODeductions != null && poProductModel.AccPODeductions.Count > 0)
                            poProductModel.RemainingQuantity = (inputQuantity - poProductModel.AccPODeductions.Sum(x => x.OutQuantity)) ?? inputQuantity;
                        else if (poProductModel.AssemblyPODeductions != null && poProductModel.AssemblyPODeductions.Count > 0)
                            poProductModel.RemainingQuantity = (inputQuantity - poProductModel.AssemblyPODeductions.Sum(x => x.OutQuantity)) ?? inputQuantity;

                        poProductModel.CanDelete = poProductModel.RemainingQuantity == poProductModel.POProduct.InputQuantity;

                        poProducts.Add(poProductModel);
                    }

                    model.PODetail = poDetail;
                    model.POProducts = poProducts.ToArray();

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

        [HttpPost, Route("DeleteProductByProductId")]
        public IHttpActionResult DeleteProductByProductId(VendorChallanNoModel vendorChallanNoModel)
        {
            var productId = vendorChallanNoModel.VendorChallanNo;

            SuccessResponse response = new SuccessResponse();

            try
            {
                using (var context = new erpdbEntities())
                {
                    var product = context.ProductDetails.Where(x => x.ProductId == productId).FirstOrDefault();

                    if (product != null)
                    {
                        var productMappings = context.ProductMappings.Where(x => x.ProductId == productId).ToArray();
                        if (productMappings != null)
                            context.ProductMappings.RemoveRange(productMappings);

                        context.ProductDetails.Remove(product);
                    }

                    context.SaveChanges();

                    response.StatusCode = HttpStatusCode.OK;
                    response.Message = "Product deleted successfully.";

                    return Ok(response);
                }
            }
            catch (Exception e)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = "Cannot delete the product as it has been used in some Challan(s)! Do you still want to delete the product and all its references?";

                return InternalServerError(new Exception("Cannot delete the product as it has been used in some Challan(s)! Do you still want to delete the product and all its references?"));
            }
        }

        [HttpPost, Route("DeleteBASFChallanByChallanId")]
        public IHttpActionResult DeleteBASFChallanByChallanId(VendorChallanNoModel vendorChallanNoModel)
        {
            var challanId = vendorChallanNoModel.VendorChallanNo;

            SuccessResponse response = new SuccessResponse();

            try
            {
                using (var context = new erpdbEntities())
                {
                    var challan = context.ChallanDetails.Where(x => x.ChallanId == challanId).FirstOrDefault();

                    if (challan != null)
                    {
                        var challanProducts = context.ChallanProducts.Where(x => x.ChallanId == challanId).ToArray();
                        if (challanProducts != null)
                            context.ChallanProducts.RemoveRange(challanProducts);

                        context.ChallanDetails.Remove(challan);
                    }

                    context.SaveChanges();

                    response.StatusCode = HttpStatusCode.OK;
                    response.Message = "BASF Challan deleted successfully.";

                    return Ok(response);
                }
            }
            catch (Exception e)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = "Cannot delete the challan as it has been used in some Vibrant Challan(s)! Do you still want to delete the challan and all its references?";

                return InternalServerError(new Exception("Cannot delete the challan as it has been used in some Vibrant Challan(s)! Do you still want to delete the challan and all its references?"));
            }
        }

        [HttpPost, Route("DeleteBASFPOByPOId")]
        public IHttpActionResult DeleteBASFPOByPOId(VendorChallanNoModel vendorChallanNoModel)
        {
            var poId = vendorChallanNoModel.VendorChallanNo;

            SuccessResponse response = new SuccessResponse();

            try
            {
                using (var context = new erpdbEntities())
                {
                    var po = context.PODetails.Where(x => x.POId == poId).FirstOrDefault();

                    if (po != null)
                    {
                        var poProducts = context.POProducts.Where(x => x.POId == poId).ToArray();
                        if (poProducts != null)
                            context.POProducts.RemoveRange(poProducts);

                        context.PODetails.Remove(po);
                    }

                    context.SaveChanges();

                    response.StatusCode = HttpStatusCode.OK;
                    response.Message = "BASF PO deleted successfully.";

                    return Ok(response);
                }
            }
            catch (Exception e)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = "Cannot delete the PO as it has been used in some Vibrant Challan(s)! Do you still want to delete the PO and all its references?";

                return InternalServerError(new Exception("Cannot delete the PO as it has been used in some Vibrant Challan(s)! Do you still want to delete the PO and all its references?"));
            }
        }

        [HttpPost, Route("DeleteVendorChallanByVendorChallanNo")]
        public IHttpActionResult DeleteVendorChallanByVendorChallanNo(VendorChallanNoModel vendorChallanNoModel)
        {
            var vendorChallanNo = vendorChallanNoModel.VendorChallanNo;

            SuccessResponse response = new SuccessResponse();

            try
            {
                using (var context = new erpdbEntities())
                {
                    var vendorChallan = context.VendorChallans.Where(x => x.VendorChallanNo == vendorChallanNo).FirstOrDefault();

                    if (vendorChallan != null)
                    {
                        var challanDeductions = context.ChallanDeductions.Where(x => x.OutStock.VendorChallanNo == vendorChallanNo).ToArray();
                        if (challanDeductions != null)
                            context.ChallanDeductions.RemoveRange(challanDeductions);

                        var assemblyChallanDeductions = context.AssemblyChallanDeductions.Where(x => x.OutAssembly.OutStock.VendorChallanNo == vendorChallanNo).ToArray();
                        if (assemblyChallanDeductions != null)
                            context.AssemblyChallanDeductions.RemoveRange(assemblyChallanDeductions);

                        var accChallanDeductions = context.AccChallanDeductions.Where(x => x.OutAcc.OutStock.VendorChallanNo == vendorChallanNo).ToArray();
                        if (accChallanDeductions != null)
                            context.AccChallanDeductions.RemoveRange(accChallanDeductions);



                        var poDeductions = context.PODeductions.Where(x => x.OutStock.VendorChallanNo == vendorChallanNo).ToArray();
                        if (poDeductions != null)
                            context.PODeductions.RemoveRange(poDeductions);

                        var assemblyPODeductions = context.AssemblyPODeductions.Where(x => x.OutAssembly.OutStock.VendorChallanNo == vendorChallanNo).ToArray();
                        if (assemblyPODeductions != null)
                            context.AssemblyPODeductions.RemoveRange(assemblyPODeductions);

                        var accPODeductions = context.AccPODeductions.Where(x => x.OutAcc.OutStock.VendorChallanNo == vendorChallanNo).ToArray();
                        if (accPODeductions != null)
                            context.AccPODeductions.RemoveRange(accPODeductions);



                        var outAssemblys = context.OutAssemblys.Where(x => x.OutStock.VendorChallanNo == vendorChallanNo).ToArray();
                        if (outAssemblys != null)
                            context.OutAssemblys.RemoveRange(outAssemblys);

                        var outAccs = context.OutAccs.Where(x => x.OutStock.VendorChallanNo == vendorChallanNo).ToArray();
                        if (outAccs != null)
                            context.OutAccs.RemoveRange(outAccs);

                        var outStocks = context.OutStocks.Where(x => x.VendorChallanNo == vendorChallanNo).ToArray();
                        if (outStocks != null)
                            context.OutStocks.RemoveRange(outStocks);



                        context.VendorChallans.Remove(vendorChallan);
                    }

                    context.SaveChanges();

                    response.StatusCode = HttpStatusCode.OK;
                    response.Message = "Vendor Challan deleted successfully.";

                    return Ok(response);
                }
            }
            catch (Exception e)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = "Something went wrong!";

                return InternalServerError(new Exception("Something went wrong!"));
            }
        }

        [HttpPost, Route("DeleteBASFInvoiceByBASFInvoiceId")]
        public IHttpActionResult DeleteBASFInvoiceByBASFInvoiceId(VendorChallanNoModel vendorChallanNoModel)
        {
            var basfInvoiceId = vendorChallanNoModel.VendorChallanNo;

            SuccessResponse response = new SuccessResponse();

            try
            {
                using (var context = new erpdbEntities())
                {
                    var basfInvoice = context.BASFInvoices.Where(x => x.BASFInvoiceId == basfInvoiceId).FirstOrDefault();

                    if (basfInvoice != null)
                    {
                        var challanDeductions = context.InvoiceChallanDeductions.Where(x => x.InvoiceOutStock.BASFInvoiceId == basfInvoiceId).ToArray();
                        if (challanDeductions != null)
                            context.InvoiceChallanDeductions.RemoveRange(challanDeductions);



                        var outStocks = context.InvoiceOutStocks.Where(x => x.BASFInvoiceId == basfInvoiceId).ToArray();
                        if (outStocks != null)
                            context.InvoiceOutStocks.RemoveRange(outStocks);


                        context.BASFInvoices.Remove(basfInvoice);
                    }

                    context.SaveChanges();

                    response.StatusCode = HttpStatusCode.OK;
                    response.Message = "BASF Invoice deleted successfully.";

                    return Ok(response);
                }
            }
            catch (Exception e)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = "Something went wrong!";

                return InternalServerError(new Exception("Something went wrong!"));
            }
        }

        [HttpPost, Route("ForceDeleteProductByProductId")]
        public IHttpActionResult ForceDeleteProductByProductId(VendorChallanNoModel vendorChallanNoModel)
        {
            var productId = vendorChallanNoModel.VendorChallanNo;

            SuccessResponse response = new SuccessResponse();

            try
            {
                List<ChallanProduct> removeChallanProducts = new List<ChallanProduct>();
                List<ChallanDetail> removeChallanDetails = new List<ChallanDetail>();

                List<POProduct> removePOProducts = new List<POProduct>();
                List<PODetail> removePODetails = new List<PODetail>();

                List<ProductMapping> removeProductMappings = new List<ProductMapping>();

                using (var context = new erpdbEntities())
                {
                    var product = context.ProductDetails.Where(x => x.ProductId == productId).FirstOrDefault();

                    if (product != null)
                    {
                        var challanProducts = context.ChallanProducts.Where(x => x.ProductId == productId).ToArray();
                        if (challanProducts != null)
                            removeChallanProducts.AddRange(challanProducts);

                        foreach (var challanProduct in challanProducts)
                        {
                            var challanDetail = challanProduct.ChallanDetail;
                            if (challanDetail != null)
                                removeChallanDetails.Add(challanDetail);
                        }

                        var poProducts = context.POProducts.Where(x => x.ProductId == productId).ToArray();
                        if (poProducts != null)
                            removePOProducts.AddRange(poProducts);

                        foreach (var poProduct in poProducts)
                        {
                            var poDetail = poProduct.PODetail;
                            if (poDetail != null)
                                removePODetails.Add(poDetail);
                        }
                    }
                }

                var distinctRemoveChallanDetails = removeChallanDetails.Distinct();
                foreach (var basfChallan in distinctRemoveChallanDetails)
                {
                    using (var context = new erpdbEntities())
                    {
                        ForceDeleteBASFChallanByChallanId(new VendorChallanNoModel() { VendorChallanNo = basfChallan.ChallanId });
                        context.SaveChanges();
                    }
                }

                var distinctRemovePODetails = removePODetails.Distinct();
                foreach (var basfPO in distinctRemovePODetails)
                {
                    using (var context = new erpdbEntities())
                    {
                        ForceDeleteBASFPOByPOId(new VendorChallanNoModel() { VendorChallanNo = basfPO.POId });
                        context.SaveChanges();
                    }
                }

                using (var context = new erpdbEntities())
                {
                    var product = context.ProductDetails.Where(x => x.ProductId == productId).FirstOrDefault();

                    if (product != null)
                    {
                        var productMappings = context.ProductMappings.Where(x => x.ProductId == productId).ToArray();
                        if (productMappings != null)
                            removeProductMappings.AddRange(productMappings);

                        context.ProductMappings.RemoveRange(removeProductMappings.Distinct());
                        context.ProductDetails.Remove(product);

                        context.SaveChanges();
                    }
                }

                response.StatusCode = HttpStatusCode.OK;
                response.Message = "Product and all its references deleted successfully.";

                return Ok(response);
            }
            catch (Exception e)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = "Something went wrong!";

                return InternalServerError(new Exception("Something went wrong!"));
            }
        }

        [HttpPost, Route("ForceDeleteProductByProductIdOld")]
        public IHttpActionResult ForceDeleteProductByProductIdOld(VendorChallanNoModel vendorChallanNoModel)
        {
            var productId = vendorChallanNoModel.VendorChallanNo;

            SuccessResponse response = new SuccessResponse();

            try
            {
                //List<AssemblyChallanDeduction> removeAssemblyChallanDeductions = new List<AssemblyChallanDeduction>();
                //List<AccChallanDeduction> removeAccChallanDeductions = new List<AccChallanDeduction>();
                //List<ChallanDeduction> removeChallanDeductions = new List<ChallanDeduction>();
                //List<InvoiceChallanDeduction> removeInvoiceChallanDeductions = new List<InvoiceChallanDeduction>();

                //List<AssemblyPODeduction> removeAssemblyPODeductions = new List<AssemblyPODeduction>();
                //List<AccPODeduction> removeAccPODeductions = new List<AccPODeduction>();
                //List<PODeduction> removePODeductions = new List<PODeduction>();

                //List<OutAssembly> removeOutAssemblys = new List<OutAssembly>();
                //List<OutAcc> removeOutAccs = new List<OutAcc>();
                //List<OutStock> removeOutStocks = new List<OutStock>();
                //List<InvoiceOutStock> removeInvoiceOutStocks = new List<InvoiceOutStock>();

                //List<VendorChallan> removeVendorChallans = new List<VendorChallan>();
                //List<BASFInvoice> removeBASFInvoices = new List<BASFInvoice>();

                List<ChallanProduct> removeChallanProducts = new List<ChallanProduct>();
                List<ChallanDetail> removeChallanDetails = new List<ChallanDetail>();

                List<POProduct> removePOProducts = new List<POProduct>();
                List<PODetail> removePODetails = new List<PODetail>();

                List<ProductMapping> removeProductMappings = new List<ProductMapping>();

                using (var context = new erpdbEntities())
                {
                    var product = context.ProductDetails.Where(x => x.ProductId == productId).FirstOrDefault();

                    if (product != null)
                    {
                        //var challanDeductions = context.ChallanDeductions.Where(x => x.ChallanProduct.ProductId == productId).ToArray();
                        //if (challanDeductions != null)
                        //    removeChallanDeductions.AddRange(challanDeductions);
                        ////context.ChallanDeductions.RemoveRange(challanDeductions);

                        //var invoiceChallanDeductions = context.InvoiceChallanDeductions.Where(x => x.ChallanProduct.ProductId == productId).ToArray();
                        //if (invoiceChallanDeductions != null)
                        //    removeInvoiceChallanDeductions.AddRange(invoiceChallanDeductions);


                        //foreach (var challanDeduction in challanDeductions)
                        //{
                        //    var assemblyChallanDeductions = context.AssemblyChallanDeductions.Where(x => x.OutAssembly.OutStockId == challanDeduction.OutStockId).ToArray();
                        //    if (assemblyChallanDeductions != null)
                        //        removeAssemblyChallanDeductions.AddRange(assemblyChallanDeductions);
                        //    //context.AssemblyChallanDeductions.RemoveRange(assemblyChallanDeductions);
                        //}

                        //foreach (var challanDeduction in challanDeductions)
                        //{
                        //    var accChallanDeductions = context.AccChallanDeductions.Where(x => x.OutAcc.OutStockId == challanDeduction.OutStockId).ToArray();
                        //    if (accChallanDeductions != null)
                        //        removeAccChallanDeductions.AddRange(accChallanDeductions);
                        //    //context.AccChallanDeductions.RemoveRange(accChallanDeductions);
                        //}



                        //var poDeductions = context.PODeductions.Where(x => x.POProduct.ProductId == productId).ToArray();
                        //if (poDeductions != null)
                        //    removePODeductions.AddRange(poDeductions);
                        ////context.PODeductions.RemoveRange(poDeductions);

                        //foreach (var poDeduction in poDeductions)
                        //{
                        //    var assemblyPODeductions = context.AssemblyPODeductions.Where(x => x.OutAssembly.OutStockId == poDeduction.OutStockId).ToArray();
                        //    if (assemblyPODeductions != null)
                        //        removeAssemblyPODeductions.AddRange(assemblyPODeductions);
                        //    //context.AssemblyPODeductions.RemoveRange(assemblyPODeductions);
                        //}

                        //foreach (var poDeduction in poDeductions)
                        //{
                        //    var accPODeductions = context.AccPODeductions.Where(x => x.OutAcc.OutStockId == poDeduction.OutStockId).ToArray();
                        //    if (accPODeductions != null)
                        //        removeAccPODeductions.AddRange(accPODeductions);
                        //    //context.AccPODeductions.RemoveRange(accPODeductions);
                        //}



                        //foreach (var assemblyChallanDeduction in removeAssemblyChallanDeductions)
                        //{
                        //    var outAssembly = context.OutAssemblys.Where(x => x.OutAssemblyId == assemblyChallanDeduction.OutAssemblyId).FirstOrDefault();
                        //    if (outAssembly != null)
                        //        removeOutAssemblys.Add(outAssembly);
                        //    //context.OutAssemblys.Remove(outAssembly);
                        //}

                        //foreach (var accChallanDeduction in removeAccChallanDeductions)
                        //{
                        //    var outAcc = context.OutAccs.Where(x => x.OutAccId == accChallanDeduction.OutAccId).FirstOrDefault();
                        //    if (outAcc != null)
                        //        removeOutAccs.Add(outAcc);
                        //    //context.OutAccs.Remove(outAcc);
                        //}

                        //foreach (var challanDeduction in challanDeductions)
                        //{
                        //    var outStock = context.OutStocks.Where(x => x.OutStockId == challanDeduction.OutStockId).FirstOrDefault();
                        //    if (outStock != null)
                        //        removeOutStocks.Add(outStock);
                        //    //context.OutStocks.Remove(outStock);
                        //}

                        //foreach (var invoiceChallanDeduction in invoiceChallanDeductions)
                        //{
                        //    var invoiceOutStock = context.InvoiceOutStocks.Where(x => x.InvoiceOutStockId == invoiceChallanDeduction.InvoiceOutStockId).FirstOrDefault();
                        //    if (invoiceOutStock != null)
                        //        removeInvoiceOutStocks.Add(invoiceOutStock);
                        //    //context.OutStocks.Remove(outStock);
                        //}



                        //foreach (var assemblyPODeduction in removeAssemblyPODeductions)
                        //{
                        //    var outAssembly = context.OutAssemblys.Where(x => x.OutAssemblyId == assemblyPODeduction.OutAssemblyId).FirstOrDefault();
                        //    if (outAssembly != null)
                        //        removeOutAssemblys.Add(outAssembly);
                        //    //context.OutAssemblys.Remove(outAssembly);
                        //}

                        //foreach (var accPODeduction in removeAccPODeductions)
                        //{
                        //    var outAcc = context.OutAccs.Where(x => x.OutAccId == accPODeduction.OutAccId).FirstOrDefault();
                        //    if (outAcc != null)
                        //        removeOutAccs.Add(outAcc);
                        //    //context.OutAccs.Remove(outAcc);
                        //}

                        //foreach (var poDeduction in poDeductions)
                        //{
                        //    var outStock = context.OutStocks.Where(x => x.OutStockId == poDeduction.OutStockId).FirstOrDefault();
                        //    if (outStock != null)
                        //        removeOutStocks.Add(outStock);
                        //    //context.OutStocks.Remove(outStock);
                        //}



                        //foreach (var outStock in removeOutStocks)
                        //{
                        //    var vendorChallan = context.VendorChallans.Where(x => x.VendorChallanNo == outStock.VendorChallanNo).ToArray();
                        //    removeVendorChallans.AddRange(vendorChallan);
                        //}

                        //foreach (var invoiceOutStock in removeInvoiceOutStocks)
                        //{
                        //    var basfInvoice = context.BASFInvoices.Where(x => x.BASFInvoiceId == invoiceOutStock.BASFInvoiceId).ToArray();
                        //    removeBASFInvoices.AddRange(basfInvoice);
                        //}



                        var challanProducts = context.ChallanProducts.Where(x => x.ProductId == productId).ToArray();
                        if (challanProducts != null)
                            removeChallanProducts.AddRange(challanProducts);
                        //context.ChallanProducts.RemoveRange(challanProducts);

                        foreach (var challanProduct in challanProducts)
                        {
                            var challanDetail = challanProduct.ChallanDetail;
                            if (challanDetail != null)
                                removeChallanDetails.Add(challanDetail);
                            //context.ChallanDetails.Remove(challanDetail);
                        }

                        var poProducts = context.POProducts.Where(x => x.ProductId == productId).ToArray();
                        if (poProducts != null)
                            removePOProducts.AddRange(poProducts);
                        //context.POProducts.RemoveRange(poProducts);

                        foreach (var poProduct in poProducts)
                        {
                            var poDetail = poProduct.PODetail;
                            if (poDetail != null)
                                removePODetails.Add(poDetail);
                            //context.PODetails.Remove(poDetail);
                        }
                    }
                }

                var distinctRemoveChallanDetails = removeChallanDetails.Distinct();
                foreach (var basfChallan in distinctRemoveChallanDetails)
                {
                    using (var context = new erpdbEntities())
                    {
                        ForceDeleteBASFChallanByChallanId(new VendorChallanNoModel() { VendorChallanNo = basfChallan.ChallanId });
                        context.SaveChanges();
                    }
                }

                var distinctRemovePODetails = removePODetails.Distinct();
                foreach (var basfPO in distinctRemovePODetails)
                {
                    using (var context = new erpdbEntities())
                    {
                        ForceDeleteBASFPOByPOId(new VendorChallanNoModel() { VendorChallanNo = basfPO.POId });
                        context.SaveChanges();
                    }
                }

                using (var context = new erpdbEntities())
                {
                    var product = context.ProductDetails.Where(x => x.ProductId == productId).FirstOrDefault();

                    if (product != null)
                    {
                        var productMappings = context.ProductMappings.Where(x => x.ProductId == productId).ToArray();
                        if (productMappings != null)
                            removeProductMappings.AddRange(productMappings);
                        //context.ProductMappings.RemoveRange(productMappings);



                        //context.AssemblyChallanDeductions.RemoveRange(removeAssemblyChallanDeductions.Distinct());
                        //context.AccChallanDeductions.RemoveRange(removeAccChallanDeductions.Distinct());
                        //context.ChallanDeductions.RemoveRange(removeChallanDeductions.Distinct());

                        //context.AssemblyPODeductions.RemoveRange(removeAssemblyPODeductions.Distinct());
                        //context.AccPODeductions.RemoveRange(removeAccPODeductions.Distinct());
                        //context.PODeductions.RemoveRange(removePODeductions.Distinct());

                        //context.OutAssemblys.RemoveRange(removeOutAssemblys.Distinct());
                        //context.OutAccs.RemoveRange(removeOutAccs.Distinct());
                        //context.OutStocks.RemoveRange(removeOutStocks.Distinct());

                        //context.VendorChallans.RemoveRange(removeVendorChallans.Distinct());

                        //context.ChallanProducts.RemoveRange(removeChallanProducts.Distinct());
                        //context.ChallanDetails.RemoveRange(removeChallanDetails.Distinct());

                        //context.POProducts.RemoveRange(removePOProducts.Distinct());
                        //context.PODetails.RemoveRange(removePODetails.Distinct());

                        context.ProductMappings.RemoveRange(removeProductMappings.Distinct());
                        context.ProductDetails.Remove(product);

                        context.SaveChanges();
                    }
                }

                response.StatusCode = HttpStatusCode.OK;
                response.Message = "Product and all its references deleted successfully.";

                return Ok(response);
            }
            catch (Exception e)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = "Something went wrong!";

                return InternalServerError(new Exception("Something went wrong!"));
            }
        }

        [HttpPost, Route("ForceDeleteBASFChallanByChallanId")]
        public IHttpActionResult ForceDeleteBASFChallanByChallanId(VendorChallanNoModel vendorChallanNoModel)
        {
            var challanId = vendorChallanNoModel.VendorChallanNo;

            SuccessResponse response = new SuccessResponse();

            try
            {
                List<OutStock> removeOutStocks = new List<OutStock>();
                List<InvoiceOutStock> removeInvoiceOutStocks = new List<InvoiceOutStock>();

                List<OutAssembly> removeOutAssemblys = new List<OutAssembly>();
                List<OutAcc> removeOutAccs = new List<OutAcc>();

                List<VendorChallan> removeVendorChallans = new List<VendorChallan>();
                List<BASFInvoice> removeBASFInvoices = new List<BASFInvoice>();

                List<ChallanProduct> removeChallanProducts = new List<ChallanProduct>();

                using (var context = new erpdbEntities())
                {
                    var challan = context.ChallanDetails.Where(x => x.ChallanId == challanId).FirstOrDefault();

                    if (challan != null)
                    {
                        var challanDeductions = context.ChallanDeductions.Where(x => x.ChallanProduct.ChallanId == challanId).ToArray();

                        foreach (var challanDeduction in challanDeductions)
                        {
                            var outStock = context.OutStocks.Where(x => x.OutStockId == challanDeduction.OutStockId).FirstOrDefault();
                            if (outStock != null)
                                removeOutStocks.Add(outStock);
                        }


                        foreach (var outStock in removeOutStocks)
                        {
                            var vendorChallan = context.VendorChallans.Where(x => x.VendorChallanNo == outStock.VendorChallanNo).ToArray();
                            removeVendorChallans.AddRange(vendorChallan);
                        }



                        var invoiceChallanDeductions = context.InvoiceChallanDeductions.Where(x => x.ChallanProduct.ChallanId == challanId).ToArray();

                        foreach (var invoiceChallanDeduction in invoiceChallanDeductions)
                        {
                            var invoiceOutStock = context.InvoiceOutStocks.Where(x => x.InvoiceOutStockId == invoiceChallanDeduction.InvoiceOutStockId).FirstOrDefault();
                            if (invoiceOutStock != null)
                                removeInvoiceOutStocks.Add(invoiceOutStock);
                        }

                        foreach (var invoiceOutStock in removeInvoiceOutStocks)
                        {
                            var basfInvoice = context.BASFInvoices.Where(x => x.BASFInvoiceId == invoiceOutStock.BASFInvoiceId).ToArray();
                            removeBASFInvoices.AddRange(basfInvoice);
                        }
                    }
                }


                var distinctVendorChallans = removeVendorChallans.Distinct();
                foreach (var vendorChallan in distinctVendorChallans)
                {
                    using (var context = new erpdbEntities())
                    {
                        DeleteVendorChallanByVendorChallanNo(new VendorChallanNoModel() { VendorChallanNo = vendorChallan.VendorChallanNo });
                        context.SaveChanges();
                    }
                }

                var distinctBASFInvoices = removeBASFInvoices.Distinct();
                foreach (var basfInvoice in distinctBASFInvoices)
                {
                    using (var context = new erpdbEntities())
                    {
                        DeleteBASFInvoiceByBASFInvoiceId(new VendorChallanNoModel() { VendorChallanNo = basfInvoice.BASFInvoiceId });
                        context.SaveChanges();
                    }
                }


                using (var context = new erpdbEntities())
                {
                    var challan = context.ChallanDetails.Where(x => x.ChallanId == challanId).FirstOrDefault();

                    if (challan != null)
                    {
                        var challanProducts = context.ChallanProducts.Where(x => x.ChallanId == challanId).ToArray();
                        if (challanProducts != null)
                            removeChallanProducts.AddRange(challanProducts);

                        context.ChallanProducts.RemoveRange(removeChallanProducts.Distinct());

                        challan = context.ChallanDetails.Where(x => x.ChallanId == challanId).FirstOrDefault();
                        if (challan != null)
                            context.ChallanDetails.Remove(challan);

                        context.SaveChanges();

                        response.StatusCode = HttpStatusCode.OK;
                        response.Message = "BASF Challan and all its references deleted successfully.";
                    }
                }

                return Ok(response);
            }
            catch (Exception e)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = "Something went wrong!";

                return InternalServerError(new Exception("Something went wrong!"));
            }
        }

        [HttpPost, Route("ForceDeleteBASFPOByPOId")]
        public IHttpActionResult ForceDeleteBASFPOByPOId(VendorChallanNoModel vendorChallanNoModel)
        {
            var poId = vendorChallanNoModel.VendorChallanNo;

            SuccessResponse response = new SuccessResponse();

            try
            {
                List<OutStock> removeOutStocks = new List<OutStock>();
                List<OutAssembly> removeOutAssemblys = new List<OutAssembly>();
                List<OutAcc> removeOutAccs = new List<OutAcc>();

                List<VendorChallan> removeVendorChallans = new List<VendorChallan>();

                List<POProduct> removePOProducts = new List<POProduct>();

                using (var context = new erpdbEntities())
                {
                    var po = context.PODetails.Where(x => x.POId == poId).FirstOrDefault();

                    if (po != null)
                    {
                        var poDeductions = context.PODeductions.Where(x => x.POProduct.POId == poId).ToArray();

                        foreach (var poDeduction in poDeductions)
                        {
                            var outStock = context.OutStocks.Where(x => x.OutStockId == poDeduction.OutStockId).FirstOrDefault();
                            if (outStock != null)
                                removeOutStocks.Add(outStock);
                        }



                        foreach (var outStock in removeOutStocks)
                        {
                            var vendorChallan = context.VendorChallans.Where(x => x.VendorChallanNo == outStock.VendorChallanNo).ToArray();
                            removeVendorChallans.AddRange(vendorChallan);
                        }

                        var distinctVendorChallans = removeVendorChallans.Distinct();
                        foreach (var vendorChallan in distinctVendorChallans)
                        {
                            DeleteVendorChallanByVendorChallanNo(new VendorChallanNoModel() { VendorChallanNo = vendorChallan.VendorChallanNo });
                        }

                    }

                    context.SaveChanges();
                }

                using (var context = new erpdbEntities())
                {
                    var po = context.PODetails.Where(x => x.POId == poId).FirstOrDefault();

                    if (po != null)
                    {
                        var poProducts = context.POProducts.Where(x => x.POId == poId).ToArray();
                        if (poProducts != null)
                            removePOProducts.AddRange(poProducts);

                        context.POProducts.RemoveRange(removePOProducts.Distinct());

                        po = context.PODetails.Where(x => x.POId == poId).FirstOrDefault();
                        if (po != null)
                            context.PODetails.Remove(po);

                        context.SaveChanges();

                        response.StatusCode = HttpStatusCode.OK;
                        response.Message = "BASF PO and all its references deleted successfully.";
                    }
                }

                return Ok(response);
            }
            catch (Exception e)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = "Something went wrong!";

                return InternalServerError(new Exception("Something went wrong!"));
            }
        }

        [HttpPost, Route("GetBASFChallanWhereUsedInVendorChallansReport")]
        public IHttpActionResult GetBASFChallanWhereUsedInVendorChallansReport(VendorChallanNoModel vendorChallanNoModel)
        {
            int challanId = vendorChallanNoModel.VendorChallanNo;

            try
            {
                List<BASFChallanPOWhereUsedModel> modelList = new List<BASFChallanPOWhereUsedModel>();

                using (var context = new erpdbEntities())
                {
                    var challanDetail = context.ChallanDetails.Where(x => x.ChallanId == challanId).FirstOrDefault();

                    foreach (var challanProduct in challanDetail.ChallanProducts)
                    {
                        var challanDeductions = context.ChallanDeductions.Where(x => x.ChallanProduct.ChallanProductId == challanProduct.ChallanProductId).ToList();

                        int challanDeductionIndex = 0;
                        if (challanDeductions.Count > 0)
                        {
                            foreach (var challanDeduction in challanDeductions)
                            {
                                BASFChallanPOWhereUsedModel model = new BASFChallanPOWhereUsedModel();

                                if (challanDeductionIndex == 0)
                                {
                                    model.InputCode = challanProduct.ProductDetail.InputCode;
                                    model.InputMaterialDesc = challanProduct.ProductDetail.InputMaterialDesc;
                                    model.InputQuantity = Convert.ToString(challanProduct.InputQuantity);
                                    model.RemainingQuantity = model.InputQuantity;

                                    if (challanProduct.ChallanDeductions != null && challanProduct.ChallanDeductions.Count > 0)
                                        model.RemainingQuantity = Convert.ToString(challanProduct.InputQuantity - challanProduct.ChallanDeductions.Sum(x => x.OutQuantity));
                                    else if (challanProduct.AccChallanDeductions != null && challanProduct.AccChallanDeductions.Count > 0)
                                        model.RemainingQuantity = Convert.ToString(challanProduct.InputQuantity - challanProduct.AccChallanDeductions.Sum(x => x.OutQuantity));
                                    else if (challanProduct.AssemblyChallanDeductions != null && challanProduct.AssemblyChallanDeductions.Count > 0)
                                        model.RemainingQuantity = Convert.ToString(challanProduct.InputQuantity - challanProduct.AssemblyChallanDeductions.Sum(x => x.OutQuantity));

                                    model.TotalUsed = Convert.ToString(Convert.ToInt32(model.InputQuantity) - Convert.ToInt32(model.RemainingQuantity));
                                }

                                model.ChallanNo = challanDetail.ChallanNo;

                                model.VendorChallanNo = Convert.ToString(challanDeduction.OutStock.VendorChallanNo);
                                model.VendorChallanDate = challanDeduction.OutStock.VendorChallan.VendorChallanDate.Value.ToShortDateString();
                                model.VendorChallanOutQnt = Convert.ToString(challanDeduction.OutQuantity);

                                modelList.Add(model);

                                challanDeductionIndex++;
                            }
                        }
                        else
                        {
                            BASFChallanPOWhereUsedModel model = new BASFChallanPOWhereUsedModel();

                            model.InputCode = challanProduct.ProductDetail.InputCode;
                            model.InputMaterialDesc = challanProduct.ProductDetail.InputMaterialDesc;
                            model.InputQuantity = Convert.ToString(challanProduct.InputQuantity);
                            model.RemainingQuantity = model.InputQuantity;
                            model.TotalUsed = "0";

                            model.ChallanNo = challanDetail.ChallanNo;

                            model.VendorChallanNo = "NA";
                            model.VendorChallanDate = "NA";
                            model.VendorChallanOutQnt = "NA";

                            modelList.Add(model);
                        }
                    }
                }

                return Ok(modelList);
            }
            catch (Exception e)
            {
                return InternalServerError();
            }
        }

        [HttpPost, Route("GetBASFPOWhereUsedInVendorChallansReport")]
        public IHttpActionResult GetBASFPOWhereUsedInVendorChallansReport(VendorChallanNoModel vendorChallanNoModel)
        {
            int poId = vendorChallanNoModel.VendorChallanNo;

            try
            {
                List<BASFChallanPOWhereUsedModel> modelList = new List<BASFChallanPOWhereUsedModel>();

                using (var context = new erpdbEntities())
                {
                    var poDetail = context.PODetails.Where(x => x.POId == poId).FirstOrDefault();

                    foreach (var poProduct in poDetail.POProducts)
                    {
                        var poDeductions = context.PODeductions.Where(x => x.POProduct.POProductId == poProduct.POProductId).ToList();

                        int poDeductionIndex = 0;
                        if (poDeductions.Count > 0)
                        {
                            foreach (var poDeduction in poDeductions)
                            {
                                BASFChallanPOWhereUsedModel model = new BASFChallanPOWhereUsedModel();

                                if (poDeductionIndex == 0)
                                {
                                    model.InputCode = poProduct.ProductDetail.InputCode;
                                    model.InputMaterialDesc = poProduct.ProductDetail.InputMaterialDesc;
                                    model.InputQuantity = Convert.ToString(poProduct.InputQuantity);
                                    model.RemainingQuantity = model.InputQuantity;

                                    if (poProduct.PODeductions != null && poProduct.PODeductions.Count > 0)
                                        model.RemainingQuantity = Convert.ToString(poProduct.InputQuantity - poProduct.PODeductions.Sum(x => x.OutQuantity));
                                    else if (poProduct.AccPODeductions != null && poProduct.AccPODeductions.Count > 0)
                                        model.RemainingQuantity = Convert.ToString(poProduct.InputQuantity - poProduct.AccPODeductions.Sum(x => x.OutQuantity));
                                    else if (poProduct.AssemblyPODeductions != null && poProduct.AssemblyPODeductions.Count > 0)
                                        model.RemainingQuantity = Convert.ToString(poProduct.InputQuantity - poProduct.AssemblyPODeductions.Sum(x => x.OutQuantity));

                                    model.TotalUsed = Convert.ToString(Convert.ToInt32(model.InputQuantity) - Convert.ToInt32(model.RemainingQuantity));
                                }

                                model.ChallanNo = poDetail.PONo;

                                model.VendorChallanNo = Convert.ToString(poDeduction.OutStock.VendorChallanNo);
                                model.VendorChallanDate = poDeduction.OutStock.VendorChallan.VendorChallanDate.Value.ToShortDateString();
                                model.VendorChallanOutQnt = Convert.ToString(poDeduction.OutQuantity);

                                modelList.Add(model);

                                poDeductionIndex++;
                            }
                        }
                        else
                        {
                            BASFChallanPOWhereUsedModel model = new BASFChallanPOWhereUsedModel();

                            model.InputCode = poProduct.ProductDetail.InputCode;
                            model.InputMaterialDesc = poProduct.ProductDetail.InputMaterialDesc;
                            model.InputQuantity = Convert.ToString(poProduct.InputQuantity);
                            model.RemainingQuantity = model.InputQuantity;
                            model.TotalUsed = "0";

                            model.ChallanNo = poDetail.PONo;

                            model.VendorChallanNo = "NA";
                            model.VendorChallanDate = "NA";
                            model.VendorChallanOutQnt = "NA";

                            modelList.Add(model);
                        }
                    }
                }

                return Ok(modelList);
            }
            catch (Exception e)
            {
                return InternalServerError();
            }
        }

        [HttpPost, Route("GetBASFChallanWhereUsedInBASFInvoicesReport")]
        public IHttpActionResult GetBASFChallanWhereUsedInBASFInvoicesReport(VendorChallanNoModel vendorChallanNoModel)
        {
            int challanId = vendorChallanNoModel.VendorChallanNo;

            try
            {
                List<BASFChallanPOWhereUsedModel> modelList = new List<BASFChallanPOWhereUsedModel>();

                using (var context = new erpdbEntities())
                {
                    var challanDetail = context.ChallanDetails.Where(x => x.ChallanId == challanId).FirstOrDefault();

                    foreach (var challanProduct in challanDetail.ChallanProducts)
                    {
                        var challanDeductions = context.InvoiceChallanDeductions.Where(x => x.ChallanProduct.ChallanProductId == challanProduct.ChallanProductId).ToList();

                        int challanDeductionIndex = 0;
                        if (challanDeductions.Count > 0)
                        {
                            foreach (var challanDeduction in challanDeductions)
                            {
                                BASFChallanPOWhereUsedModel model = new BASFChallanPOWhereUsedModel();

                                if (challanDeductionIndex == 0)
                                {
                                    model.InputCode = challanProduct.ProductDetail.InputCode;
                                    model.InputMaterialDesc = challanProduct.ProductDetail.InputMaterialDesc;
                                    model.InputQuantity = Convert.ToString(challanProduct.InputQuantity);
                                    model.RemainingQuantity = model.InputQuantity;

                                    if (challanProduct.InvoiceChallanDeductions != null && challanProduct.InvoiceChallanDeductions.Count > 0)
                                        model.RemainingQuantity = Convert.ToString(challanProduct.InputQuantity - challanProduct.InvoiceChallanDeductions.Sum(x => x.OutQuantity));

                                    model.TotalUsed = Convert.ToString(Convert.ToInt32(model.InputQuantity) - Convert.ToInt32(model.RemainingQuantity));
                                }

                                model.ChallanNo = challanDetail.ChallanNo;

                                model.VendorChallanNo = Convert.ToString(challanDeduction.InvoiceOutStock.BASFInvoice.BASFInvoiceNo);
                                model.VendorChallanDate = challanDeduction.InvoiceOutStock.BASFInvoice.BASFInvoiceDate.Value.ToShortDateString();
                                model.VendorChallanOutQnt = Convert.ToString(challanDeduction.OutQuantity);

                                modelList.Add(model);

                                challanDeductionIndex++;
                            }
                        }
                        else
                        {
                            BASFChallanPOWhereUsedModel model = new BASFChallanPOWhereUsedModel();

                            model.InputCode = challanProduct.ProductDetail.InputCode;
                            model.InputMaterialDesc = challanProduct.ProductDetail.InputMaterialDesc;
                            model.InputQuantity = Convert.ToString(challanProduct.InputQuantity);
                            model.RemainingQuantity = model.InputQuantity;
                            model.TotalUsed = "0";

                            model.ChallanNo = challanDetail.ChallanNo;

                            model.VendorChallanNo = "NA";
                            model.VendorChallanDate = "NA";
                            model.VendorChallanOutQnt = "NA";

                            modelList.Add(model);
                        }
                    }
                }

                return Ok(modelList);
            }
            catch (Exception e)
            {
                return InternalServerError();
            }
        }

        [HttpGet, Route("GetCloseChallanReport")]
        public IHttpActionResult GetCloseChallanReport()
        {
            try
            {
                using (var context = new erpdbEntities())
                {
                    List<CloseChallanReportModel> modelList = new List<CloseChallanReportModel>();

                    var challanDetails = context.ChallanDetails.OrderBy(x => new { x.ChallanDate, x.ChallanNo }).ToList();

                    foreach (var challanDetail in challanDetails)
                    {
                        foreach (var challanProduct in challanDetail.ChallanProducts)
                        {
                            CloseChallanReportModel model = new CloseChallanReportModel();

                            model.ChallanNo = challanDetail.ChallanNo;
                            model.ChallanDate = challanDetail.ChallanDate.Value.ToShortDateString();
                            model.InputCode = challanProduct.ProductDetail.InputCode;
                            model.InputQuantity = Convert.ToString(challanProduct.InputQuantity);
                            model.RemainingQuantity = model.InputQuantity;

                            var main = Convert.ToInt32(EProductCategorys.Main);
                            var assembly = Convert.ToInt32(EProductCategorys.Assembly);
                            var acc = Convert.ToInt32(EProductCategorys.Accessories);

                            if (challanProduct.ProductDetail.ProductType.ProductCategoryId == main && challanProduct.ChallanDeductions != null && challanProduct.ChallanDeductions.Count > 0)
                            {
                                //model.OutputQuantity = Convert.ToString(challanProduct.ChallanDeductions.Select(p => p.OutStock).Distinct().Sum(k => k.OutputQuantity));
                                model.OutputQuantity = Convert.ToString(challanProduct.ChallanDeductions.Sum(x => x.OutQuantity));
                                var vendorChallanNos = challanProduct.ChallanDeductions.OrderBy(k => k.OutStock.VendorChallanNo).Select(x => x.OutStock.VendorChallanNo).ToList();

                                model.VendorChallanNo = "";
                                int index = 0;
                                foreach (int vendorChallanNo in vendorChallanNos)
                                {
                                    if (index != vendorChallanNos.Count - 1)
                                        model.VendorChallanNo += vendorChallanNo.ToString() + ", ";
                                    else
                                        model.VendorChallanNo += vendorChallanNo.ToString();

                                    index++;
                                }

                                model.VendorChallanDate = challanProduct.ChallanDeductions.OrderByDescending(k => k.OutStock.VendorChallan.VendorChallanDate).Select(x => x.OutStock.VendorChallan.VendorChallanDate.Value.ToShortDateString()).FirstOrDefault();
                                model.RemainingQuantity = Convert.ToString(challanProduct.InputQuantity);
                                model.RemainingQuantity = Convert.ToString(challanProduct.InputQuantity - challanProduct.ChallanDeductions.Sum(x => x.OutQuantity));
                            }
                            else if (challanProduct.ProductDetail.ProductType.ProductCategoryId == assembly && challanProduct.AssemblyChallanDeductions != null && challanProduct.AssemblyChallanDeductions.Count > 0)
                            {
                                //model.OutputQuantity = Convert.ToString(challanProduct.AssemblyChallanDeductions.Select(p => p.OutAssembly).Distinct().Sum(k => k.OutputQuantity));
                                model.OutputQuantity = Convert.ToString(challanProduct.AssemblyChallanDeductions.Sum(x => x.OutQuantity));
                                var vendorChallanNos = challanProduct.AssemblyChallanDeductions.OrderBy(k => k.OutAssembly.OutStock.VendorChallanNo).Select(x => x.OutAssembly.OutStock.VendorChallanNo).ToList();

                                model.VendorChallanNo = "";
                                int index = 0;
                                foreach (int vendorChallanNo in vendorChallanNos)
                                {
                                    if (index != vendorChallanNos.Count - 1)
                                        model.VendorChallanNo += vendorChallanNo.ToString() + ", ";
                                    else
                                        model.VendorChallanNo += vendorChallanNo.ToString();

                                    index++;
                                }

                                model.VendorChallanDate = challanProduct.AssemblyChallanDeductions.OrderByDescending(k => k.OutAssembly.OutStock.VendorChallan.VendorChallanDate).Select(x => x.OutAssembly.OutStock.VendorChallan.VendorChallanDate.Value.ToShortDateString()).FirstOrDefault();
                                model.RemainingQuantity = Convert.ToString(challanProduct.InputQuantity);
                                model.RemainingQuantity = Convert.ToString(challanProduct.InputQuantity - challanProduct.AssemblyChallanDeductions.Sum(x => x.OutQuantity));
                            }
                            else if (challanProduct.ProductDetail.ProductType.ProductCategoryId == acc && challanProduct.AccChallanDeductions != null && challanProduct.AccChallanDeductions.Count > 0)
                            {
                                //model.OutputQuantity = Convert.ToString(challanProduct.AccChallanDeductions.Select(p => p.OutAcc).Distinct().Sum(k => k.OutputQuantity));
                                model.OutputQuantity = Convert.ToString(challanProduct.AccChallanDeductions.Sum(x => x.OutQuantity));
                                var vendorChallanNos = challanProduct.AccChallanDeductions.OrderBy(k => k.OutAcc.OutStock.VendorChallanNo).Select(x => x.OutAcc.OutStock.VendorChallanNo).ToList();

                                model.VendorChallanNo = "";
                                int index = 0;
                                foreach (int vendorChallanNo in vendorChallanNos)
                                {
                                    if (index != vendorChallanNos.Count - 1)
                                        model.VendorChallanNo += vendorChallanNo.ToString() + ", ";

                                    index++;
                                }

                                model.VendorChallanDate = challanProduct.AccChallanDeductions.OrderByDescending(k => k.OutAcc.OutStock.VendorChallan.VendorChallanDate).Select(x => x.OutAcc.OutStock.VendorChallan.VendorChallanDate.Value.ToShortDateString()).FirstOrDefault();
                                model.RemainingQuantity = Convert.ToString(challanProduct.InputQuantity - challanProduct.AccChallanDeductions.Sum(x => x.OutQuantity));
                            }

                            modelList.Add(model);
                        }
                    }

                    return Ok(modelList);
                }
            }
            catch (Exception e)
            {
                return InternalServerError(new Exception("Something went wrong"));
            }
        }

        [HttpGet, Route("GetFGStockReport")]
        public IHttpActionResult GetFGStockReport()
        {
            try
            {
                List<FGAndSemiStockReportModel> modelList = new List<FGAndSemiStockReportModel>();

                using (var context = new erpdbEntities())
                {
                    var challanDetails = context.ChallanDetails.OrderBy(x => new { x.ChallanDate, x.ChallanNo }).ToList();

                    foreach (var challanDetail in challanDetails)
                    {
                        foreach (var challanProduct in challanDetail.ChallanProducts)
                        {
                            int main = Convert.ToInt32(EProductCategorys.Main);

                            if (challanProduct.ProductDetail.ProductType.ProductCategoryId == main && challanProduct.ChallanDeductions != null)
                            {
                                FGAndSemiStockReportModel model = new FGAndSemiStockReportModel();

                                model.Code = challanProduct.ProductDetail.OutputCode;
                                model.Description = challanProduct.ProductDetail.OutputMaterialDesc;
                                //int remainingQty = Convert.ToInt32(challanProduct.InputQuantity);

                                //if (challanProduct.ChallanDeductions != null && challanProduct.ChallanDeductions.Count > 0)
                                //    remainingQty = Convert.ToInt32(challanProduct.InputQuantity) - Convert.ToInt32(challanProduct.ChallanDeductions.Sum(x => x.OutQuantity));

                                //if (remainingQty == 0)
                                //{
                                model.Quantity = challanProduct.ChallanDeductions.Where(x => x.OutStock.VendorChallan.IsNg == 0).Sum(k => k.OutStock.OutputQuantity).Value.ToString();
                                int invoiceQuantity = challanProduct.InvoiceChallanDeductions.Sum(k => k.InvoiceOutStock.OutputQuantity).Value;

                                model.Quantity = Convert.ToString(Convert.ToInt32(model.Quantity) - invoiceQuantity);
                                model.Qnt = Convert.ToInt32(model.Quantity);

                                modelList.Add(model);
                                //}
                            }
                        }
                    }

                    var grpBy = modelList.GroupBy(x => new { x.Code, x.Description }).Select(g => new { key = g.Key, sum = g.Sum(p => p.Qnt) });

                    List<FGAndSemiStockReportModel> mdlList = new List<FGAndSemiStockReportModel>();
                    foreach (var grp in grpBy)
                    {
                        FGAndSemiStockReportModel mdl = new FGAndSemiStockReportModel();
                        mdl.Code = grp.key.Code;
                        mdl.Description = grp.key.Description;
                        mdl.Quantity = grp.sum.ToString();
                        mdlList.Add(mdl);
                    }

                    return Ok(mdlList);
                }
            }
            catch (Exception e)
            {
                return InternalServerError(new Exception("Something went wrong."));
            }
        }

        [HttpGet, Route("GetSemiStockReport")]
        public IHttpActionResult GetSemiStockReport()
        {
            try
            {
                List<FGAndSemiStockReportModel> modelList = new List<FGAndSemiStockReportModel>();

                using (var context = new erpdbEntities())
                {
                    var challanDetails = context.ChallanDetails.OrderBy(x => new { x.ChallanDate, x.ChallanNo }).ToList();

                    foreach (var challanDetail in challanDetails)
                    {
                        foreach (var challanProduct in challanDetail.ChallanProducts)
                        {
                            FGAndSemiStockReportModel model = new FGAndSemiStockReportModel();

                            model.Code = challanProduct.ProductDetail.InputCode;
                            model.Description = challanProduct.ProductDetail.InputMaterialDesc;
                            int remainingQty = Convert.ToInt32(challanProduct.InputQuantity);

                            if (challanProduct.ChallanDeductions != null && challanProduct.ChallanDeductions.Count > 0)
                                remainingQty = Convert.ToInt32(challanProduct.InputQuantity) - Convert.ToInt32(challanProduct.ChallanDeductions.Sum(x => x.OutQuantity));
                            else if (challanProduct.AssemblyChallanDeductions != null && challanProduct.AssemblyChallanDeductions.Count > 0)
                                remainingQty = Convert.ToInt32(challanProduct.InputQuantity) - Convert.ToInt32(challanProduct.AssemblyChallanDeductions.Sum(x => x.OutQuantity));
                            else if (challanProduct.AccChallanDeductions != null && challanProduct.AccChallanDeductions.Count > 0)
                                remainingQty = Convert.ToInt32(challanProduct.InputQuantity) - Convert.ToInt32(challanProduct.AccChallanDeductions.Sum(x => x.OutQuantity));

                            if (remainingQty != 0)
                            {
                                model.Quantity = remainingQty.ToString();
                                model.Qnt = remainingQty;
                                modelList.Add(model);
                            }
                        }
                    }

                    modelList = modelList.OrderBy(x => x.Description).ToList();

                    var grpBy = modelList.GroupBy(x => new { x.Code, x.Description }).Select(g => new { key = g.Key, sum = g.Sum(p => p.Qnt) });

                    List<FGAndSemiStockReportModel> mdlList = new List<FGAndSemiStockReportModel>();
                    foreach (var grp in grpBy)
                    {
                        FGAndSemiStockReportModel mdl = new FGAndSemiStockReportModel();
                        mdl.Code = grp.key.Code;
                        mdl.Description = grp.key.Description;
                        mdl.Quantity = grp.sum.ToString();
                        mdlList.Add(mdl);
                    }

                    return Ok(mdlList);
                }
            }
            catch (Exception e)
            {
                return InternalServerError(new Exception("Something went wrong."));
            }
        }
    }
}
