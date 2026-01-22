using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Vtable.Balcony.Configuration;
using Vtable.Balcony.Helpers;
using Vtable.Balcony.Services;

namespace Vtable.Balcony.Pages
{
    public class IndexModel : PageModel
    {
        public IList<ImageTuple> PreviousImages { get; set; } = new List<ImageTuple>();

        public AppConfig AppConfig { get; set; }

        private RequestsService RequestsService { get; }

        public IndexModel(RequestsService requestService, IOptions<AppConfig> appconfig)
        {
            this.RequestsService = requestService;
            this.AppConfig = appconfig.Value;
        }


        public void OnGet()
        {
            this.PreviousImages = this.RequestsService.GetPreviousImages();
        }
    }
}
