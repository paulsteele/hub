using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hub.Server.Database;
using hub.Shared.Models.Finance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace hub.Server.Controllers;

[ApiController]
[Route("finance")]
public class FinanceController(
	ILogger<FinanceController> logger,
	DatabaseContext database
) : ControllerBase{

	[HttpGet]
	[Route("categories")]
	public ActionResult<IList<Category>> GetCategories()
	{
		return Ok(database.Categories.OrderBy(c => c.Order).ToList());
	}

	[HttpPut]
	[Route("category")]
	public async Task<ActionResult<Category>> AddCategory([FromBody] Category category)
	{
		database.Categories.Add(category);
		await database.SaveChangesAsync();
		return Ok(category);
	}
	
	[HttpGet]
	[Route("category/{id:int}")]
	public ActionResult GetCategory([FromRoute]int id)
	{
		var category = database.Categories.FirstOrDefault(c => c.Id == id);

		if (category == null)
		{
			return NotFound(id);
		}

		return Ok(category);
	}
	[HttpPut]
	[Route("category/{id:int}")]
	public async Task<ActionResult> AddCategory([FromRoute]int id, [FromBody] Category bodyCategory)
	{
		var category = database.Categories.FirstOrDefault(c => c.Id == id);

		if (category == null)
		{
			return NotFound(id);
		}

		category.Name = bodyCategory.Name;
		category.Budget = bodyCategory.Budget;
		category.Color = bodyCategory.Color;
		category.Emoji = bodyCategory.Emoji;
		
		await database.SaveChangesAsync();
		return Ok();
	}
	
	[HttpDelete]
	[Route("category/{id:int}")]
	public async Task<IActionResult> GetCategories([FromRoute]int id)
	{
		var category = database.Categories.FirstOrDefault(c => c.Id == id);

		if (category == null)
		{
			return NotFound(id);
		}

		database.Categories.Remove(category);
		
		await database.SaveChangesAsync();
		return Ok();
	}
	
	[HttpGet]
	[Route("transactions")]
	public ActionResult<IList<Category>> GetTransactions()
	{
		return Ok(database.Transactions.OrderBy(c => c.PostingDate).ToList());
	}
	
	[HttpPost]
	[Route("import")]
	public ActionResult Import([FromBody] List<string> fileContents)
	{
		var parsed = fileContents.Select(Transaction.TryParse).Where(t => t.transaction != null).ToList();

		var transactions = parsed.Select(t => t.transaction).ToList();
		return Ok();
	}
}