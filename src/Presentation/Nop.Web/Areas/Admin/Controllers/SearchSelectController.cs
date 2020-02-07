using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Helpers;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class SearchSelectController : BaseAdminController
    {
        private readonly ILocalizationService _localizationService;
        private readonly ICategoryService _categoryService;
        private readonly ICacheManager _cacheManager;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IVendorService _vendorService;
        private readonly IManufacturerService _manufacturerService;

        public SearchSelectController(ILocalizationService localizationService, ICategoryService categoryService,
            ICacheManager cacheManager, IRepository<Category> categoryRepository, IVendorService vendorService,
            IManufacturerService manufacturerService)
        {
            _localizationService = localizationService;
            _categoryService = categoryService;
            _cacheManager = cacheManager;
            _categoryRepository = categoryRepository;
            _vendorService = vendorService;
            _manufacturerService = manufacturerService;
        }

        [HttpPost]
        public virtual IActionResult Categories(string searchTerm)
        {
            return Json(SelectListHelper.GetSearchableCategoryList(_categoryService, _cacheManager,
                _localizationService, _categoryRepository, searchTerm, true));
        }

        [HttpPost]
        public virtual IActionResult Manufacturers(string searchTerm)
        {
            return Json(SelectListHelper.GetSearchableManufacturerList(_manufacturerService, _cacheManager,
                _localizationService, searchTerm, true));
        }

        [HttpPost]
        public virtual IActionResult Vendors(string searchTerm)
        {
            return Json(SelectListHelper.GetSearchableVendorList(_vendorService, _cacheManager, _localizationService,
                searchTerm, true));
        }

        [HttpPost]
        public virtual IActionResult DefaultSelectSearch(string searchTerm, string propName)
        {
            if (propName.ToLower().Contains("category"))
                return Categories(searchTerm);

            if (propName.ToLower().Contains("manufacturer"))
                return Manufacturers(searchTerm);

            if (propName.ToLower().Contains("vendor"))
                return Vendors(searchTerm);
            

            var items = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = _localizationService.GetResource("Admin.Catalog.Categories.Fields.Parent.None"),
                    Value = "0"
                }
            };

            return Json(items);
        }
    }
}
