using ArkLibrary.Utility;
using ArkWeb.DataAccess.Data;
using ArkWeb.DataAccess.Repository;
using ArkWeb.DataAccess.Repository.IRepository;
using ArkWeb.Models;
using ArkWeb.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Data;

namespace ArkLibrary.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = SD.Role_Admin)]
	public class ProductController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IWebHostEnvironment _webHostEnvironment;
		public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
		{
			_unitOfWork = unitOfWork;
			_webHostEnvironment = webHostEnvironment;
		}

		public IActionResult Index()
		{
			List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
			IEnumerable<SelectListItem> CategoryList = _unitOfWork.Category
				.GetAll().Select(u => new SelectListItem
				{
					Text = u.Name,
					Value = u.CategoryId.ToString()
				});
			return View(objProductList);
		}

		public IActionResult Upsert(int? id)
		{

			//ViewBag.CategoryList= CategoryList;
			ProductVM productVM = new()
			{
				CategoryList = _unitOfWork.Category
				.GetAll().Select(u => new SelectListItem
				{
					Text = u.Name,
					Value = u.CategoryId.ToString()

				}),
				Product = new Product()

			};
			if (id == null || id == 0)
			{
				return View(productVM);
			}
			else
			{
				productVM.Product = _unitOfWork.Product.Get(u => u.Id == id);
				return View(productVM);
			}

		}

		[HttpPost]
		public IActionResult Upsert(ProductVM productVM, IFormFile? file)
		{
			//if (obj.Name == obj.DisplayOrder.ToString()) 
			//{
			//	ModelState.AddModelError("name","The DisplayOrder cannot exactly match the Name");
			//}

			if (ModelState.IsValid)
			{
				string wwwRootPath = _webHostEnvironment.WebRootPath;
				if (file != null)
				{
					string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
					string productPath = Path.Combine(wwwRootPath, @"images\product");

					if (!string.IsNullOrEmpty(productVM.Product.ImageURL))
					{
						//delete old image
						var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageURL.TrimStart('\\'));

						if (System.IO.File.Exists(oldImagePath))
						{
							System.IO.File.Delete(oldImagePath);
						}
					}
					using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
					{
						file.CopyTo(fileStream);
					}

					productVM.Product.ImageURL = @"\images\product\" + fileName;
				}
				if (productVM.Product.Id == 0)
				{
					_unitOfWork.Product.Add(productVM.Product);
				}
				else
				{
					_unitOfWork.Product.Update(productVM.Product);
				}
				_unitOfWork.Save();
				TempData["success"] = "Product added successfully";
				return RedirectToAction("Index");
			}
			else
			{
				productVM.CategoryList = _unitOfWork.Category
				.GetAll().Select(u => new SelectListItem
				{
					Text = u.Name,
					Value = u.CategoryId.ToString()

				});
				return View(productVM);
			}



		}


		//*public IActionResult Delete(int? id)
		//{
		//if (id == null || id == 0)
		//{
		//	return NotFound();
		//}
		//
		//Product? productFromDb = _unitOfWork.Product.Get(u => u.Id == id);
		//Category? categoryFromDb1 = _db.Categories.FirstOrDefault(u=>u.Id==id);   Ways to retrive a record !
		//Category? categoryFromDb2 = _db.Categories.Where(u=>u.Id==id).FirstOrDefault(); Ways to retrive a record ! Only use when we need filtering of data then FirstOrDefault
		//if (productFromDb == null)
		//{
		//		return NotFound();
		//	}
		//
		//	return View(productFromDb);
		//}

		


		#region API CALLS
		[HttpGet]
		public IActionResult GetAll(int id)
		{
			List<Product> objProductList = _unitOfWork.Product
				.GetAll(includeProperties: "Category").ToList();

			return Json(new { data = objProductList });
		}

		[HttpDelete]
		public IActionResult Delete(int? id)
		{

			var productToBeDeleted = _unitOfWork.Product.Get(u => u.Id == id);
			if (productToBeDeleted == null)
			{
				return Json(new { success = false, message = "Error while deleting" });
			}

			var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath,
				productToBeDeleted.ImageURL.TrimStart('\\'));

			if (System.IO.File.Exists(oldImagePath))
			{
				System.IO.File.Delete(oldImagePath);
			}

			_unitOfWork.Product.Delete(productToBeDeleted); //? Might cause problems
			_unitOfWork.Save();

			return Json(new { success = true, message = "Deleted Successful" });
		}

		#endregion

	}
}
