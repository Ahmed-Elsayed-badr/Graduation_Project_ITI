using BLL.Configuration;
using BLL.ReviewInter;
using BLL.Services.Interfaces;
using Graduation_Project_ITI.Interfaces;

namespace Graduation_Project_ITI.ViewModels
{
    public class Parameter
    {
        IProductService productService { get; }
        ICategoryService categoryService { get;  }
        IWebHostEnvironment webHostEnvironment { get;  }
        ISellerOrderInterface sellerOrderService { get; }
        IReviewInterface _reviewService { get;  }
        AppDbContext context { get;  }
    }
}
