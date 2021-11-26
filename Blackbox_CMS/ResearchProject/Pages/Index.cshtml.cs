using BlackboxData.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Research_API.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ResearchProject.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public IEnumerable<Table> tables = new List<Table>() { new Table(){ TABLE_NAME = "TEST" } };

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            Console.WriteLine("PAGE HAS BEEN ENTERED");

            string connectionstring = "Data Source=(local);Initial Catalog=Cinema_DB;Integrated Security=true";

            InformationSchemeHelper schemeHelper = new InformationSchemeHelper(connectionstring);

            tables = (await schemeHelper.GetTablesAsync()).ToList();

            Console.WriteLine("TABLES HAVE BEEN GOTTEN");

            return Page();
        }
    }
}
