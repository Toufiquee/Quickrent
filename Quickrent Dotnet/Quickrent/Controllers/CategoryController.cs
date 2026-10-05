using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Quickrent.DTO.UserDTO;
using Quickrent.Model;
using Quickrent.Service.Interface;

namespace Quickrent.Controllers
{
    [ApiController]
    [Route("api/category")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IMapper _mapper;

        public CategoryController(ICategoryService categoryService, IMapper mapper)
        {
            _categoryService = categoryService;
            _mapper = mapper;
        }
        
        // GET: api/category/{categoryId}/products
        [AllowAnonymous]
        [HttpGet("{categoryId}/products")]
        public async Task<IActionResult> GetProductsByCategoryId(int categoryId)
        {
            var products = await _categoryService.GetProductsByCategoryIdAsync(categoryId);
            if (products == null)
            {
                return NotFound($"Category with ID {categoryId} not found.");
            }

            return Ok(products);
        }

        [AllowAnonymous]
        [HttpGet("getall")]
        public IActionResult GetAllCategories() {
            try
            {
                List<Category> categories = _categoryService.getAllCategories();
                return Ok(categories);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}