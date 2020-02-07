using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Areas.Admin.Infrastructure.Cache;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Vendors;

namespace Nop.Web.Areas.Admin.Helpers
{
    /// <summary>
    /// Select list helper
    /// </summary>
    public static class SelectListHelper
    {
        /// <summary>
        /// Get category list
        /// </summary>
        /// <param name="categoryService">Category service</param>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Category list</returns>
        public static List<SelectListItem> GetCategoryList(ICategoryService categoryService, ICacheManager cacheManager, bool showHidden = false)
        {
            if (categoryService == null)
                throw new ArgumentNullException(nameof(categoryService));

            if (cacheManager == null)
                throw new ArgumentNullException(nameof(cacheManager));

            var cacheKey = string.Format(NopModelCacheDefaults.CategoriesListKey, showHidden);
            var listItems = cacheManager.Get(cacheKey, () =>
            {
                var categories = categoryService.GetAllCategories(showHidden: showHidden);
                return categories.Select(c => new SelectListItem
                {
                    Text = categoryService.GetFormattedBreadCrumb(c, categories),
                    Value = c.Id.ToString()
                });
            });

            var result = new List<SelectListItem>();
            //clone the list to ensure that "selected" property is not set
            foreach (var item in listItems)
            {
                result.Add(new SelectListItem
                {
                    Text = item.Text,
                    Value = item.Value
                });
            }

            return result;
        }

        public static List<SelectListItem> GetSearchableCategoryList(ICategoryService categoryService,
            ICacheManager cacheManager, ILocalizationService localizationService,
            IRepository<Category> categoryRepository, string searchTerm, bool showHidden = false)
        {
            if (categoryService == null)
                throw new ArgumentNullException(nameof(categoryService));

            if (cacheManager == null)
                throw new ArgumentNullException(nameof(cacheManager));

            if (localizationService == null)
                throw new ArgumentNullException(nameof(localizationService));

            if (categoryRepository == null)
                throw new ArgumentNullException(nameof(categoryRepository));

            var listItems = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = localizationService.GetResource("Admin.Catalog.Categories.Fields.Parent.None"),
                    Value = "0"
                }
            };

            if (string.IsNullOrWhiteSpace(searchTerm.Trim()) || searchTerm.Trim().Length < 2)
                return listItems;

            var categoryName = searchTerm.Trim();
            var parentCategoryName = string.Empty;

            // if search-term contains formatted bread-crumb, parse out the last 2 terms
            if (searchTerm.Contains(">>"))
            {
                var breadCrumbs = searchTerm.Split(new[] { ">>" }, StringSplitOptions.None).ToList();
                var breadCrumbCount = breadCrumbs.Count;
                while (breadCrumbCount > 1)
                {
                    categoryName = breadCrumbs[breadCrumbCount - 1].Trim();
                    parentCategoryName = breadCrumbs[breadCrumbCount - 2].Trim();

                    if (!string.IsNullOrWhiteSpace(parentCategoryName))
                        breadCrumbCount = 0;
                    else
                        breadCrumbCount--;
                }
            }

            if ((string.IsNullOrWhiteSpace(categoryName) || categoryName.Length < 2) &&
                string.IsNullOrWhiteSpace(parentCategoryName))
                return listItems;

            var cacheKey = string.Format(NopModelCacheDefaults.CategoriesListKey, categoryName);

            if (!string.IsNullOrWhiteSpace(categoryName) && categoryName.Length > 1)
                listItems.AddRange(cacheManager.Get(cacheKey, () =>
                {
                    return categoryService.GetAllCategories(categoryName, showHidden: true).Select(x => new SelectListItem
                    {
                        Text = categoryService.GetFormattedBreadCrumb(x),
                        Value = x.Id.ToString()
                    }).ToList();
                }, 5));
            else
            {
                if (!categoryRepository.Table.Any(x => x.Name == parentCategoryName))
                    return listItems;

                var parentCategoryId = categoryRepository.Table.Where(x => x.Name == parentCategoryName)
                    .Select(x => x.Id).First();

                // return child categories of right-most breadcrumb term
                if (string.IsNullOrWhiteSpace(categoryName))
                {
                    cacheKey = string.Format(NopModelCacheDefaults.CategoriesListKey, parentCategoryId);
                    listItems.AddRange(cacheManager.Get(cacheKey, () =>
                    {
                        return categoryService.GetAllCategoriesByParentCategoryId(parentCategoryId, showHidden: true)
                            .Select(x => new SelectListItem
                            {
                                Text = categoryService.GetFormattedBreadCrumb(x), Value = x.Id.ToString()
                            }).ToList();
                    }, 5));
                }
                else
                {
                    cacheKey = string.Format(NopModelCacheDefaults.CategoriesListKey, parentCategoryName);
                    listItems.AddRange(cacheManager.Get(cacheKey, () =>
                    {
                        return categoryRepository.Table.Where(x =>
                                x.ParentCategoryId == parentCategoryId && x.Name.StartsWith(categoryName)).ToList()
                            .Select(x => new SelectListItem
                            {
                                Text = categoryService.GetFormattedBreadCrumb(x),
                                Value = x.Id.ToString()
                            }).ToList();
                    }, 5));
                }
            }

            // move default select list item to the end
            if (listItems.Count > 1)
            {
                var item = listItems[0];
                listItems.RemoveAt(0);
                listItems.Add(item);
            }

