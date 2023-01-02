using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Controllers;

public class TodoController : BaseController
{
    private readonly TodoDbContext _context;

    public TodoController(TodoDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Job>>> GetAllJobs()
    {
        return Ok(await _context.Jobs.ToListAsync());
    }

    [HttpPost]
    public async Task<ActionResult<Job>> AddJob([FromBody] Job job)
    {
        await _context.AddAsync(job);
        await _context.SaveChangesAsync();
        return Ok(job);
    }

    [HttpPut]
    public async Task<ActionResult<Job>> UpdateJob([FromBody] Job job)
    {
        var task = _context.Jobs.FirstOrDefaultAsync(item => item.Id == job.Id);
        var record = await task;
        if (record == null)
        {
            return NotFound();
        }

        record.Name = job.Name;
        await _context.SaveChangesAsync();
        return Ok(job);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteJob(int id)
    {
        var task = _context.Jobs.FirstOrDefaultAsync(item => item.Id == id);
        var record = await task;
        if (record == null)
        {
            return NotFound();
        }

        _context.Remove(record);
        return Ok();
    }
}