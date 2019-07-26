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
                        challanProductModel.ProductDetail = challanProduct.ProductDetail;
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
                        poProductModel.ProductDetail = poProduct.ProductDetail;
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
                            productQnty.ProductName = mainProduct.InputMaterialDesc;
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
                            productQnty.ProductName = mainProduct.InputMaterialDesc;
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

        [HttpPost, Route("PrintVendorChallanByVendorChallanNo")]
        public IHttpActionResult PrintVendorChallanByVendorChallanNo(VendorChallanNoModel vendorChallanNoModel)
        {
            using (var context = new erpdbEntities())
            {
                try
                {
                    VendorChallanModel vendorChallan = GetVendorChallanByVendorChallanNoPrivate(vendorChallanNoModel.VendorChallanNo);

                    string html = "";

                    html += "<html><div style=\"margin: 1%\"><table style=\"border: 1px solid\"><tr style=\"width: 100%\"><td style=\"width: 33%\"><b>Vibrant Challan No: </b>" + vendorChallan.VendorChallanNo + "</td><td style=\"width: 33%\"><b>Vibrant Challan Date: </b>" + vendorChallan.VendorChallanDate.ToShortDateString() + "</td>" + "<td style=\"width: 33%\"><b>Total Stock Out: </b>" + vendorChallan.OutStocks.Sum(x => x.OutputQuantity).ToString() + "</td></tr></table>";

                    html += "<table style=\"border: 1px solid\">";
                    foreach (var outStock in vendorChallan.OutStocks)
                    {
                        html += "<tr style=\"width: 100%\">";
                        foreach (var challanDeduction in outStock.ChallanDeductions)
                        {
                            var projName = challanDeduction.ChallanProduct.ProductDetail.ProjectName;
                            html += "<td style=\"border: 1px solid\">" + projName + "</td>";

                            var outputCode = challanDeduction.ChallanProduct.ProductDetail.OutputCode;
                            html += "<td style=\"border: 1px solid\">" + outputCode + "</td>";

                            var outputMatDesc = challanDeduction.ChallanProduct.ProductDetail.OutputMaterialDesc;
                            html += "<td style=\"border: 1px solid\">" + outputMatDesc + "</td>";

                            var outputQnt = challanDeduction.OutQuantity;
                            html += "<td style=\"border: 1px solid\">" + outputQnt + "</td>";

                            var inputCode = challanDeduction.ChallanProduct.ProductDetail.InputCode;
                            html += "<td style=\"border: 1px solid\">" + inputCode + "</td>";

                            var inputMatDesc = challanDeduction.ChallanProduct.ProductDetail.InputMaterialDesc;
                            html += "<td style=\"border: 1px solid\">" + inputMatDesc + "</td>";

                            var inputQnt = challanDeduction.ChallanProduct.ProductDetail.InputMaterialDesc;
                            html += "<td style=\"border: 1px solid\">" + inputQnt + "</td>";

                            var partType = challanDeduction.ChallanProduct.ProductDetail.ProductType.ProductTypeName;
                            html += "<td style=\"border: 1px solid\">" + partType + "</td>";

                            var basfChallanNo = challanDeduction.ChallanProduct.ChallanDetail.ChallanNo;
                            html += "<td style=\"border: 1px solid\">" + basfChallanNo + "</td>";

                            var balance = challanDeduction.ChallanProduct.RemainingQuantity;
                            html += "<td style=\"border: 1px solid\">" + balance + "</td>";
                        }

                        var poNos = "<ul>";
                        foreach (var poDeduction in outStock.PODeductions)
                        {
                            var poNo = poDeduction.POProduct.PODetail.PONo;
                            poNos += "<li>" + poNo + "</li>";
                        }

                        poNos += "</ul>";

                        html += "<td style=\"border: 1px solid\" rowspan=\"" + outStock.ChallanDeductions.Count() + "\">" + poNos + "</td>";
                    }

                    html += "</div></html>";

                    HtmlToPdf renderer = new HtmlToPdf();
                    renderer.PrintOptions.PrintHtmlBackgrounds = true;
                    renderer.PrintOptions.MarginTop = 2.5;
                    renderer.PrintOptions.MarginBottom = 0;
                    renderer.PrintOptions.MarginLeft = 1;
                    renderer.PrintOptions.MarginRight = 2.5;
                    renderer.PrintOptions.PaperSize = PdfPrintOptions.PdfPaperSize.A4;
                    renderer.PrintOptions.PaperOrientation = PdfPrintOptions.PdfPaperOrientation.Landscape;
                    renderer.PrintOptions.RenderDelay = 500;

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
                            challanProductModel.ProductDetail = challanDeduction.ChallanProduct.ProductDetail;
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
                            poProductModel.ProductDetail = poDeduction.POProduct.ProductDetail;
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
                                challanProductModel.ProductDetail = accChallanDeduction.ChallanProduct.ProductDetail;
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
                                poProductModel.ProductDetail = accPODeduction.POProduct.ProductDetail;
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
                                challanProductModel.ProductDetail = assemblyChallanDeduction.ChallanProduct.ProductDetail;
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
                                poProductModel.ProductDetail = assemblyPODeduction.POProduct.ProductDetail;
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
                        poProductModel.ProductDetail = poProduct.ProductDetail;
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

        [HttpPost, Route("ForceDeleteProductByProductId")]
        public IHttpActionResult ForceDeleteProductByProductId(VendorChallanNoModel vendorChallanNoModel)
        {
            var productId = vendorChallanNoModel.VendorChallanNo;

            SuccessResponse response = new SuccessResponse();

            try
            {
                List<AssemblyChallanDeduction> removeAssemblyChallanDeductions = new List<AssemblyChallanDeduction>();
                List<AccChallanDeduction> removeAccChallanDeductions = new List<AccChallanDeduction>();
                List<ChallanDeduction> removeChallanDeductions = new List<ChallanDeduction>();

                List<AssemblyPODeduction> removeAssemblyPODeductions = new List<AssemblyPODeduction>();
                List<AccPODeduction> removeAccPODeductions = new List<AccPODeduction>();
                List<PODeduction> removePODeductions = new List<PODeduction>();

                List<OutAssembly> removeOutAssemblys = new List<OutAssembly>();
                List<OutAcc> removeOutAccs = new List<OutAcc>();
                List<OutStock> removeOutStocks = new List<OutStock>();

                List<VendorChallan> removeVendorChallans = new List<VendorChallan>();

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
                        var challanDeductions = context.ChallanDeductions.Where(x => x.ChallanProduct.ProductId == productId).ToArray();
                        if (challanDeductions != null)
                            removeChallanDeductions.AddRange(challanDeductions);
                        //context.ChallanDeductions.RemoveRange(challanDeductions);

                        foreach (var challanDeduction in challanDeductions)
                        {
                            var assemblyChallanDeductions = context.AssemblyChallanDeductions.Where(x => x.OutAssembly.OutStockId == challanDeduction.OutStockId).ToArray();
                            if (assemblyChallanDeductions != null)
                                removeAssemblyChallanDeductions.AddRange(assemblyChallanDeductions);
                            //context.AssemblyChallanDeductions.RemoveRange(assemblyChallanDeductions);
                        }

                        foreach (var challanDeduction in challanDeductions)
                        {
                            var accChallanDeductions = context.AccChallanDeductions.Where(x => x.OutAcc.OutStockId == challanDeduction.OutStockId).ToArray();
                            if (accChallanDeductions != null)
                                removeAccChallanDeductions.AddRange(accChallanDeductions);
                            //context.AccChallanDeductions.RemoveRange(accChallanDeductions);
                        }



                        var poDeductions = context.PODeductions.Where(x => x.POProduct.ProductId == productId).ToArray();
                        if (poDeductions != null)
                            removePODeductions.AddRange(poDeductions);
                        //context.PODeductions.RemoveRange(poDeductions);

                        foreach (var poDeduction in poDeductions)
                        {
                            var assemblyPODeductions = context.AssemblyPODeductions.Where(x => x.OutAssembly.OutStockId == poDeduction.OutStockId).ToArray();
                            if (assemblyPODeductions != null)
                                removeAssemblyPODeductions.AddRange(assemblyPODeductions);
                            //context.AssemblyPODeductions.RemoveRange(assemblyPODeductions);
                        }

                        foreach (var poDeduction in poDeductions)
                        {
                            var accPODeductions = context.AccPODeductions.Where(x => x.OutAcc.OutStockId == poDeduction.OutStockId).ToArray();
                            if (accPODeductions != null)
                                removeAccPODeductions.AddRange(accPODeductions);
                            //context.AccPODeductions.RemoveRange(accPODeductions);
                        }



                        foreach (var assemblyChallanDeduction in removeAssemblyChallanDeductions)
                        {
                            var outAssembly = context.OutAssemblys.Where(x => x.OutAssemblyId == assemblyChallanDeduction.OutAssemblyId).FirstOrDefault();
                            if (outAssembly != null)
                                removeOutAssemblys.Add(outAssembly);
                            //context.OutAssemblys.Remove(outAssembly);
                        }

                        foreach (var accChallanDeduction in removeAccChallanDeductions)
                        {
                            var outAcc = context.OutAccs.Where(x => x.OutAccId == accChallanDeduction.OutAccId).FirstOrDefault();
                            if (outAcc != null)
                                removeOutAccs.Add(outAcc);
                            //context.OutAccs.Remove(outAcc);
                        }

                        foreach (var challanDeduction in challanDeductions)
                        {
                            var outStock = context.OutStocks.Where(x => x.OutStockId == challanDeduction.OutStockId).FirstOrDefault();
                            if (outStock != null)
                                removeOutStocks.Add(outStock);
                            //context.OutStocks.Remove(outStock);
                        }



                        foreach (var assemblyPODeduction in removeAssemblyPODeductions)
                        {
                            var outAssembly = context.OutAssemblys.Where(x => x.OutAssemblyId == assemblyPODeduction.OutAssemblyId).FirstOrDefault();
                            if (outAssembly != null)
                                removeOutAssemblys.Add(outAssembly);
                            //context.OutAssemblys.Remove(outAssembly);
                        }

                        foreach (var accPODeduction in removeAccPODeductions)
                        {
                            var outAcc = context.OutAccs.Where(x => x.OutAccId == accPODeduction.OutAccId).FirstOrDefault();
                            if (outAcc != null)
                                removeOutAccs.Add(outAcc);
                            //context.OutAccs.Remove(outAcc);
                        }

                        foreach (var poDeduction in poDeductions)
                        {
                            var outStock = context.OutStocks.Where(x => x.OutStockId == poDeduction.OutStockId).FirstOrDefault();
                            if (outStock != null)
                                removeOutStocks.Add(outStock);
                            //context.OutStocks.Remove(outStock);
                        }



                        foreach (var outStock in removeOutStocks)
                        {
                            var vendorChallan = context.VendorChallans.Where(x => x.VendorChallanNo == outStock.VendorChallanNo).ToArray();
                            removeVendorChallans.AddRange(vendorChallan);
                        }



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



                        var productMappings = context.ProductMappings.Where(x => x.ProductId == productId).ToArray();
                        if (productMappings != null)
                            removeProductMappings.AddRange(productMappings);
                        //context.ProductMappings.RemoveRange(productMappings);



                        context.AssemblyChallanDeductions.RemoveRange(removeAssemblyChallanDeductions.Distinct());
                        context.AccChallanDeductions.RemoveRange(removeAccChallanDeductions.Distinct());
                        context.ChallanDeductions.RemoveRange(removeChallanDeductions.Distinct());

                        context.AssemblyPODeductions.RemoveRange(removeAssemblyPODeductions.Distinct());
                        context.AccPODeductions.RemoveRange(removeAccPODeductions.Distinct());
                        context.PODeductions.RemoveRange(removePODeductions.Distinct());

                        context.OutAssemblys.RemoveRange(removeOutAssemblys.Distinct());
                        context.OutAccs.RemoveRange(removeOutAccs.Distinct());
                        context.OutStocks.RemoveRange(removeOutStocks.Distinct());

                        context.VendorChallans.RemoveRange(removeVendorChallans.Distinct());

                        context.ChallanProducts.RemoveRange(removeChallanProducts.Distinct());
                        context.ChallanDetails.RemoveRange(removeChallanDetails.Distinct());

                        context.POProducts.RemoveRange(removePOProducts.Distinct());
                        context.PODetails.RemoveRange(removePODetails.Distinct());

                        context.ProductMappings.RemoveRange(removeProductMappings.Distinct());

                        context.ProductDetails.Remove(product);
                    }

                    context.SaveChanges();

                    response.StatusCode = HttpStatusCode.OK;
                    response.Message = "Product and all its references deleted successfully.";

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

        [HttpPost, Route("ForceDeleteBASFChallanByChallanId")]
        public IHttpActionResult ForceDeleteBASFChallanByChallanId(VendorChallanNoModel vendorChallanNoModel)
        {
            var challanId = vendorChallanNoModel.VendorChallanNo;

            SuccessResponse response = new SuccessResponse();

            try
            {
                List<OutStock> removeOutStocks = new List<OutStock>();

                List<VendorChallan> removeVendorChallans = new List<VendorChallan>();

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
    }
}