            var result = new List<SelectListItem>();
            //clone the list to ensure that "selected" property is not set
            foreach (var item in listItems)
            {
                result.Add(new SelectListItem
                {
                    Text = item.Text,
                    Value = item.Value
                });
            }

            return result;
        }

        /// <summary>
        /// Get manufacturer list
        /// </summary>
        /// <param name="manufacturerService">Manufacturer service</param>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Manufacturer list</returns>
        public static List<SelectListItem> GetManufacturerList(IManufacturerService manufacturerService, ICacheManager cacheManager, bool showHidden = false)
        {
            if (manufacturerService == null)
                throw new ArgumentNullException(nameof(manufacturerService));

            if (cacheManager == null)
                throw new ArgumentNullException(nameof(cacheManager));

            var cacheKey = string.Format(NopModelCacheDefaults.ManufacturersListKey, showHidden);
            var listItems = cacheManager.Get(cacheKey, () =>
            {
                var manufacturers = manufacturerService.GetAllManufacturers(showHidden: showHidden);
                return manufacturers.Select(m => new SelectListItem
                {
                    Text = m.Name,
                    Value = m.Id.ToString()
                });
            });

            var result = new List<SelectListItem>();
            //clone the list to ensure that "selected" property is not set
            foreach (var item in listItems)
            {
                result.Add(new SelectListItem
                {
                    Text = item.Text,
                    Value = item.Value
                });
            }

            return result;
        }

        public static List<SelectListItem> GetSearchableManufacturerList(IManufacturerService manufacturerService,
            ICacheManager cacheManager, ILocalizationService localizationService, string searchTerm,
            bool showHidden = false)
        {
            if (manufacturerService == null)
                throw new ArgumentNullException(nameof(manufacturerService));

            if (cacheManager == null)
                throw new ArgumentNullException(nameof(cacheManager));

            if (localizationService == null)
                throw new ArgumentNullException(nameof(localizationService));

            var listItems = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = localizationService.GetResource("Admin.Catalog.Categories.Fields.Parent.None"),
                    Value = "0"
                }
            };

            if (string.IsNullOrWhiteSpace(searchTerm.Trim()) || searchTerm.Trim().Length < 2)
                return listItems;

            var cacheKey = string.Format(NopModelCacheDefaults.ManufacturersListKey, searchTerm);
            listItems.AddRange(cacheManager.Get(cacheKey, () =>
            {
                var manufacturers = manufacturerService.GetAllManufacturers(searchTerm, showHidden: showHidden);
                return manufacturers.Select(m => new SelectListItem
                {
                    Text = m.Name,
                    Value = m.Id.ToString()
                });
            }, 5));

            // move default select list item to the end
            if (listItems.Count > 1)
            {
                var item = listItems[0];
                listItems.RemoveAt(0);
                listItems.Add(item);
            }

            var result = new List<SelectListItem>();
            //clone the list to ensure that "selected" property is not set
            foreach (var item in listItems)
            {
                result.Add(new SelectListItem
                {
                    Text = item.Text,
                    Value = item.Value
                });
            }

            return result;
        }

        /// <summary>
        /// Get vendor list
        /// </summary>
        /// <param name="vendorService">Vendor service</param>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Vendor list</returns>
        public static List<SelectListItem> GetVendorList(IVendorService vendorService, ICacheManager cacheManager, bool showHidden = false)
        {
            if (vendorService == null)
                throw new ArgumentNullException(nameof(vendorService));

            if (cacheManager == null)
                throw new ArgumentNullException(nameof(cacheManager));

            var cacheKey = string.Format(NopModelCacheDefaults.VendorsListKey, showHidden);
            var listItems = cacheManager.Get(cacheKey, () =>
            {
                var vendors = vendorService.GetAllVendors(showHidden: showHidden);
                return vendors.Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.Id.ToString()
                });
            });

            var result = new List<SelectListItem>();
            //clone the list to ensure that "selected" property is not set
            foreach (var item in listItems)
            {
                result.Add(new SelectListItem
                {
                    Text = item.Text,
                    Value = item.Value
                });
            }

            return result;
        }

        public static List<SelectListItem> GetSearchableVendorList(IVendorService vendorService,
            ICacheManager cacheManager, ILocalizationService localizationService, string searchTerm,
            bool showHidden = false)
        {
            if (vendorService == null)
                throw new ArgumentNullException(nameof(vendorService));

            if (cacheManager == null)
                throw new ArgumentNullException(nameof(cacheManager));

            if (localizationService == null)
                throw new ArgumentNullException(nameof(localizationService));

            var listItems = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = localizationService.GetResource("Admin.Catalog.Categories.Fields.Parent.None"),
                    Value = "0"
                }
            };

            if (string.IsNullOrWhiteSpace(searchTerm.Trim()) || searchTerm.Trim().Length < 2)
                return listItems;

            var cacheKey = string.Format(NopModelCacheDefaults.VendorsListKey, searchTerm);
            listItems.AddRange(cacheManager.Get(cacheKey, () =>
            {
                var vendors = vendorService.GetAllVendors(searchTerm, showHidden: showHidden);
                return vendors.Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.Id.ToString()
                });
            }, 5));

            // move default select list item to the end
            if (listItems.Count > 1)
            {
                var item = listItems[0];
                listItems.RemoveAt(0);
                listItems.Add(item);
            }

            var result = new List<SelectListItem>();
            //clone the list to ensure that "selected" property is not set
            foreach (var item in listItems)
            {
                result.Add(new SelectListItem
                {
                    Text = item.Text,
                    Value = item.Value
                });
            }

            return result;
        }
    }
}