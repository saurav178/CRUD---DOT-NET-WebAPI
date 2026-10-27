using TaskApi.Models;

namespace TaskApi.Services;

public interface ITaskService
{
    Task<List<TaskItem>> GetAllAsync();
    Task<TaskItem?> GetByIdAsync(string id);
    Task<TaskItem> CreateAsync(TaskItem task);
    Task<bool> UpdateAsync(string id, TaskItem updatedTask);
    Task<bool> DeleteAsync(string id);
}
